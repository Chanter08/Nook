import { useEffect, useState } from "react"
import { Moon, Sun } from "lucide-react"

type Theme = "light" | "dark"

function getInitialTheme(): Theme {
  const savedTheme = localStorage.getItem("nook-theme")

  if (savedTheme === "light" || savedTheme === "dark") {
    return savedTheme
  }

  return window.matchMedia("(prefers-color-scheme: dark)").matches
    ? "dark"
    : "light"
}

function ThemeToggle() {
  const [theme, setTheme] = useState<Theme>(getInitialTheme)

  useEffect(() => {
    document.documentElement.classList.toggle(
      "dark",
      theme === "dark"
    )

    localStorage.setItem("nook-theme", theme)
  }, [theme])

  function toggleTheme() {
    setTheme((current) =>
      current === "dark" ? "light" : "dark"
    )
  }

  return (
    <button
      type="button"
      onClick={toggleTheme}
      aria-label="Toggle dark mode"
      className="
        flex h-11 w-11 items-center justify-center
        rounded-full
        bg-stone-100 text-stone-700
        transition
        hover:bg-stone-200
        dark:bg-stone-800
        dark:text-stone-200
        dark:hover:bg-stone-700
      "
    >
      {theme === "dark" ? (
        <Sun size={20} />
      ) : (
        <Moon size={20} />
      )}
    </button>
  )
}

export default ThemeToggle