import { useEffect, useState } from "react";
import { ArrowLeft, CloudRain, Leaf, Plus, Sprout, SunMedium, Trash2 } from "lucide-react";
import { useNavigate } from "react-router-dom";
import ThemeToggle from "@/components/ThemeToggle";
import { createPlant, deletePlant, getGardenWeather, getPlants } from "@/api/plants";
import { assetUrl } from "@/api/client";
import type { GardenWeatherContext, PlantPayload, PlantSummary } from "@/types/plant";

const emptyPlant: PlantPayload = { name: "", species: "", variety: "", location: "", acquiredDate: null, notes: "" };

export default function PlantsPage() {
  const navigate = useNavigate();
  const [plants, setPlants] = useState<PlantSummary[]>([]);
  const [loading, setLoading] = useState(true);
  const [showForm, setShowForm] = useState(false);
  const [form, setForm] = useState<PlantPayload>(emptyPlant);
  const [error, setError] = useState<string | null>(null);
  const [weather, setWeather] = useState<GardenWeatherContext | null>(null);

  async function refresh() {
    try { setPlants(await getPlants()); setError(null); }
    catch (e) { setError(e instanceof Error ? e.message : "Couldn't load plants."); }
    finally { setLoading(false); }
  }

  useEffect(() => { void refresh(); void getGardenWeather().then(setWeather).catch(() => setWeather(null)); }, []);

  async function submit(event: React.FormEvent) {
    event.preventDefault();
    try {
      const result = await createPlant({
        ...form,
        species: form.species?.trim() || null,
        variety: form.variety?.trim() || null,
        location: form.location?.trim() || null,
        notes: form.notes?.trim() || null,
      });
      setShowForm(false);
      setForm(emptyPlant);
      navigate(`/plants/${result.id}`);
    } catch (e) { setError(e instanceof Error ? e.message : "Couldn't add plant."); }
  }

  async function remove(event: React.MouseEvent, plant: PlantSummary) {
    event.stopPropagation();
    if (!confirm(`Remove ${plant.name} and its history?`)) return;
    await deletePlant(plant.id);
    await refresh();
  }

  return (
    <main className="mx-auto min-h-dvh w-full max-w-4xl px-4 pb-28 pt-8 sm:px-6 sm:pt-12">
      <div className="flex items-center justify-between">
        <button onClick={() => navigate(-1)} className="flex h-11 w-11 items-center justify-center rounded-full bg-stone-100 dark:bg-white/[0.06]"><ArrowLeft size={20}/></button>
        <ThemeToggle />
      </div>

      <header className="mt-7 mb-7 flex items-end justify-between gap-4">
        <div>
          <p className="text-sm font-medium text-emerald-800 dark:text-emerald-400">Our garden</p>
          <h1 className="mt-1 text-3xl font-semibold tracking-tight">Plants</h1>
          <p className="mt-1 text-sm text-stone-500 dark:text-stone-400">Keep a photo history and remember how each plant has been cared for.</p>
        </div>
        <button onClick={() => setShowForm(true)} className="flex h-11 shrink-0 items-center gap-2 rounded-full bg-emerald-900 px-4 text-sm font-medium text-white dark:bg-emerald-700"><Plus size={17}/>Add plant</button>
      </header>

      {weather && (
        <section className="mb-6 rounded-3xl bg-sky-50 p-5 dark:bg-sky-400/[0.06]">
          <div className="flex items-center justify-between gap-4">
            <div>
              <p className="text-xs font-semibold uppercase tracking-wide text-sky-700 dark:text-sky-300">Malta today</p>
              <p className="mt-1 text-xl font-semibold">{weather.today.temperatureMaxC.toFixed(0)}°C · {weather.today.rainProbabilityPercent.toFixed(0)}% rain</p>
            </div>
            {weather.rainNext48HoursMm >= 2 ? <CloudRain className="text-sky-700" size={28}/> : <SunMedium className="text-amber-600" size={28}/>}
          </div>
          <div className="mt-4 grid grid-cols-3 gap-2 text-center text-xs">
            <div className="rounded-2xl bg-white/70 p-3 dark:bg-white/[0.05]"><p className="text-lg font-semibold">{weather.rainNext48HoursMm.toFixed(1)}</p><p className="text-stone-500">mm next 48h</p></div>
            <div className="rounded-2xl bg-white/70 p-3 dark:bg-white/[0.05]"><p className="text-lg font-semibold">{weather.today.et0Mm.toFixed(1)}</p><p className="text-stone-500">ET₀ mm today</p></div>
            <div className="rounded-2xl bg-white/70 p-3 dark:bg-white/[0.05]"><p className="text-lg font-semibold">{weather.rainPast3DaysMm.toFixed(1)}</p><p className="text-stone-500">mm past 3d</p></div>
          </div>
          <p className="mt-3 text-xs text-stone-500 dark:text-stone-400">
            Compared with {weather.currentMonthBaseline.startYear}–{weather.currentMonthBaseline.endYear}: about {weather.currentMonthBaseline.averageMaxTemperatureC.toFixed(1)}°C average daily high, {weather.currentMonthBaseline.averageDailyRainMm.toFixed(1)} mm rain/day and {weather.currentMonthBaseline.averageDailyEt0Mm.toFixed(1)} mm ET₀/day this month.
          </p>
        </section>
      )}

      {error && <div className="mb-5 rounded-2xl bg-red-50 px-4 py-3 text-sm text-red-700 dark:bg-red-400/10 dark:text-red-300">{error}</div>}
      {loading && <div className="rounded-3xl bg-stone-100 p-8 text-center text-sm text-stone-500 dark:bg-white/[0.045]">Loading plants...</div>}

      {!loading && plants.length === 0 && (
        <button onClick={() => setShowForm(true)} className="w-full rounded-3xl bg-emerald-50 px-6 py-12 text-center dark:bg-emerald-400/[0.06]">
          <Sprout size={34} className="mx-auto text-emerald-700"/>
          <p className="mt-4 font-medium">Add your first plant</p>
          <p className="mt-1 text-sm text-stone-500">Start with one of the citrus plants or herbs and build its history from today.</p>
        </button>
      )}

      <div className="grid gap-4 sm:grid-cols-2">
        {plants.map((plant) => (
          <article key={plant.id} onClick={() => navigate(`/plants/${plant.id}`)} className="group cursor-pointer overflow-hidden rounded-3xl border border-stone-200 bg-white dark:border-white/[0.07] dark:bg-white/[0.035]">
            {plant.coverImageUrl ? (
              <img src={assetUrl(plant.coverImageUrl) ?? ""} alt="" className="h-44 w-full object-cover"/>
            ) : (
              <div className="flex h-44 items-center justify-center bg-emerald-50 dark:bg-emerald-400/[0.06]"><Leaf size={42} className="text-emerald-700"/></div>
            )}
            <div className="p-5">
              <div className="flex items-start justify-between gap-3">
                <div><h2 className="text-lg font-semibold">{plant.name}</h2><p className="text-sm text-stone-500">{[plant.species, plant.variety].filter(Boolean).join(" · ") || "Species not set"}</p></div>
                <button onClick={(e) => void remove(e, plant)} className="flex h-9 w-9 items-center justify-center rounded-full text-stone-400 hover:bg-red-50 hover:text-red-600"><Trash2 size={16}/></button>
              </div>
              <div className="mt-4 flex flex-wrap gap-2 text-xs text-stone-500">
                {plant.location && <span className="rounded-full bg-stone-100 px-2.5 py-1 dark:bg-white/[0.06]">{plant.location}</span>}
                {plant.lastWateredAtUtc && <span className="rounded-full bg-blue-50 px-2.5 py-1 dark:bg-blue-400/10">Watered {new Date(plant.lastWateredAtUtc).toLocaleDateString()}</span>}
              </div>
            </div>
          </article>
        ))}
      </div>

      {showForm && (
        <div className="fixed inset-0 z-[70] flex items-end bg-black/40 p-3 sm:items-center sm:justify-center" onMouseDown={() => setShowForm(false)}>
          <form onSubmit={submit} onMouseDown={(e) => e.stopPropagation()} className="w-full max-w-lg rounded-[2rem] bg-white p-6 shadow-2xl dark:bg-stone-900">
            <h2 className="text-xl font-semibold">Add plant</h2>
            <div className="mt-5 grid gap-3 sm:grid-cols-2">
              <input required placeholder="Name *" value={form.name} onChange={e => setForm({...form,name:e.target.value})} className="h-12 rounded-2xl border border-stone-200 bg-stone-50 px-4 dark:border-white/10 dark:bg-white/5"/>
              <input placeholder="Species" value={form.species ?? ""} onChange={e => setForm({...form,species:e.target.value})} className="h-12 rounded-2xl border border-stone-200 bg-stone-50 px-4 dark:border-white/10 dark:bg-white/5"/>
              <input placeholder="Variety" value={form.variety ?? ""} onChange={e => setForm({...form,variety:e.target.value})} className="h-12 rounded-2xl border border-stone-200 bg-stone-50 px-4 dark:border-white/10 dark:bg-white/5"/>
              <input placeholder="Location e.g. terrace" value={form.location ?? ""} onChange={e => setForm({...form,location:e.target.value})} className="h-12 rounded-2xl border border-stone-200 bg-stone-50 px-4 dark:border-white/10 dark:bg-white/5"/>
              <input type="date" value={form.acquiredDate ?? ""} onChange={e => setForm({...form,acquiredDate:e.target.value || null})} className="h-12 rounded-2xl border border-stone-200 bg-stone-50 px-4 dark:border-white/10 dark:bg-white/5 sm:col-span-2"/>
              <textarea placeholder="Notes" value={form.notes ?? ""} onChange={e => setForm({...form,notes:e.target.value})} className="min-h-24 rounded-2xl border border-stone-200 bg-stone-50 p-4 dark:border-white/10 dark:bg-white/5 sm:col-span-2"/>
            </div>
            <div className="mt-5 flex justify-end gap-2"><button type="button" onClick={() => setShowForm(false)} className="rounded-full px-4 py-2.5 text-sm">Cancel</button><button className="rounded-full bg-emerald-900 px-5 py-2.5 text-sm font-medium text-white dark:bg-emerald-700">Add plant</button></div>
          </form>
        </div>
      )}
    </main>
  );
}
