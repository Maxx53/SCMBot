using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Net;
using System.Windows.Forms;
using System.Drawing;
using System.Threading;

namespace SCMBot
{
    public delegate void eventDelegate(object sender, string message, flag myflag);

    [Flags]
    public enum flag : byte
    {
        Already_logged = 0,
        Login_success = 1,
        Login_cancel = 2,
        Login_error = 3,
        Price_text = 5,
        Price_htext = 6,
        Rep_progress = 7,
        Success_buy = 8,
        Scan_cancel = 9,
        Scan_progress = 10

    }


    public partial class Main
    {
        const string logPath = "logfile.txt";
        const string appName = "SCM Bot alpha";
        

        public static void AddtoLog(string logstr)
        {
            StreamWriter log;

            if (!File.Exists(logPath))
            {
                log = new StreamWriter(logPath);
            }
            else
            {
                log = File.AppendText(logPath);
            }

            log.WriteLine(DateTime.Now);
            log.WriteLine(logstr);
            log.WriteLine();
            log.Close();
        }

        static void WriteCookiesToDisk(string file, CookieContainer cookieJar)
        {
            using (Stream stream = File.Create(file))
            {
                try
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(stream, cookieJar);
                }
                catch (Exception e)
                {
                    AddtoLog("Problem writing cookies to disk: " + e.GetType());
                }
            }
        }

        static CookieContainer ReadCookiesFromDisk(string file)
        {

            try
            {
                using (Stream stream = File.Open(file, FileMode.Open))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    return (CookieContainer)formatter.Deserialize(stream);
                }
            }
            catch (Exception e)
            {
                AddtoLog("Problem reading cookies from disk: " + e.GetType());
                return new CookieContainer();
            }
        }

        public static void AppendText(RichTextBox box, string text, bool highlight)
        {
            box.SelectionStart = box.TextLength;
            box.SelectionLength = 0;

            if (highlight)
                box.SelectionColor = Color.Red;
            else
                box.SelectionColor = Color.Black;

            box.AppendText(string.Format("{0} {1} {2}", DateTime.Now.ToString("HH:mm:ss"), text, "ед.\r\n"));
            box.SelectionColor = box.ForeColor;

            box.SelectionStart = box.Text.Length + 1;
        }
    }
}
