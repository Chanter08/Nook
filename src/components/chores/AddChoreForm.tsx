import { useState } from "react";
import type { ChoreRecurrence, SaveChorePayload } from "@/types/chore";

interface AddChoreFormProps {
  onSubmit: (payload: SaveChorePayload) => Promise<void>;
  onCancel: () => void;
}

function AddChoreForm({ onSubmit, onCancel }: AddChoreFormProps) {
  const [title, setTitle] = useState("");
  const [assignedTo, setAssignedTo] = useState("");
  const [dueDate, setDueDate] = useState("");
  const [recurrence, setRecurrence] = useState<ChoreRecurrence>("None");
  const [saving, setSaving] = useState(false);

  async function handleSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!title.trim() || saving) return;
    setSaving(true);

    try {
      await onSubmit({ title: title.trim(), assignedTo: assignedTo.trim() || null, dueDate: dueDate || null, recurrence });
      onCancel();
    } finally {
      setSaving(false);
    }
  }

  return (
    <form onSubmit={handleSubmit} className="mb-5 rounded-3xl border border-emerald-200 bg-emerald-50/60 p-4 dark:border-emerald-400/15 dark:bg-emerald-400/[0.06]">
      <label className="block">
        <span className="text-xs font-semibold uppercase tracking-wide text-stone-500 dark:text-stone-400">Chore</span>
        <input autoFocus value={title} onChange={(event) => setTitle(event.target.value)} maxLength={150} placeholder="Clean the bathroom" className="mt-2 w-full rounded-2xl border border-stone-200 bg-white px-4 py-3 text-sm text-stone-900 outline-none transition focus:border-emerald-500 dark:border-white/[0.08] dark:bg-white/[0.05] dark:text-stone-100" />
      </label>

      <div className="mt-3 grid grid-cols-1 gap-3 sm:grid-cols-3">
        <label><span className="text-xs font-medium text-stone-500 dark:text-stone-400">Assigned to</span><input value={assignedTo} onChange={(event) => setAssignedTo(event.target.value)} maxLength={80} placeholder="Anyone" className="mt-1.5 w-full rounded-2xl border border-stone-200 bg-white px-3 py-2.5 text-sm outline-none transition focus:border-emerald-500 dark:border-white/[0.08] dark:bg-white/[0.05]" /></label>
        <label><span className="text-xs font-medium text-stone-500 dark:text-stone-400">Due</span><input type="date" value={dueDate} onChange={(event) => setDueDate(event.target.value)} className="mt-1.5 w-full rounded-2xl border border-stone-200 bg-white px-3 py-2.5 text-sm outline-none transition focus:border-emerald-500 dark:border-white/[0.08] dark:bg-white/[0.05]" /></label>
        <label><span className="text-xs font-medium text-stone-500 dark:text-stone-400">Repeat</span><select value={recurrence} onChange={(event) => setRecurrence(event.target.value as ChoreRecurrence)} className="mt-1.5 w-full rounded-2xl border border-stone-200 bg-white px-3 py-2.5 text-sm outline-none transition focus:border-emerald-500 dark:border-white/[0.08] dark:bg-stone-900"><option value="None">Never</option><option value="Daily">Daily</option><option value="Weekly">Weekly</option><option value="Monthly">Monthly</option></select></label>
      </div>

      <div className="mt-4 flex justify-end gap-2">
        <button type="button" onClick={onCancel} disabled={saving} className="rounded-full px-4 py-2.5 text-sm font-medium text-stone-600 transition hover:bg-stone-200/70 disabled:opacity-50 dark:text-stone-300 dark:hover:bg-white/[0.06]">Cancel</button>
        <button type="submit" disabled={!title.trim() || saving} className="rounded-full bg-emerald-900 px-5 py-2.5 text-sm font-medium text-white transition hover:bg-emerald-800 disabled:cursor-not-allowed disabled:opacity-50 dark:bg-emerald-700 dark:hover:bg-emerald-600">{saving ? "Adding..." : "Add chore"}</button>
      </div>
    </form>
  );
}

export default AddChoreForm;
