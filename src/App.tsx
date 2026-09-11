import { Route, Routes } from "react-router-dom";

import BottomNavigation from "@/components/BottomNavigation";
import HomePage from "@/pages/Home";
import PlansPage from "@/pages/Plans";
import MealsPage from "@/pages/Meals";
import RecipePage from "@/pages/Recipe";
import ShoppingPage from "./pages/Shopping";
import AddRecipePage from "@/pages/AddRecipe";
import UsPage from "@/pages/Us";

function App() {
  return (
    <div className="min-h-dvh overflow-x-hidden bg-stone-100 text-stone-900 transition-colors dark:bg-stone-950 dark:text-stone-100">
      <div className="mx-auto min-h-dvh w-full max-w-5xl overflow-x-hidden bg-white shadow-xl transition-colors dark:bg-stone-900">
        <Routes>
          <Route path="/" element={<HomePage />} />
          <Route path="/plans" element={<PlansPage />} />
          <Route path="/meals" element={<MealsPage />} />
          <Route path="/meals/:id" element={<RecipePage />} />
          <Route path="/shopping" element={<ShoppingPage />} />
          <Route path="/meals/new" element={<AddRecipePage />} />
          <Route path="/us" element={<UsPage />} />
        </Routes>
        <BottomNavigation />
      </div>
    </div>
  );
}

export default App;