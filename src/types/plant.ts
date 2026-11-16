export interface PlantSummary {
  id: number;
  name: string;
  species: string | null;
  variety: string | null;
  location: string | null;
  acquiredDate: string | null;
  notes: string | null;
  coverImageUrl: string | null;
  createdAtUtc: string;
  lastObservedAtUtc: string | null;
  lastWateredAtUtc: string | null;
}

export interface Plant extends Omit<PlantSummary, "lastObservedAtUtc" | "lastWateredAtUtc"> {}

export interface PlantObservation {
  id: number;
  observedAtUtc: string;
  notes: string | null;
  imageUrl: string | null;
  issueSummary: string | null;
}

export interface PlantCareEvent {
  id: number;
  eventType: "Watered" | "Fed" | "Pruned" | "Repotted" | "Treated" | "Moved" | "Other" | string;
  occurredAtUtc: string;
  notes: string | null;
}

export interface PlantDetailResponse {
  plant: Plant;
  observations: PlantObservation[];
  careEvents: PlantCareEvent[];
}

export interface PlantPayload {
  name: string;
  species: string | null;
  variety: string | null;
  location: string | null;
  acquiredDate: string | null;
  notes: string | null;
}
