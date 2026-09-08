import { Check, RotateCcw } from "lucide-react";
import type { ShoppingListItem } from "@/types/shopping";

interface ShoppingItemRowProps {
  item: ShoppingListItem;
  completed?: boolean;
  busy?: boolean;
  onComplete?: () => void;
  onRestore?: () => void;
}

function formatQuantity(quantity: number) {
  return Number.isInteger(quantity)
    ? quantity.toString()
    : quantity.toFixed(2).replace(/0+$/, "").replace(/\.$/, "");
}

function formatAmount(item: ShoppingListItem) {
  if (item.quantity === null) return "";

  const quantity = formatQuantity(item.quantity);

  if (!item.unit || item.unit.toLowerCase() === "each") return quantity;

  return `${quantity} ${item.unit}`;
}

function ShoppingItemRow({ item, completed = false, busy = false, onComplete, onRestore }: ShoppingItemRowProps) {
  const amount = formatAmount(item);

  return (
    <div className={`flex items-center gap-4 px-4 py-4 transition-all duration-200 ${busy ? "opacity-50" : "opacity-100"}`}>
      {completed ? (
        <div className="flex h-6 w-6 shrink-0 items-center justify-center rounded-lg bg-emerald-700 text-white dark:bg-emerald-600">
          <Check size={15} strokeWidth={3} />
        </div>
      ) : (
        <button type="button" aria-label={`Mark ${item.name} as bought`} onClick={onComplete} disabled={busy} className={`flex h-6 w-6 shrink-0 items-center justify-center rounded-lg border transition ${busy ? "border-emerald-700 bg-emerald-700 text-white dark:border-emerald-500 dark:bg-emerald-600" : "border-stone-300 hover:border-emerald-700 dark:border-stone-600 dark:hover:border-emerald-400"}`}>
          {busy && <Check size={15} strokeWidth={3} />}
        </button>
      )}

      <div className="min-w-0 flex-1">
        <p className={`truncate text-sm font-medium ${completed ? "text-stone-400 line-through dark:text-stone-500" : "text-stone-800 dark:text-stone-200"}`}>
          {item.name}
        </p>

        {item.notes && (
          <p className={`mt-0.5 truncate text-xs ${completed ? "text-stone-400 line-through dark:text-stone-600" : "text-stone-400 dark:text-stone-500"}`}>
            {item.notes}
          </p>
        )}
      </div>

      {amount && (
        <p className={`shrink-0 text-sm ${completed ? "text-stone-400 line-through dark:text-stone-500" : "text-stone-500 dark:text-stone-400"}`}>
          {amount}
        </p>
      )}

      {completed && (
        <button type="button" aria-label={`Restore ${item.name}`} disabled={busy} onClick={onRestore} className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full text-stone-400 transition hover:bg-white hover:text-emerald-700 disabled:opacity-40 dark:hover:bg-white/[0.06] dark:hover:text-emerald-400">
          <RotateCcw size={15} />
        </button>
      )}
    </div>
  );
}

export default ShoppingItemRow;