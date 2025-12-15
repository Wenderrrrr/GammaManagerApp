$content = Get-Content 'Gamma Manager\Window.cs' -Raw

# Add hotkey configuration methods
$pattern = '(?s)(public void DeletePresetByName\(string name\).*?}\s+)(// Handle hotkey messages)'
$replacement = @'
$1
        // Hotkey configuration methods
        public void SetHotkeyForPreset(string presetName, uint modifiers, uint key)
        {
            if (hotkeyManager != null)
            {
                hotkeyManager.AddHotkey(presetName, modifiers, key);
            }
        }

        public string GetHotkeyString(string presetName)
        {
            if (hotkeyManager != null)
            {
                return hotkeyManager.GetHotkeyForPreset(presetName);
            }
            return "";
        }

        public void RemoveHotkeyForPreset(string presetName)
        {
            if (hotkeyManager != null)
            {
                hotkeyManager.RemoveHotkey(presetName);
            }
        }

        $2
'@
$content = $content -replace $pattern, $replacement

$content | Set-Content 'Gamma Manager\Window.cs' -NoNewline
