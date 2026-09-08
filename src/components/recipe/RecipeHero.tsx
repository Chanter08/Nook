import { ArrowLeft, UtensilsCrossed } from "lucide-react";
import ThemeToggle from "@/components/ThemeToggle";

interface RecipeHeroProps {
  name: string;
  imageUrl: string | null;
  onBack: () => void;
}

function RecipeHero({ name, imageUrl, onBack }: RecipeHeroProps) {
  return (
    <section className="relative">
      {imageUrl ? (
        <img src={imageUrl} alt={name} className="h-72 w-full object-cover sm:h-96 sm:rounded-b-[2.5rem]" />
      ) : (
        <div className="flex h-72 w-full items-center justify-center bg-stone-100 text-stone-400 dark:bg-white/[0.05] dark:text-stone-500 sm:h-96 sm:rounded-b-[2.5rem]">
          <UtensilsCrossed size={42} />
        </div>
      )}

      <button type="button" aria-label="Go back" onClick={onBack} className="absolute left-4 top-4 flex h-11 w-11 items-center justify-center rounded-full bg-white/90 text-stone-800 shadow-sm backdrop-blur transition hover:bg-white dark:bg-[#1b201c]/90 dark:text-stone-100 dark:ring-1 dark:ring-white/[0.08] dark:hover:bg-[#252b26] sm:left-6 sm:top-6">
        <ArrowLeft size={20} />
      </button>

      <div className="absolute right-4 top-4 sm:right-6 sm:top-6">
        <ThemeToggle />
      </div>
    </section>
  );
}

export default RecipeHero;