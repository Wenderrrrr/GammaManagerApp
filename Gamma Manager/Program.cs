using System;
using System.Threading;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Gamma_Manager
{
    internal static class Program
    {
        // For single instance detection
        private static Mutex mutex = null;
        private const string MutexName = "GammaManager_SingleInstance_Mutex";
        
        // For bringing existing window to front
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        
        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        
        private const int SW_RESTORE = 9;
        private const int SW_SHOW = 5;

        [STAThread]
        static void Main()
        {
            // Check for single instance
            bool createdNew;
            mutex = new Mutex(true, MutexName, out createdNew);

            if (!createdNew)
            {
                // Another instance is already running
                // Try to bring existing window to front
                IntPtr existingWindow = FindWindow(null, "Gamma Manager");
                if (existingWindow != IntPtr.Zero)
                {
                    ShowWindow(existingWindow, SW_RESTORE);
                    SetForegroundWindow(existingWindow);
                }
                return;
            }

            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Window());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Crash Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (mutex != null)
                {
                    mutex.ReleaseMutex();
                    mutex.Dispose();
                }
            }
        }
    }
}
