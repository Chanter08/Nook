import { Clock, Flame, UtensilsCrossed } from "lucide-react";
import { Link } from "react-router-dom";
import type { RecipeSummary } from "@/types/recipe";

interface RecipeCardProps {
  recipe: RecipeSummary;
  query?: string;
}

function RecipeCard({ recipe, query = "" }: RecipeCardProps) {
    return (
    <Link to={`/meals/${recipe.id}${query}`} className="group overflow-hidden rounded-3xl bg-white transition hover:-translate-y-0.5 hover:shadow-md dark:bg-white/[0.045] dark:ring-1 dark:ring-white/[0.06]">
      <div className="relative aspect-[4/3] overflow-hidden bg-stone-100 dark:bg-white/[0.04]">
        {recipe.imageUrl ? (
          <img src={recipe.imageUrl} alt={recipe.name} className="h-full w-full object-cover transition duration-300 group-hover:scale-[1.02]" />
        ) : (
          <div className="flex h-full w-full items-center justify-center text-stone-400 dark:text-stone-500">
            <UtensilsCrossed size={32} />
          </div>
        )}
      </div>

      <div className="p-4">
        {recipe.mealTypes.length > 0 && (
          <div className="mb-2 flex flex-wrap gap-1.5">
            {recipe.mealTypes.slice(0, 2).map((type) => (
              <span key={type} className="rounded-full bg-emerald-50 px-2.5 py-1 text-[11px] font-medium text-emerald-800 dark:bg-emerald-400/10 dark:text-emerald-300">
                {type}
              </span>
            ))}
          </div>
        )}

        <h2 className="text-lg font-semibold leading-snug text-stone-900 dark:text-stone-100">{recipe.name}</h2>

        {recipe.cuisines.length > 0 && (
          <p className="mt-1 text-xs text-stone-500 dark:text-stone-400">{recipe.cuisines.join(" · ")}</p>
        )}

        {recipe.description && (
          <p className="mt-2 line-clamp-2 text-sm leading-relaxed text-stone-600 dark:text-stone-400">{recipe.description}</p>
        )}

        <div className="mt-4 flex items-center gap-4 text-xs text-stone-500 dark:text-stone-400">
          {recipe.totalTimeMinutes > 0 && (
            <span className="flex items-center gap-1.5">
              <Clock size={14} />
              {recipe.totalTimeMinutes} min
            </span>
          )}

          {recipe.calories !== null && (
            <span className="flex items-center gap-1.5">
              <Flame size={14} />
              {recipe.calories} kcal
            </span>
          )}
        </div>
      </div>
    </Link>
  );
}

export default RecipeCard;