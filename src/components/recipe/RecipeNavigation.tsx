import { ChevronLeft, ChevronRight, Grid2X2 } from "lucide-react";
import { Link } from "react-router-dom";
import type { RecipeSummary } from "@/types/recipe";

interface RecipeNavigationProps {
    previousRecipe: RecipeSummary | null;
    nextRecipe: RecipeSummary | null;
    mealsUrl: string;
    onNavigate: (recipe: RecipeSummary) => void;
  }

function RecipeNavigation({
  previousRecipe,
  nextRecipe,
  mealsUrl,
  onNavigate
}: RecipeNavigationProps) {
  return (
    <section className="mt-12 border-t border-stone-200 pt-6 dark:border-white/[0.07]">
      <div className="grid grid-cols-2 gap-3">
        {previousRecipe ? (
          <button type="button" onClick={() => onNavigate(previousRecipe)} className="flex min-w-0 items-center gap-3 rounded-2xl bg-stone-100 p-4 text-left transition hover:bg-stone-200 dark:bg-white/[0.055] dark:hover:bg-white/[0.08]">
            <ChevronLeft size={20} className="shrink-0 text-emerald-800 dark:text-emerald-400" />

            <div className="min-w-0">
              <p className="text-xs text-stone-500 dark:text-stone-400">Previous</p>
              <p className="mt-0.5 truncate text-sm font-medium text-stone-900 dark:text-stone-100">{previousRecipe.name}</p>
            </div>
          </button>
        ) : (
          <div />
        )}

        {nextRecipe ? (
          <button type="button" onClick={() => onNavigate(nextRecipe)} className="flex min-w-0 items-center justify-end gap-3 rounded-2xl bg-stone-100 p-4 text-right transition hover:bg-stone-200 dark:bg-white/[0.055] dark:hover:bg-white/[0.08]">
            <div className="min-w-0">
              <p className="text-xs text-stone-500 dark:text-stone-400">Next</p>
              <p className="mt-0.5 truncate text-sm font-medium text-stone-900 dark:text-stone-100">{nextRecipe.name}</p>
            </div>

            <ChevronRight size={20} className="shrink-0 text-emerald-800 dark:text-emerald-400" />
          </button>
        ) : (
          <div />
        )}
      </div>

      <Link to={mealsUrl} className="mt-4 flex h-12 w-full items-center justify-center gap-2 rounded-2xl text-sm font-medium text-emerald-800 transition hover:bg-stone-100 dark:text-emerald-400 dark:hover:bg-white/[0.05]">
        <Grid2X2 size={17} />
        View all meals
      </Link>

      <p className="mt-2 text-center text-xs text-stone-400 dark:text-stone-500">Swipe left or right to move between recipes</p>
    </section>
  );
}

export default RecipeNavigation;