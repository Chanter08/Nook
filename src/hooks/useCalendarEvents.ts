import { useCallback, useEffect, useState } from "react";
import type { CalendarEvent } from "@/types/calendar";

export function useCalendarEvents() {
  const [events, setEvents] = useState<CalendarEvent[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(false);

  const refresh = useCallback(async () => {
    try {
      setError(false);

      const response = await fetch("/api/calendar/upcoming");

      if (!response.ok) {
        throw new Error(`Failed to load calendar: ${response.status}`);
      }

      const data: CalendarEvent[] = await response.json();
      setEvents(data);
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