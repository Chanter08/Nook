import { useCallback, useEffect, useState } from "react";
import type { ShoppingListResponse } from "@/types/shopping";

export function useShoppingList() {
  const [shoppingList, setShoppingList] = useState<ShoppingListResponse | null>(
    null,
  );
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(false);
  const [actionError, setActionError] = useState<string | null>(null);
  const [completingIds, setCompletingIds] = useState<number[]>([]);

  const refresh = useCallback(async () => {
    try {
      setLoading(true);
      setError(false);

      const response = await fetch("/api/shopping");

      if (!response.ok) {
        throw new Error(`Failed to load shopping list: ${response.status}`);
      }

      const data: ShoppingListResponse = await response.json();
      setShoppingList(data);
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

  async function completeItem(itemId: number) {
    if (completingIds.includes(itemId)) return;

    setActionError(null);
    setCompletingIds((current) => [...current, itemId]);

    try {
      const response = await fetch(`/api/shopping/items/${itemId}/complete`, {
        method: "PATCH",
      });

      if (!response.ok) {
        throw new Error(`Failed to complete shopping item: ${response.status}`);
      }

      await new Promise((resolve) => window.setTimeout(resolve, 300));

      setShoppingList((current) => {
        if (!current) return current;

        const remainingItems = current.items.filter(
          (item) => item.id !== itemId,
        );

        return {
          ...current,
          items: remainingItems,
          itemCount: remainingItems.length,
        };
      });
    } catch (error) {
      console.error("Shopping item error:", error);
      setActionError("Couldn't update that item. Please try again.");
    } finally {
      setCompletingIds((current) => current.filter((id) => id !== itemId));
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
    refresh,
    completeItem,
    clearActionError,
  };
}