import type { CalendarEvent } from "@/types/calendar";
import type { PlannedMeal } from "@/types/mealPlan";

export function getPlanStart() {
  const today = new Date();
  today.setHours(0, 0, 0, 0);
  return today;
}

export function getGreeting() {
  const hour = new Date().getHours();

  if (hour < 12) return "Good morning";
  if (hour < 18) return "Good afternoon";

  return "Good evening";
}

const mealHours: Record<string, number> = {
  Breakfast: 8,
  Lunch: 13,
  Snack: 16,
  Dinner: 19,
};

function getMealDateTime(meal: PlannedMeal) {
  const [year, month, day] = meal.date.split("-").map(Number);
  const hour = mealHours[meal.mealType] ?? 12;

  return new Date(year, month - 1, day, hour);
}

export function getNextMeal(meals: PlannedMeal[]) {
  const now = new Date();

  return (
    [...meals]
      .filter((meal) => getMealDateTime(meal) >= now)
      .sort(
        (a, b) => getMealDateTime(a).getTime() - getMealDateTime(b).getTime(),
      )[0] ?? null
  );
}

export function getMealContext(meal: PlannedMeal) {
  const mealDate = getMealDateTime(meal);
  const today = new Date();

  if (mealDate.toDateString() === today.toDateString()) {
    if (meal.mealType === "Breakfast") return "This morning";
    if (meal.mealType === "Lunch") return "Today";
    if (meal.mealType === "Dinner") return "Tonight";

    return "Next up";
  }

  const tomorrow = new Date(today);
  tomorrow.setDate(today.getDate() + 1);

  if (mealDate.toDateString() === tomorrow.toDateString()) {
    return `Tomorrow · ${meal.mealType}`;
  }

  return mealDate.toLocaleDateString(undefined, {
    weekday: "long",
  });
}

export function getMealQuestion(mealType: string) {
  switch (mealType) {
    case "Breakfast":
      return "What's for breakfast?";
    case "Lunch":
      return "What's for lunch?";
    case "Dinner":
      return "What's for dinner?";
    default:
      return "What's next?";
  }
}

export function getEventDay(event: CalendarEvent) {
  return new Intl.DateTimeFormat("en-GB", {
    day: "2-digit",
    timeZone: event.allDay ? "UTC" : undefined,
  }).format(new Date(event.start));
}

export function getEventMonth(event: CalendarEvent) {
  return new Intl.DateTimeFormat("en-GB", {
    month: "short",
    timeZone: event.allDay ? "UTC" : undefined,
  })
    .format(new Date(event.start))
    .toUpperCase();
}

export function getEventTime(event: CalendarEvent) {
  if (event.allDay) return "All day";

  return new Intl.DateTimeFormat("en-GB", {
    hour: "2-digit",
    minute: "2-digit",
  }).format(new Date(event.start));
}