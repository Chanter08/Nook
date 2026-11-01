import { useEffect, useState } from "react";
import { PackagePlus, Save, X } from "lucide-react";
import type {
  PantryItem,
  PantryItemPayload,
  PantryStorageLocation,
} from "@/types/pantry";

interface PantryItemModalProps {
  item?: PantryItem | null;
  onClose: () => void;
  onSubmit: (payload: PantryItemPayload) => Promise<void>;
}

const categories = [
  "Groceries",
  "Household",
  "Cleaning",
  "Toiletries",
  "Pets",
  "Pharmacy",
  "Other",
];

const locations: PantryStorageLocation[] = [
  "Pantry",
  "Fridge",
  "Freezer",
  "Cleaning",
  "Bathroom",
  "Pet",
  "Medicine",
  "Other",
];

const units = [
  "",
  "each",
  "pack",
  "box",
  "bag",
  "bottle",
  "roll",
  "g",
  "kg",
  "ml",
  "l",
];

function PantryItemModal({
  item,
  onClose,
  onSubmit,
}: PantryItemModalProps) {
  const [name, setName] = useState(item?.name ?? "");
  const [quantity, setQuantity] = useState(
    item?.quantity?.toString() ?? (item ? "" : "1"),
  );
  const [unit, setUnit] = useState(item?.unit ?? "each");
  const [category, setCategory] = useState(item?.category ?? "Groceries");
  const [storageLocation, setStorageLocation] =
    useState<PantryStorageLocation>(item?.storageLocation ?? "Pantry");
  const [expiryDate, setExpiryDate] = useState(item?.expiryDate ?? "");
  const [lowStockThreshold, setLowStockThreshold] = useState(
    item?.lowStockThreshold?.toString() ?? "",
  );
  const [notes, setNotes] = useState(item?.notes ?? "");
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    function handleKeyDown(event: KeyboardEvent) {
      if (event.key === "Escape") onClose();
    }

    document.body.style.overflow = "hidden";
    window.addEventListener("keydown", handleKeyDown);

    return () => {
      document.body.style.overflow = "";
      window.removeEventListener("keydown", handleKeyDown);
    };
  }, [onClose]);

  async function submit() {
    if (!name.trim() || submitting) return;

    const parsedQuantity = quantity === "" ? null : Number(quantity);
    const parsedThreshold =
      lowStockThreshold === "" ? null : Number(lowStockThreshold);

    if (
      parsedQuantity !== null &&
      (!Number.isFinite(parsedQuantity) || parsedQuantity < 0)
    ) {
      setError("Quantity cannot be negative.");
      return;
    }

    if (
      parsedThreshold !== null &&
      (!Number.isFinite(parsedThreshold) || parsedThreshold < 0)
    ) {
      setError("Low-stock threshold cannot be negative.");
      return;
    }

    try {
      setSubmitting(true);
      setError(null);

      await onSubmit({
        shoppingCatalogItemId: item?.shoppingCatalogItemId ?? null,
        name: name.trim(),
        quantity: parsedQuantity,
        unit: unit || null,
        category: category || null,
        storageLocation,
        expiryDate: expiryDate || null,
        lowStockThreshold: parsedThreshold,
        notes: notes.trim() || null,
      });

      onClose();
    } catch (caught) {
      console.error(caught);
      setError(
        caught instanceof Error
          ? caught.message
          : "Couldn't save this pantry item.",
      );
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <div
      className="fixed inset-0 z-[100] flex items-end justify-center bg-black/40 backdrop-blur-sm sm:items-center sm:p-4"
      onClick={onClose}
    >
      <div
        role="dialog"
        aria-modal="true"
        className="w-full max-w-lg rounded-t-[2rem] bg-white shadow-2xl dark:bg-[#202521] dark:ring-1 dark:ring-white/[0.08] sm:rounded-3xl"
        onClick={(event) => event.stopPropagation()}
      >
        <div className="flex items-center justify-between border-b border-stone-100 px-5 py-4 dark:border-white/[0.06]">
          <div>
            <h2 className="text-lg font-semibold text-stone-900 dark:text-stone-100">
              {item ? "Edit pantry item" : "Add pantry item"}
            </h2>
            <p className="mt-0.5 text-sm text-stone-500 dark:text-stone-400">
              Track what is already at home
            </p>
          </div>

          <button
            type="button"
            aria-label="Close"
            onClick={onClose}
            className="flex h-9 w-9 items-center justify-center rounded-full bg-stone-100 text-stone-600 dark:bg-white/[0.06] dark:text-stone-300"
          >
            <X size={18} />
          </button>
        </div>

        <div className="max-h-[72dvh] space-y-5 overflow-y-auto px-5 py-5">
          <div>
            <label htmlFor="pantry-name" className="text-sm font-medium text-stone-700 dark:text-stone-300">
              Item
            </label>
            <input
              id="pantry-name"
              value={name}
              onChange={(event) => setName(event.target.value)}
              placeholder="e.g. Basmati rice"
              autoFocus
              className="mt-2 h-12 w-full rounded-2xl border border-stone-200 bg-stone-50 px-4 text-sm outline-none focus:border-emerald-700 dark:border-white/[0.08] dark:bg-white/[0.045] dark:text-stone-100"
            />
          </div>

          <div className="grid grid-cols-2 gap-3">
            <div>
              <label htmlFor="pantry-quantity" className="text-sm font-medium text-stone-700 dark:text-stone-300">
                Quantity
              </label>
              <input
                id="pantry-quantity"
                type="number"
                min="0"
                step="any"
                inputMode="decimal"
                value={quantity}
                onChange={(event) => setQuantity(event.target.value)}
                className="mt-2 h-12 w-full rounded-2xl border border-stone-200 bg-stone-50 px-4 text-sm outline-none focus:border-emerald-700 dark:border-white/[0.08] dark:bg-white/[0.045] dark:text-stone-100"
              />
            </div>

            <div>
              <label htmlFor="pantry-unit" className="text-sm font-medium text-stone-700 dark:text-stone-300">
                Unit
              </label>
              <select
                id="pantry-unit"
                value={unit}
                onChange={(event) => setUnit(event.target.value)}
                className="mt-2 h-12 w-full rounded-2xl border border-stone-200 bg-stone-50 px-4 text-sm dark:border-white/[0.08] dark:bg-[#292e2a] dark:text-stone-100"
              >
                {units.map((value) => (
                  <option key={value || "none"} value={value}>
                    {value || "None"}
                  </option>
                ))}
              </select>
            </div>
          </div>

          <div className="grid grid-cols-2 gap-3">
            <div>
              <label htmlFor="pantry-category" className="text-sm font-medium text-stone-700 dark:text-stone-300">
                Category
              </label>
              <select
                id="pantry-category"
                value={category}
                onChange={(event) => setCategory(event.target.value)}
                className="mt-2 h-12 w-full rounded-2xl border border-stone-200 bg-stone-50 px-4 text-sm dark:border-white/[0.08] dark:bg-[#292e2a] dark:text-stone-100"
              >
                {categories.map((value) => (
                  <option key={value}>{value}</option>
                ))}
              </select>
            </div>

            <div>
              <label htmlFor="pantry-location" className="text-sm font-medium text-stone-700 dark:text-stone-300">
                Stored in
              </label>
              <select
                id="pantry-location"
                value={storageLocation}
                onChange={(event) =>
                  setStorageLocation(event.target.value as PantryStorageLocation)
                }
                className="mt-2 h-12 w-full rounded-2xl border border-stone-200 bg-stone-50 px-4 text-sm dark:border-white/[0.08] dark:bg-[#292e2a] dark:text-stone-100"
              >
                {locations.map((value) => (
                  <option key={value}>{value}</option>
                ))}
              </select>
            </div>
          </div>

          <div className="grid grid-cols-2 gap-3">
            <div>
              <label htmlFor="pantry-expiry" className="text-sm font-medium text-stone-700 dark:text-stone-300">
                Expiry
                <span className="ml-1 font-normal text-stone-400">optional</span>
              </label>
              <input
                id="pantry-expiry"
                type="date"
                value={expiryDate}
                onChange={(event) => setExpiryDate(event.target.value)}
                className="mt-2 h-12 w-full rounded-2xl border border-stone-200 bg-stone-50 px-4 text-sm dark:border-white/[0.08] dark:bg-white/[0.045] dark:text-stone-100"
              />
            </div>

            <div>
              <label htmlFor="pantry-threshold" className="text-sm font-medium text-stone-700 dark:text-stone-300">
                Low at
                <span className="ml-1 font-normal text-stone-400">optional</span>
              </label>
              <input
                id="pantry-threshold"
                type="number"
                min="0"
                step="any"
                inputMode="decimal"
                value={lowStockThreshold}
                onChange={(event) => setLowStockThreshold(event.target.value)}
                placeholder="e.g. 1"
                className="mt-2 h-12 w-full rounded-2xl border border-stone-200 bg-stone-50 px-4 text-sm dark:border-white/[0.08] dark:bg-white/[0.045] dark:text-stone-100"
              />
            </div>
          </div>

          <div>
            <label htmlFor="pantry-notes" className="text-sm font-medium text-stone-700 dark:text-stone-300">
              Notes
              <span className="ml-1 font-normal text-stone-400">optional</span>
            </label>
            <input
              id="pantry-notes"
              value={notes}
              onChange={(event) => setNotes(event.target.value)}
              placeholder="e.g. Open pack, top shelf"
              className="mt-2 h-12 w-full rounded-2xl border border-stone-200 bg-stone-50 px-4 text-sm outline-none focus:border-emerald-700 dark:border-white/[0.08] dark:bg-white/[0.045] dark:text-stone-100"
            />
          </div>

          {error && (
            <div className="rounded-2xl bg-red-50 px-4 py-3 text-sm text-red-700 dark:bg-red-400/10 dark:text-red-300">
              {error}
            </div>
          )}
        </div>

        <div className="border-t border-stone-100 p-4 dark:border-white/[0.06]">
          <button
            type="button"
            disabled={!name.trim() || submitting}
            onClick={() => void submit()}
            className="flex h-12 w-full items-center justify-center gap-2 rounded-2xl bg-emerald-900 text-sm font-medium text-white transition hover:bg-emerald-800 disabled:opacity-40 dark:bg-emerald-700 dark:hover:bg-emerald-600"
          >
            {item ? <Save size={17} /> : <PackagePlus size={17} />}
            {submitting ? "Saving..." : item ? "Save changes" : "Add to pantry"}
          </button>
        </div>
      </div>
    </div>
  );
}

export default PantryItemModal;
