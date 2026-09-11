import { useEffect } from "react";
import { Check, ShoppingBasket, X } from "lucide-react";
import { Link } from "react-router-dom";
import type { ShoppingListItem, ShoppingListResponse } from "@/types/shopping";

interface ShoppingPreviewModalProps {
  open: boolean;
  loading: boolean;
  error: boolean;
  actionError: string | null;
  shoppingList: ShoppingListResponse | null;
  completingItems: number[];
  onClose: () => void;
  onRetry: () => void;
  onComplete: (itemId: number) => void;
}

function formatQuantity(quantity: number | null) {
  if (quantity === null) return "";
  return Number.isInteger(quantity) ? quantity.toString() : quantity.toFixed(2).replace(/0+$/, "").replace(/\.$/, "");
}

function formatAmount(item: ShoppingListItem) {
  const quantity = formatQuantity(item.quantity);
  if (!quantity) return "";
  if (!item.unit || item.unit.toLowerCase() === "each") return quantity;
  return `${quantity} ${item.unit}`;
}

function ShoppingPreviewModal({
  open,
  loading,
  error,
  actionError,
  shoppingList,
  completingItems,
  onClose,
  onRetry,
  onComplete
}: ShoppingPreviewModalProps) {
  useEffect(() => {
    if (!open) return;

    const originalOverflow = document.body.style.overflow;
    document.body.style.overflow = "hidden";

    function handleKeyDown(event: KeyboardEvent) {
      if (event.key === "Escape") onClose();
    }

    window.addEventListener("keydown", handleKeyDown);

    return () => {
      document.body.style.overflow = originalOverflow;
      window.removeEventListener("keydown", handleKeyDown);
    };
  }, [open, onClose]);

  if (!open) return null;

  return (
    <div role="presentation" className="fixed inset-0 z-[100] flex items-end justify-center bg-black/40 p-0 backdrop-blur-sm sm:items-center sm:p-4" onClick={onClose}>
      <div role="dialog" aria-modal="true" aria-labelledby="shopping-modal-title" className="w-full max-w-md overflow-hidden rounded-t-[2rem] bg-white shadow-2xl dark:bg-[#202521] dark:ring-1 dark:ring-white/[0.08] sm:rounded-3xl" onClick={(event) => event.stopPropagation()}>
        <div className="flex items-center justify-between border-b border-stone-100 px-5 py-4 dark:border-white/[0.06]">
          <div>
            <h2 id="shopping-modal-title" className="text-lg font-semibold text-stone-900 dark:text-stone-100">Shopping</h2>
            {!loading && !error && shoppingList && (
              <p className="mt-0.5 text-sm text-stone-500 dark:text-stone-400">
                {shoppingList.itemCount} {shoppingList.itemCount === 1 ? "item" : "items"} left
              </p>
            )}
          </div>

          <button type="button" aria-label="Close shopping list" onClick={onClose} className="flex h-9 w-9 items-center justify-center rounded-full bg-stone-100 text-stone-600 transition hover:bg-stone-200 dark:bg-white/[0.06] dark:text-stone-300 dark:hover:bg-white/[0.1]">
            <X size={18} />
          </button>
        </div>

        <div className="max-h-[55dvh] overflow-y-auto px-5 py-3">
          {loading && <div className="py-8 text-center text-sm text-stone-500 dark:text-stone-400">Loading shopping list...</div>}

          {!loading && error && (
            <div className="py-8 text-center">
              <p className="font-medium text-stone-900 dark:text-stone-100">Couldn't load the shopping list</p>
              <button type="button" onClick={onRetry} className="mt-3 text-sm font-medium text-emerald-800 dark:text-emerald-400">Try again</button>
            </div>
          )}

          {!loading && !error && actionError && (
            <div className="my-2 rounded-2xl bg-red-50 px-4 py-3 text-sm text-red-700 dark:bg-red-400/10 dark:text-red-300">{actionError}</div>
          )}

          {!loading && !error && shoppingList?.items.length === 0 && (
            <div className="py-10 text-center">
              <div className="mx-auto flex h-12 w-12 items-center justify-center rounded-2xl bg-emerald-50 text-emerald-800 dark:bg-emerald-400/10 dark:text-emerald-300">
                <ShoppingBasket size={22} />
              </div>
              <p className="mt-4 font-medium text-stone-900 dark:text-stone-100">Shopping list is empty</p>
              <p className="mt-1 text-sm text-stone-500 dark:text-stone-400">Add groceries, household items or ingredients from a recipe.</p>
            </div>
          )}

          {!loading && !error && shoppingList && shoppingList.items.length > 0 && (
            <div className="divide-y divide-stone-100 dark:divide-white/[0.06]">
              {shoppingList.items.map((item) => {
                const isCompleting = completingItems.includes(item.id);

                return (
                  <div key={item.id} className={`flex items-center gap-3 py-3.5 transition-all duration-200 ${isCompleting ? "opacity-50" : "opacity-100"}`}>
                    <button type="button" role="checkbox" aria-checked={isCompleting} aria-label={`Mark ${item.name} as bought`} onClick={() => onComplete(item.id)} disabled={isCompleting} className={`flex h-5 w-5 shrink-0 items-center justify-center rounded-md border transition ${isCompleting ? "border-emerald-700 bg-emerald-700 text-white dark:border-emerald-500 dark:bg-emerald-600" : "border-stone-300 bg-transparent hover:border-emerald-700 dark:border-stone-600 dark:hover:border-emerald-400"}`}>
                      {isCompleting && <Check size={14} strokeWidth={3} />}
                    </button>

                    <div className="min-w-0 flex-1">
                      <p className={`truncate text-sm font-medium text-stone-800 transition dark:text-stone-200 ${isCompleting ? "line-through" : ""}`}>{item.name}</p>
                      {item.notes && <p className="mt-0.5 truncate text-xs text-stone-400 dark:text-stone-500">{item.notes}</p>}
                    </div>

                    <p className={`shrink-0 text-sm text-stone-500 transition dark:text-stone-400 ${isCompleting ? "line-through" : ""}`}>{formatAmount(item)}</p>
                  </div>
                );
              })}
            </div>
          )}
        </div>

        <div className="border-t border-stone-100 p-4 dark:border-white/[0.06]">
          <Link to="/shopping" onClick={onClose} className="flex h-12 w-full items-center justify-center rounded-2xl bg-emerald-900 text-sm font-medium text-white transition hover:bg-emerald-800 dark:bg-emerald-700 dark:hover:bg-emerald-600">
            View full list
          </Link>
        </div>
      </div>
    </div>
  );
}

export default ShoppingPreviewModal;