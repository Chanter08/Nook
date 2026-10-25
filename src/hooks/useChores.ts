import { useCallback, useEffect, useState } from "react";
import { addChore, completeChore, deleteChore, getChores, restoreChore } from "@/api/chores";
import type { Chore, SaveChorePayload } from "@/types/chore";

export function useChores() {
  const [chores, setChores] = useState<Chore[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(false);
  const [actionError, setActionError] = useState<string | null>(null);
  const [busyIds, setBusyIds] = useState<number[]>([]);

  const refresh = useCallback(async () => {
    try {
      setLoading(true);
      setError(false);
      setChores(await getChores());
    } catch (error) {
      console.error("Chores error:", error);
      setError(true);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    void refresh();
  }, [refresh]);

  async function createChore(payload: SaveChorePayload) {
    setActionError(null);
    try {
      const created = await addChore(payload);
      setChores((current) => [...current, created]);
    } catch (error) {
      console.error("Create chore error:", error);
      setActionError("Couldn't add that chore.");
      throw error;
    }
  }

  async function complete(id: number) {
    if (busyIds.includes(id)) return;
    setActionError(null);
    setBusyIds((current) => [...current, id]);

    try {
      const updated = await completeChore(id);
      setChores((current) => current.map((chore) => chore.id === id ? updated : chore));
    } catch (error) {
      console.error("Complete chore error:", error);
      setActionError("Couldn't complete that chore.");
    } finally {
      setBusyIds((current) => current.filter((currentId) => currentId !== id));
    }
  }

  async function restore(id: number) {
    if (busyIds.includes(id)) return;
    setActionError(null);
    setBusyIds((current) => [...current, id]);

    try {
      const updated = await restoreChore(id);
      setChores((current) => current.map((chore) => chore.id === id ? updated : chore));
    } catch (error) {
      console.error("Restore chore error:", error);
      setActionError("Couldn't restore that chore.");
    } finally {
      setBusyIds((current) => current.filter((currentId) => currentId !== id));
    }
  }

  async function remove(id: number) {
    if (busyIds.includes(id)) return;
    setActionError(null);
    setBusyIds((current) => [...current, id]);

    try {
      await deleteChore(id);
      setChores((current) => current.filter((chore) => chore.id !== id));
    } catch (error) {
      console.error("Delete chore error:", error);
      setActionError("Couldn't delete that chore.");
    } finally {
      setBusyIds((current) => current.filter((currentId) => currentId !== id));
    }
  }

  return { chores, loading, error, actionError, busyIds, refresh, createChore, complete, restore, remove };
}
