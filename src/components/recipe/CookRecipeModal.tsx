import { useEffect, useMemo, useState } from "react";
import { Check, CookingPot, LoaderCircle, X } from "lucide-react";
import {
  cookRecipe,
  getCookingPreview,
  type CookingPreview,
} from "@/api/cooking";

interface Selection {
  recipeIngredientId: number;
  pantryItemId: number | null;
  quantity: number;
}

interface Props {
  recipeId: number;
  mealPlanEntryId?: number | null;
  onClose: () => void;
  onCooked: () => void;
}

function CookRecipeModal({ recipeId, mealPlanEntryId = null, onClose, onCooked }: Props) {
  const [preview, setPreview] = useState<CookingPreview | null>(null);
  const [selections, setSelections] = useState<Selection[]>([]);
  const [servings, setServings] = useState("");
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let active = true;
    getCookingPreview(recipeId)
      .then((result) => {
        if (!active) return;
        applyPreview(result);
      })
      .catch((cause: unknown) => {
        if (active) setError(cause instanceof Error ? cause.message : "Couldn't load pantry stock.");
      })
      .finally(() => { if (active) setLoading(false); });
    return () => { active = false; };
  }, [recipeId]);

  function applyPreview(result: CookingPreview) {
    setPreview(result);
    setServings(String(result.servings));
    setSelections(result.ingredients.map((ingredient) => ({
      recipeIngredientId: ingredient.recipeIngredientId,
      pantryItemId: ingredient.suggestedPantryItemId,
      quantity: ingredient.suggestedQuantity ?? 0,
    })));
  }

  async function refreshServings() {
    const value = Number(servings);
    if (!Number.isFinite(value) || value <= 0 || value > 100) {
      setError("Enter a serving quantity between 0.01 and 100.");
      return;
    }
    try {
      setLoading(true);
      setError(null);
      applyPreview(await getCookingPreview(recipeId, value));
    } catch (cause) {
      setError(cause instanceof Error ? cause.message : "Couldn't update servings.");
    } finally {
      setLoading(false);
    }
  }

  function updateSelection(recipeIngredientId: number, patch: Partial<Selection>) {
    setSelections((current) => current.map((entry) =>
      entry.recipeIngredientId === recipeIngredientId ? { ...entry, ...patch } : entry,
    ));
  }

  const issue = useMemo(() => {
    if (!preview) return null;
    if (Number(servings) !== preview.servings) {
      return "Click Update amounts to apply the new serving count.";
    }
    const totals = new Map<number, number>();
    for (const selection of selections) {
      if (!Number.isFinite(selection.quantity) || selection.quantity < 0 ||
        Math.abs(Math.round(selection.quantity * 100) - selection.quantity * 100) > 0.000001) {
        return "Quantities must be positive numbers with at most two decimal places.";
      }
      if (selection.pantryItemId === null && selection.quantity !== 0) {
        return "Choose a pantry item for every quantity you want to deduct.";
      }
      if (selection.pantryItemId !== null) {
        if (selection.quantity <= 0) return "Enter the amount used, or choose Skip.";
        totals.set(selection.pantryItemId,
          (totals.get(selection.pantryItemId) ?? 0) + selection.quantity);
      }
    }
    for (const [id, quantity] of totals) {
      const pantry = preview.pantryItems.find((item) => item.id === id);
      if (!pantry || pantry.quantity === null || pantry.quantity < quantity - 0.00001) {
        return "One or more ingredients exceed the available pantry stock.";
      }
    }
    return null;
  }, [preview, selections, servings]);

  const deductedCount = selections.filter((item) => item.pantryItemId !== null).length;

  async function confirm() {
    if (!preview || issue || submitting || loading) return;
    try {
      setSubmitting(true);
      setError(null);
      await cookRecipe(recipeId, {
        servings: preview.servings,
        mealPlanEntryId,
        items: selections,
      });
      onCooked();
      onClose();
    } catch (cause) {
      setError(cause instanceof Error ? cause.message : "Couldn't update the pantry.");
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <div className="fixed inset-0 z-[100] flex items-end justify-center bg-black/50 backdrop-blur-sm sm:items-center sm:p-4">
      <section role="dialog" aria-modal="true" aria-label="Mark recipe as cooked"
        className="flex max-h-[92dvh] w-full max-w-2xl flex-col rounded-t-3xl bg-white shadow-xl dark:bg-[#202521] sm:rounded-3xl">
        <header className="flex items-start justify-between gap-3 border-b border-stone-100 px-5 py-4 dark:border-white/10">
          <div>
            <h2 className="flex items-center gap-2 text-lg font-semibold text-stone-900 dark:text-stone-100">
              <CookingPot size={20} /> Mark as cooked
            </h2>
            <p className="mt-1 text-xs text-stone-500 dark:text-stone-400">
              Review actual ingredients used. Nothing is deducted until confirmation.
            </p>
          </div>
          <button aria-label="Close" type="button" disabled={submitting} onClick={onClose}
            className="rounded-full p-2 text-stone-500 hover:bg-stone-100 dark:hover:bg-white/10">
            <X size={20} />
          </button>
        </header>

        <div className="overflow-y-auto px-5 py-5">
          <div className="mb-5 flex items-end gap-3">
            <label className="text-sm font-medium text-stone-700 dark:text-stone-300">
              Servings prepared
              <input type="number" min="0.01" max="100" step="0.01"
                value={servings} onChange={(event) => setServings(event.target.value)}
                className="mt-1 block h-10 w-28 rounded-xl border border-stone-200 bg-transparent px-3 dark:border-white/15" />
            </label>
            <button type="button" onClick={() => void refreshServings()} disabled={loading || submitting}
              className="h-10 rounded-xl bg-stone-100 px-4 text-sm font-medium text-stone-700 disabled:opacity-50 dark:bg-white/10 dark:text-stone-200">
              Update amounts
            </button>
          </div>
          <p className="mb-4 text-xs text-stone-500 dark:text-stone-400">
            Changing servings resets ingredient selections. Suggested amounts appear only for clear name and unit matches.
            Choose Skip for anything you didn't use, or select a different pantry item for substitutions.
          </p>

          {loading && <div className="flex items-center gap-2 text-sm text-stone-500"><LoaderCircle className="animate-spin" size={16} /> Loading pantry...</div>}
          {preview && !loading && (
            <div className="space-y-3">
              {preview.ingredients.map((ingredient) => {
                const selection = selections.find((item) => item.recipeIngredientId === ingredient.recipeIngredientId);
                if (!selection) return null;
                const chosen = preview.pantryItems.find((item) => item.id === selection.pantryItemId);
                return (
                  <div key={ingredient.recipeIngredientId}
                    className="rounded-2xl border border-stone-200 p-4 dark:border-white/10">
                    <div className="mb-3">
                      <p className="font-medium text-stone-900 dark:text-stone-100">{ingredient.name}</p>
                      <p className="text-xs text-stone-500">
                        Recipe: {ingredient.requiredQuantity ?? "as needed"} {ingredient.unit ?? ""}
                      </p>
                    </div>
                    <div className="grid grid-cols-[1fr_110px] gap-2">
                      <label className="min-w-0 text-xs text-stone-500">
                        Pantry item / substitution
                        <select value={selection.pantryItemId ?? ""}
                          onChange={(event) => {
                            const id = event.target.value ? Number(event.target.value) : null;
                            updateSelection(ingredient.recipeIngredientId, {
                              pantryItemId: id,
                              quantity: id === ingredient.suggestedPantryItemId
                                ? ingredient.suggestedQuantity ?? 0 : 0,
                            });
                          }}
                          className="mt-1 h-10 w-full rounded-xl border border-stone-200 bg-white px-2 text-sm text-stone-800 dark:border-white/15 dark:bg-[#292e2a] dark:text-stone-100">
                          <option value="">Skip — do not deduct</option>
                          {preview.pantryItems.filter((item) => item.quantity !== null && item.quantity > 0)
                            .map((item) => <option value={item.id} key={item.id}>
                              {item.name} · {item.quantity} {item.unit ?? ""} ({item.storageLocation})
                            </option>)}
                        </select>
                      </label>
                      <label className="text-xs text-stone-500">
                        Used {chosen?.unit ?? ""}
                        <input type="number" min="0" step="0.01"
                          disabled={selection.pantryItemId === null}
                          value={selection.pantryItemId === null ? "" : selection.quantity}
                          onChange={(event) => updateSelection(ingredient.recipeIngredientId, {
                            quantity: Number(event.target.value),
                          })}
                          className="mt-1 h-10 w-full rounded-xl border border-stone-200 bg-transparent px-2 text-sm text-stone-900 disabled:opacity-40 dark:border-white/15 dark:text-stone-100" />
                      </label>
                    </div>
                    {chosen && <p className="mt-2 text-xs text-stone-500">
                      Available: {chosen.quantity} {chosen.unit ?? ""}. Amounts are entered in pantry units.
                    </p>}
                    {!chosen && <p className="mt-2 text-xs text-amber-700 dark:text-amber-400">
                      Not deducted. Choose an item if you used stock tracked in Nook.
                    </p>}
                  </div>
                );
              })}
            </div>
          )}
          {(issue || error) && <p role="alert"
            className="mt-4 rounded-xl bg-red-50 p-3 text-sm text-red-700 dark:bg-red-400/10 dark:text-red-300">
            {issue || error}
          </p>}
        </div>

        <footer className="border-t border-stone-100 px-5 py-4 dark:border-white/10">
          <button type="button" disabled={loading || submitting || !preview || !!issue}
            onClick={() => void confirm()}
            className="flex h-12 w-full items-center justify-center gap-2 rounded-2xl bg-emerald-900 font-medium text-white disabled:opacity-40 dark:bg-emerald-700">
            {submitting ? <LoaderCircle size={17} className="animate-spin" /> : <Check size={17} />}
            {submitting ? "Saving..." : `Confirm cooked · deduct ${deductedCount} ${deductedCount === 1 ? "ingredient" : "ingredients"}`}
          </button>
          <p className="mt-2 text-center text-xs text-stone-500">
            Unused ingredients remain in stock. You can undo this cooking record from the recipe page.
          </p>
        </footer>
      </section>
    </div>
  );
}

export default CookRecipeModal;