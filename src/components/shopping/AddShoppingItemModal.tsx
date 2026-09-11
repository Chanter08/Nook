import { useEffect, useState } from "react";
import { Plus, Search, X } from "lucide-react";
import type {
  AddShoppingItemPayload,
  ShoppingCatalogSuggestion,
} from "@/types/shopping";
import { searchShoppingCatalog } from "@/api/shopping";

interface AddShoppingItemModalProps {
  onClose: () => void;
  onSubmit: (item: AddShoppingItemPayload) => Promise<void>;
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

const units = [
  { value: "", label: "None" },
  { value: "each", label: "Each" },
  { value: "pack", label: "Pack" },
  { value: "box", label: "Box" },
  { value: "bag", label: "Bag" },
  { value: "bottle", label: "Bottle" },
  { value: "roll", label: "Roll" },
  { value: "g", label: "g" },
  { value: "kg", label: "kg" },
  { value: "ml", label: "ml" },
  { value: "l", label: "L" },
];

function AddShoppingItemModal({
  onClose,
  onSubmit,
}: AddShoppingItemModalProps) {
  const [name, setName] = useState("");
  const [quantity, setQuantity] = useState("");
  const [unit, setUnit] = useState("");
  const [category, setCategory] = useState("Household");
  const [notes, setNotes] = useState("");
  const [selectedCatalogItemId, setSelectedCatalogItemId] = useState<
    number | null
  >(null);
  const [suggestions, setSuggestions] = useState<ShoppingCatalogSuggestion[]>(
    [],
  );
  const [searching, setSearching] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const query = name.trim();

    if (query.length < 2 || selectedCatalogItemId !== null) {
      setSuggestions([]);
      setSearching(false);
      return;
    }

    const controller = new AbortController();

    const timeout = window.setTimeout(async () => {
      try {
        setSearching(true);

        const data = await searchShoppingCatalog(query, controller.signal);
        setSuggestions(data);
      } catch (error) {
        if (error instanceof DOMException && error.name === "AbortError")
          return;

        console.error("Shopping catalogue search error:", error);
        setSuggestions([]);
      } finally {
        setSearching(false);
      }
    }, 250);

    return () => {
      window.clearTimeout(timeout);
      controller.abort();
    };
  }, [name, selectedCatalogItemId]);

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

  function selectSuggestion(item: ShoppingCatalogSuggestion) {
    setSelectedCatalogItemId(item.id);
    setName(item.name);
    setUnit(item.unit ?? "");
    setCategory(item.category ?? "Household");
    setNotes(item.notes ?? "");
    setSuggestions([]);
  }

  async function submit() {
    const trimmedName = name.trim();

    if (!trimmedName || submitting) return;

    const parsedQuantity = quantity.trim() === "" ? null : Number(quantity);

    if (
      parsedQuantity !== null &&
      (!Number.isFinite(parsedQuantity) || parsedQuantity <= 0)
    ) {
      setError("Quantity must be greater than zero.");
      return;
    }

    try {
      setSubmitting(true);
      setError(null);

      await onSubmit({
        shoppingCatalogItemId: selectedCatalogItemId,
        name: trimmedName,
        quantity: parsedQuantity,
        unit: unit || null,
        category: category || null,
        notes: notes.trim() || null,
      });

      onClose();
    } catch (error) {
      console.error(error);
      setError("Couldn't add the item.");
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
        className="w-full max-w-md overflow-visible rounded-t-[2rem] bg-white shadow-2xl dark:bg-[#202521] dark:ring-1 dark:ring-white/[0.08] sm:rounded-3xl"
        onClick={(event) => event.stopPropagation()}
      >
        <div className="flex items-center justify-between border-b border-stone-100 px-5 py-4 dark:border-white/[0.06]">
          <div>
            <h2 className="text-lg font-semibold text-stone-900 dark:text-stone-100">
              Add item
            </h2>
            <p className="mt-0.5 text-sm text-stone-500 dark:text-stone-400">
              Add anything the house needs
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

        <div className="max-h-[70dvh] space-y-5 overflow-y-auto px-5 py-5">
          <div className="relative">
            <label
              htmlFor="shopping-item-name"
              className="text-sm font-medium text-stone-700 dark:text-stone-300"
            >
              Item
            </label>

            <div className="relative mt-2">
              <input
                id="shopping-item-name"
                value={name}
                onChange={(event) => {
                  setName(event.target.value);
                  setSelectedCatalogItemId(null);
                }}
                placeholder="e.g. Toilet paper"
                autoFocus
                autoComplete="off"
                className="h-12 w-full rounded-2xl border border-stone-200 bg-stone-50 px-4 pr-10 text-sm text-stone-900 outline-none focus:border-emerald-700 dark:border-white/[0.08] dark:bg-white/[0.045] dark:text-stone-100 dark:focus:border-emerald-500"
              />

              {searching && (
                <Search
                  size={16}
                  className="absolute right-4 top-1/2 -translate-y-1/2 animate-pulse text-stone-400"
                />
              )}
            </div>

            {suggestions.length > 0 && (
              <div className="absolute left-0 right-0 top-full z-20 mt-2 overflow-hidden rounded-2xl border border-stone-200 bg-white shadow-xl dark:border-white/[0.08] dark:bg-[#292e2a]">
                {suggestions.map((item) => (
                  <button
                    key={item.id}
                    type="button"
                    onClick={() => selectSuggestion(item)}
                    className="flex w-full items-center justify-between gap-4 border-b border-stone-100 px-4 py-3 text-left last:border-b-0 hover:bg-stone-50 dark:border-white/[0.06] dark:hover:bg-white/[0.05]"
                  >
                    <div className="min-w-0">
                      <p className="truncate text-sm font-medium text-stone-800 dark:text-stone-200">
                        {item.name}
                      </p>

                      {(item.category || item.notes) && (
                        <p className="mt-0.5 truncate text-xs text-stone-400 dark:text-stone-500">
                          {[item.category, item.notes]
                            .filter(Boolean)
                            .join(" · ")}
                        </p>
                      )}
                    </div>

                    {item.unit && (
                      <span className="shrink-0 text-xs text-stone-400">
                        {item.unit}
                      </span>
                    )}
                  </button>
                ))}
              </div>
            )}
          </div>

          <div className="grid grid-cols-2 gap-3">
            <div>
              <label
                htmlFor="shopping-item-quantity"
                className="text-sm font-medium text-stone-700 dark:text-stone-300"
              >
                Quantity
              </label>

              <input
                id="shopping-item-quantity"
                type="number"
                min="0"
                step="any"
                inputMode="decimal"
                value={quantity}
                onChange={(event) => setQuantity(event.target.value)}
                placeholder="1"
                className="mt-2 h-12 w-full rounded-2xl border border-stone-200 bg-stone-50 px-4 text-sm text-stone-900 outline-none focus:border-emerald-700 dark:border-white/[0.08] dark:bg-white/[0.045] dark:text-stone-100 dark:focus:border-emerald-500"
              />
            </div>

            <div>
              <label
                htmlFor="shopping-item-unit"
                className="text-sm font-medium text-stone-700 dark:text-stone-300"
              >
                Unit
              </label>

              <select
                id="shopping-item-unit"
                value={unit}
                onChange={(event) => setUnit(event.target.value)}
                className="mt-2 h-12 w-full rounded-2xl border border-stone-200 bg-stone-50 px-4 text-sm text-stone-900 outline-none dark:border-white/[0.08] dark:bg-[#292e2a] dark:text-stone-100"
              >
                {units.map((item) => (
                  <option key={item.value} value={item.value}>
                    {item.label}
                  </option>
                ))}
              </select>
            </div>
          </div>

          <div>
            <label
              htmlFor="shopping-item-category"
              className="text-sm font-medium text-stone-700 dark:text-stone-300"
            >
              Category
            </label>

            <select
              id="shopping-item-category"
              value={category}
              onChange={(event) => setCategory(event.target.value)}
              className="mt-2 h-12 w-full rounded-2xl border border-stone-200 bg-stone-50 px-4 text-sm text-stone-900 outline-none dark:border-white/[0.08] dark:bg-[#292e2a] dark:text-stone-100"
            >
              {categories.map((item) => (
                <option key={item} value={item}>
                  {item}
                </option>
              ))}
            </select>
          </div>

          <div>
            <label
              htmlFor="shopping-item-notes"
              className="text-sm font-medium text-stone-700 dark:text-stone-300"
            >
              Notes
              <span className="ml-1 font-normal text-stone-400">optional</span>
            </label>

            <input
              id="shopping-item-notes"
              value={notes}
              onChange={(event) => setNotes(event.target.value)}
              placeholder="e.g. Unscented, large size"
              className="mt-2 h-12 w-full rounded-2xl border border-stone-200 bg-stone-50 px-4 text-sm text-stone-900 outline-none focus:border-emerald-700 dark:border-white/[0.08] dark:bg-white/[0.045] dark:text-stone-100 dark:focus:border-emerald-500"
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
            className="flex h-12 w-full items-center justify-center gap-2 rounded-2xl bg-emerald-900 text-sm font-medium text-white transition hover:bg-emerald-800 disabled:cursor-not-allowed disabled:opacity-40 dark:bg-emerald-700 dark:hover:bg-emerald-600"
          >
            <Plus size={17} />
            {submitting ? "Adding..." : "Add to shopping"}
          </button>
        </div>
      </div>
    </div>
  );
}

export default AddShoppingItemModal;