import type { PlannedMeal } from "@/types/mealPlan";
import type { CalendarEvent } from "@/types/calendar";

export interface WeeklyMealPlanProps {
  startDate: Date;
  meals: PlannedMeal[];
  events: CalendarEvent[];
  loading: boolean;
  onRefresh: () => Promise<void>;
  onRemove: (id: number) => Promise<void>;
  removingIds: number[];
}