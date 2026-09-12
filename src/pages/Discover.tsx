import { useState } from "react";
import type { SubmitEvent } from "react";
import { ArrowLeft, Search, Sparkles, UtensilsCrossed } from "lucide-react";
import { useNavigate, useSearchParams } from "react-router-dom";
import ThemeToggle from "@/components/ThemeToggle";
import DiscoverRecipeCard from "@/components/discover/DiscoverRecipeCard";
import { searchDiscoverRecipes } from "@/api/discover";
import type { DiscoverRecipeSummary } from "@/types/discover";
import {
  cacheDiscoverRecipes,
  getCachedDiscoverRecipes,
} from "@/lib/discoverCache";

function DiscoverPage() {
  const navigate = useNavigate();
  const [searchParams, setSearchParams] = useSearchParams();

  const initialQuery = searchParams.get("q") ?? "";

  const [query, setQuery] = useState(initialQuery);
  const initialRecipes = getCachedDiscoverRecipes(initialQuery);

  const [recipes, setRecipes] = useState<DiscoverRecipeSummary[]>(
    initialRecipes ?? [],
  );
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(false);
  const [hasSearched, setHasSearched] = useState(
    Boolean(initialQuery && initialRecipes),
  );

  async function search(value: string) {
    const trimmed = value.trim();
  
    if (!trimmed) return;
  
    const cached = getCachedDiscoverRecipes(trimmed);
  
    if (cached) {
      setSearchParams({ q: trimmed }, { replace: true });
      setRecipes(cached);
      setHasSearched(true);
      setError(false);
      return;
    }
  
    try {
      setLoading(true);
      setError(false);
      setHasSearched(true);
  
      setSearchParams({ q: trimmed }, { replace: true });
  
      const results = await searchDiscoverRecipes(trimmed);
  
      setRecipes(results);
      cacheDiscoverRecipes(trimmed, results);
    } catch (error) {
      console.error("Discover search error:", error);
      setRecipes([]);
      setError(true);
    } finally {
      setLoading(false);
    }
  }

  function handleSubmit(event: SubmitEvent<HTMLFormElement>) {
    event.preventDefault();
    void search(query);
  }
  
  return (
    <main className="mx-auto min-h-dvh w-full max-w-5xl px-4 pb-28 pt-8 transition-colors sm:px-6 sm:pt-12">
      <header className="mb-8 flex items-start justify-between gap-4">
        <div className="flex items-start gap-3">
          <button type="button" aria-label="Back to meals" onClick={() => navigate("/meals")} className="mt-1 flex h-10 w-10 shrink-0 items-center justify-center rounded-full bg-stone-100 text-stone-600 transition hover:bg-stone-200 dark:bg-white/[0.06] dark:text-stone-300 dark:hover:bg-white/[0.1]">
            <ArrowLeft size={19} />
          </button>

          <div>
            <p className="text-sm text-stone-500 dark:text-stone-400">
              Find something new
            </p>
            <h1 className="text-3xl font-semibold tracking-tight text-stone-900 dark:text-stone-100">
              Discover
            </h1>
          </div>
        </div>

        <ThemeToggle />
      </header>

      <form onSubmit={handleSubmit} className="mb-8">
        <div className="flex items-center gap-2 rounded-2xl border border-stone-200 bg-white p-2 shadow-sm dark:border-white/[0.07] dark:bg-white/[0.045]">
          <Search size={19} className="ml-2 shrink-0 text-stone-400" />

          <input
            type="search"
            value={query}
            onChange={(event) => setQuery(event.target.value)}
            placeholder="Search chicken, pasta, curry..."
            className="min-w-0 flex-1 bg-transparent px-2 py-2 text-sm text-stone-900 outline-none placeholder:text-stone-400 dark:text-stone-100 dark:placeholder:text-stone-500"
          />

          <button type="submit" disabled={loading || !query.trim()} className="shrink-0 rounded-xl bg-emerald-800 px-4 py-2 text-sm font-semibold text-white transition hover:bg-emerald-700 disabled:cursor-not-allowed disabled:opacity-40 dark:bg-emerald-700 dark:hover:bg-emerald-600">
            {loading ? "Searching..." : "Search"}
          </button>
        </div>
      </form>

      {!hasSearched && (
        <div className="rounded-3xl bg-stone-100 p-10 text-center dark:bg-white/[0.045] dark:ring-1 dark:ring-white/[0.06]">
          <Sparkles size={30} className="mx-auto text-emerald-700 dark:text-emerald-400" />
          <p className="mt-4 font-medium text-stone-900 dark:text-stone-100">
            What are you in the mood for?
          </p>
          <p className="mx-auto mt-1 max-w-sm text-sm leading-relaxed text-stone-500 dark:text-stone-400">
            Search recipes from around the world and add the ones you like to Nook.
          </p>
        </div>
      )}

      {loading && (
        <div className="rounded-3xl bg-stone-100 p-6 text-sm text-stone-500 dark:bg-white/[0.045] dark:text-stone-400 dark:ring-1 dark:ring-white/[0.06]">
          Searching recipes...
        </div>
      )}

      {!loading && error && (
        <div className="rounded-3xl bg-stone-100 p-6 dark:bg-white/[0.045] dark:ring-1 dark:ring-white/[0.06]">
          <p className="font-medium text-stone-900 dark:text-stone-100">
            Couldn't search recipes
          </p>
          <p className="mt-1 text-sm text-stone-500 dark:text-stone-400">
            Check that the Nook API is running and try again.
          </p>
        </div>
      )}

      {!loading && !error && hasSearched && recipes.length === 0 && (
        <div className="rounded-3xl bg-stone-100 p-8 text-center dark:bg-white/[0.045] dark:ring-1 dark:ring-white/[0.06]">
          <UtensilsCrossed size={28} className="mx-auto text-stone-400 dark:text-stone-500" />
          <p className="mt-4 font-medium text-stone-900 dark:text-stone-100">
            No recipes found
          </p>
          <p className="mt-1 text-sm text-stone-500 dark:text-stone-400">
            Try a different dish or ingredient.
          </p>
        </div>
      )}

      {!loading && !error && recipes.length > 0 && (
        <>
          <p className="mb-4 text-sm text-stone-500 dark:text-stone-400">
            {recipes.length} {recipes.length === 1 ? "recipe" : "recipes"} found
          </p>

          <section className="grid grid-cols-1 gap-5 sm:grid-cols-2 lg:grid-cols-3">
            {recipes.map((recipe) => (
              <DiscoverRecipeCard key={`${recipe.provider}-${recipe.externalId}`} recipe={recipe} />
            ))}
          </section>
        </>
      )}
    </main>
  );
}

export default DiscoverPage;