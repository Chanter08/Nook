import { ShoppingBasket } from "lucide-react";
import { formatIngredientAmount } from "@/lib/recipe";
import type { RecipeIngredient } from "@/types/recipe";

interface RecipeIngredientsProps {
  ingredients: RecipeIngredient[];
  addingToShopping: boolean;
  addedToShopping: boolean;
  onAddToShopping: () => void;
}

function RecipeIngredients({
  ingredients,
  addingToShopping,
  addedToShopping,
  onAddToShopping
}: RecipeIngredientsProps) {
  return (
    <section className="mt-10">
      <div className="flex items-end justify-between gap-4">
        <div>
          <h2 className="text-xl font-semibold text-stone-900 dark:text-stone-100">Ingredients</h2>
          <p className="mt-1 text-sm text-stone-500 dark:text-stone-400">
            {ingredients.length} {ingredients.length === 1 ? "ingredient" : "ingredients"}
          </p>
        </div>

        <button type="button" onClick={onAddToShopping} disabled={addingToShopping} className="flex shrink-0 items-center gap-2 rounded-full bg-emerald-900 px-4 py-2.5 text-sm font-medium text-white transition hover:bg-emerald-800 disabled:cursor-not-allowed disabled:opacity-60 dark:bg-emerald-700 dark:hover:bg-emerald-600">
          <ShoppingBasket size={17} />
          {addingToShopping ? "Adding..." : addedToShopping ? "Added ✓" : "Add to shopping"}
        </button>
      </div>

      <div className="mt-4 grid grid-cols-2 gap-3">
        {ingredients.map((ingredient) => (
          <div key={ingredient.id} className="grid grid-cols-[minmax(0,1fr)_auto] items-center gap-2 rounded-2xl border border-stone-200 bg-white p-3 dark:border-white/[0.07] dark:bg-white/[0.045] dark:ring-1 dark:ring-white/[0.025]">
            <p className="min-w-0 text-sm leading-snug text-stone-800 dark:text-stone-200">{ingredient.name}</p>
            <p className="max-w-[120px] text-right text-sm leading-snug text-stone-500 dark:text-stone-400">{formatIngredientAmount(ingredient)}</p>
          </div>
        ))}
      </div>
    </section>
  );
}

export default RecipeIngredients;