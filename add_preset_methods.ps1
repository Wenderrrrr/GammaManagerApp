$content = Get-Content 'Gamma Manager\Window.cs' -Raw

# Add preset methods after GetMonitorListJson
$pattern = '(?s)(public string GetMonitorListJson.*?return sb\.ToString\(\);.*?}\s+)(public string GetStateJson)'
$newMethods = @'
$1
        // Preset Management
        public void SaveCurrentPreset(string name)
        {
            this.Invoke(new Action(() => {
                Preset preset = new Preset
                {
                    Name = name,
                    RGamma = currDisplay.rGamma,
                    GGamma = currDisplay.gGamma,
                    BGamma = currDisplay.bGamma,
                    RContrast = currDisplay.rContrast,
                    GContrast = currDisplay.gContrast,
                    BContrast = currDisplay.bContrast,
                    RBright = currDisplay.rBright,
                    GBright = currDisplay.gBright,
                    BBright = currDisplay.bBright
                };
                PresetManager.SavePreset(name, preset);
            }));
        }

        public void ApplyPreset(string name)
        {
            this.Invoke(new Action(() => {
                Preset preset = PresetManager.LoadPreset(name);
                if (preset != null)
                {
                    currDisplay.rGamma = preset.RGamma;
                    currDisplay.gGamma = preset.GGamma;
                    currDisplay.bGamma = preset.BGamma;
                    currDisplay.rContrast = preset.RContrast;
                    currDisplay.gContrast = preset.GContrast;
                    currDisplay.bContrast = preset.BContrast;
                    currDisplay.rBright = preset.RBright;
                    currDisplay.gBright = preset.GBright;
                    currDisplay.bBright = preset.BBright;

                    Gamma.SetGammaRamp(currDisplay.displayLink,
                        Gamma.CreateGammaRamp(currDisplay.rGamma, currDisplay.gGamma, currDisplay.bGamma,
                        currDisplay.rContrast, currDisplay.gContrast, currDisplay.bContrast,
                        currDisplay.rBright, currDisplay.gBright, currDisplay.bBright));
                }
            }));
        }

        public string GetPresetsJson()
        {
            System.Collections.Generic.List<string> presets = PresetManager.GetPresetList();
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("[");
            for (int i = 0; i < presets.Count; i++)
            {
                if (i > 0) sb.Append(",");
                sb.Append("\"");
                sb.Append(presets[i].Replace("\"", "\\\""));
                sb.Append("\"");
            }
            sb.Append("]");
            return sb.ToString();
        }

        public void DeletePresetByName(string name)
        {
            PresetManager.DeletePreset(name);
        }

        $2
'@

$content = $content -replace $pattern, $newMethods
$content | Set-Content 'Gamma Manager\Window.cs' -NoNewline
