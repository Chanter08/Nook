import { CalendarDays, Check, X } from "lucide-react";
import { useEffect, useState } from "react";
import type { PlannedMeal } from "@/types/mealPlan";

interface PlanRecipeModalProps {
  recipeId: number;
  recipeName: string;
  onClose: () => void;
  onPlanned: () => void;
}

const mealTypes = ["Breakfast", "Lunch", "Dinner", "Snack"];

function getDateKey(date: Date) {
  return [
    date.getFullYear(),
    String(date.getMonth() + 1).padStart(2, "0"),
    String(date.getDate()).padStart(2, "0")
  ].join("-");
}

function PlanRecipeModal({ recipeId, recipeName, onClose, onPlanned }: PlanRecipeModalProps) {
  const [selectedDate, setSelectedDate] = useState<Date>(new Date());
  const [selectedMealType, setSelectedMealType] = useState("");
  const [plannedMeals, setPlannedMeals] = useState<PlannedMeal[]>([]);
  const [loadingPlan, setLoadingPlan] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const today = new Date();
  today.setHours(0, 0, 0, 0);

  const days = Array.from({ length: 7 }, (_, index) => {
    const date = new Date(today);
    date.setDate(date.getDate() + index);
    return date;
  });

  const selectedDateKey = getDateKey(selectedDate);

  const usedMealTypes = plannedMeals
    .filter((meal) => meal.date === selectedDateKey)
    .map((meal) => meal.mealType);

  useEffect(() => {
    async function loadPlan() {
      try {
        const response = await fetch(`/api/meal-plan/week?start=${getDateKey(today)}`);

        if (!response.ok) {
          throw new Error(`Failed to load meal plan: ${response.status}`);
        }

        const data: PlannedMeal[] = await response.json();
        setPlannedMeals(data);
      } catch (error) {
        console.error("Meal plan error:", error);
        setError("Couldn't load the meal plan.");
      } finally {
        setLoadingPlan(false);
      }
    }

    void loadPlan();
  }, []);

  useEffect(() => {
    if (usedMealTypes.includes(selectedMealType)) {
      setSelectedMealType("");
    }
  }, [selectedDateKey]);

  useEffect(() => {
    function handleKeyDown(event: KeyboardEvent) {
      if (event.key === "Escape") onClose();
    }

    document.body.style.overflow = "hidden";
    window.addEventListener("keydown", handleKeyDown);

    return () => {
      document.body.style.overflow = "";
      window.removeEventListener("keydown", handleKeyDown);
    };
  }, [onClose]);

  async function submit() {
    if (!selectedMealType || saving) return;

    try {
      setSaving(true);
      setError(null);

      const response = await fetch("/api/meal-plan", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          date: selectedDateKey,
          mealType: selectedMealType,
          recipeId
        })
      });

      if (!response.ok) {
        const body = await response.json().catch(() => null);
        throw new Error(body?.message ?? "Couldn't plan the meal.");
      }

      onPlanned();
      onClose();
    } catch (error) {
      console.error(error);
      setError(error instanceof Error ? error.message : "Couldn't plan the meal.");
    } finally {
      setSaving(false);
    }
  }

  return (
    <div className="fixed inset-0 z-[110] flex items-end justify-center bg-black/40 backdrop-blur-sm sm:items-center sm:p-4" onClick={onClose}>
      <div role="dialog" aria-modal="true" className="w-full max-w-lg overflow-hidden rounded-t-[2rem] bg-white shadow-2xl dark:bg-[#202521] dark:ring-1 dark:ring-white/[0.08] sm:rounded-3xl" onClick={(event) => event.stopPropagation()}>
        <div className="flex items-center justify-between border-b border-stone-100 px-5 py-4 dark:border-white/[0.06]">
          <div>
            <h2 className="text-lg font-semibold text-stone-900 dark:text-stone-100">Plan meal</h2>
            <p className="mt-0.5 text-sm text-stone-500 dark:text-stone-400">{recipeName}</p>
          </div>

          <button type="button" aria-label="Close" onClick={onClose} className="flex h-9 w-9 items-center justify-center rounded-full bg-stone-100 text-stone-600 dark:bg-white/[0.06] dark:text-stone-300">
            <X size={18} />
          </button>
        </div>

        <div className="px-5 py-5">
          {loadingPlan ? (
            <p className="py-8 text-center text-sm text-stone-500 dark:text-stone-400">Loading meal plan...</p>
          ) : (
            <>
              <div>
                <p className="text-sm font-medium text-stone-700 dark:text-stone-300">Date</p>

                <div className="-mx-5 mt-3 overflow-x-auto px-5 pb-2">
                  <div className="flex min-w-max gap-2">
                    {days.map((date) => {
                      const dateKey = getDateKey(date);
                      const selected = dateKey === selectedDateKey;
                      const isToday = dateKey === getDateKey(today);

                      return (
                        <button key={dateKey} type="button" onClick={() => setSelectedDate(date)} className={`w-[74px] rounded-2xl border px-3 py-3 text-center transition ${selected ? "border-emerald-700 bg-emerald-700 text-white dark:border-emerald-600 dark:bg-emerald-700" : "border-stone-200 bg-stone-50 text-stone-700 hover:border-emerald-300 dark:border-white/[0.07] dark:bg-white/[0.04] dark:text-stone-300"}`}>
                          <p className="text-xs font-medium">
                            {isToday ? "Today" : date.toLocaleDateString(undefined, { weekday: "short" })}
                          </p>

                          <p className="mt-1 text-lg font-semibold">{date.getDate()}</p>

                          <p className={`text-[11px] ${selected ? "text-emerald-100" : "text-stone-400 dark:text-stone-500"}`}>
                            {date.toLocaleDateString(undefined, { month: "short" })}
                          </p>
                        </button>
                      );
                    })}
                  </div>
                </div>
              </div>

              <div className="mt-5">
                <p className="text-sm font-medium text-stone-700 dark:text-stone-300">Meal</p>

                <div className="mt-3 grid grid-cols-2 gap-2">
                  {mealTypes.map((mealType) => {
                    const occupied = usedMealTypes.includes(mealType);
                    const selected = selectedMealType === mealType;

                    return (
                      <button key={mealType} type="button" disabled={occupied} onClick={() => setSelectedMealType(mealType)} className={`flex h-12 items-center justify-between rounded-2xl border px-4 text-sm font-medium transition ${selected ? "border-emerald-700 bg-emerald-50 text-emerald-900 dark:border-emerald-500 dark:bg-emerald-400/[0.08] dark:text-emerald-300" : occupied ? "cursor-not-allowed border-stone-100 bg-stone-50 text-stone-300 dark:border-white/[0.04] dark:bg-white/[0.02] dark:text-stone-600" : "border-stone-200 text-stone-700 hover:border-emerald-300 dark:border-white/[0.07] dark:text-stone-300"}`}>
                        {mealType}

                        {occupied && <span className="text-xs font-normal">Planned</span>}
                        {selected && <Check size={16} />}
                      </button>
                    );
                  })}
                </div>
              </div>

              {error && (
                <div className="mt-5 rounded-2xl bg-red-50 px-4 py-3 text-sm text-red-700 dark:bg-red-400/10 dark:text-red-300">
                  {error}
                </div>
              )}
            </>
          )}
        </div>

        {!loadingPlan && (
          <div className="border-t border-stone-100 p-4 dark:border-white/[0.06]">
            <button type="button" disabled={!selectedMealType || saving} onClick={() => void submit()} className="flex h-12 w-full items-center justify-center gap-2 rounded-2xl bg-emerald-900 text-sm font-medium text-white transition hover:bg-emerald-800 disabled:cursor-not-allowed disabled:opacity-40 dark:bg-emerald-700 dark:hover:bg-emerald-600">
              <CalendarDays size={17} />
              {saving ? "Planning..." : "Add to meal plan"}
            </button>
          </div>
        )}
      </div>
    </div>
  );
}

export default PlanRecipeModal;