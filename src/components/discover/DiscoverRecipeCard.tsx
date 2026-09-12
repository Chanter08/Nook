import { MapPin, UtensilsCrossed } from "lucide-react";
import { Link } from "react-router-dom";
import type { DiscoverRecipeSummary } from "@/types/discover";

interface DiscoverRecipeCardProps {
  recipe: DiscoverRecipeSummary;
}

function DiscoverRecipeCard({ recipe }: DiscoverRecipeCardProps) {
  return (
    <Link to={`/discover/${recipe.externalId}`} className="block h-full">
      <article className="flex h-full flex-col overflow-hidden rounded-3xl border border-stone-200 bg-white transition hover:-translate-y-0.5 hover:shadow-md dark:border-white/[0.07] dark:bg-white/[0.045] dark:ring-1 dark:ring-white/[0.025] dark:hover:bg-white/[0.065] dark:hover:shadow-black/30">
        {recipe.imageUrl ? (
          <img src={recipe.imageUrl} alt={recipe.name} className="h-48 w-full object-cover" />
        ) : (
          <div className="flex h-48 w-full items-center justify-center bg-stone-100 text-stone-400 dark:bg-white/[0.05] dark:text-stone-500">
            <UtensilsCrossed size={30} />
          </div>
        )}

        <div className="flex flex-1 flex-col p-5">
          {recipe.category && (
            <div className="mb-3">
              <span className="rounded-full bg-emerald-50 px-3 py-1 text-xs font-medium text-emerald-800 dark:bg-emerald-400/10 dark:text-emerald-300">
                {recipe.category}
              </span>
            </div>
          )}

          <h2 className="text-lg font-semibold leading-snug text-stone-900 dark:text-stone-100">
            {recipe.name}
          </h2>

          {recipe.cuisine && (
            <div className="mt-2 flex items-center gap-1.5 text-sm text-stone-500 dark:text-stone-400">
              <MapPin size={15} />
              <span>{recipe.cuisine}</span>
            </div>
          )}
        </div>
      </article>
    </Link>
  );
}

export default DiscoverRecipeCard;