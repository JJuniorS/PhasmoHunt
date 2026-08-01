# Settings Window — Design Spec

**Date:** 2026-08-01  
**Status:** Approved for implementation

## Goal

Replace the expandable settings panel in `MainWindow` with a modal settings window that matches the overlay chrome.

## Decisions

| Topic | Choice |
|-------|--------|
| Behavior | Modal (`ShowDialog`), blocks main window |
| Chrome | Same as overlay (`WindowStyle=None`, dark rounded panel) |
| Approach | `SettingsWindow` shares `MainViewModel` for Opacity/UiScale bindings |

## Behavior

- Gear button opens `SettingsWindow` with `Owner = MainWindow` via `ShowDialog()`.
- Closing via ✕ or system close returns to main.
- Sliders: Transparency 0.3–1.0, UI scale 0.8–1.5 (existing ranges).
- Live updates: main window opacity/scale still react through existing `MainViewModel` property change handlers.
- Settings window keeps full opacity for readability.
- Remove inline settings panel and `IsSettingsExpanded` / `ShowSettingsPanel`.

## Files

- Create: `Views/SettingsWindow.xaml`, `Views/SettingsWindow.xaml.cs`
- Modify: `Views/MainWindow.xaml`, `Views/MainWindow.xaml.cs`, `ViewModels/MainViewModel.cs`

## Out of scope

- New settings fields, themes, hotkey remapping UI
