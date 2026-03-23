---
name: tailwind-glassmorphism-system
description: >
  Expert guidance on building glassmorphism UI systems with Tailwind CSS, including
  backdrop-blur, semi-transparent backgrounds, frosted glass cards, glass navigation,
  glass modals, and CSS custom properties for glass design tokens. Use this skill whenever
  the user mentions glassmorphism, frosted glass, glass UI, glass card, backdrop blur,
  backdrop-filter, semi-transparent, frosted effect, glass navbar, or wants a modern
  translucent UI aesthetic. Also use when combining Tailwind with CSS variables for theming.
  Trigger on: glassmorphism, frosted glass, backdrop-blur, glass card, glass UI,
  glass effect, translucent card, frosted nav, glass modal.
---

# Tailwind Glassmorphism System

## Core Glass Formula

```
background: rgba(255,255,255, α)    ← semi-transparent fill
backdrop-filter: blur(px)           ← blurs what's BEHIND the element  
border: 1px solid rgba(255,255,255, β)  ← subtle bright edge
box-shadow                          ← depth and glow
```

---

## 1. CSS Design Tokens (custom properties)

```css
/* globals.css / app.css */
:root {
  /* Glass levels */
  --glass-sm:   rgba(255, 255, 255, 0.05);
  --glass-md:   rgba(255, 255, 255, 0.10);
  --glass-lg:   rgba(255, 255, 255, 0.18);
  --glass-xl:   rgba(255, 255, 255, 0.28);

  /* Dark glass (for light backgrounds) */
  --glass-dark-sm: rgba(0, 0, 0, 0.05);
  --glass-dark-md: rgba(0, 0, 0, 0.12);

  /* Borders */
  --glass-border:       rgba(255, 255, 255, 0.18);
  --glass-border-hover: rgba(255, 255, 255, 0.35);
  --glass-border-dark:  rgba(255, 255, 255, 0.08);

  /* Blur levels */
  --blur-sm:  4px;
  --blur-md:  12px;
  --blur-lg:  24px;
  --blur-xl:  48px;

  /* Shadows */
  --shadow-glass: 0 8px 32px rgba(0, 0, 0, 0.25), inset 0 1px 0 rgba(255, 255, 255, 0.15);
  --shadow-glass-hover: 0 16px 48px rgba(0, 0, 0, 0.35), inset 0 1px 0 rgba(255, 255, 255, 0.25);
  --shadow-glow-blue: 0 0 24px rgba(99, 102, 241, 0.4);
}
```

---

## 2. Tailwind Config Extension

```js
// tailwind.config.js
module.exports = {
  theme: {
    extend: {
      backdropBlur: {
        xs: '2px',
        '2xl': '40px',
        '3xl': '60px',
      },
      backgroundColor: {
        'glass':      'rgba(255, 255, 255, 0.10)',
        'glass-dark': 'rgba(0, 0, 0, 0.12)',
        'glass-heavy': 'rgba(255, 255, 255, 0.22)',
      },
      borderColor: {
        'glass':       'rgba(255, 255, 255, 0.18)',
        'glass-light': 'rgba(255, 255, 255, 0.35)',
      },
      boxShadow: {
        'glass':        '0 8px 32px rgba(0,0,0,0.25), inset 0 1px 0 rgba(255,255,255,0.15)',
        'glass-lg':     '0 20px 60px rgba(0,0,0,0.35), inset 0 1px 0 rgba(255,255,255,0.2)',
        'glass-glow':   '0 8px 32px rgba(0,0,0,0.25), 0 0 24px rgba(99,102,241,0.4)',
        'inner-glass':  'inset 0 1px 0 rgba(255,255,255,0.15)',
      },
    },
  },
}
```

---

## 3. Glass Component Classes (Tailwind v3/v4)

### Base Glass Card

```html
<!-- Standard glass card -->
<div class="
  bg-white/10
  backdrop-blur-md
  border border-white/20
  rounded-2xl
  shadow-glass
  p-6
">
  Content
</div>

<!-- Heavy frost (more opaque) -->
<div class="
  bg-white/[0.18]
  backdrop-blur-xl
  border border-white/25
  rounded-3xl
  shadow-glass-lg
  p-8
">
```

### Glass Card with Hover

```html
<div class="
  group
  bg-white/10 hover:bg-white/[0.18]
  backdrop-blur-md
  border border-white/20 hover:border-white/35
  rounded-2xl
  shadow-glass hover:shadow-glass-lg
  transition-all duration-300 ease-out
  cursor-pointer
  p-6
">
```

---

## 4. Glass Navigation Bar

```html
<nav class="
  fixed top-0 left-0 right-0 z-50
  bg-white/8
  backdrop-blur-2xl
  border-b border-white/10
  shadow-[0_1px_0_rgba(255,255,255,0.08),0_4px_24px_rgba(0,0,0,0.2)]
">
  <div class="max-w-7xl mx-auto px-6 h-16 flex items-center justify-between">

    <a href="/" class="text-white font-bold text-xl tracking-tight">Logo</a>

    <div class="flex items-center gap-1">
      <a href="/dashboard" class="
        px-4 py-2 rounded-xl
        text-white/70 hover:text-white
        hover:bg-white/10
        transition-all duration-200
        text-sm font-medium
      ">Dashboard</a>

      <!-- Active state -->
      <a href="/orders" class="
        px-4 py-2 rounded-xl
        text-white
        bg-white/15
        border border-white/20
        text-sm font-medium
      ">Orders</a>
    </div>

    <!-- Glass button -->
    <button class="
      px-5 py-2 rounded-xl
      bg-indigo-500/80 hover:bg-indigo-500
      backdrop-blur-sm
      border border-indigo-400/30
      text-white text-sm font-medium
      shadow-[0_0_20px_rgba(99,102,241,0.4)]
      transition-all duration-200
    ">
      Sign In
    </button>

  </div>
</nav>
```

---

## 5. Glass Modal / Dialog

```html
<!-- Backdrop -->
<div class="fixed inset-0 z-50 flex items-center justify-center">

  <!-- Blur overlay -->
  <div class="absolute inset-0 bg-black/40 backdrop-blur-sm"></div>

  <!-- Glass panel -->
  <div class="
    relative z-10
    w-full max-w-lg mx-4
    bg-white/[0.12]
    backdrop-blur-2xl
    border border-white/20
    rounded-3xl
    shadow-glass-lg
    p-8
  ">
    <h2 class="text-white text-xl font-semibold mb-4">Modal Title</h2>
    <p class="text-white/70 text-sm mb-6">Modal content goes here.</p>

    <div class="flex gap-3 justify-end">
      <!-- Ghost button -->
      <button class="
        px-5 py-2 rounded-xl
        border border-white/20
        text-white/70 hover:text-white hover:border-white/35
        text-sm transition-all duration-200
      ">Cancel</button>

      <!-- Solid glass button -->
      <button class="
        px-5 py-2 rounded-xl
        bg-indigo-500/90 hover:bg-indigo-500
        border border-indigo-400/30
        text-white text-sm font-medium
        shadow-[0_0_16px_rgba(99,102,241,0.35)]
        transition-all duration-200
      ">Confirm</button>
    </div>
  </div>
</div>
```

---

## 6. Glass Stats / Dashboard Cards

```html
<!-- Metric card with colored glow -->
<div class="
  relative overflow-hidden
  bg-white/8
  backdrop-blur-md
  border border-white/15
  rounded-2xl
  p-6
  shadow-glass
">
  <!-- Ambient glow blob -->
  <div class="absolute -top-8 -right-8 w-32 h-32 rounded-full bg-indigo-500/20 blur-2xl"></div>
  <div class="absolute -bottom-8 -left-8 w-24 h-24 rounded-full bg-violet-500/15 blur-2xl"></div>

  <div class="relative">
    <p class="text-white/50 text-xs font-medium uppercase tracking-wider mb-1">Revenue</p>
    <p class="text-white text-3xl font-bold">$42,800</p>
    <span class="inline-flex items-center gap-1 mt-2 text-emerald-400 text-xs font-medium">
      ↑ 12.4%
    </span>
  </div>
</div>
```

---

## 7. Glass Input / Form Field

```html
<div class="space-y-1">
  <label class="text-white/60 text-sm font-medium">Temperature</label>
  <input
    type="text"
    class="
      w-full
      bg-white/8 hover:bg-white/12 focus:bg-white/15
      backdrop-blur-sm
      border border-white/15 hover:border-white/25 focus:border-white/40
      rounded-xl px-4 py-2.5
      text-white placeholder-white/30
      text-sm
      outline-none
      transition-all duration-200
      shadow-inner
    "
    placeholder="Enter value..."
  />
</div>
```

---

## 8. Background Required for Glass Effect

Glass NEEDS contrast behind it. Use one of:

```html
<!-- Option A: Gradient background -->
<div class="min-h-screen bg-gradient-to-br from-slate-900 via-indigo-950 to-slate-900">

<!-- Option B: Background image with dark overlay -->
<div class="min-h-screen relative">
  <img class="absolute inset-0 w-full h-full object-cover" src="/bg.jpg" />
  <div class="absolute inset-0 bg-slate-900/60"></div>
  <!-- glass components go here, above the overlays -->
</div>

<!-- Option C: Animated gradient blobs -->
<div class="min-h-screen bg-slate-900 relative overflow-hidden">
  <div class="absolute top-1/4 left-1/4 w-96 h-96 rounded-full bg-indigo-600/30 blur-3xl animate-pulse"></div>
  <div class="absolute bottom-1/4 right-1/4 w-80 h-80 rounded-full bg-violet-600/25 blur-3xl animate-pulse delay-1000"></div>
</div>
```

---

## 9. Blazor Integration

```razor
@* GlassCard.razor *@
<div class="bg-white/10 backdrop-blur-md border border-white/20 rounded-2xl shadow-glass p-6 @Class">
    @ChildContent
</div>

@code {
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public string Class { get; set; } = "";
}
```

---

## Quick Reference: Opacity Guide

| `bg-white/X` | Use case |
|---|---|
| `/5` | Barely visible tint |
| `/10` | Standard glass card |
| `/[0.18]` | Medium frost |
| `/25` | Heavy frost |
| `/40` | Near-opaque overlay |

| `backdrop-blur-X` | Effect |
|---|---|
| `sm` (4px) | Slight blur |
| `md` (12px) | Standard frosted glass |
| `xl` (24px) | Heavy frost |
| `2xl` (40px) | Maximum glass |
