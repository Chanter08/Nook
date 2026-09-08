import { useEffect, useMemo, useState } from "react";
import { Plus, Search, UtensilsCrossed } from "lucide-react";
import {useNavigate, useSearchParams } from "react-router-dom";
import ThemeToggle from "@/components/ThemeToggle";
import RecipeCard from "@/components/meals/RecipeCard";
import RecipeFilters from "@/components/meals/RecipeFilters";
import { filterRecipes, getRecipeFilterQuery } from "@/lib/recipeFilters";
import type { RecipeSummary } from "@/types/recipe";

function MealsPage() {
  const navigate = useNavigate();

  const [recipes, setRecipes] = useState<RecipeSummary[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(false);
  const [searchParams, setSearchParams] = useSearchParams();

  const searchTerm = searchParams.get("q") ?? "";
  const selectedMealType = searchParams.get("mealType") ?? "All";
  const selectedCuisine = searchParams.get("cuisine") ?? "All";

  useEffect(() => {
    async function loadRecipes() {
      try {
        setError(false);

        const response = await fetch("/api/recipes");

        if (!response.ok) {
          throw new Error(`Failed to load recipes: ${response.status}`);
        }

        const data: RecipeSummary[] = await response.json();
        setRecipes(data);
      } catch (error) {
        console.error("Recipe error:", error);
        setError(true);
      } finally {
        setLoading(false);
      }
    }

    void loadRecipes();
  }, []);

  const mealTypes = useMemo(
    () =>
      Array.from(new Set(recipes.flatMap((recipe) => recipe.mealTypes))).sort(),
    [recipes],
  );

  const cuisines = useMemo(
    () =>
      Array.from(new Set(recipes.flatMap((recipe) => recipe.cuisines))).sort(),
    [recipes],
  );

  const filteredRecipes = useMemo(
    () =>
      filterRecipes(recipes, {
        searchTerm,
        mealType: selectedMealType,
        cuisine: selectedCuisine,
      }),
    [recipes, searchTerm, selectedMealType, selectedCuisine],
  );

  const hasFilters =
    searchTerm.trim() !== "" ||
    selectedMealType !== "All" ||
    selectedCuisine !== "All";

  const recipeQuery = getRecipeFilterQuery({
    searchTerm,
    mealType: selectedMealType,
    cuisine: selectedCuisine,
  });

  function updateFilter(key: string, value: string, defaultValue = "") {
    const next = new URLSearchParams(searchParams);

    if (!value || value === defaultValue) {
      next.delete(key);
    } else {
      next.set(key, value);
    }

    setSearchParams(next, { replace: true });
  }

  function clearFilters() {
    setSearchParams(new URLSearchParams(), { replace: true });
  }

  return (
    <main className="mx-auto min-h-dvh w-full max-w-5xl px-4 pb-28 pt-8 transition-colors sm:px-6 sm:pt-12">
      <header className="mb-8 flex items-start justify-between">
        <div>
          <p className="text-sm text-stone-500 dark:text-stone-400">
            What's cooking?
          </p>
          <h1 className="text-3xl font-semibold tracking-tight text-stone-900 dark:text-stone-100">
            Meals
          </h1>
        </div>

        <div className="flex items-center gap-2">
          <ThemeToggle />

          <button
            type="button"
            aria-label="Add recipe"
            onClick={() => navigate("/meals/new")}
            className="flex h-11 w-11 items-center justify-center rounded-full bg-emerald-900 text-white transition hover:bg-emerald-800 dark:bg-emerald-700 dark:hover:bg-emerald-600"
          >
            <Plus size={21} />
          </button>
        </div>
      </header>

      <RecipeFilters
        searchTerm={searchTerm}
        selectedMealType={selectedMealType}
        selectedCuisine={selectedCuisine}
        mealTypes={mealTypes}
        cuisines={cuisines}
        hasFilters={hasFilters}
        onSearchChange={(value) => updateFilter("q", value)}
        onMealTypeChange={(value) => updateFilter("mealType", value, "All")}
        onCuisineChange={(value) => updateFilter("cuisine", value, "All")}
        onClear={clearFilters}
      />

      {!loading && !error && (
        <p className="mb-4 text-sm text-stone-500 dark:text-stone-400">
          {filteredRecipes.length}{" "}
          {filteredRecipes.length === 1 ? "recipe" : "recipes"}
          {hasFilters && recipes.length !== filteredRecipes.length && (
            <span className="text-stone-400 dark:text-stone-500">
              {" "}
              of {recipes.length}
            </span>
          )}
        </p>
      )}

      {loading && (
        <div className="rounded-3xl bg-stone-100 p-6 text-sm text-stone-500 dark:bg-white/[0.05] dark:text-stone-400 dark:ring-1 dark:ring-white/[0.06]">
          Loading recipes...
        </div>
      )}

      {!loading && error && (
        <div className="rounded-3xl bg-stone-100 p-6 dark:bg-white/[0.05] dark:ring-1 dark:ring-white/[0.06]">
          <p className="font-medium text-stone-900 dark:text-stone-100">
            Couldn't load your recipes
          </p>
          <p className="mt-1 text-sm text-stone-500 dark:text-stone-400">
            Check that the Nook API and SQL Server are running.
          </p>
        </div>
      )}

      {!loading && !error && recipes.length === 0 && (
        <div className="rounded-3xl bg-stone-100 p-8 text-center dark:bg-white/[0.05] dark:ring-1 dark:ring-white/[0.06]">
          <UtensilsCrossed
            size={28}
            className="mx-auto text-stone-400 dark:text-stone-500"
          />
          <p className="mt-4 font-medium text-stone-900 dark:text-stone-100">
            No recipes yet
          </p>
          <p className="mt-1 text-sm text-stone-500 dark:text-stone-400">
            Your recipes will appear here.
          </p>
        </div>
      )}

      {!loading &&
        !error &&
        recipes.length > 0 &&
        filteredRecipes.length === 0 && (
          <div className="rounded-3xl bg-stone-100 p-8 text-center dark:bg-white/[0.05] dark:ring-1 dark:ring-white/[0.06]">
            <Search
              size={28}
              className="mx-auto text-stone-400 dark:text-stone-500"
            />
            <p className="mt-4 font-medium text-stone-900 dark:text-stone-100">
              No matching recipes
            </p>
            <p className="mt-1 text-sm text-stone-500 dark:text-stone-400">
              Try changing your search or filters.
            </p>

            <button
              type="button"
              onClick={clearFilters}
              className="mt-4 text-sm font-medium text-emerald-800 dark:text-emerald-400"
            >
              Clear filters
            </button>
          </div>
        )}

      {!loading && !error && filteredRecipes.length > 0 && (
        <section className="grid grid-cols-1 gap-5 sm:grid-cols-2 lg:grid-cols-3">
          {filteredRecipes.map((recipe) => (
            <RecipeCard key={recipe.id} recipe={recipe} query={recipeQuery} />
          ))}
        </section>
      )}
    </main>
  );
}

export default MealsPage;