using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Net;
using System.Windows.Forms;
using System.Drawing;
using System.Security.Cryptography;
using System.Collections;

namespace SCMBot
{
    public delegate void eventDelegate(object sender, string message, int searchId, flag myflag);


    [Flags]
    public enum flag : byte
    {
        Already_logged = 0,
        Login_success = 1,
        Login_cancel = 2,
        Login_error = 3,
        Logout_ = 4,
        Price_text = 5,
        Price_htext = 6,
        Rep_progress = 7,
        Success_buy = 8,
        Scan_cancel = 9,
        Scan_progress = 10,
        Search_success = 11,
        Price_btext = 12,
        Inventory_Loaded = 13,
        Items_Sold = 14,
        Sell_progress = 15
    }


    public partial class Main
    {
        const string logPath = "logfile.txt";
        const string appName = "SCM Bot alpha";
        //Just put here your random values
        private const string initVector = "tu89geji340t89u2";
        private const string passPhrase = "o6806642kbM7c5";
        private const int keysize = 256;


        public class SearchPagePos
        {
            public SearchPagePos(int pageCount, int currentPos)
            {
                this.PageCount = pageCount;
                this.CurrentPos = currentPos;
            }

            public int PageCount { set; get; }
            public int CurrentPos { set; get; }

        }


        class ItemComparer : IComparer
        {
            int columnIndex = 0;
            bool sortAscending = true;
            public int ColumnIndex
            {
                set
                {
                    if (columnIndex == value)
                        sortAscending = !sortAscending;
                    else
                    {
                        columnIndex = value;
                        sortAscending = true;
                    }
                }
            }

            public int Compare(object x, object y)
            {
                string value1 = ((ListViewItem)x).SubItems[columnIndex].Text;
                string value2 = ((ListViewItem)y).SubItems[columnIndex].Text;
                return String.Compare(value1, value2) * (sortAscending ? 1 : -1);
            }
        }

        private static void SetColumnWidths(ListView list, bool useUpdate)
        {
            if (useUpdate)
                list.BeginUpdate();

            int width;
            int totalWidth = 0;

            foreach (ColumnHeader col in list.Columns)
            {
                if (list.Columns.Count != col.DisplayIndex)
                {
                    col.AutoResize(ColumnHeaderAutoResizeStyle.HeaderSize);
                    width = col.Width;

                    col.AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
                    if (width > col.Width)
                        col.Width = width;

                    totalWidth += col.Width;
                }
                else
                {
                    col.Width = (list.ClientSize.Width - totalWidth);
                }

            }

            if (useUpdate)
                list.EndUpdate();

        }


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


        static private Color backColor(Image img, bool doWhite)
        {
            Color back = SystemColors.Control;
            if (doWhite)
                back = Color.White;
            Bitmap bitmap =  new Bitmap(img);
            var colors = new List<Color>();
            for (int y = 0; y < bitmap.Size.Height; y++)
            {
                //Let's grab bunch of pixels from center of image
                colors.Add(bitmap.GetPixel(bitmap.Size.Width / 2, y));
            }

            float imageBrightness = colors.Average(color => color.GetBrightness());
            if (imageBrightness > 0.6)
                back = Color.Black;
            return back;
        }


        static public void loadImg(string imgurl, PictureBox picbox, bool drawtext, bool doWhite)
        {
            if (imgurl == string.Empty)
                return;

            if (drawtext)
            {
                StringFormat sf = new StringFormat();
                sf.LineAlignment = StringAlignment.Center;
                sf.Alignment = StringAlignment.Center;

                Graphics G = picbox.CreateGraphics();
                G.Clear(System.Drawing.SystemColors.Control);
                G.DrawString("Load...", Main.DefaultFont, Brushes.Black, picbox.ClientRectangle, sf);
            }

            WebClient wClient = new WebClient();
            byte[] imageByte = wClient.DownloadData(imgurl);
            using (MemoryStream ms = new MemoryStream(imageByte, 0, imageByte.Length))
            {
                ms.Write(imageByte, 0, imageByte.Length);
                var resimg = Image.FromStream(ms, true);
                picbox.BackColor = backColor(resimg, doWhite);
                picbox.Image = resimg;
            }
        }



        public static string Encrypt(string plainText)
        {
            byte[] initVectorBytes = Encoding.UTF8.GetBytes(initVector);
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            PasswordDeriveBytes password = new PasswordDeriveBytes(passPhrase, null);
            byte[] keyBytes = password.GetBytes(keysize / 8);
            RijndaelManaged symmetricKey = new RijndaelManaged();
            symmetricKey.Mode = CipherMode.CBC;
            ICryptoTransform encryptor = symmetricKey.CreateEncryptor(keyBytes, initVectorBytes);
            MemoryStream memoryStream = new MemoryStream();
            CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
            cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
            cryptoStream.FlushFinalBlock();
            byte[] cipherTextBytes = memoryStream.ToArray();
            memoryStream.Close();
            cryptoStream.Close();
            return Convert.ToBase64String(cipherTextBytes);
        }

        public static string Decrypt(string cipherText)
        {
            byte[] initVectorBytes = Encoding.ASCII.GetBytes(initVector);
            byte[] cipherTextBytes = Convert.FromBase64String(cipherText);
            PasswordDeriveBytes password = new PasswordDeriveBytes(passPhrase, null);
            byte[] keyBytes = password.GetBytes(keysize / 8);
            RijndaelManaged symmetricKey = new RijndaelManaged();
            symmetricKey.Mode = CipherMode.CBC;
            ICryptoTransform decryptor = symmetricKey.CreateDecryptor(keyBytes, initVectorBytes);
            MemoryStream memoryStream = new MemoryStream(cipherTextBytes);
            CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
            byte[] plainTextBytes = new byte[cipherTextBytes.Length];
            int decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
            memoryStream.Close();
            cryptoStream.Close();
            return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
        }

        public void StartCmdLine(string process, string param, bool wait)
        {
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo = new System.Diagnostics.ProcessStartInfo(process, param);
            p.Start();
            if (wait)
                p.WaitForExit();
        }
    }
}
