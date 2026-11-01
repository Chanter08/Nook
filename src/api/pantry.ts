import { apiRequest } from "@/api/client";
import type {
  PantryItem,
  PantryItemPayload,
  PantryResponse,
  ReceiptImportPayload,
  ReceiptImportResult,
  ReceiptScanResponse,
} from "@/types/pantry";

export function getPantry() {
  return apiRequest<PantryResponse>("/api/pantry");
}

export function addPantryItem(payload: PantryItemPayload) {
  return apiRequest<PantryItem>("/api/pantry/items", {
    method: "POST",
    body: JSON.stringify(payload),
  });
}

export function updatePantryItem(id: number, payload: PantryItemPayload) {
  return apiRequest<PantryItem>(`/api/pantry/items/${id}`, {
    method: "PUT",
    body: JSON.stringify(payload),
  });
}

export function deletePantryItem(id: number) {
  return apiRequest<void>(`/api/pantry/items/${id}`, {
    method: "DELETE",
  });
}

export function scanReceipt(file: File) {
  const form = new FormData();
  form.append("receipt", file);

  return apiRequest<ReceiptScanResponse>("/api/pantry/receipt/scan", {
    method: "POST",
    body: form,
  });
}

export function importReceipt(payload: ReceiptImportPayload) {
  return apiRequest<ReceiptImportResult>("/api/pantry/receipt/import", {
    method: "POST",
    body: JSON.stringify(payload),
  });
}
