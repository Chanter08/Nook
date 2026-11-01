export type PantryStorageLocation =
  | "Pantry"
  | "Fridge"
  | "Freezer"
  | "Cleaning"
  | "Bathroom"
  | "Pet"
  | "Medicine"
  | "Other";

export interface PantryItem {
  id: number;
  shoppingCatalogItemId: number | null;
  name: string;
  quantity: number | null;
  unit: string | null;
  category: string | null;
  storageLocation: PantryStorageLocation;
  expiryDate: string | null;
  lowStockThreshold: number | null;
  notes: string | null;
  source: "manual" | "receipt" | string;
  lastPurchasedDate: string | null;
  isLowStock: boolean;
  isExpiringSoon: boolean;
}

export interface PantryResponse {
  itemCount: number;
  lowStockCount: number;
  expiringSoonCount: number;
  items: PantryItem[];
}

export interface PantryItemPayload {
  shoppingCatalogItemId: number | null;
  name: string;
  quantity: number | null;
  unit: string | null;
  category: string | null;
  storageLocation: PantryStorageLocation;
  expiryDate: string | null;
  lowStockThreshold: number | null;
  notes: string | null;
}

export interface ReceiptScanItem {
  receiptText: string;
  name: string;
  quantity: number;
  unit: string | null;
  category: string;
  storageLocation: PantryStorageLocation;
  price: number | null;
  confidence: number;
  isLikelyInventoryItem: boolean;
}

export interface ReceiptScanResponse {
  storeName: string | null;
  purchaseDate: string | null;
  currency: string | null;
  items: ReceiptScanItem[];
}

export interface ReceiptImportPayload {
  purchaseDate: string | null;
  items: Array<{
    name: string;
    quantity: number | null;
    unit: string | null;
    category: string | null;
    storageLocation: PantryStorageLocation;
    notes: string | null;
  }>;
}

export interface ReceiptImportResult {
  imported: number;
  merged: number;
  shoppingItemsCompleted: number;
}
