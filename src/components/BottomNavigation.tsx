import {
  CalendarDays,
  Home,
  Utensils,
  UserRound,
  ShoppingCart,
} from "lucide-react";

import { NavLink } from "react-router-dom";

function BottomNavigation() {
  const navClass = ({ isActive }: { isActive: boolean }) =>
    `flex flex-col items-center gap-1 transition-colors ${
      isActive ? "text-emerald-800" : "text-stone-400"
    }`;

  return (
    <nav
      className="
    fixed
    bottom-0
    left-0
    right-0
    z-50
    mx-auto
    flex
    w-full
    max-w-5xl
    justify-around
    border-t
    border-stone-200
    bg-white/95
    px-4
    pb-[calc(env(safe-area-inset-bottom)+0.75rem)]
    pt-3
    backdrop-blur
    transition-colors
    dark:border-stone-800
    dark:bg-stone-900/95
  "
    >
      <NavLink to="/" end className={navClass}>
        <Home size={21} />
        <span className="text-xs font-medium">Home</span>
      </NavLink>

      <NavLink to="/meals" className={navClass}>
        <Utensils size={21} />

        <span className="text-xs font-medium">Meals</span>
      </NavLink>

      <NavLink to="/plans" className={navClass}>
        <CalendarDays size={21} />
        <span className="text-xs font-medium">Plans</span>
      </NavLink>

      <NavLink to="/shopping" className={navClass}>
        <ShoppingCart size={21} />
        <span className="text-xs font-medium">Shopping</span>
      </NavLink>

      {/* Not connected yet */}
      <NavLink to="/us" className={navClass}>
        <UserRound size={21} />
        <span className="text-xs font-medium">Us</span>
      </NavLink>
    </nav>
  );
}

export default BottomNavigation;