import { apiRequest } from "@/api/client";

export interface CookingPantryItem {
  id: number;
  name: string;
  quantity: number | null;
  unit: string | null;
  storageLocation: string;
}

export interface CookingIngredientPreview {
  recipeIngredientId: number;
  name: string;
  requiredQuantity: number | null;
  unit: string | null;
  suggestedPantryItemId: number | null;
  suggestedQuantity: number | null;
}

export interface CookingPreview {
  recipeId: number;
  recipeName: string;
  originalServings: number;
  servings: number;
  ingredients: CookingIngredientPreview[];
  pantryItems: CookingPantryItem[];
}

export interface CookingRecord {
  id: number;
  cookedAtUtc: string;
  undoneAtUtc: string | null;
  servings: number;
  mealPlanEntryId: number | null;
  items: Array<{
    pantryItemId: number;
    ingredientName: string;
    quantity: number;
    unit: string | null;
  }>;
}

export interface CookRecipePayload {
  servings: number;
  mealPlanEntryId: number | null;
  items: Array<{
    recipeIngredientId: number;
    pantryItemId: number | null;
    quantity: number;
  }>;
}

export function getCookingPreview(recipeId: number, servings?: number) {
  const query = servings === undefined ? "" : `?servings=${encodeURIComponent(servings)}`;
  return apiRequest<CookingPreview>(`/api/recipes/${recipeId}/cook/preview${query}`);
}

export function cookRecipe(recipeId: number, payload: CookRecipePayload) {
  return apiRequest<{ id: number; deductedItems: number }>(
    `/api/recipes/${recipeId}/cook`, {
      method: "POST",
      body: JSON.stringify(payload),
    },
  );
}

export function getCookingHistory(recipeId: number) {
  return apiRequest<CookingRecord[]>(`/api/recipes/${recipeId}/cooks`);
}

export function undoCooking(cookingId: number) {
  return apiRequest<void>(`/api/recipes/cooks/${cookingId}/undo`, {
    method: "POST",
  });
}