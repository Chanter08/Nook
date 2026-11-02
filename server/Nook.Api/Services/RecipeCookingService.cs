using System.Data;
using Microsoft.EntityFrameworkCore;
using Nook.Api.Data;
using Nook.Api.Models;

namespace Nook.Api.Services;

public sealed class RecipeCookingService(NookDbContext db)
{
    public async Task<object> GetPreviewAsync(int recipeId, decimal? servings)
    {
        var recipe = await db.Recipes.AsNoTracking()
            .FirstOrDefaultAsync(r => r.RecipeId == recipeId && !r.IsArchived)
            ?? throw new CookingException("Recipe not found.", 404);

        var targetServings = servings ?? (recipe.Servings is > 0 ? recipe.Servings.Value : 1);
        ValidateServings(targetServings);

        var pantry = await db.PantryItems.AsNoTracking()
            .OrderBy(p => p.Name).ThenBy(p => p.ExpiryDate).ToListAsync();

        var ingredients = await db.RecipeIngredients.AsNoTracking()
            .Include(x => x.Ingredient)
            .Where(x => x.RecipeId == recipeId)
            .OrderBy(x => x.SortOrder).ToListAsync();

        var ratio = targetServings / (recipe.Servings is > 0 ? recipe.Servings.Value : 1);
        var previews = ingredients.Select(ingredient =>
        {
            var candidates = pantry.Where(p =>
                NormalizeName(p.Name) == NormalizeName(ingredient.Ingredient.Name)
                && p.Quantity is > 0
                && ConvertQuantity(ingredient.Quantity, ingredient.Unit, p.Unit) is not null)
                .ToList();

            // A single exact name/unit match is safe to propose. Ambiguous or missing
            // items must be selected by the cook, never silently deducted.
            var match = candidates.Count == 1 ? candidates[0] : null;
            var suggested = match is null ? null :
                ConvertQuantity(ingredient.Quantity, ingredient.Unit, match.Unit) * ratio;
            suggested = suggested.HasValue ? decimal.Round(suggested.Value, 2) : null;

            return new
            {
                recipeIngredientId = ingredient.RecipeIngredientId,
                name = ingredient.Ingredient.Name,
                requiredQuantity = ingredient.Quantity.HasValue
                    ? decimal.Round(ingredient.Quantity.Value * ratio, 2) : (decimal?)null,
                unit = ingredient.Unit,
                suggestedPantryItemId = suggested is > 0 && match?.Quantity >= suggested
                    ? match.PantryItemId : (int?)null,
                suggestedQuantity = suggested is > 0 && match?.Quantity >= suggested
                    ? suggested : (decimal?)null
            };
        }).ToList();

        return new
        {
            recipeId = recipe.RecipeId,
            recipeName = recipe.Name,
            originalServings = recipe.Servings is > 0 ? recipe.Servings.Value : 1,
            servings = targetServings,
            ingredients = previews,
            pantryItems = pantry.Select(p => new
            {
                id = p.PantryItemId,
                name = p.Name,
                quantity = p.Quantity,
                unit = p.Unit,
                storageLocation = p.StorageLocation
            }).ToList()
        };
    }

    public async Task<object> CookAsync(int recipeId, CookRecipeRequest request)
    {
        ValidateServings(request.Servings);
        if (request.Items is null)
            throw new CookingException("Provide the ingredient review.", 400);

        await using var transaction =
            await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);

        var recipe = await db.Recipes
            .FirstOrDefaultAsync(x => x.RecipeId == recipeId && !x.IsArchived)
            ?? throw new CookingException("Recipe not found.", 404);

        var ingredientNames = await db.RecipeIngredients
            .Where(x => x.RecipeId == recipeId)
            .Select(x => new { x.RecipeIngredientId, x.Ingredient.Name })
            .ToDictionaryAsync(x => x.RecipeIngredientId, x => x.Name);

        if (request.Items.Count != ingredientNames.Count ||
            request.Items.Select(x => x.RecipeIngredientId).Distinct().Count() != request.Items.Count ||
            request.Items.Any(x => !ingredientNames.ContainsKey(x.RecipeIngredientId)))
            throw new CookingException("Review every recipe ingredient exactly once.", 400);

        if (request.MealPlanEntryId.HasValue)
        {
            var exists = await db.MealPlanEntries.AnyAsync(x =>
                x.MealPlanEntryId == request.MealPlanEntryId.Value &&
                x.RecipeId == recipeId);
            if (!exists)
                throw new CookingException("This meal plan entry does not match the recipe.", 400);

            if (await db.CookedRecipes.AnyAsync(x =>
                x.MealPlanEntryId == request.MealPlanEntryId.Value && x.UndoneAtUtc == null))
                throw new CookingException("This planned meal has already been marked as cooked.", 409);
        }

        foreach (var item in request.Items)
        {
            if (item.Quantity < 0 || item.Quantity > 9999999 ||
                decimal.Round(item.Quantity, 2) != item.Quantity ||
                (item.Quantity > 0 && item.PantryItemId is null) ||
                (item.Quantity == 0 && item.PantryItemId is not null))
                throw new CookingException("Use a valid pantry quantity or skip that ingredient.", 400);
        }

        var consumed = request.Items.Where(x => x.Quantity > 0).ToList();
        var ids = consumed.Select(x => x.PantryItemId!.Value).Distinct().ToList();
        var stock = await db.PantryItems.Where(p => ids.Contains(p.PantryItemId))
            .ToDictionaryAsync(p => p.PantryItemId);

        foreach (var group in consumed.GroupBy(x => x.PantryItemId!.Value))
        {
            if (!stock.TryGetValue(group.Key, out var pantryItem))
                throw new CookingException("One of the selected pantry items no longer exists. Review again.", 409);

            var used = group.Sum(x => x.Quantity);
            if (pantryItem.Quantity is null || pantryItem.Quantity.Value < used)
                throw new CookingException(
                    $"Not enough {pantryItem.Name} in stock. Refresh the preview and adjust the amount.", 409);

            pantryItem.Quantity -= used;
            pantryItem.UpdatedDate = DateTime.UtcNow;
        }

        var cooking = new CookedRecipe
        {
            RecipeId = recipeId,
            MealPlanEntryId = request.MealPlanEntryId,
            Servings = request.Servings,
            CookedAtUtc = DateTime.UtcNow
        };

        foreach (var item in consumed)
        {
            var pantryItem = stock[item.PantryItemId!.Value];
            cooking.Usages.Add(new CookedIngredientUsage
            {
                PantryItemId = pantryItem.PantryItemId,
                RecipeIngredientId = item.RecipeIngredientId,
                IngredientName = ingredientNames[item.RecipeIngredientId],
                Quantity = item.Quantity,
                Unit = pantryItem.Unit
            });
        }

        db.CookedRecipes.Add(cooking);
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        return new { id = cooking.CookedRecipeId, deductedItems = consumed.Count };
    }

    public async Task<object> GetHistoryAsync(int recipeId)
    {
        return await db.CookedRecipes.AsNoTracking()
            .Where(x => x.RecipeId == recipeId)
            .OrderByDescending(x => x.CookedAtUtc)
            .Take(10)
            .Select(x => new
            {
                id = x.CookedRecipeId,
                cookedAtUtc = x.CookedAtUtc,
                undoneAtUtc = x.UndoneAtUtc,
                servings = x.Servings,
                mealPlanEntryId = x.MealPlanEntryId,
                items = x.Usages.Select(u => new
                {
                    pantryItemId = u.PantryItemId,
                    ingredientName = u.IngredientName,
                    quantity = u.Quantity,
                    unit = u.Unit
                }).ToList()
            }).ToListAsync();
    }

    public async Task UndoAsync(int cookingId)
    {
        await using var transaction =
            await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);

        var cooking = await db.CookedRecipes.Include(x => x.Usages)
            .FirstOrDefaultAsync(x => x.CookedRecipeId == cookingId)
            ?? throw new CookingException("Cooking record not found.", 404);

        if (cooking.UndoneAtUtc.HasValue)
            throw new CookingException("This cooking record has already been undone.", 409);

        var ids = cooking.Usages.Select(u => u.PantryItemId).Distinct().ToList();
        var pantry = await db.PantryItems.Where(x => ids.Contains(x.PantryItemId))
            .ToDictionaryAsync(x => x.PantryItemId);

        foreach (var group in cooking.Usages.GroupBy(x => x.PantryItemId))
        {
            if (!pantry.TryGetValue(group.Key, out var item) || item.Quantity is null)
                throw new CookingException(
                    "Cannot undo because one of the pantry entries was removed or changed to an untracked quantity.", 409);

            item.Quantity += group.Sum(x => x.Quantity);
            item.UpdatedDate = DateTime.UtcNow;
        }

        cooking.UndoneAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    private static void ValidateServings(decimal servings)
    {
        if (servings <= 0 || servings > 100 || decimal.Round(servings, 2) != servings)
            throw new CookingException("Servings must be between 0.01 and 100.", 400);
    }

    private static string NormalizeName(string value) =>
        string.Join(" ", value.Trim().ToLowerInvariant()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries));

    // Recipe and pantry quantities are compared only when units are compatible.
    // We never convert a pack to grams or guess the size of an onion.
    private static decimal? ConvertQuantity(decimal? quantity, string? fromUnit, string? toUnit)
    {
        if (quantity is null) return null;
        static (string? Unit, decimal Multiplier) Canonical(string? input)
        {
            var unit = input?.Trim().ToLowerInvariant();
            return unit switch
            {
                "kg" or "kilogram" or "kilograms" => ("g", 1000m),
                "g" or "gram" or "grams" => ("g", 1m),
                "l" or "litre" or "litres" or "liter" or "liters" => ("ml", 1000m),
                "ml" or "millilitre" or "millilitres" or "milliliter" or "milliliters" => ("ml", 1m),
                "piece" or "pieces" or "pcs" or "each" => ("each", 1m),
                _ => (unit, 1m)
            };
        }

        var from = Canonical(fromUnit);
        var to = Canonical(toUnit);
        if (string.IsNullOrEmpty(from.Unit) || from.Unit != to.Unit) return null;
        return quantity.Value * from.Multiplier / to.Multiplier;
    }
}

public sealed class CookRecipeRequest
{
    public decimal Servings { get; set; }
    public int? MealPlanEntryId { get; set; }
    public List<CookIngredientRequest> Items { get; set; } = new();
}

public sealed class CookIngredientRequest
{
    public int RecipeIngredientId { get; set; }
    public int? PantryItemId { get; set; }
    public decimal Quantity { get; set; }
}

public sealed class CookingException(string message, int statusCode) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}