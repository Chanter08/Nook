export interface DiscoverRecipeSummary {
  provider: string;
  externalId: string;
  name: string;
  imageUrl: string | null;
  category: string | null;
  cuisine: string | null;
}

export interface DiscoverIngredient {
  name: string;
  quantity: number | null;
  unit: string | null;
  notes: string | null;
}

export interface DiscoverStep {
  stepNumber: number;
  instruction: string;
}

export interface DiscoverRecipeDetail {
  provider: string;
  externalId: string;

  name: string;
  description: string | null;
  imageUrl: string | null;

  category: string | null;
  cuisine: string | null;
  difficulty: string | null;

  prepTimeMinutes: number | null;
  cookTimeMinutes: number | null;
  totalTimeMinutes: number | null;
  servings: number | null;

  calories: number | null;
  proteinGrams: number | null;
  carbohydrateGrams: number | null;
  fatGrams: number | null;

  sourceUrl: string | null;
  videoUrl: string | null;

  ingredients: DiscoverIngredient[];
  steps: DiscoverStep[];
}