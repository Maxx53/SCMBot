using System;
using System.Windows.Forms;

namespace SCMBot
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            var lang = Properties.Settings.Default.Language;

            if (!String.IsNullOrEmpty(lang))
            {
                System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.GetCultureInfo(lang);
                System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo(lang);
            }
            else
            {
                Properties.Settings.Default.Language = System.Globalization.CultureInfo.CurrentUICulture.ToString();
                System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.CurrentUICulture;
                System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.CurrentCulture;           
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Main());
        }
    }
}
