import { apiRequest } from "@/api/client";
import type { PantryItem, SavePantryItemPayload } from "@/types/pantry";

export function getPantryItems() {
  return apiRequest<PantryItem[]>("/api/pantry");
}

export function addPantryItem(payload: SavePantryItemPayload) {
  return apiRequest<PantryItem>("/api/pantry", { method: "POST", body: JSON.stringify(payload) });
}

export function bulkAddPantryItems(items: SavePantryItemPayload[]) {
  return apiRequest<PantryItem[]>("/api/pantry/bulk", { method: "POST", body: JSON.stringify({ items }) });
}

export function updatePantryItem(id: number, payload: SavePantryItemPayload) {
  return apiRequest<PantryItem>("/api/pantry/" + id, { method: "PUT", body: JSON.stringify(payload) });
}

export function deletePantryItem(id: number) {
  return apiRequest<void>("/api/pantry/" + id, { method: "DELETE" });
}
