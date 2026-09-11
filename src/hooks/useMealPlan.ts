import { useCallback, useEffect, useState } from "react";
import { deleteMealPlanEntry, getMealPlan } from "@/api/mealPlan";
import type { PlannedMeal } from "@/types/mealPlan";

function getDateKey(date: Date) {
  return [
    date.getFullYear(),
    String(date.getMonth() + 1).padStart(2, "0"),
    String(date.getDate()).padStart(2, "0")
  ].join("-");
}

export function useMealPlan(startDate: Date) {
  const [meals, setMeals] = useState<PlannedMeal[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(false);
  const [actionError, setActionError] = useState<string | null>(null);
  const [removingIds, setRemovingIds] = useState<number[]>([]);
  const startKey = getDateKey(startDate);

  const refresh = useCallback(async () => {
    try {
      setError(false);
      setMeals(await getMealPlan(startKey));
    } catch (error) {
      console.error("Meal plan error:", error);
      setError(true);
    } finally {
      setLoading(false);
    }
  }, [startKey]);

  useEffect(() => {
    void refresh();
  }, [refresh]);

  async function removeMeal(id: number) {
    if (removingIds.includes(id)) return;

    try {
      setActionError(null);
      setRemovingIds((current) => [...current, id]);

      await deleteMealPlanEntry(id);

      setMeals((current) => current.filter((meal) => meal.id !== id));
    } catch (error) {
      console.error("Remove meal error:", error);
      setActionError("Couldn't remove that meal.");
    } finally {
      setRemovingIds((current) => current.filter((mealId) => mealId !== id));
    }
  }

  return {
    meals,
    loading,
    error,
    actionError,
    removingIds,
    refresh,
    removeMeal
  };
}