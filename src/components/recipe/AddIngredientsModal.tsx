import { Check, ShoppingBasket, X } from "lucide-react";
import type { RecipeIngredient } from "@/types/recipe";

interface AddIngredientsModalProps {
  open: boolean;
  ingredients: RecipeIngredient[];
  selectedIds: number[];
  loading: boolean;
  error: string | null;
  onClose: () => void;
  onToggle: (id: number) => void;
  onToggleAll: () => void;
  onSubmit: () => void;
}

function formatAmount(ingredient: RecipeIngredient) {
  if (ingredient.quantity === null) return "";

  const quantity = Number.isInteger(ingredient.quantity)
    ? ingredient.quantity.toString()
    : ingredient.quantity.toFixed(2).replace(/0+$/, "").replace(/\.$/, "");

  if (!ingredient.unit || ingredient.unit.toLowerCase() === "each") return quantity;

  return `${quantity} ${ingredient.unit}`;
}

function AddIngredientsModal({
  open,
  ingredients,
  selectedIds,
  loading,
  error,
  onClose,
  onToggle,
  onToggleAll,
  onSubmit
}: AddIngredientsModalProps) {
  if (!open) return null;

  const allSelected = selectedIds.length === ingredients.length;

  return (
    <div className="fixed inset-0 z-[100] flex items-end justify-center bg-black/40 backdrop-blur-sm sm:items-center sm:p-4" onClick={onClose}>
      <div role="dialog" aria-modal="true" className="w-full max-w-md overflow-hidden rounded-t-[2rem] bg-white shadow-2xl dark:bg-[#202521] dark:ring-1 dark:ring-white/[0.08] sm:rounded-3xl" onClick={(event) => event.stopPropagation()}>
        <div className="flex items-center justify-between border-b border-stone-100 px-5 py-4 dark:border-white/[0.06]">
          <div>
            <h2 className="text-lg font-semibold text-stone-900 dark:text-stone-100">Add ingredients</h2>
            <p className="mt-0.5 text-sm text-stone-500 dark:text-stone-400">Choose what you need</p>
          </div>

          <button type="button" aria-label="Close" onClick={onClose} className="flex h-9 w-9 items-center justify-center rounded-full bg-stone-100 text-stone-600 dark:bg-white/[0.06] dark:text-stone-300">
            <X size={18} />
          </button>
        </div>

        <div className="flex items-center justify-between border-b border-stone-100 px-5 py-3 dark:border-white/[0.06]">
          <p className="text-sm text-stone-500 dark:text-stone-400">{selectedIds.length} of {ingredients.length} selected</p>

          <button type="button" onClick={onToggleAll} className="text-sm font-medium text-emerald-800 dark:text-emerald-400">
            {allSelected ? "Deselect all" : "Select all"}
          </button>
        </div>

        <div className="max-h-[55dvh] overflow-y-auto px-5">
          <div className="divide-y divide-stone-100 dark:divide-white/[0.06]">
            {ingredients.map((ingredient) => {
              const selected = selectedIds.includes(ingredient.id);

              return (
                <button type="button" key={ingredient.id} onClick={() => onToggle(ingredient.id)} className="flex w-full items-center gap-3 py-4 text-left">
                  <span className={`flex h-5 w-5 shrink-0 items-center justify-center rounded-md border transition ${selected ? "border-emerald-700 bg-emerald-700 text-white dark:border-emerald-500 dark:bg-emerald-600" : "border-stone-300 dark:border-stone-600"}`}>
                    {selected && <Check size={14} strokeWidth={3} />}
                  </span>

                  <div className="min-w-0 flex-1">
                    <p className="text-sm font-medium text-stone-800 dark:text-stone-200">{ingredient.name}</p>
                    {ingredient.notes && <p className="mt-0.5 text-xs text-stone-400 dark:text-stone-500">{ingredient.notes}</p>}
                  </div>

                  <p className="shrink-0 text-sm text-stone-500 dark:text-stone-400">{formatAmount(ingredient)}</p>
                </button>
              );
            })}
          </div>
        </div>

        {error && <div className="mx-5 mb-2 rounded-2xl bg-red-50 px-4 py-3 text-sm text-red-700 dark:bg-red-400/10 dark:text-red-300">{error}</div>}

        <div className="border-t border-stone-100 p-4 dark:border-white/[0.06]">
          <button type="button" disabled={selectedIds.length === 0 || loading} onClick={onSubmit} className="flex h-12 w-full items-center justify-center gap-2 rounded-2xl bg-emerald-900 text-sm font-medium text-white transition hover:bg-emerald-800 disabled:cursor-not-allowed disabled:opacity-40 dark:bg-emerald-700 dark:hover:bg-emerald-600">
            <ShoppingBasket size={17} />
            {loading ? "Adding..." : `Add ${selectedIds.length} ${selectedIds.length === 1 ? "item" : "items"}`}
          </button>
        </div>
      </div>
    </div>
  );
}

export default AddIngredientsModal;