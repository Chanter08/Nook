import { Home, Users } from "lucide-react";
import { useNavigate } from "react-router-dom";
import ThemeToggle from "@/components/ThemeToggle";

function UsPage() {
  const navigate = useNavigate();

  return (
    <main className="mx-auto min-h-dvh w-full max-w-3xl px-4 pb-28 pt-8 sm:px-6 sm:pt-12">
      <header className="flex items-center justify-between">
        <div>
          <p className="text-sm font-medium text-emerald-800 dark:text-emerald-400">Our home</p>
          <h1 className="mt-1 text-3xl font-semibold tracking-tight text-stone-900 dark:text-stone-100">Us</h1>
        </div>

        <ThemeToggle />
      </header>

      <section className="mt-12 rounded-3xl border border-stone-200 bg-white px-6 py-12 text-center dark:border-white/[0.07] dark:bg-white/[0.035]">
        <div className="mx-auto flex h-14 w-14 items-center justify-center rounded-full bg-emerald-50 text-emerald-800 dark:bg-emerald-400/10 dark:text-emerald-400">
          <Users size={26} />
        </div>

        <h2 className="mt-5 text-xl font-semibold text-stone-900 dark:text-stone-100">Our space is coming soon</h2>

        <p className="mx-auto mt-2 max-w-sm text-sm leading-6 text-stone-500 dark:text-stone-400">
          A place for the people, pets and little things that make Nook ours.
        </p>

        <button type="button" onClick={() => navigate("/")} className="mt-6 inline-flex items-center gap-2 rounded-full bg-emerald-900 px-5 py-2.5 text-sm font-medium text-white transition hover:bg-emerald-800 dark:bg-emerald-700 dark:hover:bg-emerald-600">
          <Home size={16} />
          Back home
        </button>
      </section>
    </main>
  );
}

export default UsPage;