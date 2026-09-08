import { useCallback, useEffect, useMemo, useState } from "react";
import { ArrowLeft, CheckCircle2, Plus, Trash2 } from "lucide-react";
import { useNavigate } from "react-router-dom";
import ThemeToggle from "@/components/ThemeToggle";
import AddShoppingItemModal from "@/components/shopping/AddShoppingItemModal";
import ShoppingItemRow from "@/components/shopping/ShoppingItemRow";
import type { AddShoppingItemPayload, ShoppingListItem, ShoppingListResponse } from "@/types/shopping";

const categoryOrder = [
  "Groceries",
  "Household",
  "Cleaning",
  "Toiletries",
  "Pets",
  "Pharmacy",
  "Other"
];

interface ShoppingGroup {
  category: string;
  items: ShoppingListItem[];
}

function groupItems(items: ShoppingListItem[]) {
  const groups = new Map<string, ShoppingListItem[]>();

  for (const item of items) {
    const category = item.category?.trim() || "Other";
    const existing = groups.get(category) ?? [];

    existing.push(item);
    groups.set(category, existing);
  }

  return Array.from(groups.entries())
    .map(([category, groupedItems]) => ({
      category,
      items: groupedItems.sort((a, b) => a.name.localeCompare(b.name))
    }))
    .sort((a, b) => {
      const aIndex = categoryOrder.indexOf(a.category);
      const bIndex = categoryOrder.indexOf(b.category);

      const aOrder = aIndex === -1 ? categoryOrder.length : aIndex;
      const bOrder = bIndex === -1 ? categoryOrder.length : bIndex;

      return aOrder - bOrder || a.category.localeCompare(b.category);
    });
}

function ShoppingPage() {
  const navigate = useNavigate();

  const [shoppingList, setShoppingList] = useState<ShoppingListResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(false);
  const [actionError, setActionError] = useState<string | null>(null);

  const [completingIds, setCompletingIds] = useState<number[]>([]);
  const [restoringIds, setRestoringIds] = useState<number[]>([]);
  const [clearingCompleted, setClearingCompleted] = useState(false);
  const [addItemOpen, setAddItemOpen] = useState(false);

  const activeGroups = useMemo(
    () => groupItems(shoppingList?.items ?? []),
    [shoppingList?.items]
  );

  const loadShoppingList = useCallback(async () => {
    try {
      setError(false);

      const response = await fetch("/api/shopping");

      if (!response.ok) {
        throw new Error(`Failed to load shopping list: ${response.status}`);
      }

      const data: ShoppingListResponse = await response.json();

      setShoppingList({
        ...data,
        itemCount: data.itemCount ?? data.items?.length ?? 0,
        completedCount: data.completedCount ?? 0,
        items: data.items ?? [],
        completedItems: data.completedItems ?? []
      });
    } catch (error) {
      console.error("Shopping list error:", error);
      setError(true);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    void loadShoppingList();
  }, [loadShoppingList]);

  async function addManualItem(item: AddShoppingItemPayload) {
    const response = await fetch("/api/shopping/items", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(item)
    });

    if (!response.ok) {
      throw new Error(await response.text());
    }

    await loadShoppingList();
  }

  async function completeItem(itemId: number) {
    if (completingIds.includes(itemId)) return;

    setActionError(null);
    setCompletingIds((current) => [...current, itemId]);

    try {
      const response = await fetch(`/api/shopping/items/${itemId}/complete`, {
        method: "PATCH"
      });

      if (!response.ok) throw new Error("Failed to complete item");

      await new Promise((resolve) => window.setTimeout(resolve, 250));

      setShoppingList((current) => {
        if (!current) return current;

        const item = current.items.find((item) => item.id === itemId);

        if (!item) return current;

        const remaining = current.items.filter((item) => item.id !== itemId);

        return {
          ...current,
          items: remaining,
          itemCount: remaining.length,
          completedItems: [item, ...current.completedItems],
          completedCount: current.completedItems.length + 1
        };
      });
    } catch (error) {
      console.error(error);
      setActionError("Couldn't mark that item as bought.");
    } finally {
      setCompletingIds((current) => current.filter((id) => id !== itemId));
    }
  }

  async function restoreItem(itemId: number) {
    if (restoringIds.includes(itemId)) return;

    setActionError(null);
    setRestoringIds((current) => [...current, itemId]);

    try {
      const response = await fetch(`/api/shopping/items/${itemId}/uncomplete`, {
        method: "PATCH"
      });

      if (!response.ok) throw new Error("Failed to restore item");

      setShoppingList((current) => {
        if (!current) return current;

        const item = current.completedItems.find((item) => item.id === itemId);

        if (!item) return current;

        const completedItems = current.completedItems.filter((item) => item.id !== itemId);

        return {
          ...current,
          items: [...current.items, item],
          itemCount: current.items.length + 1,
          completedItems,
          completedCount: completedItems.length
        };
      });
    } catch (error) {
      console.error(error);
      setActionError("Couldn't restore that item.");
    } finally {
      setRestoringIds((current) => current.filter((id) => id !== itemId));
    }
  }

  async function clearCompleted() {
    if (clearingCompleted || !shoppingList || shoppingList.completedItems.length === 0) return;

    setActionError(null);
    setClearingCompleted(true);

    try {
      const response = await fetch("/api/shopping/completed", {
        method: "DELETE"
      });

      if (!response.ok) throw new Error("Failed to clear completed items");

      setShoppingList((current) =>
        current
          ? {
              ...current,
              completedItems: [],
              completedCount: 0
            }
          : current
      );
    } catch (error) {
      console.error(error);
      setActionError("Couldn't clear completed items.");
    } finally {
      setClearingCompleted(false);
    }
  }

  return (
    <main className="mx-auto min-h-dvh w-full max-w-3xl px-4 pb-28 pt-8 sm:px-6 sm:pt-12">
      <header className="mb-8">
        <div className="flex items-center justify-between">
          <button type="button" aria-label="Go back" onClick={() => navigate(-1)} className="flex h-11 w-11 items-center justify-center rounded-full bg-stone-100 text-stone-700 transition hover:bg-stone-200 dark:bg-white/[0.06] dark:text-stone-200 dark:ring-1 dark:ring-white/[0.06] dark:hover:bg-white/[0.1]">
            <ArrowLeft size={20} />
          </button>

          <ThemeToggle />
        </div>

        <div className="mt-7">
          <p className="text-sm font-medium text-emerald-800 dark:text-emerald-400">Our home</p>

          <div className="mt-1 flex items-end justify-between gap-4">
            <div>
              <h1 className="text-3xl font-semibold tracking-tight text-stone-900 dark:text-stone-100">Shopping</h1>

              {!loading && !error && shoppingList && (
                <p className="mt-1 text-sm text-stone-500 dark:text-stone-400">
                  {shoppingList.itemCount === 0
                    ? "Nothing left to buy"
                    : `${shoppingList.itemCount} ${shoppingList.itemCount === 1 ? "item" : "items"} left`}
                </p>
              )}
            </div>

            <button type="button" onClick={() => setAddItemOpen(true)} className="flex h-11 items-center gap-2 rounded-full bg-emerald-900 px-4 text-sm font-medium text-white transition hover:bg-emerald-800 dark:bg-emerald-700 dark:hover:bg-emerald-600">
              <Plus size={17} />
              Add item
            </button>
          </div>
        </div>
      </header>

      {actionError && (
        <div className="mb-5 rounded-2xl bg-red-50 px-4 py-3 text-sm text-red-700 dark:bg-red-400/10 dark:text-red-300">
          {actionError}
        </div>
      )}

      {loading && (
        <div className="rounded-3xl bg-stone-100 p-8 text-center text-sm text-stone-500 dark:bg-white/[0.045] dark:text-stone-400">
          Loading shopping list...
        </div>
      )}

      {!loading && error && (
        <div className="rounded-3xl bg-stone-100 p-8 text-center dark:bg-white/[0.045]">
          <p className="font-medium text-stone-900 dark:text-stone-100">Couldn't load shopping</p>
          <p className="mt-1 text-sm text-stone-500 dark:text-stone-400">Check that the Nook API is running.</p>

          <button type="button" onClick={() => {
            setLoading(true);
            void loadShoppingList();
          }} className="mt-4 rounded-full bg-emerald-900 px-5 py-2.5 text-sm font-medium text-white dark:bg-emerald-700">
            Try again
          </button>
        </div>
      )}

      {!loading && !error && shoppingList && (
        <>
          <section>
            <div className="mb-4 flex items-center justify-between">
              <h2 className="text-lg font-semibold text-stone-900 dark:text-stone-100">To buy</h2>

              {shoppingList.itemCount > 0 && (
                <span className="rounded-full bg-stone-100 px-3 py-1 text-xs font-medium text-stone-500 dark:bg-white/[0.06] dark:text-stone-400">
                  {shoppingList.itemCount}
                </span>
              )}
            </div>

            {shoppingList.items.length === 0 ? (
              <div className="rounded-3xl bg-emerald-50 px-6 py-10 text-center dark:bg-emerald-400/[0.07] dark:ring-1 dark:ring-emerald-400/10">
                <div className="mx-auto flex h-12 w-12 items-center justify-center rounded-full bg-emerald-100 text-emerald-800 dark:bg-emerald-400/10 dark:text-emerald-400">
                  <CheckCircle2 size={24} />
                </div>

                <p className="mt-4 font-medium text-stone-900 dark:text-stone-100">All done</p>
                <p className="mt-1 text-sm text-stone-500 dark:text-stone-400">There's nothing left to buy.</p>
              </div>
            ) : (
              <div className="space-y-6">
                {activeGroups.map((group: ShoppingGroup) => (
                  <div key={group.category}>
                    <div className="mb-2 flex items-center gap-2 px-1">
                      <h3 className="text-xs font-semibold uppercase tracking-wide text-stone-400 dark:text-stone-500">{group.category}</h3>
                      <span className="text-xs text-stone-300 dark:text-stone-600">{group.items.length}</span>
                    </div>

                    <div className="overflow-hidden rounded-3xl border border-stone-200 bg-white dark:border-white/[0.07] dark:bg-white/[0.035]">
                      <div className="divide-y divide-stone-100 dark:divide-white/[0.06]">
                        {group.items.map((item) => (
                          <ShoppingItemRow
                            key={item.id}
                            item={item}
                            busy={completingIds.includes(item.id)}
                            onComplete={() => void completeItem(item.id)}
                          />
                        ))}
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </section>

          {shoppingList.completedItems.length > 0 && (
            <section className="mt-8">
              <div className="mb-3 flex items-center justify-between">
                <div>
                  <h2 className="text-lg font-semibold text-stone-900 dark:text-stone-100">Completed</h2>
                  <p className="mt-0.5 text-xs text-stone-400 dark:text-stone-500">Tap undo if you checked something by mistake.</p>
                </div>

                <button type="button" disabled={clearingCompleted} onClick={() => void clearCompleted()} className="flex items-center gap-1.5 rounded-full px-3 py-2 text-xs font-medium text-red-600 transition hover:bg-red-50 disabled:opacity-40 dark:text-red-400 dark:hover:bg-red-400/10">
                  <Trash2 size={14} />
                  {clearingCompleted ? "Clearing..." : "Clear"}
                </button>
              </div>

              <div className="overflow-hidden rounded-3xl bg-stone-100 dark:bg-white/[0.025] dark:ring-1 dark:ring-white/[0.04]">
                <div className="divide-y divide-stone-200/70 dark:divide-white/[0.05]">
                  {shoppingList.completedItems.map((item) => (
                    <ShoppingItemRow
                      key={item.id}
                      item={item}
                      completed
                      busy={restoringIds.includes(item.id)}
                      onRestore={() => void restoreItem(item.id)}
                    />
                  ))}
                </div>
              </div>
            </section>
          )}
        </>
      )}

      {addItemOpen && (
        <AddShoppingItemModal
          onClose={() => setAddItemOpen(false)}
          onSubmit={addManualItem}
        />
      )}
    </main>
  );
}

export default ShoppingPage;