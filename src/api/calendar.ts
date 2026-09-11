import { apiRequest } from "@/api/client";
import type { CalendarEvent } from "@/types/calendar";

export function getUpcomingCalendarEvents() {
  return apiRequest<CalendarEvent[]>("/api/calendar/upcoming");
}