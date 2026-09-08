export interface PlannedMeal {
    id: number;
    date: string;
    mealType: string;
    recipe: {
      id: number;
      name: string;
      imageUrl: string | null;
    };
  }