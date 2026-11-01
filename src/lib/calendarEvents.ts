import type { CalendarEvent } from "@/types/calendar";

export type MealPlanningEventKind = "duty" | "leave" | "event";

function localDateKey(date: Date) {
  return [
    date.getFullYear(),
    String(date.getMonth() + 1).padStart(2, "0"),
    String(date.getDate()).padStart(2, "0"),
  ].join("-");
}

export function getCalendarEventDateKey(event: CalendarEvent) {
  if (event.allDay) {
    return event.start.slice(0, 10);
  }

  return localDateKey(new Date(event.start));
}

export function getMealPlanningEventKind(
  event: CalendarEvent,
): MealPlanningEventKind {
  const title = event.title.trim().toLowerCase();

  if (/\bduty\b/.test(title)) return "duty";
  if (/\bleave\b/.test(title)) return "leave";

  return "event";
}

export function isDutyEvent(event: CalendarEvent) {
  return getMealPlanningEventKind(event) === "duty";
}

export function isLeaveEvent(event: CalendarEvent) {
  return getMealPlanningEventKind(event) === "leave";
}
