import { useEffect, useState } from "react";
import { ArrowLeft, ExternalLink, Play, UtensilsCrossed } from "lucide-react";
import { useNavigate, useParams } from "react-router-dom";
import { getDiscoverRecipe } from "@/api/discover";
import type { DiscoverRecipeDetail } from "@/types/discover";
import RecipeIngredients from "@/components/recipe/RecipeIngredients";
import RecipeMethod from "@/components/recipe/RecipeMethod";
import RecipeStats from "@/components/recipe/RecipeStats";
import RecipeNutrition from "@/components/recipe/RecipeNutrition";

function DiscoverRecipePage() {
  const { externalId } = useParams();
  const navigate = useNavigate();

  const [recipe, setRecipe] = useState<DiscoverRecipeDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(false);

  useEffect(() => {
    async function loadRecipe() {
      if (!externalId) {
        setError(true);
        setLoading(false);
        return;
      }

      try {
        setLoading(true);
        setError(false);

        const data = await getDiscoverRecipe(externalId);
        setRecipe(data);
      } catch (error) {
        console.error("Discover recipe error:", error);
        setError(true);
      } finally {
        setLoading(false);
      }
    }

    void loadRecipe();
  }, [externalId]);

  if (loading) {
    return (
      <main className="mx-auto min-h-dvh w-full max-w-4xl px-4 pb-28 pt-8 sm:px-6">
        <p className="text-sm text-stone-500 dark:text-stone-400">
          Loading recipe...
        </p>
      </main>
    );
  }

  if (error || !recipe) {
    return (
      <main className="mx-auto min-h-dvh w-full max-w-4xl px-4 pb-28 pt-8 sm:px-6">
        <button type="button" onClick={() => navigate(-1)} className="mb-6 flex items-center gap-2 text-sm font-medium text-stone-700 dark:text-stone-200">
          <ArrowLeft size={18} />
          Back
        </button>

        <div className="rounded-3xl bg-stone-100 p-6 dark:bg-white/[0.05] dark:ring-1 dark:ring-white/[0.06]">
          <p className="font-medium text-stone-900 dark:text-stone-100">
            Couldn't load recipe
          </p>
          <p className="mt-1 text-sm text-stone-500 dark:text-stone-400">
            The recipe may no longer be available.
          </p>
        </div>
      </main>
    );
  }

  return (
    <main className="mx-auto min-h-dvh w-full max-w-4xl pb-28 text-stone-900 dark:text-stone-100">
      <div className="relative">
        {recipe.imageUrl ? (
          <img src={recipe.imageUrl} alt={recipe.name} className="h-[320px] w-full object-cover sm:h-[420px]" />
        ) : (
          <div className="flex h-[320px] w-full items-center justify-center bg-stone-100 text-stone-400 dark:bg-white/[0.05] dark:text-stone-500 sm:h-[420px]">
            <UtensilsCrossed size={36} />
          </div>
        )}

        <div className="absolute inset-x-0 top-0 flex items-center justify-between p-4 sm:p-6">
          <button type="button" aria-label="Back" onClick={() => navigate(-1)} className="flex h-11 w-11 items-center justify-center rounded-full bg-black/45 text-white backdrop-blur transition hover:bg-black/60">
            <ArrowLeft size={20} />
          </button>

          <span className="rounded-full bg-black/45 px-3 py-1.5 text-xs font-medium text-white backdrop-blur">
            {recipe.provider}
          </span>
        </div>
      </div>

      <div className="mx-auto max-w-3xl px-4 pt-6 sm:px-6">
        <div className="flex flex-wrap gap-2">
          {recipe.category && (
            <span className="rounded-full bg-emerald-50 px-3 py-1 text-xs font-medium text-emerald-800 dark:bg-emerald-400/10 dark:text-emerald-300">
              {recipe.category}
            </span>
          )}

          {recipe.cuisine && (
            <span className="rounded-full bg-stone-100 px-3 py-1 text-xs font-medium text-stone-600 dark:bg-white/[0.06] dark:text-stone-300">
              {recipe.cuisine}
            </span>
          )}
        </div>

        <h1 className="mt-4 text-3xl font-semibold tracking-tight text-stone-900 dark:text-stone-100 sm:text-4xl">
          {recipe.name}
        </h1>

        {recipe.description && (
          <p className="mt-4 max-w-2xl leading-relaxed text-stone-600 dark:text-stone-300">
            {recipe.description}
          </p>
        )}

        {(recipe.sourceUrl || recipe.videoUrl) && (
          <div className="mt-5 flex flex-wrap gap-3">
            {recipe.sourceUrl && (
              <a href={recipe.sourceUrl} target="_blank" rel="noreferrer" className="flex items-center gap-2 rounded-full bg-stone-100 px-4 py-2 text-sm font-medium text-stone-700 transition hover:bg-stone-200 dark:bg-white/[0.06] dark:text-stone-200 dark:hover:bg-white/[0.1]">
                <ExternalLink size={16} />
                Source
              </a>
            )}

            {recipe.videoUrl && (
              <a href={recipe.videoUrl} target="_blank" rel="noreferrer" className="flex items-center gap-2 rounded-full bg-stone-100 px-4 py-2 text-sm font-medium text-stone-700 transition hover:bg-stone-200 dark:bg-white/[0.06] dark:text-stone-200 dark:hover:bg-white/[0.1]">
                <Play size={16} />
                Video
              </a>
            )}
          </div>
        )}

        <RecipeStats recipe={recipe} />
        <RecipeNutrition recipe={recipe} />

        <RecipeIngredients ingredients={recipe.ingredients} />

        <RecipeMethod steps={recipe.steps} />
      </div>
    </main>
  );
}

export default DiscoverRecipePage;