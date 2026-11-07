import type { CalendarEvent } from "@/types/calendar";
import type { PlannedMeal } from "@/types/mealPlan";

export interface WeeklyMealPlanProps {
  startDate: Date;
  meals: PlannedMeal[];
  events: CalendarEvent[];
  loading: boolean;
  onRefresh: () => Promise<void>;
  onRemove: (id: number) => Promise<void>;
  removingIds: number[];
  movingIds: number[];
  onMove: (id: number, date: string) => Promise<boolean>;
  moveError: string | null;
}
