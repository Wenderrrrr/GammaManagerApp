using System;
using System.Runtime.InteropServices;

namespace Gamma_Manager
{
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    public class AppBridge
    {
        private Window _window;

        public AppBridge(Window window)
        {
            _window = window;
        }

        public void SetGamma(string channel, int value)
        {
            _window.UpdateGammaFromWeb(channel, value);
        }

        public void SetContrast(string channel, int value)
        {
            _window.UpdateContrastFromWeb(channel, value);
        }

        public void SetBrightness(string channel, int value)
        {
            _window.UpdateBrightnessFromWeb(channel, value);
        }

        public void SetMonitor(int monitorIndex)
        {
            _window.UpdateMonitorFromWeb(monitorIndex);
        }

        public string GetMonitorList()
        {
            return _window.GetMonitorListJson();
        }

        public string GetInitialState()
        {
            return _window.GetStateJson();
        }

        // Preset methods
        public void SavePreset(string name)
        {
            _window.SaveCurrentPreset(name);
        }

        public void LoadPreset(string name)
        {
            _window.ApplyPreset(name);
        }

        public string GetPresetList()
        {
            return _window.GetPresetsJson();
        }

        public void DeletePreset(string name)
        {
            _window.DeletePresetByName(name);
        }

        // Hotkey configuration methods
        public void SetHotkey(string presetName, int modifiers, int key)
        {
            _window.SetHotkeyForPreset(presetName, (uint)modifiers, (uint)key);
        }

        public string GetHotkeyForPreset(string presetName)
        {
            return _window.GetHotkeyString(presetName);
        }

        public void RemoveHotkey(string presetName)
        {
            _window.RemoveHotkeyForPreset(presetName);
        }
    }
}
