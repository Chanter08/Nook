import { useRef, useState } from "react";
import { Camera, Check, LoaderCircle, ReceiptText, X } from "lucide-react";
import { parseReceiptText } from "@/lib/receiptParser";
import type { PantryLocation, SavePantryItemPayload } from "@/types/pantry";

interface Candidate {
  id: string;
  selected: boolean;
  name: string;
  quantity: number;
  location: PantryLocation;
}

interface ReceiptScanModalProps {
  onClose: () => void;
  onImport: (items: SavePantryItemPayload[]) => Promise<number>;
}

async function readReceipt(file: File, onProgress: (progress: number) => void) {
  const moduleUrl = "https://cdn.jsdelivr.net/npm/tesseract.js@5/dist/tesseract.esm.min.js";
  const tesseract = await import(/* @vite-ignore */ moduleUrl);
  const worker = await tesseract.createWorker("eng", 1, {
    logger: (message: { status?: string; progress?: number }) => {
      if (message.status === "recognizing text" && typeof message.progress === "number") {
        onProgress(message.progress);
      }
    }
  });

  try {
    const result = await worker.recognize(file);
    return result.data.text as string;
  } finally {
    await worker.terminate();
  }
}

function ReceiptScanModal({ onClose, onImport }: ReceiptScanModalProps) {
  const inputRef = useRef<HTMLInputElement>(null);
  const [scanning, setScanning] = useState(false);
  const [progress, setProgress] = useState(0);
  const [candidates, setCandidates] = useState<Candidate[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [importing, setImporting] = useState(false);

  async function scan(file: File) {
    setError(null);
    setCandidates([]);
    setProgress(0);
    setScanning(true);

    try {
      const text = await readReceipt(file, setProgress);
      const parsed = parseReceiptText(text);

      if (parsed.length === 0) {
        setError("I couldn't confidently find product lines. Try a clearer, flatter photo with the whole receipt visible.");
        return;
      }

      setCandidates(parsed.map((item) => ({ ...item, selected: true, location: "Pantry" as PantryLocation })));
    } catch (error) {
      console.error("Receipt OCR error:", error);
      setError("Receipt scanning failed. The image stays on this device; try another photo or add the items manually.");
    } finally {
      setScanning(false);
    }
  }

  function patchCandidate(id: string, patch: Partial<Candidate>) {
    setCandidates((current) => current.map((candidate) => candidate.id === id ? { ...candidate, ...patch } : candidate));
  }

  async function importSelected() {
    const selected = candidates.filter((candidate) => candidate.selected && candidate.name.trim());
    if (selected.length === 0 || importing) return;

    setImporting(true);
    setError(null);

    try {
      await onImport(selected.map((candidate) => ({
        name: candidate.name.trim(),
        quantity: candidate.quantity > 0 ? candidate.quantity : 1,
        unit: null,
        location: candidate.location,
        expiryDate: null
      })));
      onClose();
    } catch {
      setError("Nook couldn't save the selected items.");
    } finally {
      setImporting(false);
    }
  }

  return (
    <div className="fixed inset-0 z-[70] flex items-end justify-center bg-black/50 p-0 backdrop-blur-sm sm:items-center sm:p-6">
      <div className="max-h-[92dvh] w-full max-w-2xl overflow-hidden rounded-t-3xl bg-white shadow-2xl dark:bg-stone-900 sm:rounded-3xl">
        <header className="flex items-center justify-between border-b border-stone-100 px-5 py-4 dark:border-white/[0.06]"><div><p className="text-xs font-semibold uppercase tracking-wide text-emerald-700 dark:text-emerald-400">Pantry</p><h2 className="text-xl font-semibold text-stone-900 dark:text-stone-100">Scan receipt</h2></div><button type="button" onClick={onClose} aria-label="Close receipt scanner" className="flex h-10 w-10 items-center justify-center rounded-full bg-stone-100 text-stone-500 dark:bg-white/[0.06] dark:text-stone-300"><X size={18} /></button></header>

        <div className="max-h-[calc(92dvh-5rem)] overflow-y-auto p-5">
          <input ref={inputRef} type="file" accept="image/*" capture="environment" className="hidden" onChange={(event) => { const file = event.target.files?.[0]; if (file) void scan(file); event.currentTarget.value = ""; }} />

          {candidates.length === 0 && !scanning && <div className="rounded-3xl border border-dashed border-stone-300 px-6 py-10 text-center dark:border-white/[0.12]"><div className="mx-auto flex h-14 w-14 items-center justify-center rounded-full bg-emerald-50 text-emerald-800 dark:bg-emerald-400/10 dark:text-emerald-300"><ReceiptText size={26} /></div><p className="mt-4 font-medium text-stone-900 dark:text-stone-100">Photograph the full receipt</p><p className="mx-auto mt-2 max-w-md text-sm leading-relaxed text-stone-500 dark:text-stone-400">Keep it flat, well lit and fill most of the frame. OCR runs in your browser; the receipt image itself is not sent to Nook or stored.</p><button type="button" onClick={() => inputRef.current?.click()} className="mt-5 inline-flex items-center gap-2 rounded-full bg-emerald-900 px-5 py-3 text-sm font-medium text-white dark:bg-emerald-700"><Camera size={17} />Take photo / choose image</button></div>}

          {scanning && <div className="rounded-3xl bg-stone-100 px-6 py-10 text-center dark:bg-white/[0.045]"><LoaderCircle size={28} className="mx-auto animate-spin text-emerald-700 dark:text-emerald-400" /><p className="mt-4 font-medium">Reading receipt...</p><p className="mt-1 text-sm text-stone-500 dark:text-stone-400">{Math.round(progress * 100)}%</p><div className="mx-auto mt-4 h-2 max-w-sm overflow-hidden rounded-full bg-stone-200 dark:bg-white/[0.08]"><div className="h-full rounded-full bg-emerald-700 transition-all" style={{ width: Math.max(4, progress * 100) + "%" }} /></div></div>}

          {error && <div className="mt-4 rounded-2xl bg-red-50 px-4 py-3 text-sm text-red-700 dark:bg-red-400/10 dark:text-red-300">{error}</div>}

          {candidates.length > 0 && !scanning && <>
            <div className="mb-4 flex items-center justify-between gap-3"><div><p className="font-medium text-stone-900 dark:text-stone-100">Check what I found</p><p className="mt-1 text-sm text-stone-500 dark:text-stone-400">Untick non-products and correct OCR mistakes before adding.</p></div><button type="button" onClick={() => inputRef.current?.click()} className="shrink-0 rounded-full bg-stone-100 px-4 py-2 text-xs font-medium text-stone-600 dark:bg-white/[0.06] dark:text-stone-300">Rescan</button></div>

            <div className="space-y-2">
              {candidates.map((candidate) => <div key={candidate.id} className="rounded-2xl border border-stone-200 p-3 dark:border-white/[0.07]"><div className="flex items-start gap-3"><button type="button" onClick={() => patchCandidate(candidate.id, { selected: !candidate.selected })} className={candidate.selected ? "mt-1 flex h-6 w-6 shrink-0 items-center justify-center rounded-md bg-emerald-700 text-white" : "mt-1 h-6 w-6 shrink-0 rounded-md border-2 border-stone-300 dark:border-stone-600"}>{candidate.selected && <Check size={14} />}</button><div className="grid min-w-0 flex-1 grid-cols-1 gap-2 sm:grid-cols-[1fr_90px_120px]"><input value={candidate.name} onChange={(event) => patchCandidate(candidate.id, { name: event.target.value })} className="min-w-0 rounded-xl bg-stone-50 px-3 py-2 text-sm font-medium outline-none focus:ring-2 focus:ring-emerald-500/30 dark:bg-white/[0.05]" /><input type="number" min="0.01" step="0.01" value={candidate.quantity} onChange={(event) => patchCandidate(candidate.id, { quantity: Number(event.target.value) })} className="rounded-xl bg-stone-50 px-3 py-2 text-sm outline-none focus:ring-2 focus:ring-emerald-500/30 dark:bg-white/[0.05]" /><select value={candidate.location} onChange={(event) => patchCandidate(candidate.id, { location: event.target.value as PantryLocation })} className="rounded-xl bg-stone-50 px-3 py-2 text-sm outline-none focus:ring-2 focus:ring-emerald-500/30 dark:bg-stone-800"><option>Pantry</option><option>Fridge</option><option>Freezer</option><option>Other</option></select></div></div></div>)}
            </div>

            <button type="button" disabled={importing || candidates.every((candidate) => !candidate.selected)} onClick={() => void importSelected()} className="mt-5 w-full rounded-full bg-emerald-900 px-5 py-3 text-sm font-medium text-white transition hover:bg-emerald-800 disabled:opacity-40 dark:bg-emerald-700 dark:hover:bg-emerald-600">{importing ? "Adding items..." : "Add selected to pantry"}</button>
          </>}
        </div>
      </div>
    </div>
  );
}

export default ReceiptScanModal;
