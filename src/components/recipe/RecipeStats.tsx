import { ChefHat, Clock, Flame, UsersRound } from "lucide-react";
import type { RecipeDetail } from "@/types/recipe";

interface RecipeStatsProps {
  recipe: RecipeDetail;
}

function RecipeStats({ recipe }: RecipeStatsProps) {
  const cardClass = "rounded-2xl bg-stone-100 p-4 dark:bg-white/[0.055] dark:ring-1 dark:ring-white/[0.06]";
  const iconClass = "mb-3 text-emerald-800 dark:text-emerald-400";

  return (
    <section className="mt-7 grid grid-cols-2 gap-3 sm:grid-cols-4">
      {recipe.totalTimeMinutes > 0 && (
        <div className={cardClass}>
          <Clock size={19} className={iconClass} />
          <p className="text-xs text-stone-500 dark:text-stone-400">Total time</p>
          <p className="mt-1 font-semibold text-stone-900 dark:text-stone-100">{recipe.totalTimeMinutes} min</p>
        </div>
      )}

      {recipe.servings !== null && (
        <div className={cardClass}>
          <UsersRound size={19} className={iconClass} />
          <p className="text-xs text-stone-500 dark:text-stone-400">Servings</p>
          <p className="mt-1 font-semibold text-stone-900 dark:text-stone-100">{recipe.servings}</p>
        </div>
      )}

      {recipe.calories !== null && (
        <div className={cardClass}>
          <Flame size={19} className={iconClass} />
          <p className="text-xs text-stone-500 dark:text-stone-400">Calories</p>
          <p className="mt-1 font-semibold text-stone-900 dark:text-stone-100">{recipe.calories} kcal</p>
        </div>
      )}

      <div className={cardClass}>
        <ChefHat size={19} className={iconClass} />
        <p className="text-xs text-stone-500 dark:text-stone-400">Prep / Cook</p>
        <p className="mt-1 font-semibold text-stone-900 dark:text-stone-100">
          {recipe.prepTimeMinutes ?? "—"} / {recipe.cookTimeMinutes ?? "—"} min
        </p>
      </div>
    </section>
  );
}

export default RecipeStats;