import type { DiscoverRecipeSummary } from "@/types/discover";

interface DiscoverCacheEntry {
  createdAt: number;
  recipes: DiscoverRecipeSummary[];
}

const CACHE_PREFIX = "nook:discover:";
const CACHE_DURATION_MS = 55 * 60 * 1000;

function getCacheKey(query: string) {
  return `${CACHE_PREFIX}${query.trim().toLowerCase()}`;
}

export function getCachedDiscoverRecipes(
  query: string,
): DiscoverRecipeSummary[] | null {
  if (!query.trim()) return null;

  const key = getCacheKey(query);
  const raw = sessionStorage.getItem(key);

  if (!raw) return null;

  try {
    const entry = JSON.parse(raw) as DiscoverCacheEntry;

    if (Date.now() - entry.createdAt >= CACHE_DURATION_MS) {
      sessionStorage.removeItem(key);
      return null;
    }

    return entry.recipes;
  } catch {
    sessionStorage.removeItem(key);
    return null;
  }
}

export function cacheDiscoverRecipes(
  query: string,
  recipes: DiscoverRecipeSummary[],
) {
  const entry: DiscoverCacheEntry = {
    createdAt: Date.now(),
    recipes,
  };

  sessionStorage.setItem(
    getCacheKey(query),
    JSON.stringify(entry),
  );
}