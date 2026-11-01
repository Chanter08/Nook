import { useCallback, useEffect, useState } from "react";
import { addPantryItem, bulkAddPantryItems, deletePantryItem, getPantryItems, updatePantryItem } from "@/api/pantry";
import type { PantryItem, SavePantryItemPayload } from "@/types/pantry";

export function usePantry() {
  const [items, setItems] = useState<PantryItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(false);
  const [actionError, setActionError] = useState<string | null>(null);
  const [busyIds, setBusyIds] = useState<number[]>([]);

  const refresh = useCallback(async () => {
    try {
      setLoading(true);
      setError(false);
      setItems(await getPantryItems());
    } catch (error) {
      console.error("Pantry error:", error);
      setError(true);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    void refresh();
  }, [refresh]);

  async function create(payload: SavePantryItemPayload) {
    setActionError(null);
    try {
      const created = await addPantryItem(payload);
      setItems((current) => [...current, created]);
    } catch (error) {
      console.error("Add pantry item error:", error);
      setActionError("Couldn't add that item.");
      throw error;
    }
  }

  async function createMany(payloads: SavePantryItemPayload[]) {
    setActionError(null);
    try {
      const created = await bulkAddPantryItems(payloads);
      setItems((current) => [...current, ...created]);
      return created.length;
    } catch (error) {
      console.error("Receipt import error:", error);
      setActionError("Couldn't add the receipt items.");
      throw error;
    }
  }

  async function update(id: number, payload: SavePantryItemPayload) {
    if (busyIds.includes(id)) return;

    setActionError(null);
    setBusyIds((current) => [...current, id]);

    try {
      const updated = await updatePantryItem(id, payload);
      setItems((current) => current.map((item) => item.id === id ? updated : item));
    } catch (error) {
      console.error("Update pantry item error:", error);
      setActionError("Couldn't update that item.");
      throw error;
    } finally {
      setBusyIds((current) => current.filter((currentId) => currentId !== id));
    }
  }

  async function remove(id: number) {
    if (busyIds.includes(id)) return;

    setActionError(null);
    setBusyIds((current) => [...current, id]);

    try {
      await deletePantryItem(id);
      setItems((current) => current.filter((item) => item.id !== id));
    } catch (error) {
      console.error("Delete pantry item error:", error);
      setActionError("Couldn't delete that item.");
    } finally {
      setBusyIds((current) => current.filter((currentId) => currentId !== id));
    }
  }

  return { items, loading, error, actionError, busyIds, refresh, create, createMany, update, remove };
}
