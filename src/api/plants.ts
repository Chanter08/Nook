import { apiRequest } from "@/api/client";
import type { GardenWeatherContext, PlantAnalysis, PlantCareAdvice, PlantDetailResponse, PlantPayload, PlantSummary } from "@/types/plant";

export const getPlants = () => apiRequest<PlantSummary[]>("/api/plants");
export const getPlant = (id: number) => apiRequest<PlantDetailResponse>(`/api/plants/${id}`);

export const createPlant = (payload: PlantPayload) =>
  apiRequest<{ id: number }>("/api/plants", { method: "POST", body: JSON.stringify(payload) });

export const updatePlant = (id: number, payload: PlantPayload) =>
  apiRequest<void>(`/api/plants/${id}`, { method: "PUT", body: JSON.stringify(payload) });

export const deletePlant = (id: number) =>
  apiRequest<void>(`/api/plants/${id}`, { method: "DELETE" });

export function addPlantObservation(id: number, payload: { notes: string; issueSummary: string; image: File | null }) {
  const form = new FormData();
  form.append("observedAtUtc", new Date().toISOString());
  form.append("notes", payload.notes);
  form.append("issueSummary", payload.issueSummary);
  if (payload.image) form.append("image", payload.image);
  return apiRequest<{ id: number; imageUrl: string | null }>(`/api/plants/${id}/observations`, { method: "POST", body: form });
}

export const addPlantCareEvent = (id: number, payload: { eventType: string; notes: string | null }) =>
  apiRequest<{ id: number }>(`/api/plants/${id}/care`, {
    method: "POST",
    body: JSON.stringify({ ...payload, occurredAtUtc: new Date().toISOString() }),
  });

export function analyzePlantPhoto(image: File, plantName: string, species: string | null) {
  const form = new FormData();
  form.append("image", image);
  form.append("plantName", plantName);
  if (species) form.append("species", species);
  return apiRequest<PlantAnalysis>("/api/plants/analyze", { method: "POST", body: form });
}

export const getGardenWeather = () =>
  apiRequest<GardenWeatherContext>("/api/plants/weather");

export const getPlantAdvice = (id: number) =>
  apiRequest<PlantCareAdvice>(`/api/plants/${id}/advice`);
