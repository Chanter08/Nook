using System.Text.Json.Serialization;

namespace Nook.Api.Integrations.Recipes.Spoonacular.Models;

public class SpoonacularRecipe
{
    public int Id { get; set; }

    [JsonPropertyName("title")]
    public string Name { get; set; } = string.Empty;

    public string? Image { get; set; }
    public string? Summary { get; set; }
    public string? SourceUrl { get; set; }

    public int? ReadyInMinutes { get; set; }
    public int? PreparationMinutes { get; set; }
    public int? CookingMinutes { get; set; }
    public int? Servings { get; set; }

    public List<string> Cuisines { get; set; } = [];
    public List<string> DishTypes { get; set; } = [];

    public List<SpoonacularIngredient> ExtendedIngredients { get; set; } = [];
    public List<SpoonacularInstructionGroup> AnalyzedInstructions { get; set; } = [];

    public SpoonacularNutrition? Nutrition { get; set; }
}

public class SpoonacularIngredient
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Original { get; set; }
    public decimal Amount { get; set; }
    public string? Unit { get; set; }
    public List<string> Meta { get; set; } = [];
    public SpoonacularMeasures? Measures { get; set; }
}

public class SpoonacularMeasures
{
    public SpoonacularMeasure? Metric { get; set; }
}

public class SpoonacularMeasure
{
    public decimal Amount { get; set; }
    public string? UnitShort { get; set; }
    public string? UnitLong { get; set; }
}

public class SpoonacularInstructionGroup
{
    public string? Name { get; set; }
    public List<SpoonacularInstructionStep> Steps { get; set; } = [];
}

public class SpoonacularInstructionStep
{
    public int Number { get; set; }
    public string Step { get; set; } = string.Empty;
}

public class SpoonacularNutrition
{
    public List<SpoonacularNutrient> Nutrients { get; set; } = [];
}

public class SpoonacularNutrient
{
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Unit { get; set; }
}