interface RecipeNutritionData {
  proteinGrams: number | null;
  carbohydrateGrams: number | null;
  fatGrams: number | null;
}

interface RecipeNutritionProps {
  recipe: RecipeNutritionData;
}

function RecipeNutrition({ recipe }: RecipeNutritionProps) {
  const hasNutrition =
    recipe.proteinGrams !== null ||
    recipe.carbohydrateGrams !== null ||
    recipe.fatGrams !== null;

  if (!hasNutrition) return null;

  const cardClass = "rounded-2xl bg-stone-100 p-4 dark:bg-white/[0.055] dark:ring-1 dark:ring-white/[0.06]";

  return (
    <section className="mt-9">
      <h2 className="text-xl font-semibold text-stone-900 dark:text-stone-100">Nutrition</h2>
      <p className="mt-1 text-sm text-stone-500 dark:text-stone-400">Per serving</p>

      <div className="mt-4 grid grid-cols-3 gap-3">
        <div className={cardClass}>
          <p className="text-xs text-stone-500 dark:text-stone-400">Protein</p>
          <p className="mt-1 text-lg font-semibold text-stone-900 dark:text-stone-100">{recipe.proteinGrams ?? "—"}g</p>
        </div>

        <div className={cardClass}>
          <p className="text-xs text-stone-500 dark:text-stone-400">Carbs</p>
          <p className="mt-1 text-lg font-semibold text-stone-900 dark:text-stone-100">{recipe.carbohydrateGrams ?? "—"}g</p>
        </div>

        <div className={cardClass}>
          <p className="text-xs text-stone-500 dark:text-stone-400">Fat</p>
          <p className="mt-1 text-lg font-semibold text-stone-900 dark:text-stone-100">{recipe.fatGrams ?? "—"}g</p>
        </div>
      </div>
    </section>
  );
}

export default RecipeNutrition;