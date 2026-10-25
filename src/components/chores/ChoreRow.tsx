import { CalendarDays, Check, Repeat2, RotateCcw, Trash2, UserRound } from "lucide-react";
import type { Chore } from "@/types/chore";

interface ChoreRowProps {
  chore: Chore;
  busy: boolean;
  onComplete: () => void;
  onRestore: () => void;
  onDelete: () => void;
}

function localDateKey() {
  const date = new Date();
  const month = String(date.getMonth() + 1).padStart(2, "0");
  const day = String(date.getDate()).padStart(2, "0");
  return date.getFullYear() + "-" + month + "-" + day;
}

function formatDate(value: string) {
  return new Date(value + "T00:00:00").toLocaleDateString(undefined, { day: "numeric", month: "short" });
}

function ChoreRow({ chore, busy, onComplete, onRestore, onDelete }: ChoreRowProps) {
  const today = localDateKey();
  const overdue = !chore.isCompleted && chore.dueDate !== null && chore.dueDate < today;
  const dueToday = !chore.isCompleted && chore.dueDate === today;

  return (
    <div className="flex items-center gap-3 px-4 py-3.5">
      <button type="button" disabled={busy} onClick={chore.isCompleted ? onRestore : onComplete} aria-label={chore.isCompleted ? "Restore chore" : "Complete chore"} className={chore.isCompleted ? "flex h-9 w-9 shrink-0 items-center justify-center rounded-full bg-stone-200 text-stone-500 transition hover:bg-stone-300 disabled:opacity-40 dark:bg-white/[0.08] dark:text-stone-300" : "flex h-9 w-9 shrink-0 items-center justify-center rounded-full border-2 border-emerald-700 text-emerald-800 transition hover:bg-emerald-50 disabled:opacity-40 dark:border-emerald-500 dark:text-emerald-400 dark:hover:bg-emerald-400/10"}>
        {chore.isCompleted ? <RotateCcw size={16} /> : <Check size={17} />}
      </button>

      <div className="min-w-0 flex-1">
        <p className={chore.isCompleted ? "truncate text-sm font-medium text-stone-400 line-through dark:text-stone-500" : "truncate text-sm font-medium text-stone-900 dark:text-stone-100"}>{chore.title}</p>
        <div className="mt-1 flex flex-wrap items-center gap-x-3 gap-y-1 text-xs text-stone-400 dark:text-stone-500">
          {chore.assignedTo && <span className="inline-flex items-center gap-1"><UserRound size={12} />{chore.assignedTo}</span>}
          {chore.dueDate && <span className={overdue ? "inline-flex items-center gap-1 font-medium text-red-600 dark:text-red-400" : dueToday ? "inline-flex items-center gap-1 font-medium text-amber-700 dark:text-amber-400" : "inline-flex items-center gap-1"}><CalendarDays size={12} />{overdue ? "Overdue · " : dueToday ? "Today · " : ""}{formatDate(chore.dueDate)}</span>}
          {chore.recurrence !== "None" && <span className="inline-flex items-center gap-1"><Repeat2 size={12} />{chore.recurrence}</span>}
        </div>
      </div>

      <button type="button" disabled={busy} onClick={onDelete} aria-label="Delete chore" className="flex h-9 w-9 shrink-0 items-center justify-center rounded-full text-stone-300 transition hover:bg-red-50 hover:text-red-600 disabled:opacity-40 dark:text-stone-600 dark:hover:bg-red-400/10 dark:hover:text-red-400"><Trash2 size={16} /></button>
    </div>
  );
}

export default ChoreRow;
