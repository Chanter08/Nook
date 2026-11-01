import { useMemo, useState } from "react";
import { ArrowLeft, Camera, Minus, PackageOpen, Pencil, Plus, Search, Trash2 } from "lucide-react";
import { useNavigate } from "react-router-dom";
import ThemeToggle from "@/components/ThemeToggle";
import PantryItemForm from "@/components/pantry/PantryItemForm";
import ReceiptScanModal from "@/components/pantry/ReceiptScanModal";
import { usePantry } from "@/hooks/usePantry";
import type { PantryItem, PantryLocation, SavePantryItemPayload } from "@/types/pantry";

const locations: Array<"All" | PantryLocation> = ["All", "Pantry", "Fridge", "Freezer", "Other"];

function localDateKey(daysFromToday = 0) {
  const date = new Date();
  date.setDate(date.getDate() + daysFromToday);
  return [date.getFullYear(), String(date.getMonth() + 1).padStart(2, "0"), String(date.getDate()).padStart(2, "0")].join("-");
}

function expiryLabel(expiryDate: string | null) {
  if (!expiryDate) return null;

  const today = localDateKey();
  const soon = localDateKey(3);

  if (expiryDate < today) return { text: "Expired", className: "text-red-600 dark:text-red-400" };
  if (expiryDate === today) return { text: "Expires today", className: "text-amber-700 dark:text-amber-400" };
  if (expiryDate <= soon) return { text: "Use soon", className: "text-amber-700 dark:text-amber-400" };

  return { text: new Date(expiryDate + "T00:00:00").toLocaleDateString(undefined, { day: "numeric", month: "short" }), className: "text-stone-400 dark:text-stone-500" };
}

function payloadFromItem(item: PantryItem, quantity: number): SavePantryItemPayload {
  return { name: item.name, quantity, unit: item.unit, location: item.location, expiryDate: item.expiryDate };
}

function PantryPage() {
  const navigate = useNavigate();
  const [adding, setAdding] = useState(false);
  const [editing, setEditing] = useState<PantryItem | null>(null);
  const [scanning, setScanning] = useState(false);
  const [query, setQuery] = useState("");
  const [location, setLocation] = useState<"All" | PantryLocation>("All");
  const { items, loading, error, actionError, busyIds, refresh, create, createMany, update, remove } = usePantry();

  const filteredItems = useMemo(() => {
    const normalizedQuery = query.trim().toLowerCase();

    return items.filter((item) => {
      const locationMatches = location === "All" || item.location === location;
      const queryMatches = !normalizedQuery || item.name.toLowerCase().includes(normalizedQuery);
      return locationMatches && queryMatches;
    }).sort((a, b) => {
      if (a.expiryDate === b.expiryDate) return a.name.localeCompare(b.name);
      if (a.expiryDate === null) return 1;
      if (b.expiryDate === null) return -1;
      return a.expiryDate.localeCompare(b.expiryDate);
    });
  }, [items, location, query]);

  const expiringSoon = items.filter((item) => item.expiryDate !== null && item.expiryDate >= localDateKey() && item.expiryDate <= localDateKey(3)).length;

  return (
    <main className="mx-auto min-h-dvh w-full max-w-4xl px-4 pb-28 pt-8 sm:px-6 sm:pt-12">
      <header>
        <div className="flex items-center justify-between"><button type="button" onClick={() => navigate(-1)} aria-label="Go back" className="flex h-11 w-11 items-center justify-center rounded-full bg-stone-100 text-stone-700 transition hover:bg-stone-200 dark:bg-white/[0.06] dark:text-stone-200"><ArrowLeft size={20} /></button><ThemeToggle /></div>
        <div className="mt-7 flex flex-wrap items-end justify-between gap-4"><div><p className="text-sm font-medium text-emerald-800 dark:text-emerald-400">Our home</p><h1 className="mt-1 text-3xl font-semibold tracking-tight text-stone-900 dark:text-stone-100">Pantry</h1><p className="mt-1 text-sm text-stone-500 dark:text-stone-400">{loading ? "Loading inventory..." : items.length + " " + (items.length === 1 ? "item" : "items") + (expiringSoon > 0 ? " · " + expiringSoon + " use soon" : "")}</p></div><div className="flex gap-2"><button type="button" onClick={() => setScanning(true)} className="flex h-11 items-center gap-2 rounded-full bg-stone-100 px-4 text-sm font-medium text-stone-700 transition hover:bg-stone-200 dark:bg-white/[0.06] dark:text-stone-200 dark:hover:bg-white/[0.1]"><Camera size={17} />Scan receipt</button><button type="button" onClick={() => { setEditing(null); setAdding((current) => !current); }} className="flex h-11 items-center gap-2 rounded-full bg-emerald-900 px-4 text-sm font-medium text-white transition hover:bg-emerald-800 dark:bg-emerald-700 dark:hover:bg-emerald-600"><Plus size={17} />Add item</button></div></div>
      </header>

      {(adding || editing) && <div className="mt-5"><PantryItemForm item={editing} onCancel={() => { setAdding(false); setEditing(null); }} onSubmit={async (payload) => { if (editing) await update(editing.id, payload); else await create(payload); }} /></div>}
      {actionError && <div className="mt-5 rounded-2xl bg-red-50 px-4 py-3 text-sm text-red-700 dark:bg-red-400/10 dark:text-red-300">{actionError}</div>}

      <section className="mt-6">
        <div className="flex flex-col gap-3 sm:flex-row sm:items-center">
          <label className="relative flex-1"><Search size={17} className="pointer-events-none absolute left-4 top-1/2 -translate-y-1/2 text-stone-400" /><input value={query} onChange={(event) => setQuery(event.target.value)} placeholder="Search inventory" className="h-11 w-full rounded-full border border-stone-200 bg-white pl-11 pr-4 text-sm outline-none transition focus:border-emerald-500 dark:border-white/[0.07] dark:bg-white/[0.035]" /></label>
          <div className="flex gap-2 overflow-x-auto pb-1">{locations.map((value) => <button key={value} type="button" onClick={() => setLocation(value)} className={location === value ? "shrink-0 rounded-full bg-emerald-900 px-4 py-2.5 text-xs font-medium text-white dark:bg-emerald-700" : "shrink-0 rounded-full bg-stone-100 px-4 py-2.5 text-xs font-medium text-stone-600 dark:bg-white/[0.06] dark:text-stone-300"}>{value}</button>)}</div>
        </div>
      </section>

      {loading && <div className="mt-6 rounded-3xl bg-stone-100 p-8 text-center text-sm text-stone-500 dark:bg-white/[0.045] dark:text-stone-400">Loading pantry...</div>}

      {!loading && error && <div className="mt-6 rounded-3xl bg-stone-100 p-8 text-center dark:bg-white/[0.045]"><p className="font-medium text-stone-900 dark:text-stone-100">Couldn't load pantry</p><p className="mt-1 text-sm text-stone-500 dark:text-stone-400">Check that the Nook API is running.</p><button type="button" onClick={() => void refresh()} className="mt-4 rounded-full bg-emerald-900 px-5 py-2.5 text-sm font-medium text-white dark:bg-emerald-700">Try again</button></div>}

      {!loading && !error && filteredItems.length === 0 && <div className="mt-6 rounded-3xl bg-emerald-50 px-6 py-10 text-center dark:bg-emerald-400/[0.07] dark:ring-1 dark:ring-emerald-400/10"><div className="mx-auto flex h-12 w-12 items-center justify-center rounded-full bg-emerald-100 text-emerald-800 dark:bg-emerald-400/10 dark:text-emerald-400"><PackageOpen size={23} /></div><p className="mt-4 font-medium text-stone-900 dark:text-stone-100">{items.length === 0 ? "Pantry is empty" : "No matches"}</p><p className="mt-1 text-sm text-stone-500 dark:text-stone-400">{items.length === 0 ? "Add items manually or scan your next grocery receipt." : "Try another search or location."}</p></div>}

      {!loading && !error && filteredItems.length > 0 && <div className="mt-6 overflow-hidden rounded-3xl border border-stone-200 bg-white dark:border-white/[0.07] dark:bg-white/[0.035]"><div className="divide-y divide-stone-100 dark:divide-white/[0.06]">{filteredItems.map((item) => {
        const expiry = expiryLabel(item.expiryDate);
        const busy = busyIds.includes(item.id);

        return <div key={item.id} className="flex items-center gap-3 p-4"><div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-2xl bg-stone-100 text-stone-500 dark:bg-white/[0.06] dark:text-stone-400"><PackageOpen size={18} /></div><div className="min-w-0 flex-1"><div className="flex flex-wrap items-center gap-x-2 gap-y-1"><p className="truncate text-sm font-semibold text-stone-900 dark:text-stone-100">{item.name}</p><span className="rounded-full bg-stone-100 px-2 py-0.5 text-[10px] font-medium text-stone-500 dark:bg-white/[0.06] dark:text-stone-400">{item.location}</span></div><div className="mt-1 flex flex-wrap items-center gap-x-3 text-xs text-stone-400 dark:text-stone-500"><span>{item.quantity} {item.unit ?? (item.quantity === 1 ? "item" : "items")}</span>{expiry && <span className={expiry.className}>{expiry.text}</span>}</div></div><div className="flex shrink-0 items-center gap-1"><button type="button" disabled={busy || item.quantity <= 1} onClick={() => void update(item.id, payloadFromItem(item, Math.max(1, item.quantity - 1)))} aria-label={"Reduce " + item.name} className="flex h-8 w-8 items-center justify-center rounded-full bg-stone-100 text-stone-500 disabled:opacity-30 dark:bg-white/[0.06]"><Minus size={14} /></button><button type="button" disabled={busy} onClick={() => void update(item.id, payloadFromItem(item, item.quantity + 1))} aria-label={"Increase " + item.name} className="flex h-8 w-8 items-center justify-center rounded-full bg-stone-100 text-stone-500 disabled:opacity-30 dark:bg-white/[0.06]"><Plus size={14} /></button><button type="button" disabled={busy} onClick={() => { setAdding(false); setEditing(item); window.scrollTo({ top: 0, behavior: "smooth" }); }} aria-label={"Edit " + item.name} className="flex h-8 w-8 items-center justify-center rounded-full text-stone-400 transition hover:bg-stone-100 hover:text-stone-700 disabled:opacity-30 dark:hover:bg-white/[0.06] dark:hover:text-stone-200"><Pencil size={14} /></button><button type="button" disabled={busy} onClick={() => { if (window.confirm("Remove " + item.name + " from the pantry?")) void remove(item.id); }} aria-label={"Delete " + item.name} className="flex h-8 w-8 items-center justify-center rounded-full text-stone-300 transition hover:bg-red-50 hover:text-red-600 disabled:opacity-30 dark:text-stone-600 dark:hover:bg-red-400/10 dark:hover:text-red-400"><Trash2 size={14} /></button></div></div>;
      })}</div></div>}

      {scanning && <ReceiptScanModal onClose={() => setScanning(false)} onImport={createMany} />}
    </main>
  );
}

export default PantryPage;
