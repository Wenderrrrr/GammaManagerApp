# Gamma Manager

![image](GammaManager.jpg?raw=true)

**Gamma Manager** allows you to manage gamma ramp options independently for RGB channels or altogether. Designed for Windows.

## Installation
Download the latest installer from the [Releases](https://github.com/Wenderrrrr/GammaManagerApp/releases) page.
Run `GammaSetup.exe` to install.

## Features
*   **Gamma Control**: Range 0.30 — 4.40 (Default 1.00).
*   **Brightness**: Range -1.00 — 1.00 (Default 0.00).
*   **Contrast**: Range 0.10 — 3.00 (Default 1.00). Extensible to 100.00.
*   **Monitor Support**: Manage multiple monitors independently.
*   **Presets**: Save and load settings presets.
*   **Tray Integration**: Minimalistic tray icon for quick access and preset switching.

## Usage
*   **Sliders**: Pull trackbar pointer for major changes. Use keyboard arrows or mouse click for minor adjustments.
*   **Presets**:
    *   To **Save**: Type a name in the empty list box and click "Save".
    *   To **Delete**: Select a preset and click "Delete".
*   **Reset**: Returns all values to default positions (does not undo previous changes).
*   **Hide**: Minimizes app to tray.
*   **Restore**: Double-click tray icon or Right-click -> Settings.

## Technical Details
This application overrides current gamma settings using the Windows API `CreateGammaRamp`.
Settings are saved in an `.ini` file in the application directory.
