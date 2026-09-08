export type ShoppingItemOrigin = "recipe" | "manual";

export interface ShoppingListItem {
  id: number;
  ingredientId: number | null;
  shoppingCatalogItemId: number | null;
  name: string;
  quantity: number | null;
  unit: string | null;
  notes: string | null;
  category: string | null;
  origin: ShoppingItemOrigin;
}

export interface ShoppingListResponse {
  id: number;
  name: string;
  itemCount: number;
  completedCount: number;
  items: ShoppingListItem[];
  completedItems: ShoppingListItem[];
}

export interface ShoppingCatalogSuggestion {
  id: number;
  name: string;
  unit: string | null;
  category: string | null;
  notes: string | null;
}

export interface AddShoppingItemPayload {
  shoppingCatalogItemId: number | null;
  name: string;
  quantity: number | null;
  unit: string | null;
  category: string | null;
  notes: string | null;
}