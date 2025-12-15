$content = Get-Content 'Gamma Manager\Window.cs' -Raw

# Add monitor methods after UpdateBrightnessFromWeb
$pattern = '(?s)(Gamma\.SetGammaRamp\(currDisplay\.displayLink,.*?currDisplay\.bBright\)\);.*?\}\)\);.*?}\s+)(public string GetStateJson)'
$newMethods = @'
$1
        public void UpdateMonitorFromWeb(int monitorIndex)
        {
            this.Invoke(new Action(() => {
                if (monitorIndex >= 0 && monitorIndex < displays.Count)
                {
                    numDisplay = monitorIndex;
                    currDisplay = displays[numDisplay];
                    // Optionally refresh UI or apply current settings to new monitor
                }
            }));
        }

        public string GetMonitorListJson()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("[");
            for (int i = 0; i < displays.Count; i++)
            {
                if (i > 0) sb.Append(",");
                sb.Append("\"");
                sb.Append((i + 1).ToString());
                sb.Append(") ");
                sb.Append(displays[i].displayName.Replace("\"", "\\\""));
                sb.Append("\"");
            }
            sb.Append("]");
            return sb.ToString();
        }

        $2
'@

$content = $content -replace $pattern, $newMethods
$content | Set-Content 'Gamma Manager\Window.cs' -NoNewline
