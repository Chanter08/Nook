# Cooking from recipes and meal plans

The **Mark as cooked** action is available on recipe pages, and from weekly meal-plan cards.

## Behaviour

- Opening the flow only previews ingredients; planning a meal does not deduct pantry stock.
- Choose servings and click **Update amounts** before confirming when the portion count changes.
- Nook suggests a pantry match only when the ingredient name matches exactly (ignoring case/spacing), a compatible measured unit exists, the ingredient has a quantity, and there is a unique matching stocked item with enough quantity.
- `g/kg`, `ml/l`, and `each/piece` are converted where unambiguous. Other conversions are not guessed.
- You can substitute any pantry item and enter the quantity actually used **in that pantry item's unit**. Set an ingredient to **Skip** when the recipe calls for a non-stocked item or you did not use it. Quantity rounding is limited to 2 decimal places.
- The server re-checks all stock quantities within a serializable SQL transaction before reducing anything. Overdrawing stock returns HTTP 409 without recording a cooking event.
- A cooking history record is created even when no tracked stock is deducted.
- Undo adds the recorded quantities back atomically. Undoing the same cooking record twice is prevented. If a referenced pantry item was deleted, the record is retained but undo returns HTTP 409 rather than restoring stock to a different item.
- Cooking from a planned meal records the meal-plan entry and prevents counting the same planned meal twice (until its cooking record is undone). The meal is not removed from the plan.

## Migration and deployment

Apply EF migration `20261102093000_AddCookedRecipeInventory` before using the cooking endpoints. The application supports a `--migrate` mode:

```bash
dotnet Nook.Api.dll --migrate
```

Use the established deployment migration procedure against the same production connection string. Do not recreate or delete SQL volumes.

## Manual verification

1. Add 1 kg of chicken and 4 peppers to pantry, and create a recipe using 500 g chicken and 2 peppers. Preview should suggest 500 g and 2 each where names match.
2. Confirm cooking; stock should decrease to 500 g and 2 peppers.
3. Undo; stock should return to 1 kg and 4 peppers; retrying undo should be rejected.
4. Try consuming more than available, including the same pantry row from two recipe ingredients; the entire request must fail and leave stock unchanged.
5. Confirm a recipe with all ingredients skipped; a history event should be recorded without stock changes.
6. From a meal-plan card, mark cooked; retrying the same planned meal should be rejected until undo.
7. Try a substitute ingredient and a portion-size change; unconfirmed recipe preview must not change stock.
8. Delete a previously consumed pantry item; historical usage remains, and undo should explain why it cannot restore the missing item.
