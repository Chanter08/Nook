using Microsoft.EntityFrameworkCore;
using Nook.Api.Data;
using Nook.Api.DTOs;
using Nook.Api.Models;
using Nook.Api.DTOs.Recipes;

namespace Nook.Api.Endpoints;

public static class RecipeEndpoints
{
    public static IEndpointRouteBuilder MapRecipeEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/recipes");

        group.MapGet("", GetRecipes);
        group.MapGet("/options", GetOptions);
        group.MapPost("", CreateRecipe);
        group.MapGet("/{id:int}", GetRecipe);

        return app;
    }

    private static async Task<IResult> GetRecipes(NookDbContext db)
    {
        var recipes = await db.Recipes
            .AsNoTracking()
            .Where(r => !r.IsArchived)
            .OrderBy(r => r.Name)
            .Select(r => new RecipeSummaryResponse
            {
                Id = r.RecipeId,
                Name = r.Name,
                Description = r.Description,
                PrepTimeMinutes = r.PrepTimeMinutes,
                CookTimeMinutes = r.CookTimeMinutes,
                TotalTimeMinutes = (r.PrepTimeMinutes ?? 0) + (r.CookTimeMinutes ?? 0),
                Servings = r.Servings,
                ImageUrl = r.ImageUrl,
                Calories = r.Calories,
                ProteinGrams = r.ProteinGrams,
                CarbohydrateGrams = r.CarbohydrateGrams,
                FatGrams = r.FatGrams,
                MealTypes = r.RecipeRecipeTypes
                .Select(x => x.RecipeType.Name)
                .OrderBy(x => x)
                .ToList(),
                Cuisines = r.RecipeCuisines
                .Select(x => x.Cuisine.Name)
                .OrderBy(x => x)
                .ToList()
            })
            .ToListAsync();

        return Results.Ok(recipes);
    }

    private static async Task<IResult> GetRecipe(int id, NookDbContext db)
    {
        var recipe = await db.Recipes
            .AsNoTracking()
            .Where(r => r.RecipeId == id && !r.IsArchived)
            .Select(r => new RecipeSummaryResponse
            {
                Id = r.RecipeId,
                Name = r.Name,
                Description = r.Description,
                PrepTimeMinutes = r.PrepTimeMinutes,
                CookTimeMinutes = r.CookTimeMinutes,
                TotalTimeMinutes = (r.PrepTimeMinutes ?? 0) + (r.CookTimeMinutes ?? 0),
                Servings = r.Servings,
                ImageUrl = r.ImageUrl,
                Calories = r.Calories,
                ProteinGrams = r.ProteinGrams,
                CarbohydrateGrams = r.CarbohydrateGrams,
                FatGrams = r.FatGrams
            })
            .FirstOrDefaultAsync();

        if (recipe is null)
        {
            return Results.NotFound();
        }

        var mealTypes = await db.Recipes
            .AsNoTracking()
            .Where(r => r.RecipeId == id)
            .SelectMany(r => r.RecipeRecipeTypes)
            .OrderBy(x => x.RecipeType.Name)
            .Select(x => x.RecipeType.Name)
            .ToListAsync();

        var cuisines = await db.Recipes
            .AsNoTracking()
            .Where(r => r.RecipeId == id)
            .SelectMany(r => r.RecipeCuisines)
            .OrderBy(x => x.Cuisine.Name)
            .Select(x => x.Cuisine.Name)
            .ToListAsync();

        var ingredients = await db.Recipes
            .AsNoTracking()
            .Where(r => r.RecipeId == id)
            .SelectMany(r => r.RecipeIngredients)
            .OrderBy(x => x.SortOrder)
            .Select(x => new RecipeIngredientResponse
            {
                Id = x.RecipeIngredientId,
                Name = x.Ingredient.Name,
                Quantity = x.Quantity,
                Unit = x.Unit,
                Notes = x.Notes
            })
            .ToListAsync();

        var steps = await db.Recipes
            .AsNoTracking()
            .Where(r => r.RecipeId == id)
            .SelectMany(r => r.RecipeSteps)
            .OrderBy(x => x.StepNumber)
            .Select(x => new RecipeStepResponse
            {
                StepNumber = x.StepNumber,
                Instruction = x.Instruction
            })
            .ToListAsync();

        return Results.Ok(new RecipeDetailResponse
        {
            Id = recipe.Id,
            Name = recipe.Name,
            Description = recipe.Description,
            PrepTimeMinutes = recipe.PrepTimeMinutes,
            CookTimeMinutes = recipe.CookTimeMinutes,
            TotalTimeMinutes = recipe.TotalTimeMinutes,
            Servings = recipe.Servings,
            ImageUrl = recipe.ImageUrl,
            Calories = recipe.Calories,
            ProteinGrams = recipe.ProteinGrams,
            CarbohydrateGrams = recipe.CarbohydrateGrams,
            FatGrams = recipe.FatGrams,
            MealTypes = mealTypes,
            Cuisines = cuisines,
            Ingredients = ingredients,
            Steps = steps
        });
    }

    private static async Task<IResult> CreateRecipe(CreateRecipeRequest request, NookDbContext db)
    {
        var name = request.Name.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            return Results.BadRequest(new { message = "Recipe name is required." });
        }

        if (request.Ingredients.Count == 0)
        {
            return Results.BadRequest(new { message = "Add at least one ingredient." });
        }

        if (request.Steps.Count == 0)
        {
            return Results.BadRequest(new { message = "Add at least one method step." });
        }

        var recipe = new Recipe
        {
            Name = name,
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            PrepTimeMinutes = request.PrepTimeMinutes,
            CookTimeMinutes = request.CookTimeMinutes,
            Servings = request.Servings,
            ImageUrl = string.IsNullOrWhiteSpace(request.ImageUrl) ? null : request.ImageUrl.Trim(),
            Calories = request.Calories,
            ProteinGrams = request.ProteinGrams,
            CarbohydrateGrams = request.CarbohydrateGrams,
            FatGrams = request.FatGrams,
            CreatedDate = DateTime.UtcNow,
            IsArchived = false
        };

        for (var i = 0; i < request.Ingredients.Count; i++)
        {
            var item = request.Ingredients[i];
            var ingredientName = item.Name.Trim();

            if (string.IsNullOrWhiteSpace(ingredientName)) continue;

            var normalizedName = ingredientName.ToLower();

            var ingredient = await db.Ingredients
                .FirstOrDefaultAsync(x => x.Name.ToLower() == normalizedName);

            if (ingredient is null)
            {
                ingredient = new Ingredient
                {
                    Name = ingredientName,
                    DefaultUnit = string.IsNullOrWhiteSpace(item.Unit) ? null : item.Unit.Trim()
                };

                db.Ingredients.Add(ingredient);
            }

            recipe.RecipeIngredients.Add(new RecipeIngredient
            {
                Ingredient = ingredient,
                Quantity = item.Quantity,
                Unit = string.IsNullOrWhiteSpace(item.Unit) ? null : item.Unit.Trim(),
                Notes = string.IsNullOrWhiteSpace(item.Notes) ? null : item.Notes.Trim(),
                SortOrder = i + 1
            });
        }

        for (var i = 0; i < request.Steps.Count; i++)
        {
            var instruction = request.Steps[i].Instruction.Trim();

            if (string.IsNullOrWhiteSpace(instruction)) continue;

            recipe.RecipeSteps.Add(new RecipeStep
            {
                StepNumber = i + 1,
                Instruction = instruction
            });
        }

        var requestedMealTypes = request.MealTypes
            .Select(x => x.Trim())
            .Where(x => x.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var mealTypes = await db.RecipeTypes
            .Where(x => requestedMealTypes.Contains(x.Name))
            .ToListAsync();

        foreach (var mealType in mealTypes)
        {
            recipe.RecipeRecipeTypes.Add(new RecipeRecipeType
            {
                RecipeType = mealType
            });
        }

        var requestedCuisines = request.Cuisines
            .Select(x => x.Trim())
            .Where(x => x.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var cuisines = await db.Cuisines
            .Where(x => requestedCuisines.Contains(x.Name))
            .ToListAsync();

        foreach (var cuisine in cuisines)
        {
            recipe.RecipeCuisines.Add(new RecipeCuisine
            {
                Cuisine = cuisine
            });
        }

        db.Recipes.Add(recipe);
        await db.SaveChangesAsync();

        return Results.Created(
        $"/api/recipes/{recipe.RecipeId}",
        new CreateRecipeResponse
        {
            Id = recipe.RecipeId
        });
    }

    private static async Task<IResult> GetOptions(NookDbContext db)
    {
        var mealTypes = await db.RecipeTypes
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => x.Name)
            .ToListAsync();

        var cuisines = await db.Cuisines
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => x.Name)
            .ToListAsync();

        return Results.Ok(new RecipeOptionsResponse
        {
            MealTypes = mealTypes,
            Cuisines = cuisines
        });
    }
}