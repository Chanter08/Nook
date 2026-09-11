import { useEffect, useState } from "react";
import { CalendarDays } from "lucide-react";
import ThemeToggle from "@/components/ThemeToggle";
import type { CalendarEvent } from "@/types/calendar";
import { getUpcomingCalendarEvents } from "@/api/calendar";

function formatDate(event: CalendarEvent) {
  return new Intl.DateTimeFormat("en-GB", {
    weekday: "short",
    day: "2-digit",
    month: "short",
    timeZone: event.allDay ? "UTC" : undefined,
  }).format(new Date(event.start));
}

function formatTime(event: CalendarEvent) {
  if (event.allDay) return "All day";

  return new Intl.DateTimeFormat("en-GB", {
    hour: "2-digit",
    minute: "2-digit",
  }).format(new Date(event.start));
}

function PlansPage() {
  const [events, setEvents] = useState<CalendarEvent[]>([]);
  const [loading, setLoading] = useState(true);
  const [calendarError, setCalendarError] = useState(false);

  useEffect(() => {
    async function loadCalendar() {
      try {
        setCalendarError(false);

        const data = await getUpcomingCalendarEvents();
        setEvents(data);
      } catch (error) {
        console.error("Calendar error:", error);
        setCalendarError(true);
      } finally {
        setLoading(false);
      }
    }

    void loadCalendar();
  }, []);

  return (
    <main className="mx-auto min-h-dvh w-full max-w-5xl px-4 pb-28 pt-8 text-stone-900 transition-colors dark:text-stone-100 sm:px-6 sm:pt-12">
      <header className="mb-8 flex items-start justify-between">
        <div>
          <p className="text-sm text-stone-500 dark:text-stone-400">
            Our schedule
          </p>
          <h1 className="text-3xl font-semibold tracking-tight text-stone-900 dark:text-stone-100">
            Plans
          </h1>
        </div>

        <div className="flex items-center gap-2">
          <ThemeToggle />
        </div>
      </header>

      <section className="mb-6 rounded-3xl bg-emerald-900 p-6 text-white shadow-sm dark:bg-emerald-950 dark:ring-1 dark:ring-emerald-700/30">
        <div className="flex items-center gap-4">
          <div className="flex h-12 w-12 items-center justify-center rounded-2xl bg-white/15 dark:bg-emerald-300/10">
            <CalendarDays size={22} />
          </div>

          <div>
            <p className="text-sm text-emerald-100 dark:text-emerald-300">
              Coming up
            </p>
            <p className="text-xl font-semibold">
              {loading
                ? "Loading..."
                : `${events.length} upcoming ${events.length === 1 ? "event" : "events"}`}
            </p>
          </div>
        </div>
      </section>

      <section>
        <h2 className="mb-3 text-lg font-semibold text-stone-900 dark:text-stone-100">
          Upcoming
        </h2>

        <div className="overflow-hidden rounded-3xl border border-stone-200 bg-white dark:border-white/[0.07] dark:bg-white/[0.035] dark:ring-1 dark:ring-white/[0.025]">
          {loading && (
            <div className="p-5 text-sm text-stone-500 dark:text-stone-400">
              Loading calendar...
            </div>
          )}

          {!loading && calendarError && (
            <div className="p-5">
              <p className="font-medium text-stone-900 dark:text-stone-100">
                Couldn't load calendar
              </p>
              <p className="mt-1 text-sm text-stone-500 dark:text-stone-400">
                Check that the Nook API is running.
              </p>
            </div>
          )}

          {!loading && !calendarError && events.length === 0 && (
            <div className="p-5">
              <p className="font-medium text-stone-900 dark:text-stone-100">
                Nothing planned
              </p>
              <p className="mt-1 text-sm text-stone-500 dark:text-stone-400">
                Your calendar is clear.
              </p>
            </div>
          )}

          {!loading &&
            !calendarError &&
            events.map((event, index) => (
              <div key={`${event.id}-${event.start}`} className={`flex w-full items-center gap-4 p-4 ${index !== events.length - 1 ? "border-b border-stone-100 dark:border-white/[0.06]" : ""}`}>

                <div className="flex h-12 w-12 shrink-0 items-center justify-center rounded-2xl bg-emerald-50 text-emerald-900 dark:bg-emerald-400/10 dark:text-emerald-300">
                  <CalendarDays size={21} />
                </div>

                <div className="min-w-0 flex-1">
                  <p className="truncate font-medium text-stone-900 dark:text-stone-100">
                    {event.title}
                  </p>
                  <p className="mt-1 text-sm text-stone-500 dark:text-stone-400">
                    {formatDate(event)} · {formatTime(event)}
                  </p>
                </div>
              </div>
            ))}
        </div>
      </section>
    </main>
  );
}

export default PlansPage;