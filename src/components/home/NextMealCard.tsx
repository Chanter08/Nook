import { Link } from "react-router-dom";
import { getMealContext, getMealQuestion } from "@/lib/homeDashboard";
import type { PlannedMeal } from "@/types/mealPlan";

interface NextMealCardProps {
  meals: PlannedMeal[];
}

function NextMealCard({ meals }: NextMealCardProps) {
  const meal = meals[0] ?? null;
  function scrollToMealPlan() {
    document.getElementById("meal-plan")?.scrollIntoView({
      behavior: "smooth"
    });
  }

  return (
    <section className="rounded-[1.75rem] bg-emerald-900 px-6 py-7 text-white shadow-sm dark:bg-emerald-800">
      {meal ? (
        <>
          <p className="text-sm text-emerald-100">{getMealContext(meal)}</p>
          <h2 className="mt-2 text-2xl font-semibold tracking-tight">{getMealQuestion(meal.mealType)}</h2>
          <div className="mt-2 space-y-2">
            {meals.map((planned) => (
              <Link key={planned.id} to={`/meals/${planned.recipe.id}`} className="block rounded-xl bg-white/10 px-3 py-2 text-sm font-medium text-white hover:bg-white/20">
                {planned.recipe.name}
              </Link>
            ))}
          </div>
        </>
      ) : (
        <>
          <p className="text-sm text-emerald-100">Next up</p>
          <h2 className="mt-2 text-2xl font-semibold tracking-tight">Nothing planned yet</h2>
          <p className="mt-2 text-sm text-emerald-100">Add a meal to your upcoming plan.</p>

          <button type="button" onClick={scrollToMealPlan} className="mt-5 inline-flex h-11 items-center rounded-full bg-white px-5 text-sm font-medium text-emerald-950 transition hover:bg-emerald-50">
            Plan a meal
          </button>
        </>
      )}
    </section>
  );
}

export default NextMealCard;