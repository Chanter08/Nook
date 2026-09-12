interface IngredientAmount {
  quantity: number | null;
  unit: string | null;
  notes: string | null;
}

export function formatIngredientAmount(ingredient: IngredientAmount) {
  const quantity =
    ingredient.quantity === null
      ? ""
      : Number.isInteger(ingredient.quantity)
        ? ingredient.quantity.toString()
        : ingredient.quantity.toFixed(2).replace(/0+$/, "").replace(/\.$/, "");

  if (ingredient.unit?.toLowerCase() === "each") {
    return [quantity, ingredient.notes].filter(Boolean).join(" ");
  }

  return [quantity, ingredient.unit, ingredient.notes].filter(Boolean).join(" ");
}