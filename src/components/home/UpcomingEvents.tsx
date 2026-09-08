import { CalendarDays } from "lucide-react";
import { Link } from "react-router-dom";
import { getEventDay, getEventMonth, getEventTime } from "@/lib/homeDashboard";
import type { CalendarEvent } from "@/types/calendar";

interface UpcomingEventsProps {
  events: CalendarEvent[];
  loading: boolean;
  error: boolean;
}

function UpcomingEvents({ events, loading, error }: UpcomingEventsProps) {
  return (
    <section className="mt-6 flex flex-col">
      <div className="mb-3 flex items-center justify-between">
        <h2 className="text-lg font-semibold text-stone-900 dark:text-stone-100">Coming up</h2>

        <Link to="/plans" className="text-sm font-medium text-emerald-800 transition hover:text-emerald-950 dark:text-emerald-400 dark:hover:text-emerald-300">
          View all
        </Link>
      </div>

      <div className="max-h-[calc(100dvh-330px)] overflow-y-auto rounded-3xl border border-stone-200 bg-white dark:border-white/[0.07] dark:bg-white/[0.035] dark:ring-1 dark:ring-white/[0.025]">
        {loading && <div className="p-5 text-sm text-stone-500 dark:text-stone-400">Loading calendar...</div>}

        {!loading && error && (
          <div className="p-5">
            <p className="font-medium text-stone-900 dark:text-stone-100">Couldn't load calendar</p>
            <p className="mt-1 text-sm text-stone-500 dark:text-stone-400">Check that the Nook API is running.</p>
          </div>
        )}

        {!loading && !error && events.length === 0 && (
          <div className="p-5">
            <p className="font-medium text-stone-900 dark:text-stone-100">Nothing planned</p>
            <p className="mt-1 text-sm text-stone-500 dark:text-stone-400">Your calendar is clear.</p>
          </div>
        )}

        {!loading && !error && events.map((event, index) => (
          <div key={`${event.id}-${event.start}`} className={`flex items-center gap-4 p-4 transition hover:bg-stone-50 dark:hover:bg-white/[0.035] ${index !== events.length - 1 ? "border-b border-stone-100 dark:border-white/[0.06]" : ""}`}>
            <div className="flex w-12 shrink-0 flex-col items-center">
              <span className="text-xl font-semibold leading-none text-stone-900 dark:text-stone-100">{getEventDay(event)}</span>
              <span className="mt-1 text-xs font-medium text-emerald-800 dark:text-emerald-400">{getEventMonth(event)}</span>
            </div>

            <div className="min-w-0 flex-1">
              <p className="truncate font-medium text-stone-900 dark:text-stone-100">{event.title}</p>
              <p className="mt-1 text-sm text-stone-500 dark:text-stone-400">{getEventTime(event)}</p>
            </div>

            <CalendarDays size={19} className="shrink-0 text-stone-400 dark:text-stone-500" />
          </div>
        ))}
      </div>
    </section>
  );
}

export default UpcomingEvents;