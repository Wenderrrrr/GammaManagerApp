# Gamma Manager

![Gamma Manager UI](GammaManager.png?raw=true)

**Gamma Manager** allows you to manage gamma ramp options independently for RGB channels or altogether. Designed for Windows with a modern, dark-themed interface.

## Installation
Download the latest installer from the [Releases](https://github.com/Wenderrrrr/GammaManagerApp/releases) page.
Run `GammaSetup.exe` to install.

## Features
*   **Gamma Control**: Range 0.30 — 4.40 (Default 1.00).
*   **Brightness**: Range -1.00 — 1.00 (Default 0.00).
*   **Contrast**: Range 0.10 — 3.00 (Default 1.00). Extensible to 100.00.
*   **Monitor Support**: Manage multiple monitors independently.
*   **Presets**: Save and load settings presets.
*   **Custom Hotkeys**: Assign keyboard shortcuts to any preset.
*   **Tray Integration**: Minimalistic tray icon for quick access and preset switching.

## Usage
*   **Sliders**: Drag sliders for adjustments or click the number box to fine-tune.
*   **Navigation**: Use the sidebar to switch between Gamma, Contrast, Brightness, and Presets.
*   **Reset**: Returns all values to default positions (does not undo previous changes).
*   **Hide**: Minimizes app to tray.
*   **Restore**: Double-click tray icon or Right-click -> Settings.

### Custom Keybinds
You can assign global hotkeys to apply specific presets instantly, even while gaming or in other apps.

1.  Navigate to the **Presets** tab.
2.  Save your current settings as a new preset (or use an existing one).
3.  Click the **"Set Hotkey"** button next to the preset name.
4.  Press your desired key combination (e.g., `Ctrl` + `Alt` + `1`).
5.  Click **Save**.

The hotkey will now be active globally. You can see assigned hotkeys listed next to each preset.

## Technical Details
This application overrides current gamma settings using the Windows API `CreateGammaRamp`.
Settings and hotkeys are saved in the application directory.
