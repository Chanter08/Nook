import type { RecipeSummary } from "@/types/recipe";

export interface RecipeFilters {
  searchTerm: string;
  mealType: string;
  cuisine: string;
}

export function filterRecipes(recipes: RecipeSummary[], filters: RecipeFilters) {
  const search = filters.searchTerm.trim().toLowerCase();

  return recipes.filter((recipe) => {
    const searchableText = [
      recipe.name,
      recipe.description ?? "",
      ...recipe.mealTypes,
      ...recipe.cuisines
    ]
      .join(" ")
      .toLowerCase();

    const matchesSearch = !search || searchableText.includes(search);
    const matchesMealType = filters.mealType === "All" || recipe.mealTypes.includes(filters.mealType);
    const matchesCuisine = filters.cuisine === "All" || recipe.cuisines.includes(filters.cuisine);

    return matchesSearch && matchesMealType && matchesCuisine;
  });
}

export function getRecipeFilterQuery(filters: RecipeFilters) {
  const params = new URLSearchParams();

  if (filters.searchTerm.trim()) params.set("q", filters.searchTerm.trim());
  if (filters.mealType !== "All") params.set("mealType", filters.mealType);
  if (filters.cuisine !== "All") params.set("cuisine", filters.cuisine);

  const query = params.toString();
  return query ? `?${query}` : "";
}