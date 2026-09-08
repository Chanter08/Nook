import { useState } from "react";
import { UserRound } from "lucide-react";
import ThemeToggle from "@/components/ThemeToggle";
import HomeQuickActions from "@/components/home/HomeQuickActions";
import NextMealCard from "@/components/home/NextMealCard";
import ShoppingPreviewModal from "@/components/home/ShoppingPreviewModal";
import UpcomingEvents from "@/components/home/UpcomingEvents";
import WeeklyMealPlan from "@/components/home/WeeklyMealPlan";
import { useCalendarEvents } from "@/hooks/useCalendarEvents";
import { useMealPlan } from "@/hooks/useMealPlan";
import { useShoppingList } from "@/hooks/useShoppingList";
import { getGreeting, getNextMeal, getPlanStart } from "@/lib/homeDashboard";

function HomePage() {
  const [shoppingOpen, setShoppingOpen] = useState(false);
  const planStart = getPlanStart();

  const {
    events,
    loading: calendarLoading,
    error: calendarError,
  } = useCalendarEvents();

  const {
    meals,
    loading: mealPlanLoading,
    removingIds,
    refresh: refreshMealPlan,
    removeMeal,
  } = useMealPlan(planStart);

  const {
    shoppingList,
    loading: shoppingLoading,
    error: shoppingError,
    actionError: shoppingActionError,
    completingIds,
    refresh: refreshShopping,
    completeItem,
    clearActionError,
  } = useShoppingList();

  const nextMeal = getNextMeal(meals);

  function openShoppingList() {
    setShoppingOpen(true);
    clearActionError();
    void refreshShopping();
  }

  return (
    <main className="mx-auto flex min-h-dvh w-full max-w-5xl flex-col px-4 pb-28 pt-8 transition-colors sm:px-6 sm:pt-12">
      <header className="mb-6 flex items-center justify-between">
        <div>
          <p className="text-sm text-stone-500 dark:text-stone-400">
            {getGreeting()}
          </p>
          <h1 className="text-3xl font-semibold tracking-tight text-stone-900 dark:text-stone-100">
            Nook
          </h1>
        </div>

        <div className="flex items-center gap-2">
          <ThemeToggle />

          <button
            type="button"
            aria-label="Profile"
            className="flex h-11 w-11 items-center justify-center rounded-full bg-stone-100 text-stone-700 transition hover:bg-stone-200 dark:bg-white/[0.06] dark:text-stone-200 dark:ring-1 dark:ring-white/[0.06] dark:hover:bg-white/[0.1]"
          >
            <UserRound size={20} />
          </button>
        </div>
      </header>

      <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 sm:items-stretch">
        <NextMealCard meal={nextMeal} />

        <HomeQuickActions
          shoppingList={shoppingList}
          shoppingLoading={shoppingLoading}
          onShoppingOpen={openShoppingList}
        />
      </div>

      <WeeklyMealPlan
        startDate={planStart}
        meals={meals}
        loading={mealPlanLoading}
        onRefresh={refreshMealPlan}
        onRemove={removeMeal}
        removingIds={removingIds}
      />

      <UpcomingEvents
        events={events}
        loading={calendarLoading}
        error={calendarError}
      />  

      <ShoppingPreviewModal
        open={shoppingOpen}
        loading={shoppingLoading}
        error={shoppingError}
        actionError={shoppingActionError}
        shoppingList={shoppingList}
        completingItems={completingIds}
        onClose={() => setShoppingOpen(false)}
        onRetry={() => void refreshShopping()}
        onComplete={(itemId) => void completeItem(itemId)}
      />
    </main>
  );
}

export default HomePage;