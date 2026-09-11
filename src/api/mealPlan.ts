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

export function deleteMealPlanEntry(id: number) {
  return apiRequest<void>(`/api/meal-plan/${id}`, {
    method: "DELETE",
  });
}