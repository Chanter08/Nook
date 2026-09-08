import type { RecipeStep } from "@/types/recipe";

interface RecipeMethodProps {
  steps: RecipeStep[];
}

function RecipeMethod({ steps }: RecipeMethodProps) {
  return (
    <section className="mt-10">
      <h2 className="text-xl font-semibold text-stone-900 dark:text-stone-100">Method</h2>

      <div className="mt-5 space-y-6">
        {steps.map((step) => (
          <div key={step.stepNumber} className="flex gap-4">
            <div className="flex h-9 w-9 shrink-0 items-center justify-center rounded-full bg-emerald-900 text-sm font-semibold text-white dark:bg-emerald-700">
              {step.stepNumber}
            </div>

            <p className="pt-1 leading-relaxed text-stone-700 dark:text-stone-300">{step.instruction}</p>
          </div>
        ))}
      </div>
    </section>
  );
}

export default RecipeMethod;