import { apiRequest } from "@/api/client";
import type { AddShoppingItemPayload, ShoppingCatalogSuggestion, ShoppingListResponse } from "@/types/shopping";

export function getShoppingList() {
  return apiRequest<ShoppingListResponse>("/api/shopping");
}

export function searchShoppingCatalog(query: string, signal?: AbortSignal) {
    return apiRequest<ShoppingCatalogSuggestion[]>(
      `/api/shopping/catalog/search?q=${encodeURIComponent(query)}`,
      { signal }
    );
  }
export function addShoppingItem(payload: AddShoppingItemPayload) {
  return apiRequest<{ id: number; name: string; merged: boolean }>("/api/shopping/items", {
    method: "POST",
    body: JSON.stringify(payload)
  });
}

export function addRecipeIngredientsToShopping(recipeId: number, recipeIngredientIds: number[]) {
  return apiRequest(`/api/shopping/from-recipe/${recipeId}`, {
    method: "POST",
    body: JSON.stringify({ recipeIngredientIds })
  });
}

export function completeShoppingItem(id: number) {
  return apiRequest<void>(`/api/shopping/items/${id}/complete`, {
    method: "PATCH"
  });
}

export function restoreShoppingItem(id: number) {
  return apiRequest<void>(`/api/shopping/items/${id}/uncomplete`, {
    method: "PATCH"
  });
}

export function clearCompletedShoppingItems() {
  return apiRequest<{ deleted: number }>("/api/shopping/completed", {
    method: "DELETE"
  });
}