import { Link } from "react-router-dom";
import { getMealContext, getMealQuestion } from "@/lib/homeDashboard";
import type { PlannedMeal } from "@/types/mealPlan";

interface NextMealCardProps {
  meal: PlannedMeal | null;
}

function NextMealCard({ meal }: NextMealCardProps) {
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
          <p className="mt-2 text-base text-emerald-50">{meal.recipe.name}</p>

          <Link to={`/meals/${meal.recipe.id}`} className="mt-5 inline-flex h-11 items-center rounded-full bg-white px-5 text-sm font-medium text-emerald-950 transition hover:bg-emerald-50">
            View recipe
          </Link>
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