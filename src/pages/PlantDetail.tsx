import { useEffect, useState } from "react";
import { ArrowLeft, Camera, Droplets, Leaf, Plus } from "lucide-react";
import { useNavigate, useParams } from "react-router-dom";
import ThemeToggle from "@/components/ThemeToggle";
import { addPlantCareEvent, addPlantObservation, getPlant } from "@/api/plants";
import { assetUrl } from "@/api/client";
import type { PlantDetailResponse } from "@/types/plant";

const careTypes = ["Watered", "Fed", "Pruned", "Repotted", "Treated", "Moved", "Other"];

export default function PlantDetailPage() {
  const navigate = useNavigate();
  const { id } = useParams();
  const plantId = Number(id);
  const [data, setData] = useState<PlantDetailResponse | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [notes, setNotes] = useState("");
  const [issueSummary, setIssueSummary] = useState("");
  const [image, setImage] = useState<File | null>(null);
  const [careType, setCareType] = useState("Watered");
  const [careNotes, setCareNotes] = useState("");
  const [saving, setSaving] = useState(false);

  async function refresh() {
    try { setData(await getPlant(plantId)); setError(null); }
    catch (e) { setError(e instanceof Error ? e.message : "Couldn't load plant."); }
  }
  useEffect(() => { if (Number.isFinite(plantId)) void refresh(); }, [plantId]);

  async function saveObservation(event: React.FormEvent) {
    event.preventDefault(); setSaving(true);
    try {
      await addPlantObservation(plantId, { notes, issueSummary, image });
      setNotes(""); setIssueSummary(""); setImage(null); await refresh();
    } catch (e) { setError(e instanceof Error ? e.message : "Couldn't save observation."); }
    finally { setSaving(false); }
  }

  async function saveCare(event: React.FormEvent) {
    event.preventDefault(); setSaving(true);
    try { await addPlantCareEvent(plantId, { eventType: careType, notes: careNotes.trim() || null }); setCareNotes(""); await refresh(); }
    catch (e) { setError(e instanceof Error ? e.message : "Couldn't save care event."); }
    finally { setSaving(false); }
  }

  if (!data) return <main className="p-8 pb-28">{error ?? "Loading plant..."}</main>;
  const { plant } = data;

  return (
    <main className="mx-auto min-h-dvh w-full max-w-4xl px-4 pb-28 pt-8 sm:px-6 sm:pt-12">
      <div className="flex items-center justify-between">
        <button onClick={() => navigate("/plants")} className="flex h-11 w-11 items-center justify-center rounded-full bg-stone-100 dark:bg-white/[0.06]"><ArrowLeft size={20}/></button><ThemeToggle/>
      </div>

      <header className="mt-7 overflow-hidden rounded-[2rem] bg-emerald-950 text-white">
        {plant.coverImageUrl && <img src={assetUrl(plant.coverImageUrl) ?? ""} alt="" className="h-60 w-full object-cover"/>}
        <div className="p-6">
          <p className="text-sm text-emerald-200">{[plant.species, plant.variety].filter(Boolean).join(" · ") || "Plant profile"}</p>
          <h1 className="mt-1 text-3xl font-semibold">{plant.name}</h1>
          <p className="mt-2 text-sm text-emerald-100/80">{[plant.location, plant.notes].filter(Boolean).join(" · ")}</p>
        </div>
      </header>

      {error && <div className="mt-5 rounded-2xl bg-red-50 p-4 text-sm text-red-700">{error}</div>}

      <section className="mt-6 grid gap-4 lg:grid-cols-2">
        <form onSubmit={saveObservation} className="rounded-3xl border border-stone-200 p-5 dark:border-white/[0.08]">
          <div className="flex items-center gap-2"><Camera size={19} className="text-emerald-700"/><h2 className="font-semibold">Progress check</h2></div>
          <p className="mt-1 text-xs text-stone-500">Add a photo and notes so you can compare how the plant changes over time.</p>
          <textarea value={notes} onChange={e=>setNotes(e.target.value)} placeholder="What changed? New growth, yellow leaves, pests..." className="mt-4 min-h-24 w-full rounded-2xl border border-stone-200 bg-stone-50 p-3 text-sm dark:border-white/10 dark:bg-white/5"/>
          <input value={issueSummary} onChange={e=>setIssueSummary(e.target.value)} placeholder="Possible issue (optional)" className="mt-2 h-11 w-full rounded-2xl border border-stone-200 bg-stone-50 px-3 text-sm dark:border-white/10 dark:bg-white/5"/>
          <label className="mt-3 flex cursor-pointer items-center gap-2 rounded-2xl bg-stone-100 px-4 py-3 text-sm dark:bg-white/[0.06]"><Plus size={16}/>{image ? image.name : "Choose photo"}<input type="file" accept="image/*" onChange={e=>setImage(e.target.files?.[0] ?? null)} className="hidden"/></label>
          <button disabled={saving || (!notes.trim() && !image)} className="mt-3 w-full rounded-full bg-emerald-900 py-2.5 text-sm font-medium text-white disabled:opacity-40 dark:bg-emerald-700">Save progress</button>
        </form>

        <form onSubmit={saveCare} className="rounded-3xl border border-stone-200 p-5 dark:border-white/[0.08]">
          <div className="flex items-center gap-2"><Droplets size={19} className="text-blue-600"/><h2 className="font-semibold">Log care</h2></div>
          <div className="mt-4 grid grid-cols-2 gap-2">
            {careTypes.map(type => <button type="button" key={type} onClick={()=>setCareType(type)} className={`rounded-2xl px-3 py-2 text-sm ${careType===type ? "bg-emerald-900 text-white dark:bg-emerald-700" : "bg-stone-100 dark:bg-white/[0.06]"}`}>{type}</button>)}
          </div>
          <textarea value={careNotes} onChange={e=>setCareNotes(e.target.value)} placeholder="Optional notes" className="mt-3 min-h-20 w-full rounded-2xl border border-stone-200 bg-stone-50 p-3 text-sm dark:border-white/10 dark:bg-white/5"/>
          <button disabled={saving} className="mt-3 w-full rounded-full bg-emerald-900 py-2.5 text-sm font-medium text-white dark:bg-emerald-700">Log {careType.toLowerCase()}</button>
        </form>
      </section>

      <section className="mt-8">
        <div className="mb-3 flex items-center gap-2"><Leaf size={18} className="text-emerald-700"/><h2 className="text-lg font-semibold">Progress history</h2></div>
        {data.observations.length === 0 ? <p className="rounded-3xl bg-stone-100 p-6 text-sm text-stone-500 dark:bg-white/[0.045]">No progress photos yet.</p> :
          <div className="grid gap-4 sm:grid-cols-2">{data.observations.map(o => <article key={o.id} className="overflow-hidden rounded-3xl border border-stone-200 dark:border-white/[0.08]">{o.imageUrl && <img src={assetUrl(o.imageUrl) ?? ""} alt="" className="h-52 w-full object-cover"/>}<div className="p-4"><p className="text-xs text-stone-400">{new Date(o.observedAtUtc).toLocaleString()}</p>{o.notes && <p className="mt-2 text-sm">{o.notes}</p>}{o.issueSummary && <p className="mt-2 rounded-xl bg-amber-50 px-3 py-2 text-xs text-amber-800 dark:bg-amber-400/10 dark:text-amber-300">Watch: {o.issueSummary}</p>}</div></article>)}</div>}
      </section>

      <section className="mt-8">
        <h2 className="text-lg font-semibold">Care history</h2>
        <div className="mt-3 space-y-2">{data.careEvents.length === 0 ? <p className="text-sm text-stone-500">No care logged yet.</p> : data.careEvents.map(c => <div key={c.id} className="flex items-start justify-between gap-4 rounded-2xl bg-stone-100 px-4 py-3 dark:bg-white/[0.045]"><div><p className="text-sm font-medium">{c.eventType}</p>{c.notes && <p className="text-xs text-stone-500">{c.notes}</p>}</div><time className="shrink-0 text-xs text-stone-400">{new Date(c.occurredAtUtc).toLocaleDateString()}</time></div>)}</div>
      </section>
    </main>
  );
}
