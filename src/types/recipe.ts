export interface RecipeSummary {
    id: number;
    name: string;
    description: string | null;
    prepTimeMinutes: number | null;
    cookTimeMinutes: number | null;
    totalTimeMinutes: number;
    servings: number | null;
    imageUrl: string | null;
    calories: number | null;
    proteinGrams: number | null;
    carbohydrateGrams: number | null;
    fatGrams: number | null;
    mealTypes: string[];
    cuisines: string[];
  }
  
  export interface RecipeIngredient {
    id: number;
    name: string;
    quantity: number | null;
    unit: string | null;
    notes: string | null;
  }
  
  export interface RecipeStep {
    stepNumber: number;
    instruction: string;
  }
  
  export interface RecipeDetail extends RecipeSummary {
    ingredients: RecipeIngredient[];
    steps: RecipeStep[];
  }

  export interface RecipeOptions {
    mealTypes: string[];
    cuisines: string[];
  }
  
  export interface NewRecipeIngredient {
    name: string;
    quantity: string;
    unit: string;
    notes: string;
  }
  
  export interface NewRecipeStep {
    instruction: string;
  }