import { useEffect, useMemo, useRef, useState } from "react";
import { ArrowLeft, CalendarPlus } from "lucide-react";
import { useNavigate, useParams, useSearchParams } from "react-router-dom";
import { filterRecipes, getRecipeFilterQuery } from "@/lib/recipeFilters";
import AddIngredientsModal from "@/components/recipe/AddIngredientsModal";
import RecipeHero from "@/components/recipe/RecipeHero";
import RecipeIngredients from "@/components/recipe/RecipeIngredients";
import RecipeMethod from "@/components/recipe/RecipeMethod";
import RecipeNavigation from "@/components/recipe/RecipeNavigation";
import RecipeNutrition from "@/components/recipe/RecipeNutrition";
import RecipeStats from "@/components/recipe/RecipeStats";
import type { RecipeDetail, RecipeSummary } from "@/types/recipe";
import PlanRecipeModal from "@/components/recipe/PlanRecipeModal";

interface SwipeStart {
  x: number;
  y: number;
}

function RecipePage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const swipeStart = useRef<SwipeStart | null>(null);

  const [recipe, setRecipe] = useState<RecipeDetail | null>(null);
  const [recipes, setRecipes] = useState<RecipeSummary[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(false);

  const [shoppingPickerOpen, setShoppingPickerOpen] = useState(false);
  const [selectedIngredientIds, setSelectedIngredientIds] = useState<number[]>(
    [],
  );
  const [addingToShopping, setAddingToShopping] = useState(false);
  const [addedToShopping, setAddedToShopping] = useState(false);
  const [shoppingError, setShoppingError] = useState<string | null>(null);

  const searchTerm = searchParams.get("q") ?? "";
  const selectedMealType = searchParams.get("mealType") ?? "All";
  const selectedCuisine = searchParams.get("cuisine") ?? "All";

  const [planModalOpen, setPlanModalOpen] = useState(false);
  const [plannedSuccessfully, setPlannedSuccessfully] = useState(false);

  const recipeQuery = getRecipeFilterQuery({
    searchTerm,
    mealType: selectedMealType,
    cuisine: selectedCuisine,
  });

  const navigationRecipes = useMemo(
    () =>
      filterRecipes(recipes, {
        searchTerm,
        mealType: selectedMealType,
        cuisine: selectedCuisine,
      }),
    [recipes, searchTerm, selectedMealType, selectedCuisine],
  );

  const currentIndex = navigationRecipes.findIndex(
    (item) => item.id === recipe?.id,
  );
  const previousRecipe =
    currentIndex > 0 ? navigationRecipes[currentIndex - 1] : null;
  const nextRecipe =
    currentIndex >= 0 && currentIndex < navigationRecipes.length - 1
      ? navigationRecipes[currentIndex + 1]
      : null;

  useEffect(() => {
    async function loadRecipes() {
      try {
        const response = await fetch("/api/recipes");

        if (!response.ok) {
          throw new Error(`Failed to load recipes: ${response.status}`);
        }

        const data: RecipeSummary[] = await response.json();
        setRecipes(data);
      } catch (error) {
        console.error("Recipe navigation error:", error);
      }
    }

    void loadRecipes();
  }, []);

  useEffect(() => {
    async function loadRecipe() {
      try {
        setLoading(true);
        setError(false);

        const response = await fetch(`/api/recipes/${id}`);

        if (!response.ok) {
          throw new Error(`Failed to load recipe: ${response.status}`);
        }

        const data: RecipeDetail = await response.json();
        setRecipe(data);
      } catch (error) {
        console.error("Recipe error:", error);
        setError(true);
      } finally {
        setLoading(false);
      }
    }

    void loadRecipe();
  }, [id]);

  function navigateToRecipe(target: RecipeSummary) {
    navigate(`/meals/${target.id}${recipeQuery}`);
    window.scrollTo({ top: 0, behavior: "smooth" });
  }

  function handleTouchStart(event: React.TouchEvent) {
    if (shoppingPickerOpen) return;

    const target = event.target as HTMLElement;

    if (target.closest("button, a, input, select, textarea")) {
      swipeStart.current = null;
      return;
    }

    const touch = event.touches[0];

    if (touch.clientX < 35 || touch.clientX > window.innerWidth - 35) {
      swipeStart.current = null;
      return;
    }

    swipeStart.current = {
      x: touch.clientX,
      y: touch.clientY,
    };
  }

  function handleTouchEnd(event: React.TouchEvent) {
    if (!swipeStart.current || shoppingPickerOpen) return;

    const touch = event.changedTouches[0];
    const deltaX = touch.clientX - swipeStart.current.x;
    const deltaY = touch.clientY - swipeStart.current.y;

    swipeStart.current = null;

    const horizontalDistance = Math.abs(deltaX);
    const verticalDistance = Math.abs(deltaY);

    if (horizontalDistance < 80) return;
    if (horizontalDistance <= verticalDistance * 1.25) return;

    if (deltaX < 0 && nextRecipe) {
      navigateToRecipe(nextRecipe);
    }

    if (deltaX > 0 && previousRecipe) {
      navigateToRecipe(previousRecipe);
    }
  }

  function openShoppingPicker() {
    if (!recipe) return;

    setSelectedIngredientIds(
      recipe.ingredients.map((ingredient) => ingredient.id),
    );
    setShoppingError(null);
    setShoppingPickerOpen(true);
  }

  function toggleIngredient(ingredientId: number) {
    setSelectedIngredientIds((current) =>
      current.includes(ingredientId)
        ? current.filter((id) => id !== ingredientId)
        : [...current, ingredientId],
    );
  }

  function toggleAllIngredients() {
    if (!recipe) return;

    setSelectedIngredientIds(
      selectedIngredientIds.length === recipe.ingredients.length
        ? []
        : recipe.ingredients.map((ingredient) => ingredient.id),
    );
  }

  async function addSelectedIngredientsToShopping() {
    if (!recipe || selectedIngredientIds.length === 0) return;

    try {
      setAddingToShopping(true);
      setShoppingError(null);

      const response = await fetch(`/api/shopping/from-recipe/${recipe.id}`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ recipeIngredientIds: selectedIngredientIds }),
      });

      if (!response.ok) {
        throw new Error(await response.text());
      }

      setShoppingPickerOpen(false);
      setAddedToShopping(true);
      window.setTimeout(() => setAddedToShopping(false), 2000);
    } catch (error) {
      console.error("Shopping error:", error);
      setShoppingError("Couldn't add the selected ingredients.");
    } finally {
      setAddingToShopping(false);
    }
  }

  if (loading) {
    return (
      <main className="mx-auto min-h-dvh w-full max-w-4xl px-4 pb-28 pt-8 text-stone-900 dark:text-stone-100 sm:px-6">
        <p className="text-sm text-stone-500 dark:text-stone-400">
          Loading recipe...
        </p>
      </main>
    );
  }

  if (error || !recipe) {
    return (
      <main className="mx-auto min-h-dvh w-full max-w-4xl px-4 pb-28 pt-8 sm:px-6">
        <button
          type="button"
          onClick={() => navigate(-1)}
          className="mb-6 flex items-center gap-2 text-sm font-medium text-stone-700 dark:text-stone-200"
        >
          <ArrowLeft size={18} />
          Back
        </button>

        <div className="rounded-3xl bg-stone-100 p-6 dark:bg-white/[0.05] dark:ring-1 dark:ring-white/[0.06]">
          <p className="font-medium text-stone-900 dark:text-stone-100">
            Couldn't load recipe
          </p>
          <p className="mt-1 text-sm text-stone-500 dark:text-stone-400">
            Check that the Nook API is running.
          </p>
        </div>
      </main>
    );
  }

  return (
    <main
      className="mx-auto min-h-dvh w-full max-w-4xl pb-28 text-stone-900 transition-colors dark:text-stone-100"
      onTouchStart={handleTouchStart}
      onTouchEnd={handleTouchEnd}
    >
      <RecipeHero
        name={recipe.name}
        imageUrl={recipe.imageUrl}
        onBack={() => navigate(-1)}
      />

      <div className="px-4 pt-6 sm:px-6">
        {recipe.mealTypes.length > 0 && (
          <div className="mb-3 flex flex-wrap gap-2">
            {recipe.mealTypes.map((type) => (
              <span
                key={type}
                className="rounded-full bg-emerald-50 px-3 py-1 text-xs font-medium text-emerald-800 dark:bg-emerald-400/10 dark:text-emerald-300"
              >
                {type}
              </span>
            ))}
          </div>
        )}

        <h1 className="text-3xl font-semibold tracking-tight text-stone-900 dark:text-stone-100 sm:text-4xl">
          {recipe.name}
        </h1>

        {recipe.cuisines.length > 0 && (
          <p className="mt-2 text-sm text-stone-500 dark:text-stone-400">
            {recipe.cuisines.join(" · ")}
          </p>
        )}

        {recipe.description && (
          <p className="mt-5 max-w-2xl leading-relaxed text-stone-700 dark:text-stone-300">
            {recipe.description}
          </p>
        )}

        <div className="mt-6 flex flex-wrap gap-3">
          <button
            type="button"
            onClick={() => setPlanModalOpen(true)}
            className="flex items-center gap-2 rounded-full bg-emerald-900 px-4 py-2.5 text-sm font-medium text-white transition hover:bg-emerald-800 dark:bg-emerald-700 dark:hover:bg-emerald-600"
          >
            <CalendarPlus size={17} />
            {plannedSuccessfully ? "Planned ✓" : "Plan meal"}
          </button>
        </div>
        <RecipeStats recipe={recipe} />
        <RecipeNutrition recipe={recipe} />

        <RecipeIngredients
          ingredients={recipe.ingredients}
          addingToShopping={addingToShopping}
          addedToShopping={addedToShopping}
          onAddToShopping={openShoppingPicker}
        />

        <RecipeMethod steps={recipe.steps} />

        <RecipeNavigation
          previousRecipe={previousRecipe}
          nextRecipe={nextRecipe}
          mealsUrl={`/meals${recipeQuery}`}
          onNavigate={navigateToRecipe}
        />
      </div>

      {planModalOpen && (
        <PlanRecipeModal
          recipeId={recipe.id}
          recipeName={recipe.name}
          onClose={() => setPlanModalOpen(false)}
          onPlanned={() => {
            setPlannedSuccessfully(true);
            window.setTimeout(() => setPlannedSuccessfully(false), 2000);
          }}
        />
      )}

      <AddIngredientsModal
        open={shoppingPickerOpen}
        ingredients={recipe.ingredients}
        selectedIds={selectedIngredientIds}
        loading={addingToShopping}
        error={shoppingError}
        onClose={() => setShoppingPickerOpen(false)}
        onToggle={toggleIngredient}
        onToggleAll={toggleAllIngredients}
        onSubmit={() => void addSelectedIngredientsToShopping()}
      />
    </main>
  );
}

export default RecipePage;