import { apiRequest } from "@/api/client";
import type {
  RecipeDetail,
  RecipeOptions,
  RecipeSummary,
} from "@/types/recipe";

export interface CreateRecipePayload {
  name: string;
  description: string | null;
  prepTimeMinutes: number | null;
  cookTimeMinutes: number | null;
  servings: number | null;
  imageUrl: string | null;
  calories: number | null;
  proteinGrams: number | null;
  carbohydrateGrams: number | null;
  fatGrams: number | null;
  mealTypes: string[];
  cuisines: string[];
  ingredients: {
    name: string;
    quantity: number | null;
    unit: string | null;
    notes: string | null;
  }[];
  steps: {
    instruction: string;
  }[];
}

export function getRecipes() {
  return apiRequest<RecipeSummary[]>("/api/recipes");
}

export function getRecipe(id: number | string) {
  return apiRequest<RecipeDetail>(`/api/recipes/${id}`);
}

export function getRecipeOptions() {
  return apiRequest<RecipeOptions>("/api/recipes/options");
}

export function createRecipe(payload: CreateRecipePayload) {
  return apiRequest<{ id: number }>("/api/recipes", {
    method: "POST",
    body: JSON.stringify(payload),
  });
}