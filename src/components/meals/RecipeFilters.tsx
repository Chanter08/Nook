import { Search, X } from "lucide-react";

interface RecipeFiltersProps {
  searchTerm: string;
  selectedMealType: string;
  selectedCuisine: string;
  mealTypes: string[];
  cuisines: string[];
  hasFilters: boolean;
  onSearchChange: (value: string) => void;
  onMealTypeChange: (value: string) => void;
  onCuisineChange: (value: string) => void;
  onClear: () => void;
}

function RecipeFilters({
  searchTerm,
  selectedMealType,
  selectedCuisine,
  mealTypes,
  cuisines,
  hasFilters,
  onSearchChange,
  onMealTypeChange,
  onCuisineChange,
  onClear
}: RecipeFiltersProps) {
  return (
    <section className="mb-6">
      <div className="flex flex-col gap-3 sm:flex-row">
        <div className="relative flex-1">
          <Search size={18} className="absolute left-4 top-1/2 -translate-y-1/2 text-stone-400 dark:text-stone-500" />

          <input
            type="search"
            value={searchTerm}
            onChange={(event) => onSearchChange(event.target.value)}
            placeholder="Search recipes..."
            className="h-12 w-full rounded-2xl border border-stone-200 bg-stone-50 pl-11 pr-4 text-sm text-stone-900 outline-none transition placeholder:text-stone-400 focus:border-emerald-700 focus:bg-white dark:border-white/[0.08] dark:bg-white/[0.05] dark:text-stone-100 dark:placeholder:text-stone-500 dark:focus:border-emerald-500 dark:focus:bg-white/[0.07]"
          />
        </div>

        <select
          value={selectedMealType}
          onChange={(event) => onMealTypeChange(event.target.value)}
          className="h-12 rounded-2xl border border-stone-200 bg-stone-50 px-4 text-sm text-stone-800 outline-none transition focus:border-emerald-700 dark:border-white/[0.08] dark:bg-[#252b26] dark:text-stone-200 dark:focus:border-emerald-500"
        >
          <option value="All">All meals</option>
          {mealTypes.map((type) => (
            <option key={type} value={type}>{type}</option>
          ))}
        </select>

        <select
          value={selectedCuisine}
          onChange={(event) => onCuisineChange(event.target.value)}
          className="h-12 rounded-2xl border border-stone-200 bg-stone-50 px-4 text-sm text-stone-800 outline-none transition focus:border-emerald-700 dark:border-white/[0.08] dark:bg-[#252b26] dark:text-stone-200 dark:focus:border-emerald-500"
        >
          <option value="All">All cuisines</option>
          {cuisines.map((cuisine) => (
            <option key={cuisine} value={cuisine}>{cuisine}</option>
          ))}
        </select>
      </div>

      {hasFilters && (
        <button type="button" onClick={onClear} className="mt-3 flex items-center gap-1.5 text-sm font-medium text-emerald-800 transition hover:text-emerald-950 dark:text-emerald-400 dark:hover:text-emerald-300">
          <X size={15} />
          Clear filters
        </button>
      )}
    </section>
  );
}

export default RecipeFilters;