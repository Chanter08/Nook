import { useEffect, useMemo, useState } from "react";
import {
  Camera,
  Check,
  FileImage,
  LoaderCircle,
  ScanLine,
  X,
} from "lucide-react";
import { importReceipt, scanReceipt } from "@/api/pantry";
import type {
  PantryStorageLocation,
  ReceiptImportResult,
  ReceiptScanItem,
  ReceiptScanResponse,
} from "@/types/pantry";

interface ReceiptScanModalProps {
  onClose: () => void;
  onImported: (result: ReceiptImportResult) => Promise<void> | void;
}

interface EditableReceiptItem extends ReceiptScanItem {
  selected: boolean;
}

const categories = [
  "Groceries",
  "Household",
  "Cleaning",
  "Toiletries",
  "Pets",
  "Pharmacy",
  "Other",
];

const locations: PantryStorageLocation[] = [
  "Pantry",
  "Fridge",
  "Freezer",
  "Cleaning",
  "Bathroom",
  "Pet",
  "Medicine",
  "Other",
];

function ReceiptScanModal({
  onClose,
  onImported,
}: ReceiptScanModalProps) {
  const [scan, setScan] = useState<ReceiptScanResponse | null>(null);
  const [items, setItems] = useState<EditableReceiptItem[]>([]);
  const [scanning, setScanning] = useState(false);
  const [importing, setImporting] = useState(false);
  const [fileName, setFileName] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  const selectedCount = useMemo(
    () => items.filter((item) => item.selected && item.name.trim()).length,
    [items],
  );

  useEffect(() => {
    function handleKeyDown(event: KeyboardEvent) {
      if (event.key === "Escape" && !scanning && !importing) onClose();
    }

    document.body.style.overflow = "hidden";
    window.addEventListener("keydown", handleKeyDown);

    return () => {
      document.body.style.overflow = "";
      window.removeEventListener("keydown", handleKeyDown);
    };
  }, [importing, onClose, scanning]);

  async function handleFile(file: File | undefined) {
    if (!file) return;

    try {
      setScanning(true);
      setError(null);
      setFileName(file.name);

      const result = await scanReceipt(file);
      setScan(result);
      setItems(
        result.items.map((item) => ({
          ...item,
          selected: item.isLikelyInventoryItem,
        })),
      );
    } catch (caught) {
      console.error(caught);
      setError(
        caught instanceof Error
          ? caught.message
          : "Couldn't scan this receipt.",
      );
    } finally {
      setScanning(false);
    }
  }

  function patchItem(index: number, patch: Partial<EditableReceiptItem>) {
    setItems((current) =>
      current.map((item, itemIndex) =>
        itemIndex === index ? { ...item, ...patch } : item,
      ),
    );
  }

  async function submitImport() {
    const selected = items.filter(
      (item) => item.selected && item.name.trim(),
    );

    if (selected.length === 0 || importing) return;

    try {
      setImporting(true);
      setError(null);

      const result = await importReceipt({
        purchaseDate: scan?.purchaseDate ?? null,
        items: selected.map((item) => ({
          name: item.name.trim(),
          quantity:
            Number.isFinite(item.quantity) && item.quantity >= 0
              ? item.quantity
              : null,
          unit: item.unit?.trim() || null,
          category: item.category || null,
          storageLocation: item.storageLocation,
          notes:
            item.receiptText && item.receiptText !== item.name
              ? `Receipt: ${item.receiptText}`
              : null,
        })),
      });

      await onImported(result);
      onClose();
    } catch (caught) {
      console.error(caught);
      setError(
        caught instanceof Error
          ? caught.message
          : "Couldn't import the selected items.",
      );
    } finally {
      setImporting(false);
    }
  }

  return (
    <div
      className="fixed inset-0 z-[100] flex items-end justify-center bg-black/45 backdrop-blur-sm sm:items-center sm:p-4"
      onClick={() => {
        if (!scanning && !importing) onClose();
      }}
    >
      <div
        role="dialog"
        aria-modal="true"
        className="w-full max-w-2xl rounded-t-[2rem] bg-white shadow-2xl dark:bg-[#202521] dark:ring-1 dark:ring-white/[0.08] sm:rounded-3xl"
        onClick={(event) => event.stopPropagation()}
      >
        <div className="flex items-center justify-between border-b border-stone-100 px-5 py-4 dark:border-white/[0.06]">
          <div>
            <h2 className="text-lg font-semibold text-stone-900 dark:text-stone-100">
              Scan receipt
            </h2>
            <p className="mt-0.5 text-sm text-stone-500 dark:text-stone-400">
              Review detected items before anything is added
            </p>
          </div>

          <button
            type="button"
            aria-label="Close"
            disabled={scanning || importing}
            onClick={onClose}
            className="flex h-9 w-9 items-center justify-center rounded-full bg-stone-100 text-stone-600 disabled:opacity-40 dark:bg-white/[0.06] dark:text-stone-300"
          >
            <X size={18} />
          </button>
        </div>

        <div className="max-h-[72dvh] overflow-y-auto px-5 py-5">
          {!scan && (
            <div className="rounded-3xl border border-dashed border-stone-300 bg-stone-50 p-7 text-center dark:border-white/[0.12] dark:bg-white/[0.035]">
              <div className="mx-auto flex h-14 w-14 items-center justify-center rounded-full bg-emerald-100 text-emerald-800 dark:bg-emerald-400/10 dark:text-emerald-400">
                {scanning ? (
                  <LoaderCircle size={26} className="animate-spin" />
                ) : (
                  <Camera size={26} />
                )}
              </div>

              <p className="mt-4 font-medium text-stone-900 dark:text-stone-100">
                {scanning ? "Reading your receipt..." : "Take or choose a photo"}
              </p>
              <p className="mx-auto mt-1 max-w-sm text-sm text-stone-500 dark:text-stone-400">
                JPEG, PNG, or WebP works best. Keep the full receipt flat and readable.
              </p>

              <label className="mx-auto mt-5 inline-flex h-11 cursor-pointer items-center gap-2 rounded-full bg-emerald-900 px-5 text-sm font-medium text-white dark:bg-emerald-700">
                <FileImage size={17} />
                {fileName ? "Choose another" : "Choose receipt"}
                <input
                  type="file"
                  accept="image/jpeg,image/png,image/webp"
                  capture="environment"
                  disabled={scanning}
                  className="sr-only"
                  onChange={(event) =>
                    void handleFile(event.target.files?.[0])
                  }
                />
              </label>
            </div>
          )}

          {scan && (
            <>
              <div className="mb-5 flex flex-wrap items-center gap-2">
                <div className="flex items-center gap-2 rounded-full bg-emerald-50 px-3 py-1.5 text-xs font-medium text-emerald-800 dark:bg-emerald-400/10 dark:text-emerald-300">
                  <ScanLine size={14} />
                  {scan.storeName ?? "Receipt"}
                </div>
                {scan.purchaseDate && (
                  <span className="rounded-full bg-stone-100 px-3 py-1.5 text-xs text-stone-500 dark:bg-white/[0.06] dark:text-stone-400">
                    {scan.purchaseDate}
                  </span>
                )}
                <label className="ml-auto cursor-pointer text-xs font-medium text-emerald-800 dark:text-emerald-400">
                  Scan another
                  <input
                    type="file"
                    accept="image/jpeg,image/png,image/webp"
                    capture="environment"
                    className="sr-only"
                    onChange={(event) =>
                      void handleFile(event.target.files?.[0])
                    }
                  />
                </label>
              </div>

              {items.length === 0 ? (
                <div className="rounded-3xl bg-stone-100 p-8 text-center text-sm text-stone-500 dark:bg-white/[0.04] dark:text-stone-400">
                  No product lines were detected. Try a clearer photo.
                </div>
              ) : (
                <div className="space-y-3">
                  {items.map((item, index) => (
                    <div
                      key={`${item.receiptText}-${index}`}
                      className={`rounded-3xl border p-4 transition ${
                        item.selected
                          ? "border-emerald-200 bg-emerald-50/40 dark:border-emerald-400/20 dark:bg-emerald-400/[0.05]"
                          : "border-stone-200 bg-stone-50 opacity-70 dark:border-white/[0.07] dark:bg-white/[0.025]"
                      }`}
                    >
                      <div className="flex items-start gap-3">
                        <button
                          type="button"
                          aria-label={
                            item.selected ? "Exclude item" : "Include item"
                          }
                          onClick={() =>
                            patchItem(index, { selected: !item.selected })
                          }
                          className={`mt-1 flex h-6 w-6 shrink-0 items-center justify-center rounded-full border ${
                            item.selected
                              ? "border-emerald-700 bg-emerald-700 text-white"
                              : "border-stone-300 text-transparent dark:border-stone-600"
                          }`}
                        >
                          <Check size={14} />
                        </button>

                        <div className="min-w-0 flex-1">
                          <div className="flex items-center gap-2">
                            <input
                              value={item.name}
                              onChange={(event) =>
                                patchItem(index, { name: event.target.value })
                              }
                              className="min-w-0 flex-1 border-0 bg-transparent p-0 font-medium text-stone-900 outline-none dark:text-stone-100"
                            />
                            {item.confidence < 0.7 && (
                              <span className="shrink-0 rounded-full bg-amber-100 px-2 py-0.5 text-[10px] font-semibold text-amber-800 dark:bg-amber-400/10 dark:text-amber-300">
                                Check
                              </span>
                            )}
                          </div>

                          <p className="mt-1 truncate text-xs text-stone-400 dark:text-stone-500">
                            {item.receiptText}
                          </p>

                          <div className="mt-3 grid grid-cols-2 gap-2 sm:grid-cols-4">
                            <input
                              type="number"
                              min="0"
                              step="any"
                              value={item.quantity}
                              onChange={(event) =>
                                patchItem(index, {
                                  quantity: Number(event.target.value),
                                })
                              }
                              aria-label="Quantity"
                              className="h-10 rounded-xl border border-stone-200 bg-white px-3 text-xs dark:border-white/[0.08] dark:bg-white/[0.05] dark:text-stone-100"
                            />
                            <input
                              value={item.unit ?? ""}
                              onChange={(event) =>
                                patchItem(index, {
                                  unit: event.target.value || null,
                                })
                              }
                              placeholder="Unit"
                              aria-label="Unit"
                              className="h-10 rounded-xl border border-stone-200 bg-white px-3 text-xs dark:border-white/[0.08] dark:bg-white/[0.05] dark:text-stone-100"
                            />
                            <select
                              value={item.category}
                              onChange={(event) =>
                                patchItem(index, {
                                  category: event.target.value,
                                })
                              }
                              aria-label="Category"
                              className="h-10 rounded-xl border border-stone-200 bg-white px-2 text-xs dark:border-white/[0.08] dark:bg-[#292e2a] dark:text-stone-100"
                            >
                              {categories.map((value) => (
                                <option key={value}>{value}</option>
                              ))}
                            </select>
                            <select
                              value={item.storageLocation}
                              onChange={(event) =>
                                patchItem(index, {
                                  storageLocation:
                                    event.target.value as PantryStorageLocation,
                                })
                              }
                              aria-label="Storage location"
                              className="h-10 rounded-xl border border-stone-200 bg-white px-2 text-xs dark:border-white/[0.08] dark:bg-[#292e2a] dark:text-stone-100"
                            >
                              {locations.map((value) => (
                                <option key={value}>{value}</option>
                              ))}
                            </select>
                          </div>
                        </div>
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </>
          )}

          {error && (
            <div className="mt-4 rounded-2xl bg-red-50 px-4 py-3 text-sm text-red-700 dark:bg-red-400/10 dark:text-red-300">
              {error}
            </div>
          )}
        </div>

        {scan && (
          <div className="border-t border-stone-100 p-4 dark:border-white/[0.06]">
            <button
              type="button"
              disabled={selectedCount === 0 || importing || scanning}
              onClick={() => void submitImport()}
              className="flex h-12 w-full items-center justify-center gap-2 rounded-2xl bg-emerald-900 text-sm font-medium text-white transition hover:bg-emerald-800 disabled:opacity-40 dark:bg-emerald-700 dark:hover:bg-emerald-600"
            >
              {importing ? (
                <LoaderCircle size={17} className="animate-spin" />
              ) : (
                <Check size={17} />
              )}
              {importing
                ? "Adding..."
                : `Add ${selectedCount} ${selectedCount === 1 ? "item" : "items"} to pantry`}
            </button>
          </div>
        )}
      </div>
    </div>
  );
}

export default ReceiptScanModal;
