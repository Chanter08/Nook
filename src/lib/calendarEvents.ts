import type { CalendarEvent } from "@/types/calendar";

export function isDutyEvent(event: CalendarEvent) {
  return /\bduty\b/i.test(event.title);
}

export function getCalendarEventDateKey(event: CalendarEvent) {
  const date = new Date(event.start);
  const year = event.allDay ? date.getUTCFullYear() : date.getFullYear();
  const month = event.allDay ? date.getUTCMonth() + 1 : date.getMonth() + 1;
  const day = event.allDay ? date.getUTCDate() : date.getDate();

  return [year, String(month).padStart(2, "0"), String(day).padStart(2, "0")].join("-");
}
