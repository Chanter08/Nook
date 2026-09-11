using Microsoft.EntityFrameworkCore;
using Nook.Api.Models;

namespace Nook.Api.Data;

public class NookDbContext : DbContext
{
    public NookDbContext(DbContextOptions<NookDbContext> options)
        : base(options)
    {
    }

    public DbSet<Recipe> Recipes => Set<Recipe>();
    public DbSet<RecipeType> RecipeTypes => Set<RecipeType>();
    public DbSet<RecipeRecipeType> RecipeRecipeTypes => Set<RecipeRecipeType>();
    public DbSet<Cuisine> Cuisines => Set<Cuisine>();
    public DbSet<RecipeCuisine> RecipeCuisines => Set<RecipeCuisine>();
    public DbSet<Ingredient> Ingredients => Set<Ingredient>();
    public DbSet<RecipeIngredient> RecipeIngredients => Set<RecipeIngredient>();
    public DbSet<RecipeStep> RecipeSteps => Set<RecipeStep>();
    public DbSet<ShoppingList> ShoppingLists => Set<ShoppingList>();
    public DbSet<ShoppingListItem> ShoppingListItems => Set<ShoppingListItem>();
    public DbSet<ShoppingCatalogItem> ShoppingCatalogItems => Set<ShoppingCatalogItem>();
    public DbSet<MealPlanEntry> MealPlanEntries => Set<MealPlanEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Recipe>(entity =>
        {
            entity.ToTable("Recipe");

            entity.Property(x => x.ProteinGrams)
                .HasPrecision(18, 2);

            entity.Property(x => x.CarbohydrateGrams)
                .HasPrecision(18, 2);

            entity.Property(x => x.FatGrams)
                .HasPrecision(18, 2);
        });

        modelBuilder.Entity<RecipeType>()
            .ToTable("RecipeType");

        modelBuilder.Entity<Cuisine>()
            .ToTable("Cuisine");

        modelBuilder.Entity<RecipeRecipeType>()
            .ToTable("RecipeRecipeType");

        modelBuilder.Entity<RecipeCuisine>()
            .ToTable("RecipeCuisine");

        modelBuilder.Entity<Ingredient>()
        .ToTable("Ingredient");

        modelBuilder.Entity<RecipeIngredient>(entity =>
        {
            entity.ToTable("RecipeIngredient");

            entity.Property(x => x.Quantity)
                .HasPrecision(18, 2);
        });

        modelBuilder.Entity<RecipeStep>()
        .ToTable("RecipeStep");


        modelBuilder.Entity<RecipeRecipeType>()
            .HasKey(x => new
            {
                x.RecipeId,
                x.RecipeTypeId
            });

        modelBuilder.Entity<RecipeRecipeType>()
            .HasOne(x => x.Recipe)
            .WithMany(x => x.RecipeRecipeTypes)
            .HasForeignKey(x => x.RecipeId);

        modelBuilder.Entity<RecipeRecipeType>()
            .HasOne(x => x.RecipeType)
            .WithMany(x => x.RecipeRecipeTypes)
            .HasForeignKey(x => x.RecipeTypeId);


        modelBuilder.Entity<RecipeCuisine>()
            .HasKey(x => new
            {
                x.RecipeId,
                x.CuisineId
            });

        modelBuilder.Entity<RecipeCuisine>()
            .HasOne(x => x.Recipe)
            .WithMany(x => x.RecipeCuisines)
            .HasForeignKey(x => x.RecipeId);

        modelBuilder.Entity<RecipeCuisine>()
            .HasOne(x => x.Cuisine)
            .WithMany(x => x.RecipeCuisines)
            .HasForeignKey(x => x.CuisineId);

        modelBuilder.Entity<RecipeIngredient>()
            .HasOne(x => x.Recipe)
            .WithMany(x => x.RecipeIngredients)
            .HasForeignKey(x => x.RecipeId);

        modelBuilder.Entity<RecipeIngredient>()
            .HasOne(x => x.Ingredient)
            .WithMany(x => x.RecipeIngredients)
            .HasForeignKey(x => x.IngredientId);

        modelBuilder.Entity<RecipeStep>()
            .HasOne(x => x.Recipe)
            .WithMany(x => x.RecipeSteps)
            .HasForeignKey(x => x.RecipeId);

        modelBuilder.Entity<ShoppingList>(entity =>
        {
            entity.ToTable("ShoppingList");

            entity.HasKey(x => x.ShoppingListId);

            entity.Property(x => x.Name)
                .HasMaxLength(100)
                .IsRequired();
        });


        modelBuilder.Entity<ShoppingListItem>(entity =>
        {
            entity.ToTable("ShoppingListItem");

            entity.HasKey(x => x.ShoppingListItemId);

            entity.Property(x => x.Name)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(x => x.Quantity)
                .HasPrecision(10, 2);

            entity.Property(x => x.Unit)
                .HasMaxLength(50);

            entity.Property(x => x.Notes)
                .HasMaxLength(250);

            entity.Property(x => x.Category)
                .HasMaxLength(100);


            entity.HasOne(x => x.ShoppingList)
                .WithMany(x => x.Items)
                .HasForeignKey(x => x.ShoppingListId)
                .OnDelete(DeleteBehavior.Cascade);


            entity.HasOne(x => x.Ingredient)
                .WithMany()
                .HasForeignKey(x => x.IngredientId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<ShoppingCatalogItem>(entity =>
        {
            entity.ToTable("ShoppingCatalogItem");

            entity.HasKey(x => x.ShoppingCatalogItemId);

            entity.Property(x => x.Name)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(x => x.NormalizedName)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(x => x.DefaultUnit)
                .HasMaxLength(50);

            entity.Property(x => x.Category)
                .HasMaxLength(100);

            entity.Property(x => x.DefaultNotes)
                .HasMaxLength(250);

            entity.HasIndex(x => x.NormalizedName)
                .IsUnique();
        });

        modelBuilder.Entity<ShoppingListItem>()
            .HasOne(x =>
                x.ShoppingCatalogItem)
            .WithMany(x =>
                x.ShoppingListItems)
            .HasForeignKey(x =>
                x.ShoppingCatalogItemId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<MealPlanEntry>(entity =>
         {
             entity.ToTable("MealPlanEntry");

             entity.HasKey(x => x.MealPlanEntryId);

             entity.Property(x => x.MealType)
                 .HasMaxLength(50)
                 .IsRequired();

             entity.HasIndex(x => new { x.PlanDate, x.MealType })
                 .IsUnique();

             entity.HasOne(x => x.Recipe)
                  .WithMany(x => x.MealPlanEntries)
                  .HasForeignKey(x => x.RecipeId)
                  .OnDelete(DeleteBehavior.NoAction);
         });
    }
}