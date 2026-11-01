export type PantryLocation = "Pantry" | "Fridge" | "Freezer" | "Other";

export interface PantryItem {
  id: number;
  name: string;
  quantity: number;
  unit: string | null;
  location: PantryLocation;
  expiryDate: string | null;
  addedDate: string;
  updatedDate: string;
}

export interface SavePantryItemPayload {
  name: string;
  quantity: number;
  unit: string | null;
  location: PantryLocation;
  expiryDate: string | null;
}
