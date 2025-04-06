"use client";

import React, { createContext, useContext, useEffect, useState } from "react";

export const ThemeContext = createContext<
  | {
      darkMode: boolean;
      setDarkMode: React.Dispatch<React.SetStateAction<boolean>>;
    }
  | undefined
>(undefined);

export function ThemeProvider({ children }: { children: React.ReactNode }) {
  const [darkMode, setDarkMode] = useState(false);

  useEffect(() => {
    if (typeof window !== undefined) {
      const savedTheme = localStorage.getItem("theme");

      if (savedTheme === "dark") {
        setDarkMode(true);
      }
    }
  }, []);

  useEffect(() => {
    if (darkMode) {
      document.documentElement.classList.add("dark");
      localStorage.setItem("theme", "dark");
    } else {
      document.documentElement.classList.remove("dark");
      localStorage.setItem("theme", "dark");
    }
  }, [darkMode]);

  return (
    <ThemeContext.Provider value={{ darkMode, setDarkMode }}>
      {children}
    </ThemeContext.Provider>
  );
}

export const useDarkMode = () => {
  const context = useContext(ThemeContext);
  if (!context) {
    throw new Error("useDarkMode must be used within a ThemeProvider");
  }
  return context;
};
