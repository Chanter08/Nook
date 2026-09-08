import { ArrowLeft, ChevronDown, Plus, Save, Trash2 } from "lucide-react";
import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import ThemeToggle from "@/components/ThemeToggle";
import type { NewRecipeIngredient, NewRecipeStep, RecipeOptions } from "@/types/recipe";

const units = ["", "each", "g", "kg", "ml", "l", "tbsp", "tsp", "cloves", "pack", "can"];

function AddRecipePage() {
  const navigate = useNavigate();

  const [name, setName] = useState("");
  const [description, setDescription] = useState("");
  const [prepTime, setPrepTime] = useState("");
  const [cookTime, setCookTime] = useState("");
  const [servings, setServings] = useState("");
  const [imageUrl, setImageUrl] = useState("");

  const [calories, setCalories] = useState("");
  const [protein, setProtein] = useState("");
  const [carbs, setCarbs] = useState("");
  const [fat, setFat] = useState("");

  const [options, setOptions] = useState<RecipeOptions>({ mealTypes: [], cuisines: [] });
  const [selectedMealTypes, setSelectedMealTypes] = useState<string[]>([]);
  const [selectedCuisines, setSelectedCuisines] = useState<string[]>([]);

  const [ingredients, setIngredients] = useState<NewRecipeIngredient[]>([
    { name: "", quantity: "", unit: "", notes: "" }
  ]);

  const [steps, setSteps] = useState<NewRecipeStep[]>([
    { instruction: "" }
  ]);

  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    async function loadOptions() {
      try {
        const response = await fetch("/api/recipes/options");

        if (!response.ok) {
          throw new Error(`Failed to load recipe options: ${response.status}`);
        }

        const data: RecipeOptions = await response.json();
        setOptions(data);
      } catch (error) {
        console.error("Recipe options error:", error);
      }
    }

    void loadOptions();
  }, []);

  function toggleMealType(value: string) {
    setSelectedMealTypes((current) =>
      current.includes(value)
        ? current.filter((item) => item !== value)
        : [...current, value]
    );
  }

  function toggleCuisine(value: string) {
    setSelectedCuisines((current) =>
      current.includes(value)
        ? current.filter((item) => item !== value)
        : [...current, value]
    );
  }

  function updateIngredient(index: number, field: keyof NewRecipeIngredient, value: string) {
    setIngredients((current) =>
      current.map((ingredient, currentIndex) =>
        currentIndex === index ? { ...ingredient, [field]: value } : ingredient
      )
    );
  }

  function addIngredient() {
    setIngredients((current) => [
      ...current,
      { name: "", quantity: "", unit: "", notes: "" }
    ]);
  }

  function removeIngredient(index: number) {
    setIngredients((current) =>
      current.length === 1
        ? current
        : current.filter((_, currentIndex) => currentIndex !== index)
    );
  }

  function updateStep(index: number, value: string) {
    setSteps((current) =>
      current.map((step, currentIndex) =>
        currentIndex === index ? { instruction: value } : step
      )
    );
  }

  function addStep() {
    setSteps((current) => [...current, { instruction: "" }]);
  }

  function removeStep(index: number) {
    setSteps((current) =>
      current.length === 1
        ? current
        : current.filter((_, currentIndex) => currentIndex !== index)
    );
  }

  function numberOrNull(value: string) {
    const trimmed = value.trim();
    return trimmed === "" ? null : Number(trimmed);
  }

  async function saveRecipe() {
    if (!name.trim() || saving) return;

    const validIngredients = ingredients.filter((ingredient) => ingredient.name.trim());
    const validSteps = steps.filter((step) => step.instruction.trim());

    if (validIngredients.length === 0) {
      setError("Add at least one ingredient.");
      return;
    }

    if (validSteps.length === 0) {
      setError("Add at least one method step.");
      return;
    }

    try {
      setSaving(true);
      setError(null);

      const response = await fetch("/api/recipes", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          name: name.trim(),
          description: description.trim() || null,
          prepTimeMinutes: numberOrNull(prepTime),
          cookTimeMinutes: numberOrNull(cookTime),
          servings: numberOrNull(servings),
          imageUrl: imageUrl.trim() || null,
          calories: numberOrNull(calories),
          proteinGrams: numberOrNull(protein),
          carbohydrateGrams: numberOrNull(carbs),
          fatGrams: numberOrNull(fat),
          mealTypes: selectedMealTypes,
          cuisines: selectedCuisines,
          ingredients: validIngredients.map((ingredient) => ({
            name: ingredient.name.trim(),
            quantity: numberOrNull(ingredient.quantity),
            unit: ingredient.unit || null,
            notes: ingredient.notes.trim() || null
          })),
          steps: validSteps.map((step) => ({
            instruction: step.instruction.trim()
          }))
        })
      });

      if (!response.ok) {
        const body = await response.json().catch(() => null);
        throw new Error(body?.message ?? "Failed to save recipe.");
      }

      const result: { id: number } = await response.json();
      navigate(`/meals/${result.id}`);
    } catch (error) {
      console.error("Save recipe error:", error);
      setError(error instanceof Error ? error.message : "Couldn't save recipe.");
    } finally {
      setSaving(false);
    }
  }

  const inputClass = "mt-2 h-12 w-full rounded-2xl border border-stone-200 bg-stone-50 px-4 text-sm text-stone-900 outline-none transition focus:border-emerald-700 dark:border-white/[0.08] dark:bg-white/[0.045] dark:text-stone-100 dark:focus:border-emerald-500";
  const labelClass = "text-sm font-medium text-stone-700 dark:text-stone-300";

  return (
    <main className="mx-auto min-h-dvh w-full max-w-3xl px-4 pb-32 pt-8 sm:px-6 sm:pt-12">
      <header className="mb-8 flex items-center justify-between">
        <button type="button" aria-label="Go back" onClick={() => navigate(-1)} className="flex h-11 w-11 items-center justify-center rounded-full bg-stone-100 text-stone-700 transition hover:bg-stone-200 dark:bg-white/[0.06] dark:text-stone-200">
          <ArrowLeft size={20} />
        </button>

        <ThemeToggle />
      </header>

      <div className="mb-8">
        <p className="text-sm font-medium text-emerald-800 dark:text-emerald-400">Your kitchen</p>
        <h1 className="mt-1 text-3xl font-semibold tracking-tight text-stone-900 dark:text-stone-100">Add recipe</h1>
        <p className="mt-2 text-sm text-stone-500 dark:text-stone-400">Add something you cook at home.</p>
      </div>

      <div className="space-y-10">
        <section>
          <h2 className="text-lg font-semibold text-stone-900 dark:text-stone-100">Recipe</h2>

          <div className="mt-5 space-y-5">
            <div>
              <label htmlFor="recipe-name" className={labelClass}>Name</label>
              <input id="recipe-name" value={name} onChange={(event) => setName(event.target.value)} placeholder="e.g. Chicken Massaman Curry" autoFocus className={inputClass} />
            </div>

            <div>
              <label htmlFor="recipe-description" className={labelClass}>Description <span className="font-normal text-stone-400">optional</span></label>
              <textarea id="recipe-description" value={description} onChange={(event) => setDescription(event.target.value)} rows={3} placeholder="A short description..." className="mt-2 w-full resize-none rounded-2xl border border-stone-200 bg-stone-50 px-4 py-3 text-sm text-stone-900 outline-none transition focus:border-emerald-700 dark:border-white/[0.08] dark:bg-white/[0.045] dark:text-stone-100 dark:focus:border-emerald-500" />
            </div>

            <div className="grid grid-cols-2 gap-3 sm:grid-cols-3">
              <div>
                <label htmlFor="prep-time" className={labelClass}>Prep min</label>
                <input id="prep-time" type="number" min="0" value={prepTime} onChange={(event) => setPrepTime(event.target.value)} className={inputClass} />
              </div>

              <div>
                <label htmlFor="cook-time" className={labelClass}>Cook min</label>
                <input id="cook-time" type="number" min="0" value={cookTime} onChange={(event) => setCookTime(event.target.value)} className={inputClass} />
              </div>

              <div>
                <label htmlFor="servings" className={labelClass}>Servings</label>
                <input id="servings" type="number" min="1" value={servings} onChange={(event) => setServings(event.target.value)} className={inputClass} />
              </div>
            </div>

            <div>
              <label htmlFor="image-url" className={labelClass}>Image URL <span className="font-normal text-stone-400">optional for now</span></label>
              <input id="image-url" value={imageUrl} onChange={(event) => setImageUrl(event.target.value)} placeholder="https://..." className={inputClass} />
            </div>
          </div>
        </section>

        <section>
          <h2 className="text-lg font-semibold text-stone-900 dark:text-stone-100">Meal type</h2>

          <div className="mt-4 flex flex-wrap gap-2">
            {options.mealTypes.map((type) => {
              const selected = selectedMealTypes.includes(type);

              return (
                <button key={type} type="button" onClick={() => toggleMealType(type)} className={`rounded-full px-4 py-2 text-sm font-medium transition ${selected ? "bg-emerald-900 text-white dark:bg-emerald-700" : "bg-stone-100 text-stone-600 hover:bg-stone-200 dark:bg-white/[0.05] dark:text-stone-300 dark:hover:bg-white/[0.08]"}`}>
                  {type}
                </button>
              );
            })}
          </div>

          <h2 className="mt-6 text-lg font-semibold text-stone-900 dark:text-stone-100">Cuisine</h2>

          <div className="mt-4 flex flex-wrap gap-2">
            {options.cuisines.map((cuisine) => {
              const selected = selectedCuisines.includes(cuisine);

              return (
                <button key={cuisine} type="button" onClick={() => toggleCuisine(cuisine)} className={`rounded-full px-4 py-2 text-sm font-medium transition ${selected ? "bg-emerald-900 text-white dark:bg-emerald-700" : "bg-stone-100 text-stone-600 hover:bg-stone-200 dark:bg-white/[0.05] dark:text-stone-300 dark:hover:bg-white/[0.08]"}`}>
                  {cuisine}
                </button>
              );
            })}
          </div>
        </section>

        <section>
          <h2 className="text-lg font-semibold text-stone-900 dark:text-stone-100">Nutrition</h2>
          <p className="mt-1 text-sm text-stone-500 dark:text-stone-400">Per serving · optional</p>

          <div className="mt-4 grid grid-cols-2 gap-3 sm:grid-cols-4">
            <div>
              <label htmlFor="calories" className={labelClass}>Calories</label>
              <input id="calories" type="number" min="0" value={calories} onChange={(event) => setCalories(event.target.value)} className={inputClass} />
            </div>

            <div>
              <label htmlFor="protein" className={labelClass}>Protein g</label>
              <input id="protein" type="number" min="0" step="any" value={protein} onChange={(event) => setProtein(event.target.value)} className={inputClass} />
            </div>

            <div>
              <label htmlFor="carbs" className={labelClass}>Carbs g</label>
              <input id="carbs" type="number" min="0" step="any" value={carbs} onChange={(event) => setCarbs(event.target.value)} className={inputClass} />
            </div>

            <div>
              <label htmlFor="fat" className={labelClass}>Fat g</label>
              <input id="fat" type="number" min="0" step="any" value={fat} onChange={(event) => setFat(event.target.value)} className={inputClass} />
            </div>
          </div>
        </section>

        <section>
          <div className="flex items-end justify-between">
            <div>
              <h2 className="text-lg font-semibold text-stone-900 dark:text-stone-100">Ingredients</h2>
              <p className="mt-1 text-sm text-stone-500 dark:text-stone-400">Add them in the order you use them.</p>
            </div>

            <button type="button" onClick={addIngredient} className="flex items-center gap-1.5 text-sm font-medium text-emerald-800 dark:text-emerald-400">
              <Plus size={16} />
              Add
            </button>
          </div>

          <div className="mt-4 space-y-3">
            {ingredients.map((ingredient, index) => (
              <div key={index} className="rounded-3xl border border-stone-200 bg-white p-4 dark:border-white/[0.07] dark:bg-white/[0.035]">
                <div className="flex items-center justify-between">
                  <p className="text-xs font-semibold uppercase tracking-wide text-stone-400">Ingredient {index + 1}</p>

                  {ingredients.length > 1 && (
                    <button type="button" aria-label="Remove ingredient" onClick={() => removeIngredient(index)} className="flex h-8 w-8 items-center justify-center rounded-full text-stone-400 hover:bg-red-50 hover:text-red-600 dark:hover:bg-red-400/10 dark:hover:text-red-400">
                      <Trash2 size={15} />
                    </button>
                  )}
                </div>

                <input value={ingredient.name} onChange={(event) => updateIngredient(index, "name", event.target.value)} placeholder="Ingredient name" className={inputClass} />

                <div className="mt-3 grid grid-cols-2 gap-3">
                  <input type="number" min="0" step="any" value={ingredient.quantity} onChange={(event) => updateIngredient(index, "quantity", event.target.value)} placeholder="Quantity" className="h-12 w-full rounded-2xl border border-stone-200 bg-stone-50 px-4 text-sm outline-none dark:border-white/[0.08] dark:bg-white/[0.045] dark:text-stone-100" />

                  <div className="relative">
                    <select value={ingredient.unit} onChange={(event) => updateIngredient(index, "unit", event.target.value)} className="h-12 w-full appearance-none rounded-2xl border border-stone-200 bg-stone-50 px-4 pr-10 text-sm outline-none dark:border-white/[0.08] dark:bg-[#292e2a] dark:text-stone-100">
                      {units.map((unit) => (
                        <option key={unit} value={unit}>{unit || "Unit"}</option>
                      ))}
                    </select>
                    <ChevronDown size={16} className="pointer-events-none absolute right-4 top-1/2 -translate-y-1/2 text-stone-400" />
                  </div>
                </div>

                <input value={ingredient.notes} onChange={(event) => updateIngredient(index, "notes", event.target.value)} placeholder="Notes, e.g. finely chopped" className="mt-3 h-12 w-full rounded-2xl border border-stone-200 bg-stone-50 px-4 text-sm outline-none dark:border-white/[0.08] dark:bg-white/[0.045] dark:text-stone-100" />
              </div>
            ))}
          </div>
        </section>

        <section>
          <div className="flex items-end justify-between">
            <div>
              <h2 className="text-lg font-semibold text-stone-900 dark:text-stone-100">Method</h2>
              <p className="mt-1 text-sm text-stone-500 dark:text-stone-400">Write the recipe step by step.</p>
            </div>

            <button type="button" onClick={addStep} className="flex items-center gap-1.5 text-sm font-medium text-emerald-800 dark:text-emerald-400">
              <Plus size={16} />
              Add
            </button>
          </div>

          <div className="mt-4 space-y-3">
            {steps.map((step, index) => (
              <div key={index} className="flex gap-3">
                <div className="flex h-9 w-9 shrink-0 items-center justify-center rounded-full bg-emerald-900 text-sm font-semibold text-white dark:bg-emerald-700">
                  {index + 1}
                </div>

                <textarea value={step.instruction} onChange={(event) => updateStep(index, event.target.value)} rows={3} placeholder={`Step ${index + 1}`} className="min-w-0 flex-1 resize-none rounded-2xl border border-stone-200 bg-stone-50 px-4 py-3 text-sm text-stone-900 outline-none focus:border-emerald-700 dark:border-white/[0.08] dark:bg-white/[0.045] dark:text-stone-100 dark:focus:border-emerald-500" />

                {steps.length > 1 && (
                  <button type="button" aria-label="Remove step" onClick={() => removeStep(index)} className="flex h-9 w-9 shrink-0 items-center justify-center rounded-full text-stone-400 hover:bg-red-50 hover:text-red-600 dark:hover:bg-red-400/10 dark:hover:text-red-400">
                    <Trash2 size={15} />
                  </button>
                )}
              </div>
            ))}
          </div>
        </section>
      </div>

      {error && <div className="mt-8 rounded-2xl bg-red-50 px-4 py-3 text-sm text-red-700 dark:bg-red-400/10 dark:text-red-300">{error}</div>}

      <div className="mt-10 border-t border-stone-200 pt-5 dark:border-white/[0.07]">
        <button type="button" disabled={!name.trim() || saving} onClick={() => void saveRecipe()} className="flex h-13 w-full items-center justify-center gap-2 rounded-2xl bg-emerald-900 text-sm font-medium text-white transition hover:bg-emerald-800 disabled:cursor-not-allowed disabled:opacity-40 dark:bg-emerald-700 dark:hover:bg-emerald-600">
          <Save size={17} />
          {saving ? "Saving recipe..." : "Save recipe"}
        </button>
      </div>
    </main>
  );
}

export default AddRecipePage;