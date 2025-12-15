using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Gamma_Manager
{
    public class HotkeyConfig
    {
        public string PresetName { get; set; }
        public uint Modifiers { get; set; }
        public uint Key { get; set; }
        public int HotkeyId { get; set; }
    }

    public class HotkeyManager
    {
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        // Modifiers
        public const uint MOD_ALT = 0x0001;
        public const uint MOD_CONTROL = 0x0002;
        public const uint MOD_SHIFT = 0x0004;
        public const uint MOD_WIN = 0x0008;

        private IntPtr _windowHandle;
        private Dictionary<int, string> _hotkeyPresetMap;
        private List<HotkeyConfig> _hotkeyConfigs;
        private int _nextHotkeyId = 1;

        private static string ConfigFile
        {
            get
            {
                string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string dir = Path.Combine(appData, "GammaManager");
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                return Path.Combine(dir, "hotkeys.txt");
            }
        }

        public HotkeyManager(IntPtr windowHandle)
        {
            _windowHandle = windowHandle;
            _hotkeyPresetMap = new Dictionary<int, string>();
            _hotkeyConfigs = new List<HotkeyConfig>();
            LoadHotkeyConfigs();
        }

        private void LoadHotkeyConfigs()
        {
            if (!File.Exists(ConfigFile))
            {
                return;
            }

            string[] lines = File.ReadAllLines(ConfigFile);
            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                string[] parts = line.Split('|');
                if (parts.Length == 3)
                {
                    HotkeyConfig config = new HotkeyConfig
                    {
                        PresetName = parts[0],
                        Modifiers = uint.Parse(parts[1]),
                        Key = uint.Parse(parts[2]),
                        HotkeyId = _nextHotkeyId++
                    };
                    _hotkeyConfigs.Add(config);
                }
            }
        }

        private void SaveHotkeyConfigs()
        {
            StringBuilder sb = new StringBuilder();
            foreach (HotkeyConfig config in _hotkeyConfigs)
            {
                sb.AppendLine(string.Format("{0}|{1}|{2}", config.PresetName, config.Modifiers, config.Key));
            }
            File.WriteAllText(ConfigFile, sb.ToString());
        }

        public void RegisterHotkeys()
        {
            foreach (HotkeyConfig config in _hotkeyConfigs)
            {
                bool success = RegisterHotKey(_windowHandle, config.HotkeyId, config.Modifiers, config.Key);
                if (success)
                {
                    _hotkeyPresetMap[config.HotkeyId] = config.PresetName;
                }
            }
        }

        public void UnregisterAllHotkeys()
        {
            foreach (int hotkeyId in _hotkeyPresetMap.Keys)
            {
                UnregisterHotKey(_windowHandle, hotkeyId);
            }
            _hotkeyPresetMap.Clear();
        }

        public string GetPresetForHotkey(int hotkeyId)
        {
            if (_hotkeyPresetMap.ContainsKey(hotkeyId))
            {
                return _hotkeyPresetMap[hotkeyId];
            }
            return null;
        }

        public void AddHotkey(string presetName, uint modifiers, uint key)
        {
            // Remove existing hotkey for this preset
            RemoveHotkey(presetName);

            HotkeyConfig config = new HotkeyConfig
            {
                PresetName = presetName,
                Modifiers = modifiers,
                Key = key,
                HotkeyId = _nextHotkeyId++
            };
            _hotkeyConfigs.Add(config);
            SaveHotkeyConfigs();

            // Register immediately if window is available
            if (_windowHandle != IntPtr.Zero)
            {
                if (RegisterHotKey(_windowHandle, config.HotkeyId, config.Modifiers, config.Key))
                {
                    _hotkeyPresetMap[config.HotkeyId] = config.PresetName;
                }
            }
        }

        public void RemoveHotkey(string presetName)
        {
            HotkeyConfig existing = _hotkeyConfigs.Find(c => c.PresetName == presetName);
            if (existing != null)
            {
                UnregisterHotKey(_windowHandle, existing.HotkeyId);
                _hotkeyPresetMap.Remove(existing.HotkeyId);
                _hotkeyConfigs.Remove(existing);
                SaveHotkeyConfigs();
            }
        }

        public string GetHotkeyForPreset(string presetName)
        {
            HotkeyConfig config = _hotkeyConfigs.Find(c => c.PresetName == presetName);
            if (config == null) return "";

            StringBuilder sb = new StringBuilder();
            if ((config.Modifiers & MOD_CONTROL) != 0) sb.Append("Ctrl+");
            if ((config.Modifiers & MOD_ALT) != 0) sb.Append("Alt+");
            if ((config.Modifiers & MOD_SHIFT) != 0) sb.Append("Shift+");
            if ((config.Modifiers & MOD_WIN) != 0) sb.Append("Win+");
            
            sb.Append(((Keys)config.Key).ToString());
            return sb.ToString();
        }

        public List<HotkeyConfig> GetAllHotkeys()
        {
            return new List<HotkeyConfig>(_hotkeyConfigs);
        }
    }
}
