import { Search, UtensilsCrossed, X } from "lucide-react";
import { useEffect, useMemo, useState } from "react";
import type { PlannedMeal } from "@/types/mealPlan";
import type { RecipeSummary } from "@/types/recipe";

interface AddMealPlanModalProps {
  date: Date;
  existingMeals: PlannedMeal[];
  onClose: () => void;
  onAdded: () => Promise<void>;
}

const mealTypes = ["Breakfast", "Lunch", "Dinner", "Snack"];

function getDateKey(date: Date) {
  return [
    date.getFullYear(),
    String(date.getMonth() + 1).padStart(2, "0"),
    String(date.getDate()).padStart(2, "0")
  ].join("-");
}

function AddMealPlanModal({ date, existingMeals, onClose, onAdded }: AddMealPlanModalProps) {
  const [recipes, setRecipes] = useState<RecipeSummary[]>([]);
  const [mealType, setMealType] = useState("");
  const [search, setSearch] = useState("");
  const [selectedRecipeId, setSelectedRecipeId] = useState<number | null>(null);
  const [loadingRecipes, setLoadingRecipes] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const usedMealTypes = existingMeals.map((meal) => meal.mealType);
  const availableMealTypes = mealTypes.filter((type) => !usedMealTypes.includes(type));

  useEffect(() => {
    if (availableMealTypes.length > 0) {
      setMealType(availableMealTypes[0]);
    }
  }, []);

  useEffect(() => {
    async function loadRecipes() {
      try {
        const response = await fetch("/api/recipes");

        if (!response.ok) {
          throw new Error(`Failed to load recipes: ${response.status}`);
        }

        const data: RecipeSummary[] = await response.json();
        setRecipes(data);
      } catch (error) {
        console.error(error);
        setError("Couldn't load recipes.");
      } finally {
        setLoadingRecipes(false);
      }
    }

    void loadRecipes();
  }, []);

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

  const filteredRecipes = useMemo(() => {
    const query = search.trim().toLowerCase();

    if (!query) return recipes;

    return recipes.filter((recipe) =>
      [
        recipe.name,
        recipe.description ?? "",
        ...recipe.mealTypes,
        ...recipe.cuisines
      ].join(" ").toLowerCase().includes(query)
    );
  }, [recipes, search]);

  async function submit() {
    if (!mealType || selectedRecipeId === null || saving) return;

    try {
      setSaving(true);
      setError(null);

      const response = await fetch("/api/meal-plan", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          date: getDateKey(date),
          mealType,
          recipeId: selectedRecipeId
        })
      });

      if (!response.ok) {
        const body = await response.json().catch(() => null);
        throw new Error(body?.message ?? "Failed to add meal.");
      }

      await onAdded();
      onClose();
    } catch (error) {
      console.error(error);
      setError(error instanceof Error ? error.message : "Couldn't add meal.");
    } finally {
      setSaving(false);
    }
  }

  return (
    <div className="fixed inset-0 z-[110] flex items-end justify-center bg-black/40 backdrop-blur-sm sm:items-center sm:p-4" onClick={onClose}>
      <div role="dialog" aria-modal="true" className="w-full max-w-lg overflow-hidden rounded-t-[2rem] bg-white shadow-2xl dark:bg-[#202521] dark:ring-1 dark:ring-white/[0.08] sm:rounded-3xl" onClick={(event) => event.stopPropagation()}>
        <div className="flex items-center justify-between border-b border-stone-100 px-5 py-4 dark:border-white/[0.06]">
          <div>
            <h2 className="text-lg font-semibold text-stone-900 dark:text-stone-100">Add meal</h2>
            <p className="mt-0.5 text-sm text-stone-500 dark:text-stone-400">
              {date.toLocaleDateString(undefined, {
                weekday: "long",
                day: "numeric",
                month: "long"
              })}
            </p>
          </div>

          <button type="button" aria-label="Close" onClick={onClose} className="flex h-9 w-9 items-center justify-center rounded-full bg-stone-100 text-stone-600 dark:bg-white/[0.06] dark:text-stone-300">
            <X size={18} />
          </button>
        </div>

        <div className="max-h-[70dvh] overflow-y-auto px-5 py-5">
          {availableMealTypes.length === 0 ? (
            <div className="rounded-2xl bg-stone-100 p-5 text-center dark:bg-white/[0.045]">
              <p className="font-medium text-stone-800 dark:text-stone-200">This day is full</p>
              <p className="mt-1 text-sm text-stone-500 dark:text-stone-400">Breakfast, lunch, dinner and snack are already planned.</p>
            </div>
          ) : (
            <>
              <div>
                <label htmlFor="meal-plan-type" className="text-sm font-medium text-stone-700 dark:text-stone-300">Meal</label>

                <select id="meal-plan-type" value={mealType} onChange={(event) => setMealType(event.target.value)} className="mt-2 h-12 w-full rounded-2xl border border-stone-200 bg-stone-50 px-4 text-sm text-stone-900 outline-none dark:border-white/[0.08] dark:bg-[#292e2a] dark:text-stone-100">
                  {availableMealTypes.map((type) => (
                    <option key={type} value={type}>{type}</option>
                  ))}
                </select>
              </div>

              <div className="mt-5">
                <label htmlFor="meal-plan-search" className="text-sm font-medium text-stone-700 dark:text-stone-300">Recipe</label>

                <div className="relative mt-2">
                  <Search size={17} className="absolute left-4 top-1/2 -translate-y-1/2 text-stone-400" />

                  <input id="meal-plan-search" value={search} onChange={(event) => setSearch(event.target.value)} placeholder="Search recipes..." className="h-12 w-full rounded-2xl border border-stone-200 bg-stone-50 pl-11 pr-4 text-sm text-stone-900 outline-none focus:border-emerald-700 dark:border-white/[0.08] dark:bg-white/[0.045] dark:text-stone-100 dark:focus:border-emerald-500" />
                </div>
              </div>

              <div className="mt-4 space-y-2">
                {loadingRecipes ? (
                  <p className="py-6 text-center text-sm text-stone-500 dark:text-stone-400">Loading recipes...</p>
                ) : filteredRecipes.length === 0 ? (
                  <p className="py-6 text-center text-sm text-stone-500 dark:text-stone-400">No matching recipes.</p>
                ) : (
                  filteredRecipes.map((recipe) => {
                    const selected = selectedRecipeId === recipe.id;

                    return (
                      <button key={recipe.id} type="button" onClick={() => setSelectedRecipeId(recipe.id)} className={`flex w-full items-center gap-3 rounded-2xl border p-3 text-left transition ${selected ? "border-emerald-500 bg-emerald-50 dark:border-emerald-500/50 dark:bg-emerald-400/[0.08]" : "border-stone-200 hover:bg-stone-50 dark:border-white/[0.07] dark:hover:bg-white/[0.04]"}`}>
                        {recipe.imageUrl ? (
                          <img src={recipe.imageUrl} alt="" className="h-14 w-14 shrink-0 rounded-xl object-cover" />
                        ) : (
                          <div className="flex h-14 w-14 shrink-0 items-center justify-center rounded-xl bg-stone-100 text-stone-400 dark:bg-white/[0.05]">
                            <UtensilsCrossed size={18} />
                          </div>
                        )}

                        <div className="min-w-0 flex-1">
                          <p className="font-medium text-stone-800 dark:text-stone-200">{recipe.name}</p>

                          {recipe.cuisines.length > 0 && (
                            <p className="mt-0.5 text-xs text-stone-400 dark:text-stone-500">{recipe.cuisines.join(" · ")}</p>
                          )}
                        </div>

                        <span className={`h-5 w-5 shrink-0 rounded-full border-2 ${selected ? "border-emerald-700 bg-emerald-700 ring-4 ring-emerald-100 dark:border-emerald-500 dark:bg-emerald-500 dark:ring-emerald-400/10" : "border-stone-300 dark:border-stone-600"}`} />
                      </button>
                    );
                  })
                )}
              </div>

              {error && <div className="mt-4 rounded-2xl bg-red-50 px-4 py-3 text-sm text-red-700 dark:bg-red-400/10 dark:text-red-300">{error}</div>}
            </>
          )}
        </div>

        {availableMealTypes.length > 0 && (
          <div className="border-t border-stone-100 p-4 dark:border-white/[0.06]">
            <button type="button" disabled={!mealType || selectedRecipeId === null || saving} onClick={() => void submit()} className="flex h-12 w-full items-center justify-center rounded-2xl bg-emerald-900 text-sm font-medium text-white transition hover:bg-emerald-800 disabled:cursor-not-allowed disabled:opacity-40 dark:bg-emerald-700 dark:hover:bg-emerald-600">
              {saving ? "Adding..." : "Add to plan"}
            </button>
          </div>
        )}
      </div>
    </div>
  );
}

export default AddMealPlanModal;