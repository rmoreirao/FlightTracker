import type { Config } from "tailwindcss";

const config: Config = {
  darkMode: "class", // Enable dark mode
  content: [
    "./src/pages/**/*.{js,ts,jsx,tsx,mdx}",
    "./src/components/**/*.{js,ts,jsx,tsx,mdx}",
    "./src/app/**/*.{js,ts,jsx,tsx,mdx}",
  ],
  theme: {
    extend: {
      colors: {
        // background: "var(--background)", // Will be handled by globals.css
        // foreground: "var(--foreground)", // Will be handled by globals.css
        primary: { "500": "#2563EB", "600": "#1D4ED8" },
        accent:  { "500": "#F97316" },
        success: { "500": "#10B981" },
        warning: { "500": "#FACC15" },
        error:   { "500": "#EF4444" },
        neutral: {
          "50": "#F9FAFB", // Light mode body background
          "100": "#F3F4F6", // Light mode card background
          "700": "#374151", // Light mode body text
          "900": "#111827", // Light mode titles/headings
          // Dark mode equivalents (can also be handled in globals.css or via dark: prefix)
          // For direct use in Tailwind, you might want to define dark mode specific neutral shades
          // or rely on CSS variables set in globals.css
        }
      },
    },
  },
  plugins: [],
};
export default config;
