# PhasmoHunt Website — Design Spec

**Date:** 2026-08-01  
**Status:** Approved for planning  
**Location:** `src/website/`

## Goal

Landing page estática para apresentar o Phasmo Hunt à comunidade de Phasmophobia e disponibilizar um CTA de download (placeholder por enquanto). Tom de publicação comunitária: útil, transparente, explicitamente não-cheat.

## Decisions

| Topic | Choice |
|-------|--------|
| Download link | Placeholder button only (`#` / `#download`) |
| Language | Bilingual: PT default + EN toggle (`localStorage`) |
| Structure | Single landing page |
| Visual direction | Evidence Board (cold blue, case-file / HUD) |
| Hero visual | Screenshot placeholder (replaceable later) |
| Approach | Classic static: `index.html` + `css/styles.css` + `js/app.js` |

## Structure

```
src/website/
  index.html
  css/styles.css
  js/app.js
  assets/
    app-icon.png
    screenshot-placeholder.svg
```

### Page sections (anchors)

1. **Nav** — logo/wordmark, links (Features, Uso, Download), PT/EN toggle  
2. **Hero** — eyebrow “CASE FILE”, headline, short pitch, download CTA, screenshot placeholder  
3. **Features** — cards: velocidade, evidências, peculiaridades, overlay Always-On-Top  
4. **Como usar** — numbered steps from README (hotkey → 3s idle → 3 averages → evidence filter)  
5. **Download** — repeated CTA + Windows / offline notes  
6. **Disclaimer** — strong “what this app does NOT do” list  
7. **Footer** — community tool disclaimer (not affiliated with Kinetic Games)

## Visual design

- **Palette:** background `#12151a`, panels `#1a1e24`, borders `#2a3140`, accent `#7c9cff`, text `#e8ecf2`, muted `#9aa4b2`
- **Typography:** IBM Plex Sans via Google Fonts — avoid Inter/Roboto/Arial/system-only stack as the brand voice
- **Atmosphere:** subtle grain or soft radial vignette OK; no heavy glow particles; no purple-gradient clichés
- **Hero:** one composition — brand + one headline + one supporting sentence + one CTA group + one dominant screenshot placeholder
- **Cards:** allowed only as interactive/feature containers in Features
- **Responsive:** desktop + mobile

## Content sources

Copy derived from `README.md` (features, hotkeys, anti-cheat list). Ghost names stay in English. UI strings exist in both PT and EN dictionaries in `js/app.js`.

## Behavior (`js/app.js`)

- Apply i18n via `data-i18n` keys; default `pt`; persist choice in `localStorage`
- Smooth scroll for nav anchors
- Optional light “active section” highlight
- Light motion: section fade-in on viewport entry; hover on cards/CTA
- Download buttons do not point to a real binary yet

## Out of scope

- Backend, analytics, forms
- Hosting/publishing the `.exe`
- Build tooling / framework
- Real screenshots (placeholder only)
- Theme switcher beyond PT/EN
- Integration with the WPF project beyond copying `app-icon.png`

## Success criteria

- Opening `src/website/index.html` shows a complete community-ready landing page
- PT/EN toggle works without reload
- Download CTA is visible and clearly a button (placeholder href)
- Disclaimer is unmistakable
- Look matches Evidence Board direction and feels Phasmophobia-adjacent without claiming official affiliation
