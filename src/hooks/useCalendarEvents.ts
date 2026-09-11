import { useCallback, useEffect, useState } from "react";
import { getUpcomingCalendarEvents } from "@/api/calendar";
import type { CalendarEvent } from "@/types/calendar";

export function useCalendarEvents() {
  const [events, setEvents] = useState<CalendarEvent[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(false);

  const refresh = useCallback(async () => {
    try {
      setError(false);
      setEvents(await getUpcomingCalendarEvents());
    } catch (error) {
      console.error("Calendar error:", error);
      setError(true);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    void refresh();
  }, [refresh]);

  return {
    events,
    loading,
    error,
    refresh
  };
}