import { apiRequest } from "@/api/client";
import type {
  DiscoverRecipeDetail,
  DiscoverRecipeSummary,
} from "@/types/discover";

export function searchDiscoverRecipes(query: string) {
  return apiRequest<DiscoverRecipeSummary[]>(
    `/api/discover/search?q=${encodeURIComponent(query.trim())}`,
  );
}

export function getDiscoverRecipe(externalId: string) {
  return apiRequest<DiscoverRecipeDetail>(
    `/api/discover/${encodeURIComponent(externalId)}`,
  );
}