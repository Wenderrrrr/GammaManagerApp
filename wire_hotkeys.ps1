$content = Get-Content 'Gamma Manager\Window.cs' -Raw

# Add HotkeyManager field and initialization
$pattern = '(?s)(private WebView2 webView;)(.*?)(private void Window_Load)'
$replacement = @'
$1
        private HotkeyManager hotkeyManager;
$2$3
'@
$content = $content -replace $pattern, $replacement

# Add hotkey initialization in Window_Load
$pattern = '(?s)(InitializeWebView\(\);)(.*?)(}\s+// Bridge Methods)'
$replacement = @'
$1

            // Initialize hotkey manager
            hotkeyManager = new HotkeyManager(this.Handle);
            hotkeyManager.RegisterDefaultHotkeys();
$2$3
'@
$content = $content -replace $pattern, $replacement

# Add WndProc override
$pattern = '(?s)(public void DeletePresetByName.*?}\s+)(public string GetStateJson)'
$replacement = @'
$1
        // Handle hotkey messages
        protected override void WndProc(ref Message m)
        {
            const int WM_HOTKEY = 0x0312;
            if (m.Msg == WM_HOTKEY)
            {
                int hotkeyId = m.WParam.ToInt32();
                string presetName = hotkeyManager.GetPresetForHotkey(hotkeyId);
                if (presetName != null)
                {
                    ApplyPreset(presetName);
                }
            }
            base.WndProc(ref m);
        }

        protected override void OnFormClosing(System.Windows.Forms.FormClosingEventArgs e)
        {
            if (hotkeyManager != null)
            {
                hotkeyManager.UnregisterAllHotkeys();
            }
            base.OnFormClosing(e);
        }

        $2
'@
$content = $content -replace $pattern, $replacement

$content | Set-Content 'Gamma Manager\Window.cs' -NoNewline
