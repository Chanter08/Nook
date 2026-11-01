import { useCallback, useEffect, useMemo, useState } from "react";
import {
  AlertTriangle,
  ArrowLeft,
  Camera,
  Clock3,
  Package,
  Pencil,
  Plus,
  Search,
  ShoppingCart,
  Trash2,
} from "lucide-react";
import { useNavigate } from "react-router-dom";
import ThemeToggle from "@/components/ThemeToggle";
import PantryItemModal from "@/components/pantry/PantryItemModal";
import ReceiptScanModal from "@/components/pantry/ReceiptScanModal";
import {
  addPantryItem,
  deletePantryItem,
  getPantry,
  updatePantryItem,
} from "@/api/pantry";
import { addShoppingItem } from "@/api/shopping";
import type {
  PantryItem,
  PantryItemPayload,
  PantryResponse,
  PantryStorageLocation,
  ReceiptImportResult,
} from "@/types/pantry";

const locations: Array<PantryStorageLocation | "All"> = [
  "All",
  "Pantry",
  "Fridge",
  "Freezer",
  "Cleaning",
  "Bathroom",
  "Pet",
  "Medicine",
  "Other",
];

const locationOrder: PantryStorageLocation[] = [
  "Pantry",
  "Fridge",
  "Freezer",
  "Cleaning",
  "Bathroom",
  "Pet",
  "Medicine",
  "Other",
];

function formatQuantity(item: PantryItem) {
  if (item.quantity === null) return "In stock";

  if (item.unit === "g" && item.quantity >= 1000) {
    return `${item.quantity / 1000} kg`;
  }

  if (item.unit === "ml" && item.quantity >= 1000) {
    return `${item.quantity / 1000} L`;
  }

  return `${item.quantity}${item.unit ? ` ${item.unit}` : ""}`;
}

function formatDate(value: string) {
  return new Intl.DateTimeFormat(undefined, {
    day: "numeric",
    month: "short",
    year: "numeric",
  }).format(new Date(`${value}T12:00:00`));
}

function PantryPage() {
  const navigate = useNavigate();
  const [pantry, setPantry] = useState<PantryResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const [loadError, setLoadError] = useState<string | null>(null);
  const [actionError, setActionError] = useState<string | null>(null);
  const [status, setStatus] = useState<string | null>(null);
  const [query, setQuery] = useState("");
  const [location, setLocation] =
    useState<PantryStorageLocation | "All">("All");
  const [itemModalOpen, setItemModalOpen] = useState(false);
  const [editingItem, setEditingItem] = useState<PantryItem | null>(null);
  const [receiptOpen, setReceiptOpen] = useState(false);
  const [deletingIds, setDeletingIds] = useState<number[]>([]);
  const [shoppingIds, setShoppingIds] = useState<number[]>([]);

  const refresh = useCallback(async () => {
    try {
      setLoadError(null);
      const data = await getPantry();
      setPantry(data);
    } catch (caught) {
      console.error(caught);
      setLoadError(
        caught instanceof Error ? caught.message : "Couldn't load the pantry.",
      );
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    void refresh();
  }, [refresh]);

  const filteredItems = useMemo(() => {
    const normalizedQuery = query.trim().toLowerCase();

    return (pantry?.items ?? []).filter((item) => {
      if (location !== "All" && item.storageLocation !== location) {
        return false;
      }

      if (!normalizedQuery) return true;

      return [item.name, item.category, item.notes]
        .filter(Boolean)
        .some((value) => value!.toLowerCase().includes(normalizedQuery));
    });
  }, [location, pantry?.items, query]);

  const groups = useMemo(() => {
    const map = new Map<PantryStorageLocation, PantryItem[]>();

    for (const item of filteredItems) {
      const current = map.get(item.storageLocation) ?? [];
      current.push(item);
      map.set(item.storageLocation, current);
    }

    return Array.from(map.entries())
      .map(([storageLocation, items]) => ({
        storageLocation,
        items: items.sort((a, b) => a.name.localeCompare(b.name)),
      }))
      .sort(
        (a, b) =>
          locationOrder.indexOf(a.storageLocation) -
          locationOrder.indexOf(b.storageLocation),
      );
  }, [filteredItems]);

  async function saveItem(payload: PantryItemPayload) {
    setActionError(null);
    setStatus(null);

    if (editingItem) {
      await updatePantryItem(editingItem.id, payload);
      setStatus(`${payload.name} updated.`);
    } else {
      await addPantryItem(payload);
      setStatus(`${payload.name} added to pantry.`);
    }

    await refresh();
  }

  async function removeItem(item: PantryItem) {
    if (!window.confirm(`Remove ${item.name} from the pantry?`)) return;

    try {
      setDeletingIds((current) => [...current, item.id]);
      setActionError(null);
      setStatus(null);
      await deletePantryItem(item.id);
      setStatus(`${item.name} removed.`);
      await refresh();
    } catch (caught) {
      console.error(caught);
      setActionError(
        caught instanceof Error ? caught.message : "Couldn't remove the item.",
      );
    } finally {
      setDeletingIds((current) => current.filter((id) => id !== item.id));
    }
  }

  async function sendToShopping(item: PantryItem) {
    try {
      setShoppingIds((current) => [...current, item.id]);
      setActionError(null);
      setStatus(null);

      await addShoppingItem({
        shoppingCatalogItemId: item.shoppingCatalogItemId,
        name: item.name,
        quantity: null,
        unit: item.unit,
        category: item.category ?? "Groceries",
        notes: item.notes,
      });

      setStatus(`${item.name} added to Shopping.`);
    } catch (caught) {
      console.error(caught);
      setActionError(
        caught instanceof Error
          ? caught.message
          : "Couldn't add the item to Shopping.",
      );
    } finally {
      setShoppingIds((current) => current.filter((id) => id !== item.id));
    }
  }

  async function handleReceiptImported(result: ReceiptImportResult) {
    const added = result.imported + result.merged;
    const shoppingMessage =
      result.shoppingItemsCompleted > 0
        ? ` ${result.shoppingItemsCompleted} matching Shopping ${result.shoppingItemsCompleted === 1 ? "item was" : "items were"} completed.`
        : "";

    setStatus(
      `${added} ${added === 1 ? "item" : "items"} added from the receipt.${shoppingMessage}`,
    );
    await refresh();
  }

  function openAdd() {
    setEditingItem(null);
    setItemModalOpen(true);
  }

  function openEdit(item: PantryItem) {
    setEditingItem(item);
    setItemModalOpen(true);
  }

  return (
    <main className="mx-auto min-h-dvh w-full max-w-4xl px-4 pb-28 pt-8 sm:px-6 sm:pt-12">
      <header className="mb-7">
        <div className="flex items-center justify-between">
          <button
            type="button"
            aria-label="Go back"
            onClick={() => navigate(-1)}
            className="flex h-11 w-11 items-center justify-center rounded-full bg-stone-100 text-stone-700 transition hover:bg-stone-200 dark:bg-white/[0.06] dark:text-stone-200"
          >
            <ArrowLeft size={20} />
          </button>
          <ThemeToggle />
        </div>

        <div className="mt-7 flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between">
          <div>
            <p className="text-sm font-medium text-emerald-800 dark:text-emerald-400">
              Our home
            </p>
            <h1 className="mt-1 text-3xl font-semibold tracking-tight text-stone-900 dark:text-stone-100">
              Pantry
            </h1>
            <p className="mt-1 text-sm text-stone-500 dark:text-stone-400">
              Inventory for the pantry, fridge, freezer and household cupboards
            </p>
          </div>

          <div className="flex gap-2">
            <button
              type="button"
              onClick={() => setReceiptOpen(true)}
              className="flex h-11 items-center gap-2 rounded-full bg-stone-100 px-4 text-sm font-medium text-stone-700 transition hover:bg-stone-200 dark:bg-white/[0.06] dark:text-stone-200"
            >
              <Camera size={17} />
              Scan receipt
            </button>
            <button
              type="button"
              onClick={openAdd}
              className="flex h-11 items-center gap-2 rounded-full bg-emerald-900 px-4 text-sm font-medium text-white transition hover:bg-emerald-800 dark:bg-emerald-700 dark:hover:bg-emerald-600"
            >
              <Plus size={17} />
              Add item
            </button>
          </div>
        </div>
      </header>

      {pantry && (
        <section className="mb-6 grid grid-cols-3 gap-3">
          <div className="rounded-3xl bg-stone-100 p-4 dark:bg-white/[0.045]">
            <Package size={20} className="text-stone-500 dark:text-stone-400" />
            <p className="mt-4 text-2xl font-semibold text-stone-900 dark:text-stone-100">
              {pantry.itemCount}
            </p>
            <p className="text-xs text-stone-500 dark:text-stone-400">
              Tracked
            </p>
          </div>
          <div className="rounded-3xl bg-amber-50 p-4 dark:bg-amber-400/[0.06]">
            <AlertTriangle size={20} className="text-amber-700 dark:text-amber-400" />
            <p className="mt-4 text-2xl font-semibold text-stone-900 dark:text-stone-100">
              {pantry.lowStockCount}
            </p>
            <p className="text-xs text-stone-500 dark:text-stone-400">
              Low stock
            </p>
          </div>
          <div className="rounded-3xl bg-rose-50 p-4 dark:bg-rose-400/[0.06]">
            <Clock3 size={20} className="text-rose-700 dark:text-rose-400" />
            <p className="mt-4 text-2xl font-semibold text-stone-900 dark:text-stone-100">
              {pantry.expiringSoonCount}
            </p>
            <p className="text-xs text-stone-500 dark:text-stone-400">
              Expiring soon
            </p>
          </div>
        </section>
      )}

      {(actionError || status) && (
        <div
          className={`mb-5 rounded-2xl px-4 py-3 text-sm ${
            actionError
              ? "bg-red-50 text-red-700 dark:bg-red-400/10 dark:text-red-300"
              : "bg-emerald-50 text-emerald-800 dark:bg-emerald-400/10 dark:text-emerald-300"
          }`}
        >
          {actionError ?? status}
        </div>
      )}

      <section className="mb-6 space-y-3">
        <div className="relative">
          <Search
            size={18}
            className="absolute left-4 top-1/2 -translate-y-1/2 text-stone-400"
          />
          <input
            value={query}
            onChange={(event) => setQuery(event.target.value)}
            placeholder="Search pantry..."
            className="h-12 w-full rounded-2xl border border-stone-200 bg-stone-50 pl-11 pr-4 text-sm outline-none focus:border-emerald-700 dark:border-white/[0.08] dark:bg-white/[0.045] dark:text-stone-100"
          />
        </div>

        <div className="flex gap-2 overflow-x-auto pb-1">
          {locations.map((value) => (
            <button
              key={value}
              type="button"
              onClick={() => setLocation(value)}
              className={`shrink-0 rounded-full px-3.5 py-2 text-xs font-medium transition ${
                location === value
                  ? "bg-emerald-900 text-white dark:bg-emerald-700"
                  : "bg-stone-100 text-stone-600 dark:bg-white/[0.055] dark:text-stone-300"
              }`}
            >
              {value}
            </button>
          ))}
        </div>
      </section>

      {loading && (
        <div className="rounded-3xl bg-stone-100 p-8 text-center text-sm text-stone-500 dark:bg-white/[0.045] dark:text-stone-400">
          Loading pantry...
        </div>
      )}

      {!loading && loadError && (
        <div className="rounded-3xl bg-stone-100 p-8 text-center dark:bg-white/[0.045]">
          <p className="font-medium text-stone-900 dark:text-stone-100">
            Couldn't load Pantry
          </p>
          <p className="mt-1 text-sm text-stone-500 dark:text-stone-400">
            {loadError}
          </p>
          <button
            type="button"
            onClick={() => void refresh()}
            className="mt-4 rounded-full bg-emerald-900 px-5 py-2.5 text-sm font-medium text-white dark:bg-emerald-700"
          >
            Try again
          </button>
        </div>
      )}

      {!loading && !loadError && pantry && groups.length === 0 && (
        <div className="rounded-3xl bg-emerald-50 px-6 py-10 text-center dark:bg-emerald-400/[0.06]">
          <Package
            size={26}
            className="mx-auto text-emerald-800 dark:text-emerald-400"
          />
          <p className="mt-4 font-medium text-stone-900 dark:text-stone-100">
            {pantry.itemCount === 0 ? "Your pantry is empty" : "No matches"}
          </p>
          <p className="mt-1 text-sm text-stone-500 dark:text-stone-400">
            {pantry.itemCount === 0
              ? "Add items manually or scan your next shopping receipt."
              : "Try another search or storage filter."}
          </p>
        </div>
      )}

      {!loading && !loadError && groups.length > 0 && (
        <div className="space-y-7">
          {groups.map((group) => (
            <section key={group.storageLocation}>
              <div className="mb-2 flex items-center gap-2 px-1">
                <h2 className="text-xs font-semibold uppercase tracking-wide text-stone-400 dark:text-stone-500">
                  {group.storageLocation}
                </h2>
                <span className="text-xs text-stone-300 dark:text-stone-600">
                  {group.items.length}
                </span>
              </div>

              <div className="overflow-hidden rounded-3xl border border-stone-200 bg-white dark:border-white/[0.07] dark:bg-white/[0.035]">
                <div className="divide-y divide-stone-100 dark:divide-white/[0.06]">
                  {group.items.map((item) => (
                    <article
                      key={item.id}
                      className="flex gap-3 px-4 py-4 sm:px-5"
                    >
                      <div className="min-w-0 flex-1">
                        <div className="flex flex-wrap items-center gap-2">
                          <h3 className="font-medium text-stone-900 dark:text-stone-100">
                            {item.name}
                          </h3>
                          {item.isLowStock && (
                            <span className="rounded-full bg-amber-100 px-2 py-0.5 text-[10px] font-semibold text-amber-800 dark:bg-amber-400/10 dark:text-amber-300">
                              Low
                            </span>
                          )}
                          {item.isExpiringSoon && (
                            <span className="rounded-full bg-rose-100 px-2 py-0.5 text-[10px] font-semibold text-rose-800 dark:bg-rose-400/10 dark:text-rose-300">
                              Expiring
                            </span>
                          )}
                        </div>

                        <p className="mt-1 text-sm text-stone-500 dark:text-stone-400">
                          {formatQuantity(item)}
                          {item.category ? ` · ${item.category}` : ""}
                        </p>

                        {(item.expiryDate || item.notes) && (
                          <p className="mt-1 text-xs text-stone-400 dark:text-stone-500">
                            {[
                              item.expiryDate
                                ? `Expires ${formatDate(item.expiryDate)}`
                                : null,
                              item.notes,
                            ]
                              .filter(Boolean)
                              .join(" · ")}
                          </p>
                        )}

                        {item.isLowStock && (
                          <button
                            type="button"
                            disabled={shoppingIds.includes(item.id)}
                            onClick={() => void sendToShopping(item)}
                            className="mt-3 inline-flex items-center gap-1.5 rounded-full bg-amber-50 px-3 py-1.5 text-xs font-medium text-amber-800 transition hover:bg-amber-100 disabled:opacity-40 dark:bg-amber-400/10 dark:text-amber-300"
                          >
                            <ShoppingCart size={13} />
                            {shoppingIds.includes(item.id)
                              ? "Adding..."
                              : "Add to Shopping"}
                          </button>
                        )}
                      </div>

                      <div className="flex shrink-0 items-start gap-1">
                        <button
                          type="button"
                          aria-label={`Edit ${item.name}`}
                          onClick={() => openEdit(item)}
                          className="flex h-9 w-9 items-center justify-center rounded-full text-stone-400 transition hover:bg-stone-100 hover:text-stone-700 dark:hover:bg-white/[0.06] dark:hover:text-stone-200"
                        >
                          <Pencil size={16} />
                        </button>
                        <button
                          type="button"
                          aria-label={`Remove ${item.name}`}
                          disabled={deletingIds.includes(item.id)}
                          onClick={() => void removeItem(item)}
                          className="flex h-9 w-9 items-center justify-center rounded-full text-stone-400 transition hover:bg-red-50 hover:text-red-600 disabled:opacity-30 dark:hover:bg-red-400/10 dark:hover:text-red-400"
                        >
                          <Trash2 size={16} />
                        </button>
                      </div>
                    </article>
                  ))}
                </div>
              </div>
            </section>
          ))}
        </div>
      )}

      {itemModalOpen && (
        <PantryItemModal
          item={editingItem}
          onClose={() => {
            setItemModalOpen(false);
            setEditingItem(null);
          }}
          onSubmit={saveItem}
        />
      )}

      {receiptOpen && (
        <ReceiptScanModal
          onClose={() => setReceiptOpen(false)}
          onImported={handleReceiptImported}
        />
      )}
    </main>
  );
}

export default PantryPage;
