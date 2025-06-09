# FlightTracker ‑ UI Style Guide

This document defines the *baseline visual language* for FlightTracker’s front-end.  
It is intentionally minimalist and responsive-first; everything here can be implemented with Tailwind CSS (recommended) or any utility-class framework.

---

## 1. Design Goals
1. Clean, distraction-free layout that highlights live flight data.
2. Mobile-first; must gracefully expand to tablets and desktops.
3. Accessible: WCAG 2.1 AA contrast, keyboard navigation, prefers-color-scheme support.
4. Fully theme-able with a small, **atomic** color palette.

---

## 2. Color Palette

| Token            | Light Mode | Dark Mode | Example Usage |
|------------------|-----------|-----------|---------------|
| **Primary 500**  | `#2563EB` | `#3B82F6` | Buttons, links, active nav |
| **Primary 600**  | `#1D4ED8` | `#2563EB` | Button hover, focused states |
| **Accent 500**   | `#F97316` | `#FB923C` | Map markers, CTA highlights |
| **Neutral 50**   | `#F9FAFB` | `#0A0A0A` | Body background |
| **Neutral 100**  | `#F3F4F6` | `#1F2937` | Card backgrounds |
| **Neutral 700**  | `#374151` | `#D1D5DB` | Body text |
| **Neutral 900**  | `#111827` | `#FFFFFF` | Titles, headings |
| **Success 500**  | `#10B981` | `#34D399` | Operational indicators |
| **Warning 500**  | `#FACC15` | `#FDE047` | Delayed flight badge |
| **Error 500**    | `#EF4444` | `#F87171` | Error states / alerts |

```css
/* Tailwind extension example */
module.exports = {
  theme: {
    extend: {
      colors: {
        primary: { 500: '#2563EB', 600: '#1D4ED8' },
        accent:  { 500: '#F97316' },
        success: { 500: '#10B981' },
        warning: { 500: '#FACC15' },
        error:   { 500: '#EF4444' },
        neutral: {
          50: '#F9FAFB',
          100: '#F3F4F6',
          700: '#374151',
          900: '#111827'
        }
      }
    }
  }
}
```

---

## 3. Typography
| Element      | Font               | Weight | Size (rem) | Tailwind class |
|--------------|--------------------|--------|------------|----------------|
| Heading 1    | “Inter”, system UI | 700    | 2.25 rem   | `text-4xl` |
| Heading 2    | “Inter”, system UI | 600    | 1.875 rem  | `text-3xl` |
| Body text    | “Inter”, system UI | 400    | 1 rem      | `text-base` |
| Small / meta | “Inter”, system UI | 400    | 0.875 rem  | `text-sm`   |

---

## 4. Spacing & Layout
• Use an **8-point grid**: 4 px for tight elements, 8 px multiples everywhere else.  
• Default container max-widths: `sm (640px)`, `md (768px)`, `lg (1024px)`, `xl (1280px)`.  
• Cards: `rounded-lg`, `shadow-sm`, `p-4` on mobile, `p-6` on desktop.

---

## 5. Components Overview
1. **Navbar**  
   • Sticky top, height `56px`, backdrop-blur.  
   • Collapses to hamburger on `< md`.

2. **Flight List**  
   • Mobile: vertical cards (`flex-col gap-4`).  
   • ≥ md: data table with sticky header, virtual scrolling.

3. **MapView**  
   • Uses full viewport width; maintain 16:9 aspect until expanded.  
   • Markers adopt `accent-500`; cluster color scales via Tailwind `accent-300/400/500`.

4. **Drawer / Modal**  
   • Overlay color: `bg-neutral-900/60`.  
   • Prefer `dialog` element for a11y.

---

## 6. Responsive Breakpoints (Tailwind defaults)
```text
sm  640px  → portrait phones
md  768px  → landscape phones / small tablets
lg 1024px  → tablets / small laptop
xl 1280px  → desktop
2xl 1536px → large desktop / ultrawide
```
Design **mobile-first**; introduce layout changes at `md` and `lg`.

---

## 7. Accessibility Checklist
- [ ] Text contrast ≥ 4.5 : 1 against background  
- [ ] Focus states clearly visible (`ring-2 ring-primary-500`)  
- [ ] All interactive components focus-trap inside modals/drawers  
- [ ] Keyboard shortcuts exposed via `title` attribute  
- [ ] Motion only as subtle opacity/scale transitions (`duration-150`)

---

## 8. Asset Guidelines
• Icons: Heroicons 24 px outline; fill variant on active states.  
• Images: WebP first, <= 200 kB; use `srcSet` for 1× / 2×.  
• Map tiles: compressed PNG; limit to retina tiles only when `devicePixelRatio > 1`.

---

## 9. Implementation Notes
```
pnpm create vite flight-tracker-ui --template react-ts
cd flight-tracker-ui
pnpm add -D tailwindcss postcss autoprefixer
npx tailwindcss init -p
```
Enable dark mode:  
```js
// tailwind.config.js
module.exports = { darkMode: 'class', /* … */ }
```

---

### Keep this file updated!
When you introduce new colors, spacing rules, or components, extend this document in the same style.
