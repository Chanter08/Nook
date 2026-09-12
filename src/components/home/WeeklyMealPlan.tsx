import { Link } from "react-router-dom";
import type { PlannedMeal } from "@/types/mealPlan";
import { Plus, Trash2, UtensilsCrossed } from "lucide-react";
import { useState } from "react";
import AddMealPlanModal from "@/components/home/AddMealPlanModal";
import type { WeeklyMealPlanProps } from "@/types/weeklyMealPlan";

const mealTypeOrder = ["Breakfast", "Lunch", "Dinner", "Snack"];

function getDateKey(date: Date) {
  return [
    date.getFullYear(),
    String(date.getMonth() + 1).padStart(2, "0"),
    String(date.getDate()).padStart(2, "0"),
  ].join("-");
}

function sortMeals(meals: PlannedMeal[]) {
  return [...meals].sort((a, b) => {
    const aIndex = mealTypeOrder.indexOf(a.mealType);
    const bIndex = mealTypeOrder.indexOf(b.mealType);

    const aOrder = aIndex === -1 ? mealTypeOrder.length : aIndex;
    const bOrder = bIndex === -1 ? mealTypeOrder.length : bIndex;

    return aOrder - bOrder || a.mealType.localeCompare(b.mealType);
  });
}

function WeeklyMealPlan({
  startDate,
  meals,
  loading,
  onRefresh,
  onRemove,
  removingIds,
}: WeeklyMealPlanProps) {
  const [addingDate, setAddingDate] = useState<Date | null>(null);
  const todayKey = getDateKey(new Date());

  const days = Array.from({ length: 7 }, (_, index) => {
    const date = new Date(startDate);
    date.setDate(date.getDate() + index);
    return date;
  });

  return (
    <section id="meal-plan" className="mt-8 scroll-mt-6">
      <div className="mb-4">
        <p className="text-sm text-stone-500 dark:text-stone-400">
          What's cooking
        </p>
        <h2 className="text-xl font-semibold text-stone-900 dark:text-stone-100">
          Next 7 days
        </h2>
        <p className="mt-1 text-sm text-stone-400 dark:text-stone-500">
          Meals planned from today onwards
        </p>
      </div>

      {loading ? (
        <div className="rounded-3xl bg-stone-100 p-6 text-sm text-stone-500 dark:bg-white/[0.045] dark:text-stone-400 dark:ring-1 dark:ring-white/[0.06]">
          Loading meal plan...
        </div>
      ) : (
        <div className="-mx-6 overflow-x-auto px-6 pb-6 pt-2 sm:mx-0 sm:px-2">
          <div className="flex min-w-max snap-x snap-mandatory gap-3 sm:grid sm:min-w-0 sm:grid-cols-7 sm:gap-2">
            {days.map((date) => {
              const dateKey = getDateKey(date);
              const isToday = dateKey === todayKey;
              const dayMeals = sortMeals(
                meals.filter((meal) => meal.date === dateKey),
              );

              return (
                <article
                  key={dateKey}
                  className={`flex w-[78vw] max-w-[270px] shrink-0 snap-start flex-col overflow-hidden rounded-3xl border transition-all sm:w-auto sm:max-w-none 
                    ${
                      isToday
                        ? "relative z-10 border-emerald-400 bg-white shadow-lg shadow-black/10 sm:-translate-y-1 dark:border-emerald-500/40 dark:bg-white/[0.035] dark:shadow-black/30"
                        : "border-stone-200 bg-white dark:border-white/[0.07] dark:bg-white/[0.035]"
                    }`}
                >
                  <header
                    className={`border-b px-4 py-4 ${
                      isToday
                        ? "border-emerald-600 bg-emerald-700 dark:border-emerald-500 dark:bg-emerald-700"
                        : "border-stone-100 dark:border-white/[0.06]"
                    }`}
                  >
                    <div className="flex items-start justify-between gap-2">
                      <div>
                        <p
                          className={`text-sm font-semibold ${isToday ? "text-white" : "text-stone-800 dark:text-stone-200"}`}
                        >
                          {date.toLocaleDateString(undefined, {
                            weekday: "short",
                          })}
                        </p>

                        <p
                          className={`mt-0.5 text-sm ${isToday ? "text-emerald-100" : "text-stone-500 dark:text-stone-400"}`}
                        >
                          {date.toLocaleDateString(undefined, {
                            day: "numeric",
                            month: "short",
                          })}
                        </p>
                      </div>

                      <button
                        type="button"
                        aria-label={`Add meal to ${date.toLocaleDateString()}`}
                        onClick={() => setAddingDate(date)}
                        className={`flex h-8 w-8 shrink-0 items-center justify-center rounded-full transition ${isToday ? "bg-white/15 text-white hover:bg-white/25" : "bg-stone-100 text-stone-500 hover:bg-emerald-100 hover:text-emerald-800 dark:bg-white/[0.06] dark:text-stone-400 dark:hover:bg-emerald-400/10 dark:hover:text-emerald-400"}`}
                      >
                        <Plus size={15} />
                      </button>
                    </div>
                  </header>

                  <div className="flex flex-1 flex-col gap-3 p-3">
                    {dayMeals.length > 0 ? (
                      dayMeals.map((meal) => (
                        <div
                          key={meal.id}
                          className="group relative rounded-2xl bg-stone-50 transition hover:bg-stone-100 dark:bg-white/[0.04] dark:hover:bg-white/[0.07]"
                        >
                          <Link
                            to={`/meals/${meal.recipe.id}`}
                            className="block p-3 pr-10"
                          >
                            <p className="mb-2 text-xs font-medium text-stone-500 dark:text-stone-400">
                              {meal.mealType}
                            </p>

                            <div className="flex items-center gap-3 sm:block">
                              {meal.recipe.imageUrl ? (
                                <img
                                  src={meal.recipe.imageUrl}
                                  alt={meal.recipe.name}
                                  className="h-16 w-16 shrink-0 rounded-xl object-cover sm:h-20 sm:w-full"
                                />
                              ) : (
                                <div className="flex h-16 w-16 shrink-0 items-center justify-center rounded-xl bg-stone-200 text-stone-400 dark:bg-white/[0.06] dark:text-stone-500 sm:h-20 sm:w-full">
                                  <UtensilsCrossed size={18} />
                                </div>
                              )}

                              <p className="min-w-0 text-sm font-medium leading-snug text-stone-800 group-hover:text-emerald-900 dark:text-stone-200 dark:group-hover:text-emerald-300 sm:mt-2">
                                {meal.recipe.name}
                              </p>
                            </div>
                          </Link>

                          <button
                            type="button"
                            aria-label={`Remove ${meal.recipe.name} from plan`}
                            disabled={removingIds.includes(meal.id)}
                            onClick={() => {
                              const confirmed = window.confirm(
                                `Remove ${meal.recipe.name} from ${meal.mealType.toLowerCase()}?`,
                              );

                              if (confirmed) {
                                void onRemove(meal.id);
                              }
                            }}
                            className="absolute right-2 top-2 flex h-8 w-8 items-center justify-center rounded-full bg-white/90 text-stone-400 shadow-sm transition hover:bg-red-50 hover:text-red-600 disabled:opacity-40 dark:bg-[#292e2a]/90 dark:hover:bg-red-400/10 dark:hover:text-red-400"
                          >
                            <Trash2 size={14} />
                          </button>
                        </div>
                      ))
                    ) : (
                      <div className="flex flex-1 items-center justify-center py-8 text-center">
                        <div>
                          <UtensilsCrossed
                            size={20}
                            className="mx-auto text-stone-300 dark:text-stone-600"
                          />
                          <p className="mt-2 text-sm text-stone-400 dark:text-stone-500">
                            Nothing planned
                          </p>
                        </div>
                      </div>
                    )}
                  </div>
                </article>
              );
            })}
          </div>
        </div>
      )}

      {!loading && (
        <p className="mt-1 text-center text-xs text-stone-400 dark:text-stone-500 sm:hidden">
          Swipe to see the next days
        </p>
      )}

      {addingDate && (
        <AddMealPlanModal
          date={addingDate}
          existingMeals={meals.filter(
            (meal) => meal.date === getDateKey(addingDate),
          )}
          onClose={() => setAddingDate(null)}
          onAdded={onRefresh}
        />
      )}
    </section>
  );
}

export default WeeklyMealPlan;