"use client";

import React from "react";
import { useDarkMode } from "../../contexts/ThemeContext";

const ThemeToggler = () => {
  const { darkMode, setDarkMode } = useDarkMode();

  return (
    <div className="flex items-center justify-center w-fit p-1 mx-auto text-black dark:text-white hover:text-slate-500">
      {darkMode ? (
        <i
          title="Switch Light Theme"
          onClick={() => setDarkMode(!darkMode)}
          className="pi pi-sun text-lg cursor-pointer"
        ></i>
      ) : (
        <i
          title="Switch Dark Theme"
          onClick={() => setDarkMode(!darkMode)}
          className="pi pi-moon text-lg cursor-pointer"
        ></i>
      )}
    </div>
  );
};

export default ThemeToggler;
