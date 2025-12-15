using System;
using System.Windows.Forms;

namespace Gamma_Manager
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
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
        }
    }
}
