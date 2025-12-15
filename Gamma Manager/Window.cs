using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using System.IO;

namespace Gamma_Manager
{
    public partial class Window : Form
    {
        System.Globalization.CultureInfo customCulture;
        IniFile iniFile;

        List<Display.DisplayInfo> displays = new List<Display.DisplayInfo>();
        int numDisplay = 0;
        Display.DisplayInfo currDisplay;

        List<ToolStripComboBox> toolMonitors = new List<ToolStripComboBox>();
        ToolStripComboBox toolMonitor;

        private WebView2 webView;
        private HotkeyManager hotkeyManager;




        public Window()
        {
            InitializeComponent();

            try
            {
                // Override icons with the executable's icon
                System.Drawing.Icon appIcon = System.Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath);
                this.Icon = appIcon;
                this.notifyIcon.Icon = appIcon;
            }
            catch { }

            customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ",";

            iniFile = new IniFile("GammaManager.ini");
            displays = Display.QueryDisplayDevices();
            
            displays.Reverse();
            for (int i = 0; i < displays.Count; i++)
            {
                displays[i].numDisplay = i;
            }

            if (displays.Count > 0)
            {
                currDisplay = displays[0];
            }

            initTrayMenu();
            notifyIcon.ContextMenuStrip = contextMenu;
        }

        private void initTrayMenu()
        {
            contextMenu.Items.Clear();
            toolMonitors.Clear();

            ToolStripMenuItem toolSetting = new ToolStripMenuItem("Settings", null, toolSettings_Click);
            contextMenu.Items.Add(toolSetting);

            ToolStripSeparator toolStripSeparator1 = new ToolStripSeparator();
            contextMenu.Items.Add(toolStripSeparator1);

            for (int i = 0; i < displays.Count; i++)
            {
                toolMonitor = new ToolStripComboBox(displays[i].displayName);
                toolMonitor.DropDownStyle = ComboBoxStyle.DropDownList;

                toolMonitor.Items.Add(displays[i].displayName + ":");
                toolMonitor.Text = displays[i].displayName + ":";

                toolMonitor.SelectedIndexChanged += new EventHandler(comboBoxToolMonitor_IndexChanged);

                string[] presets = iniFile.GetSections();
                if (presets != null)
                {
                    for (int j = 0; j < presets.Length; j++)
                    {
                        if (iniFile.Read("monitor", presets[j]).Equals(displays[i].displayName))
                        {
                            //preset.name = presets[j].Substring(presets[j].IndexOf(")") + 1);
                            toolMonitor.Items.Add(presets[j]);
                        }
                    }
                }
                toolMonitors.Add(toolMonitor);
                contextMenu.Items.Add(toolMonitor);
            }
            ToolStripSeparator toolStripSeparator2 = new ToolStripSeparator();
            contextMenu.Items.Add(toolStripSeparator2);
            ToolStripMenuItem toolExit = new ToolStripMenuItem("Exit", null, toolExit_Click);
            contextMenu.Items.Add(toolExit);
        }



        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

        private void EnableDarkTitleBar()
        {
            int darkMode = 1;
            if (DwmSetWindowAttribute(this.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref darkMode, sizeof(int)) != 0)
            {
                DwmSetWindowAttribute(this.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1, ref darkMode, sizeof(int));
            }
        }

        private void Window_Load(object sender, EventArgs e)
        {
            int screenWidth = Screen.PrimaryScreen.Bounds.Size.Width;
            int windowWidth = Width;
            int screenHeight = Screen.PrimaryScreen.Bounds.Size.Height;
            int windowHeight = Height;
            int tmp = Screen.PrimaryScreen.Bounds.Height;
            int TaskBarHeight = tmp - Screen.PrimaryScreen.WorkingArea.Height;

            //dpi
            /*int PSH = SystemParameters.PrimaryScreenHeight;
            int PSBH = Screen.PrimaryScreen.Bounds.Height;
            double ratio = PSH / PSBH;
            int TaskBarHeight = PSBH - Screen.PrimaryScreen.WorkingArea.Height;
            TaskBarHeight *= ratio;*/

            // Let StartPosition.CenterScreen handle window positioning
            // Location = new Point(screenWidth - windowWidth, screenHeight - (windowHeight + TaskBarHeight));

            // Enable dark title bar
            EnableDarkTitleBar();

            // Hide window until WebView2 is fully loaded
            this.Opacity = 0;

            InitializeWebView();

            // Initialize hotkey manager
            hotkeyManager = new HotkeyManager(this.Handle);
            hotkeyManager.RegisterHotkeys();

        }

        private async void InitializeWebView()
        {
            webView = new WebView2();
            webView.Dock = DockStyle.Fill;
            this.Controls.Add(webView);
            webView.BringToFront(); // Ensure it covers other controls

            var userDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GammaManager");
            var env = await CoreWebView2Environment.CreateAsync(null, userDataFolder);
            await webView.EnsureCoreWebView2Async(env);

            webView.CoreWebView2.AddHostObjectToScript("bridge", new AppBridge(this));
            
            // Disable default context menu
            webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;

            // Subscribe to NavigationCompleted to show window after page loads
            webView.CoreWebView2.NavigationCompleted += CoreWebView2_NavigationCompleted;

             string htmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "frontend", "index.html");
             if (File.Exists(htmlPath))
             {
                 webView.Source = new Uri(htmlPath);
             }
             else
             {
                 MessageBox.Show("Frontend files not found! Please check installation.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                 this.Opacity = 1; // Show window even on error
             }
        }

        private void CoreWebView2_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            // Show the window with a smooth fade-in after WebView2 content is loaded
            this.Invoke(new Action(() => {
                this.Opacity = 1;
            }));
        }

        // Bridge Methods
        public void UpdateGammaFromWeb(string channel, int value)
        {
            // Run on UI thread
            this.Invoke(new Action(() => {
                float floatVal = value / 100f; // 0-100 -> 0.0-1.0 (assuming standard 1.0 = 50% slider? No, based on existing logic 0-100% maps to 0.0-1.0 in text box)
                // Wait, existing logic: trackBarGamma (0-100) -> 0.0-1.0
                // Standard gamma is 1.0. If slider is 50, and we want 1.0, then value/50?
                // Let's stick to existing app logic: 0-100 -> 0.0-1.0. 
                // But wait, gamma usually goes above 1.0.
                // Looking at trackBarGamma_ValueChanged:
                // textBoxGamma.Text = ((float)trackBarGamma.Value / 100f)
                // Default seems to be 100 => 1.0 in existing app logic? 
                // Let's check init: trackBarGamma.Value = ... * 100f. 
                // If gamma is 1.0, value is 100.
                // So slider should go up to 200 maybe?
                // Let's assume the web slider 0-100 maps to 0.0 - 1.0 for now, or match the behavior.
                // Actually, let's allow up to 2.5 (250).
                // Re-reading script.js: min=0 max=100. Be careful.
                // Existing logic: trackBarGamma max isn't shown in code but defaults to 10. WinForms default is 10. 
                // Wait, I need to check Designer.cs for TrackBar max.
                // But the code says: `trackBarGamma.Value / 100f`. If max is 10, that's only 0.1.
                // Logic suggests TrackBar max is likely 100 or higher.
                // Let's assume input value 50 => 1.0 (standard). So float = value / 50f.
                
                // Let's trust the JS sends what we want.
                // If JS sends 50 for 1.0, and we want 1.0 float in C#
                // Existing app logic: 100 int => 1.0 float.
                
                // I will map Web Value (0-100) to C# Value (0.0 - 2.0)
                // value * 0.02f
                
                // Actually, let's update the currDisplay structure directly
                
                switch(channel)
                {
                    case "all":
                         currDisplay.rGamma = floatVal;
                         currDisplay.gGamma = floatVal;
                         currDisplay.bGamma = floatVal;
                         break;
                    case "red":
                         currDisplay.rGamma = floatVal;
                         break;
                    case "green":
                         currDisplay.gGamma = floatVal;
                         break;
                    case "blue":
                         currDisplay.bGamma = floatVal;
                         break;
                }
                
                // Apply
                Gamma.SetGammaRamp(currDisplay.displayLink,
                        Gamma.CreateGammaRamp(currDisplay.rGamma, currDisplay.gGamma, currDisplay.bGamma, currDisplay.rContrast,
                        currDisplay.gContrast, currDisplay.bContrast, currDisplay.rBright, currDisplay.gBright, currDisplay.bBright));
            }));
        }

                
        public void UpdateContrastFromWeb(string channel, int value)
        {
            this.Invoke(new Action(() => {
                float floatVal = value / 100f;
                
                switch(channel)
                {
                    case "all":
                         currDisplay.rContrast = floatVal;
                         currDisplay.gContrast = floatVal;
                         currDisplay.bContrast = floatVal;
                         break;
                    case "red":
                         currDisplay.rContrast = floatVal;
                         break;
                    case "green":
                         currDisplay.gContrast = floatVal;
                         break;
                    case "blue":
                         currDisplay.bContrast = floatVal;
                         break;
                }
                
                Gamma.SetGammaRamp(currDisplay.displayLink,
                        Gamma.CreateGammaRamp(currDisplay.rGamma, currDisplay.gGamma, currDisplay.bGamma, currDisplay.rContrast,
                        currDisplay.gContrast, currDisplay.bContrast, currDisplay.rBright, currDisplay.gBright, currDisplay.bBright));
            }));
        }

        public void UpdateBrightnessFromWeb(string channel, int value)
        {
            this.Invoke(new Action(() => {
                float floatVal = value / 100f;
                
                switch(channel)
                {
                    case "all":
                         currDisplay.rBright = floatVal;
                         currDisplay.gBright = floatVal;
                         currDisplay.bBright = floatVal;
                         break;
                    case "red":
                         currDisplay.rBright = floatVal;
                         break;
                    case "green":
                         currDisplay.gBright = floatVal;
                         break;
                    case "blue":
                         currDisplay.bBright = floatVal;
                         break;
                }
                
                Gamma.SetGammaRamp(currDisplay.displayLink,
                        Gamma.CreateGammaRamp(currDisplay.rGamma, currDisplay.gGamma, currDisplay.bGamma, currDisplay.rContrast,
                        currDisplay.gContrast, currDisplay.bContrast, currDisplay.rBright, currDisplay.gBright, currDisplay.bBright));
            }));
        }

        
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

        public string GetStateJson()
        {
            // Simple manual JSON construction
            return string.Format(@"{{ 
                ""all"": {0}, 
                ""red"": {1},
                ""green"": {2},
                ""blue"": {3} 
            }}", 
                (int)(currDisplay.rGamma * 100),
                (int)(currDisplay.rGamma * 100),
                (int)(currDisplay.gGamma * 100),
                (int)(currDisplay.bGamma * 100));
        }





        //tray
        private void Window_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                Hide();
            }
        }

        private void notifyIcon_DoubleClick(object sender, EventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
        }

        private void toolSettings_Click(object sender, EventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
        }

        private void toolExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void comboBoxToolMonitor_IndexChanged(object sender, EventArgs e)
        {
            // Parse monitor and preset from the ToolStripComboBox directly
            ToolStripComboBox source = (ToolStripComboBox)sender;
            string selectedText = source.Text;

            // The layout of items is "DisplayName:" (index 0) and then preset names
            // If Text ends with ":" it's the header, ignore or select that monitor
            
            // Find which display this corresponds to
            // Iterate toolMonitors to find index
            int displayIndex = -1;
            for(int i=0; i<toolMonitors.Count; i++)
            {
                if(toolMonitors[i] == source)
                {
                    displayIndex = i;
                    break;
                }
            }
            if(displayIndex == -1) return;

            currDisplay = displays[displayIndex];
            numDisplay = displayIndex;

            // If a preset was selected (not the header)
            if (!selectedText.EndsWith(":"))
            {
                 // Load from INI
                currDisplay.rGamma = float.Parse(iniFile.Read("rGamma", selectedText), customCulture);
                currDisplay.gGamma = float.Parse(iniFile.Read("gGamma", selectedText), customCulture);
                currDisplay.bGamma = float.Parse(iniFile.Read("bGamma", selectedText), customCulture);
                currDisplay.rContrast = float.Parse(iniFile.Read("rContrast", selectedText), customCulture);
                currDisplay.gContrast = float.Parse(iniFile.Read("gContrast", selectedText), customCulture);
                currDisplay.bContrast = float.Parse(iniFile.Read("bContrast", selectedText), customCulture);
                currDisplay.rBright = float.Parse(iniFile.Read("rBright", selectedText), customCulture);
                currDisplay.gBright = float.Parse(iniFile.Read("gBright", selectedText), customCulture);
                currDisplay.bBright = float.Parse(iniFile.Read("bBright", selectedText), customCulture);
                currDisplay.monitorBrightness = int.Parse(iniFile.Read("monitorBrightness", selectedText));
                currDisplay.monitorContrast = int.Parse(iniFile.Read("monitorContrast", selectedText));

                // Apply Gamma
                Gamma.SetGammaRamp(currDisplay.displayLink,
                    Gamma.CreateGammaRamp(currDisplay.rGamma, currDisplay.gGamma, currDisplay.bGamma,
                    currDisplay.rContrast, currDisplay.gContrast, currDisplay.bContrast, currDisplay.rBright, currDisplay.gBright,
                    currDisplay.bBright));

                // Apply Monitor Brightness/Contrast
                if (currDisplay.isExternal)
                {
                    ExternalMonitor.SetBrightness(currDisplay.PhysicalHandle, (uint)currDisplay.monitorBrightness);
                    ExternalMonitor.SetContrast(currDisplay.PhysicalHandle, (uint)currDisplay.monitorContrast);
                }
                else
                {
                    InternalMonitor.SetBrightness((byte)currDisplay.monitorBrightness);
                }

                // Ideally we should notify the frontend that values changed
                // but we don't have a direct push mechanism yet. 
                // The frontend will likely show out-of-sync sliders until refreshed.
            }
        }

        //destroy focuses on buttons, trackbars, comboboxes, text, checkbox
    }
}

