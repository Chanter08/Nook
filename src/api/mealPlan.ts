import { apiRequest } from "@/api/client";
import type { PlannedMeal } from "@/types/mealPlan";

export interface AddMealPlanEntryPayload {
  date: string;
  mealType: string;
  recipeId: number;
}

export function getMealPlan(start: string) {
  return apiRequest<PlannedMeal[]>(
    `/api/meal-plan/week?start=${encodeURIComponent(start)}`,
  );
}

export function addMealPlanEntry(payload: AddMealPlanEntryPayload) {
  return apiRequest<{ id: number }>("/api/meal-plan", {
    method: "POST",
    body: JSON.stringify(payload),
  });
}


export interface AddMealPlanEntriesPayload {
  date: string;
  mealType: string;
  recipeIds: number[];
}

// One request allows selecting two dinners at once; the API saves both atomically.
export function addMealPlanEntries(payload: AddMealPlanEntriesPayload) {
  return apiRequest<{ ids: number[] }>("/api/meal-plan/batch", {
    method: "POST",
    body: JSON.stringify(payload),
  });
}

export function deleteMealPlanEntry(id: number) {
  return apiRequest<void>(`/api/meal-plan/${id}`, {
    method: "DELETE",
  });
}
export function moveMealPlanEntry(id: number, date: string) {
  return apiRequest<void>(`/api/meal-plan/${id}/move`, {
    method: "PATCH",
    body: JSON.stringify({ date }),
  });
}
