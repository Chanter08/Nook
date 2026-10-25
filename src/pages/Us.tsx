import { useMemo, useState } from "react";
import { CheckCircle2, Plus, Sparkles } from "lucide-react";
import ThemeToggle from "@/components/ThemeToggle";
import AddChoreForm from "@/components/chores/AddChoreForm";
import ChoreRow from "@/components/chores/ChoreRow";
import { useChores } from "@/hooks/useChores";

function UsPage() {
  const [adding, setAdding] = useState(false);
  const { chores, loading, error, actionError, busyIds, refresh, createChore, complete, restore, remove } = useChores();

  const activeChores = useMemo(() => chores.filter((chore) => !chore.isCompleted).sort((a, b) => {
    if (a.dueDate === b.dueDate) return a.title.localeCompare(b.title);
    if (a.dueDate === null) return 1;
    if (b.dueDate === null) return -1;
    return a.dueDate.localeCompare(b.dueDate);
  }), [chores]);

  const completedChores = useMemo(() => chores.filter((chore) => chore.isCompleted).sort((a, b) => (b.lastCompletedDate ?? "").localeCompare(a.lastCompletedDate ?? "")), [chores]);

  return (
    <main className="mx-auto min-h-dvh w-full max-w-3xl px-4 pb-28 pt-8 sm:px-6 sm:pt-12">
      <header className="flex items-center justify-between">
        <div><p className="text-sm font-medium text-emerald-800 dark:text-emerald-400">Our home</p><h1 className="mt-1 text-3xl font-semibold tracking-tight text-stone-900 dark:text-stone-100">Us</h1></div>
        <ThemeToggle />
      </header>

      <section className="mt-8">
        <div className="mb-5 flex items-end justify-between gap-4">
          <div>
            <h2 className="text-xl font-semibold text-stone-900 dark:text-stone-100">Chores</h2>
            {!loading && !error && <p className="mt-1 text-sm text-stone-500 dark:text-stone-400">{activeChores.length === 0 ? "Nothing hanging over us" : activeChores.length + " " + (activeChores.length === 1 ? "chore" : "chores") + " to do"}</p>}
          </div>
          <button type="button" onClick={() => setAdding((current) => !current)} className="flex h-11 items-center gap-2 rounded-full bg-emerald-900 px-4 text-sm font-medium text-white transition hover:bg-emerald-800 dark:bg-emerald-700 dark:hover:bg-emerald-600"><Plus size={17} />Add chore</button>
        </div>

        {adding && <AddChoreForm onSubmit={createChore} onCancel={() => setAdding(false)} />}
        {actionError && <div className="mb-4 rounded-2xl bg-red-50 px-4 py-3 text-sm text-red-700 dark:bg-red-400/10 dark:text-red-300">{actionError}</div>}
        {loading && <div className="rounded-3xl bg-stone-100 p-8 text-center text-sm text-stone-500 dark:bg-white/[0.045] dark:text-stone-400">Loading chores...</div>}

        {!loading && error && (
          <div className="rounded-3xl bg-stone-100 p-8 text-center dark:bg-white/[0.045]">
            <p className="font-medium text-stone-900 dark:text-stone-100">Couldn't load chores</p>
            <p className="mt-1 text-sm text-stone-500 dark:text-stone-400">Nook couldn't reach the chores API.</p>
            <button type="button" onClick={() => void refresh()} className="mt-4 rounded-full bg-emerald-900 px-5 py-2.5 text-sm font-medium text-white dark:bg-emerald-700">Try again</button>
          </div>
        )}

        {!loading && !error && activeChores.length === 0 && (
          <div className="rounded-3xl bg-emerald-50 px-6 py-10 text-center dark:bg-emerald-400/[0.07] dark:ring-1 dark:ring-emerald-400/10">
            <div className="mx-auto flex h-12 w-12 items-center justify-center rounded-full bg-emerald-100 text-emerald-800 dark:bg-emerald-400/10 dark:text-emerald-400"><Sparkles size={23} /></div>
            <p className="mt-4 font-medium text-stone-900 dark:text-stone-100">All clear</p>
            <p className="mt-1 text-sm text-stone-500 dark:text-stone-400">No active chores right now.</p>
          </div>
        )}

        {!loading && !error && activeChores.length > 0 && (
          <div className="overflow-hidden rounded-3xl border border-stone-200 bg-white dark:border-white/[0.07] dark:bg-white/[0.035]">
            <div className="divide-y divide-stone-100 dark:divide-white/[0.06]">
              {activeChores.map((chore) => <ChoreRow key={chore.id} chore={chore} busy={busyIds.includes(chore.id)} onComplete={() => void complete(chore.id)} onRestore={() => void restore(chore.id)} onDelete={() => void remove(chore.id)} />)}
            </div>
          </div>
        )}

        {!loading && !error && completedChores.length > 0 && (
          <section className="mt-8">
            <div className="mb-3 flex items-center gap-2"><CheckCircle2 size={18} className="text-stone-400" /><h3 className="text-sm font-semibold text-stone-600 dark:text-stone-300">Completed</h3><span className="text-xs text-stone-400">{completedChores.length}</span></div>
            <div className="overflow-hidden rounded-3xl bg-stone-100 dark:bg-white/[0.025] dark:ring-1 dark:ring-white/[0.04]">
              <div className="divide-y divide-stone-200/70 dark:divide-white/[0.05]">
                {completedChores.map((chore) => <ChoreRow key={chore.id} chore={chore} busy={busyIds.includes(chore.id)} onComplete={() => void complete(chore.id)} onRestore={() => void restore(chore.id)} onDelete={() => void remove(chore.id)} />)}
              </div>
            </div>
          </section>
        )}
      </section>
    </main>
  );
}

export default UsPage;
