import { Clock, Flame, UtensilsCrossed } from "lucide-react";
import { Link } from "react-router-dom";
import type { RecipeSummary } from "@/types/recipe";

interface RecipeCardProps {
  recipe: RecipeSummary;
  query?: string;
}

function RecipeCard({ recipe, query }: RecipeCardProps) {
  const hasMacros =
    recipe.proteinGrams !== null ||
    recipe.carbohydrateGrams !== null ||
    recipe.fatGrams !== null;

  return (
    <Link to={`/meals/${recipe.id}${query}`} className="block h-full">
      <article className="flex h-full flex-col overflow-hidden rounded-3xl border border-stone-200 bg-white transition hover:-translate-y-0.5 hover:shadow-md dark:border-white/[0.07] dark:bg-white/[0.045] dark:ring-1 dark:ring-white/[0.025] dark:hover:bg-white/[0.065] dark:hover:shadow-black/30">
        {recipe.imageUrl ? (
          <img src={recipe.imageUrl} alt={recipe.name} className="h-48 w-full object-cover" />
        ) : (
          <div className="flex h-48 w-full items-center justify-center bg-stone-100 text-stone-400 dark:bg-white/[0.05] dark:text-stone-500">
            <UtensilsCrossed size={30} />
          </div>
        )}

        <div className="flex flex-1 flex-col p-5">
          <div className="flex-1">
            {recipe.mealTypes.length > 0 && (
              <div className="mb-3 flex flex-wrap gap-2">
                {recipe.mealTypes.map((type) => (
                  <span key={type} className="rounded-full bg-emerald-50 px-3 py-1 text-xs font-medium text-emerald-800 dark:bg-emerald-400/10 dark:text-emerald-300">
                    {type}
                  </span>
                ))}
              </div>
            )}

            <h2 className="text-lg font-semibold leading-snug text-stone-900 dark:text-stone-100">{recipe.name}</h2>

            {recipe.cuisines.length > 0 && (
              <p className="mt-1 text-sm text-stone-500 dark:text-stone-400">{recipe.cuisines.join(" · ")}</p>
            )}

            {recipe.description && (
              <p className="mt-3 line-clamp-2 text-sm leading-relaxed text-stone-600 dark:text-stone-300">{recipe.description}</p>
            )}
          </div>

          <div className="mt-5 flex flex-wrap gap-4 text-sm text-stone-500 dark:text-stone-400">
            {recipe.totalTimeMinutes > 0 && (
              <div className="flex items-center gap-1.5">
                <Clock size={16} className="text-stone-500 dark:text-emerald-400" />
                <span>{recipe.totalTimeMinutes} min</span>
              </div>
            )}

            {recipe.calories !== null && (
              <div className="flex items-center gap-1.5">
                <Flame size={16} className="text-stone-500 dark:text-emerald-400" />
                <span>{recipe.calories} kcal</span>
              </div>
            )}
          </div>

          {hasMacros && (
            <div className="mt-4 flex flex-wrap gap-x-4 gap-y-1 border-t border-stone-100 pt-4 text-xs text-stone-500 dark:border-white/[0.07] dark:text-stone-400">
              {recipe.proteinGrams !== null && (
                <span><strong className="font-semibold text-stone-800 dark:text-stone-100">{recipe.proteinGrams}g</strong> protein</span>
              )}

              {recipe.carbohydrateGrams !== null && (
                <span><strong className="font-semibold text-stone-800 dark:text-stone-100">{recipe.carbohydrateGrams}g</strong> carbs</span>
              )}

              {recipe.fatGrams !== null && (
                <span><strong className="font-semibold text-stone-800 dark:text-stone-100">{recipe.fatGrams}g</strong> fat</span>
              )}
            </div>
          )}
        </div>
      </article>
    </Link>
  );
}

export default RecipeCard;