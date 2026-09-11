import { useCallback, useEffect, useState } from "react";
import { addShoppingItem, clearCompletedShoppingItems, completeShoppingItem, getShoppingList, restoreShoppingItem } from "@/api/shopping";
import type { AddShoppingItemPayload, ShoppingListResponse } from "@/types/shopping";

export function useShoppingList() {
  const [shoppingList, setShoppingList] = useState<ShoppingListResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(false);
  const [actionError, setActionError] = useState<string | null>(null);
  const [completingIds, setCompletingIds] = useState<number[]>([]);
  const [restoringIds, setRestoringIds] = useState<number[]>([]);
  const [clearingCompleted, setClearingCompleted] = useState(false);

  const refresh = useCallback(async () => {
    try {
      setLoading(true);
      setError(false);

      const data = await getShoppingList();

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
    void refresh();
  }, [refresh]);

  async function addItem(item: AddShoppingItemPayload) {
    await addShoppingItem(item);
    await refresh();
  }

  async function completeItem(itemId: number) {
    if (completingIds.includes(itemId)) return;

    setActionError(null);
    setCompletingIds((current) => [...current, itemId]);

    try {
      await completeShoppingItem(itemId);

      await new Promise((resolve) => window.setTimeout(resolve, 250));

      setShoppingList((current) => {
        if (!current) return current;

        const item = current.items.find((item) => item.id === itemId);
        if (!item) return current;

        const remainingItems = current.items.filter((item) => item.id !== itemId);

        return {
          ...current,
          items: remainingItems,
          itemCount: remainingItems.length,
          completedItems: [item, ...current.completedItems],
          completedCount: current.completedItems.length + 1
        };
      });
    } catch (error) {
      console.error("Shopping item error:", error);
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
      await restoreShoppingItem(itemId);

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
      console.error("Shopping item error:", error);
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
      await clearCompletedShoppingItems();

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
      console.error("Shopping list error:", error);
      setActionError("Couldn't clear completed items.");
    } finally {
      setClearingCompleted(false);
    }
  }

  function clearActionError() {
    setActionError(null);
  }

  return {
    shoppingList,
    loading,
    error,
    actionError,
    completingIds,
    restoringIds,
    clearingCompleted,
    refresh,
    addItem,
    completeItem,
    restoreItem,
    clearCompleted,
    clearActionError
  };
}