import React from "react";
import ThemeToggler from "./ThemeToggler";

const Navbar = () => {
  return (
      <div className="fixed z-20 bg-white/70 dark:bg-slate-900/70 backdrop-blur-md shadow-lg w-full">
        <div className="flex justify-between items-center h-16">
          <div className="text-2xl font-bold text-black dark:text-white">
            Mess<span className="text-slate-300 dark:text-slate-700">App</span>
          </div>
          <div className="flex items-center gap-x-4 pr-5">
            <ThemeToggler />
          </div>
        </div>
      </div>
  );
};

export default Navbar;
