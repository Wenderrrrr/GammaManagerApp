$content = Get-Content 'Gamma Manager\Window.cs' -Raw

# Add DwmSetWindowAttribute import and dark title bar method
$pattern = '(?s)(using System\.Runtime\.InteropServices;)'
$replacement = @'
$1
using System.Drawing;
'@
$content = $content -replace $pattern, $replacement

# Add dark title bar method before Window_Load
$pattern = '(?s)(private void Window_Load\(object sender, EventArgs e\))'
$replacement = @'
[DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

        private void EnableDarkTitleBar()
        {
            if (DwmSetWindowAttribute(this.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref int value, sizeof(int)) != 0)
            {
                DwmSetWindowAttribute(this.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1, ref value, sizeof(int));
            }
        }

        $1
'@
$content = $content -replace $pattern, $replacement

# Fix the method - need to pass value correctly
$content = $content -replace 'ref int value', 'ref darkMode'
$content = $content -replace 'private void EnableDarkTitleBar\(\)', @'
private void EnableDarkTitleBar()
        {
            int darkMode = 1;
'@

$content | Set-Content 'Gamma Manager\Window.cs' -NoNewline
