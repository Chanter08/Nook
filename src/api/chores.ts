import { apiRequest } from "@/api/client";
import type { Chore, SaveChorePayload } from "@/types/chore";

export function getChores() {
  return apiRequest<Chore[]>("/api/chores");
}

export function addChore(payload: SaveChorePayload) {
  return apiRequest<Chore>("/api/chores", { method: "POST", body: JSON.stringify(payload) });
}

export function updateChore(id: number, payload: SaveChorePayload) {
  return apiRequest<Chore>("/api/chores/" + id, { method: "PUT", body: JSON.stringify(payload) });
}

export function completeChore(id: number) {
  return apiRequest<Chore>("/api/chores/" + id + "/complete", { method: "PATCH" });
}

export function restoreChore(id: number) {
  return apiRequest<Chore>("/api/chores/" + id + "/restore", { method: "PATCH" });
}

export function deleteChore(id: number) {
  return apiRequest<void>("/api/chores/" + id, { method: "DELETE" });
}
