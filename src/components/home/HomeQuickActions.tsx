import { CheckSquare, ShoppingBasket } from "lucide-react";
import type { ShoppingListResponse } from "@/types/shopping";

interface HomeQuickActionsProps {
  shoppingList: ShoppingListResponse | null;
  shoppingLoading: boolean;
  onShoppingOpen: () => void;
}

function HomeQuickActions({ shoppingList, shoppingLoading, onShoppingOpen }: HomeQuickActionsProps) {
  return (
    <section>
      <h2 className="mb-3 text-lg font-semibold text-stone-900 dark:text-stone-100">Our home</h2>

      <div className="grid h-[calc(100%-2.25rem)] grid-cols-2 gap-3">
        <button type="button" onClick={onShoppingOpen} className="flex flex-col rounded-3xl bg-stone-100 p-5 text-left text-stone-900 transition hover:bg-stone-200 dark:bg-white/[0.055] dark:text-stone-100 dark:ring-1 dark:ring-white/[0.06] dark:hover:bg-white/[0.08]">
          <ShoppingBasket size={24} className="mb-auto text-stone-700 dark:text-emerald-400" />
          <p className="mt-8 font-medium">Shopping</p>
          <p className="mt-1 text-sm text-stone-500 dark:text-stone-400">
            {shoppingLoading && !shoppingList
              ? "Loading..."
              : shoppingList
                ? `${shoppingList.itemCount} ${shoppingList.itemCount === 1 ? "item" : "items"}`
                : "View list"}
          </p>
        </button>

        <button type="button" className="flex flex-col rounded-3xl bg-stone-100 p-5 text-left text-stone-900 transition hover:bg-stone-200 dark:bg-white/[0.055] dark:text-stone-100 dark:ring-1 dark:ring-white/[0.06] dark:hover:bg-white/[0.08]">
          <CheckSquare size={24} className="mb-auto text-stone-700 dark:text-emerald-400" />
          <p className="mt-8 font-medium">Chores</p>
          <p className="mt-1 text-sm text-stone-500 dark:text-stone-400">All caught up</p>
        </button>
      </div>
    </section>
  );
}

export default HomeQuickActions;