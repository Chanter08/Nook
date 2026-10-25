export type ChoreRecurrence = "None" | "Daily" | "Weekly" | "Monthly";

export interface Chore {
  id: number;
  title: string;
  assignedTo: string | null;
  dueDate: string | null;
  recurrence: ChoreRecurrence;
  isCompleted: boolean;
  createdDate: string;
  lastCompletedDate: string | null;
}

export interface SaveChorePayload {
  title: string;
  assignedTo: string | null;
  dueDate: string | null;
  recurrence: ChoreRecurrence;
}
