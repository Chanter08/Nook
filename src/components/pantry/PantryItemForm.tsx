import { useEffect, useState } from "react";
import type { PantryItem, PantryLocation, SavePantryItemPayload } from "@/types/pantry";

interface PantryItemFormProps {
  item?: PantryItem | null;
  onSubmit: (payload: SavePantryItemPayload) => Promise<void>;
  onCancel: () => void;
}

function PantryItemForm({ item, onSubmit, onCancel }: PantryItemFormProps) {
  const [name, setName] = useState(item?.name ?? "");
  const [quantity, setQuantity] = useState(String(item?.quantity ?? 1));
  const [unit, setUnit] = useState(item?.unit ?? "");
  const [location, setLocation] = useState<PantryLocation>(item?.location ?? "Pantry");
  const [expiryDate, setExpiryDate] = useState(item?.expiryDate ?? "");
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    setName(item?.name ?? "");
    setQuantity(String(item?.quantity ?? 1));
    setUnit(item?.unit ?? "");
    setLocation(item?.location ?? "Pantry");
    setExpiryDate(item?.expiryDate ?? "");
  }, [item]);

  async function handleSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();

    const parsedQuantity = Number(quantity);
    if (!name.trim() || !Number.isFinite(parsedQuantity) || parsedQuantity <= 0 || saving) return;

    setSaving(true);

    try {
      await onSubmit({
        name: name.trim(),
        quantity: parsedQuantity,
        unit: unit.trim() || null,
        location,
        expiryDate: expiryDate || null
      });
      onCancel();
    } finally {
      setSaving(false);
    }
  }

  return (
    <form onSubmit={handleSubmit} className="rounded-3xl border border-emerald-200 bg-emerald-50/60 p-4 dark:border-emerald-400/15 dark:bg-emerald-400/[0.06]">
      <label className="block"><span className="text-xs font-semibold uppercase tracking-wide text-stone-500 dark:text-stone-400">Item</span><input autoFocus value={name} onChange={(event) => setName(event.target.value)} maxLength={150} placeholder="Basmati rice" className="mt-2 w-full rounded-2xl border border-stone-200 bg-white px-4 py-3 text-sm outline-none transition focus:border-emerald-500 dark:border-white/[0.08] dark:bg-white/[0.05]" /></label>

      <div className="mt-3 grid grid-cols-2 gap-3 sm:grid-cols-4">
        <label><span className="text-xs font-medium text-stone-500 dark:text-stone-400">Quantity</span><input type="number" min="0.01" step="0.01" value={quantity} onChange={(event) => setQuantity(event.target.value)} className="mt-1.5 w-full rounded-2xl border border-stone-200 bg-white px-3 py-2.5 text-sm outline-none transition focus:border-emerald-500 dark:border-white/[0.08] dark:bg-white/[0.05]" /></label>
        <label><span className="text-xs font-medium text-stone-500 dark:text-stone-400">Unit</span><input value={unit} onChange={(event) => setUnit(event.target.value)} maxLength={50} placeholder="packs / g / ml" className="mt-1.5 w-full rounded-2xl border border-stone-200 bg-white px-3 py-2.5 text-sm outline-none transition focus:border-emerald-500 dark:border-white/[0.08] dark:bg-white/[0.05]" /></label>
        <label><span className="text-xs font-medium text-stone-500 dark:text-stone-400">Location</span><select value={location} onChange={(event) => setLocation(event.target.value as PantryLocation)} className="mt-1.5 w-full rounded-2xl border border-stone-200 bg-white px-3 py-2.5 text-sm outline-none transition focus:border-emerald-500 dark:border-white/[0.08] dark:bg-stone-900"><option>Pantry</option><option>Fridge</option><option>Freezer</option><option>Other</option></select></label>
        <label><span className="text-xs font-medium text-stone-500 dark:text-stone-400">Expiry</span><input type="date" value={expiryDate} onChange={(event) => setExpiryDate(event.target.value)} className="mt-1.5 w-full rounded-2xl border border-stone-200 bg-white px-3 py-2.5 text-sm outline-none transition focus:border-emerald-500 dark:border-white/[0.08] dark:bg-white/[0.05]" /></label>
      </div>

      <div className="mt-4 flex justify-end gap-2"><button type="button" onClick={onCancel} disabled={saving} className="rounded-full px-4 py-2.5 text-sm font-medium text-stone-600 transition hover:bg-stone-200/70 disabled:opacity-50 dark:text-stone-300 dark:hover:bg-white/[0.06]">Cancel</button><button type="submit" disabled={!name.trim() || saving} className="rounded-full bg-emerald-900 px-5 py-2.5 text-sm font-medium text-white transition hover:bg-emerald-800 disabled:opacity-50 dark:bg-emerald-700 dark:hover:bg-emerald-600">{saving ? "Saving..." : item ? "Save changes" : "Add item"}</button></div>
    </form>
  );
}

export default PantryItemForm;
