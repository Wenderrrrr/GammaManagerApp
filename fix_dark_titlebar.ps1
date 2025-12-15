$content = Get-Content 'Gamma Manager\Window.cs' -Raw

# Add DwmSetWindowAttribute import at the top with other DllImports
$pattern = '(?s)(\[DllImport\("user32\.dll"\)\].*?private static extern bool RegisterHotKey)'
$replacement = @'
[DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

        $1
'@
$content = $content -replace $pattern, $replacement

# Add EnableDarkTitleBar method before Window_Load
$pattern = '(?s)(private void Window_Load\(object sender, EventArgs e\))'
$replacement = @'
private void EnableDarkTitleBar()
        {
            int darkMode = 1;
            if (DwmSetWindowAttribute(this.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref darkMode, sizeof(int)) != 0)
            {
                DwmSetWindowAttribute(this.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1, ref darkMode, sizeof(int));
            }
        }

        $1
'@
$content = $content -replace $pattern, $replacement

$content | Set-Content 'Gamma Manager\Window.cs' -NoNewline
