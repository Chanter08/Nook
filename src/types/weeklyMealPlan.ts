import type { PlannedMeal } from "@/types/mealPlan";

export interface WeeklyMealPlanProps {
  startDate: Date;
  meals: PlannedMeal[];
  loading: boolean;
  onRefresh: () => Promise<void>;
  onRemove: (id: number) => Promise<void>;
  removingIds: number[];
}