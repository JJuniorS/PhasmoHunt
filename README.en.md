# Phasmo Hunt

**Language / Idioma:** [Português](README.md) | [English](README.en.md)

Manual external assistant for **Phasmophobia** — Always-On-Top overlay on Windows (.NET 8 + WPF).

Helps you identify the ghost by measuring footstep speed, cross-checking evidence, and tracking hunt peculiarities. **Every input is entered by you** (hotkeys / UI). Nothing is read from the game process.

---

## Features

### Speed measurement

- Each press of the step hotkey records a timestamp (the first press also starts the session).
- After **3 seconds** with no new steps, the reading finishes automatically.
- Clicks are split into **3 averages** (`P1` / `P2` / `P3`): `floor(n/3)` steps per part; remainder is discarded.
- Useful minimum: **6 clicks** (2 per part). E.g. 6 → 2/2/2 · 24 → 8/8/8 · 7 → 2/2/2 (drops 1).
- Speed in m/s aligned with the Zero-Network community BPM finder.
- Match tolerance: **±0.10 m/s**.
- Shows reading pattern (stable, speeding up, slowing down, irregular) and **reliability**.

#### Reliability

**Reliability** (0–100%) reflects how **regular** the intervals between the steps you logged were. A steady rhythm raises the percentage; uneven taps, delays, or mixed pace lower it. It helps you decide whether the average is safe to use for ghost filtering or whether to **re-run the reading** — it does not name the ghost; it only rates the quality of the measurement.

### Catalog and filters

- Offline catalog of **30 ghosts** (speeds + evidence), based on public [wiki](https://phasmophobia.fandom.com/wiki/Ghost) data.
- Filters by measured speed **and** selected evidence.
- **The Mimic** includes the fake Orb in the filter (as in the game).
- List colors: green = match · red = out · neutral = pending.
- Reading history with session consensus across measurements.

### Peculiarities

Timers and markers useful during the investigation:

| Peculiarity | Hotkey | Behavior |
|-------------|--------|----------|
| **Demon** | `Shift + 1` | Cooldown stopwatch. If it stops under 25 s, filters the catalog to Demon. |
| **Incense** | `Shift + 2` | 3-minute countdown (resets on each press). |
| **Obambo** | `Shift + 3` | Peace / aggressive cycle every 2 minutes. |

### Interface

- Always-On-Top window, collapsible to free screen space.
- **Evidence** panel to filter alongside speed.
- **Settings:** opacity (0.3–1.0) and UI scale (0.8–1.5), saved in `%AppData%\PhasmoHunt\settings.json`.
- **Clear** resets measurement, evidence, history, and peculiarities.

### Hotkeys

| Action | Hotkey |
|--------|--------|
| Log step | Key `1` or mouse **side button** (rear) |
| Demon | `Shift + 1` |
| Incense | `Shift + 2` |
| Obambo | `Shift + 3` |

---

## Quick start

1. When you hear steps, press `1` or the **side button** (each press = 1 step).
2. Stop clicking for **3 seconds** — the reading finishes on its own.
3. Check overall average, the 3 parts, and matching ghosts.
4. Mark **Evidence** to refine the filter.
5. Use peculiarities (`Shift + 1/2/3`) as the investigation goes.

---

## What this app does NOT do

- Does not read game memory
- Does not access the Phasmophobia process
- Does not do graphics hooking / DLL injection
- Does not capture game audio or screen
- Does not automate in-game actions

It is a manual helper overlay — not a cheat client.

---

## Ghost catalog

Hunt speeds are embedded in `GhostCatalogService`. Nothing is read from the game client. Public reference: [Ghost — Phasmophobia Wiki](https://phasmophobia.fandom.com/wiki/Ghost).

---

## Development

Requirements: .NET 8 SDK (Windows).

```bash
dotnet build src/PhasmoHunt/PhasmoHunt.csproj
dotnet run --project src/PhasmoHunt/PhasmoHunt.csproj
```

### Structure

```
src/PhasmoHunt/
  Models/       # Ghosts, evidence, measurements, settings
  Services/     # Catalog, speed calc, hotkeys, persistence
  ViewModels/   # UI logic
  Views/        # MainWindow (WPF)
  Themes/       # Dark theme
  Assets/       # Evidence and peculiarity icons
src/website/    # Landing page (GitHub Pages)
```

### Assets / icons

Evidence, peculiarity, and app icons were **created for this project** (AI-generated / original art). They do not include official Phasmophobia or Kinetic Games assets.

---

## Website and download

| What | Where |
|------|--------|
| Static site | `src/website/` |
| Deploy | GitHub Actions → **GitHub Pages** (`Deploy website` workflow) |
| Binary link | `downloadUrl` field in [`src/website/release.json`](src/website/release.json) (Google Drive, etc.) |
| Version history | [CHANGELOG.md](CHANGELOG.md) + [GitHub Releases](https://github.com/JJuniorS/PhasmoHunt/releases) (`v*` tags) |

The site download button reads `release.json` at runtime. While `downloadUrl` is empty, the CTA shows as “coming soon”.

---

## License

Code under the [MIT license](LICENSE).

Phasmophobia and Kinetic Games are trademarks of their respective owners. Phasmo Hunt is an unofficial community tool and is **not affiliated with, endorsed by, or sponsored by** Kinetic Games.
