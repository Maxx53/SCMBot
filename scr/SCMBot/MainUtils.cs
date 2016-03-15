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
using System.Threading;
using System.Media;
using System.Runtime.InteropServices;
using System.Security.AccessControl;

namespace SCMBot
{
    public delegate void eventDelegate(object sender, object data, int searchId, flag myflag, bool isMain);
    
    [Flags]
    public enum flag : byte
    {
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
        Sell_progress = 15,
        Error_buy = 16,
        StripImg = 17,
        Send_cancel = 18,
        Error_scan = 19,
        Lang_Changed = 20,
        InvPrice = 21,
        Resold = 22,
        SetHeadName = 23,
        ReLogin = 24,
        ResellErr = 25,
        ActPrice = 26
    }


    [Flags]
    public enum status : byte
    {
        Ready = 0,
        Warning = 1,
        InProcess = 2,
        Wait = 3
    }


    public partial class Main
    {
        const string logPath = "logfile.txt";
        const string hostsPath = "hosts.txt";
        const string historyPath = "history.json";

        const string appName = "SCM Bot alpha";
        const string notifTxt = "{0}\r\n{1} {2}";

        string aboutApp = appName + "\r\n" + Strings.aboutBody;

        const string homePage = "https://github.com/Maxx53/SCMBot";
        const string helpPage = homePage + "/wiki";

        public const string cockPath = "coockies.dat";
        const string steamUA = "Mozilla/5.0 (Windows NT 6.1; rv:38.0) Gecko/20100101 Firefox/38.0";
        
        //Just put here your random values
        private const string initVector = "tu89geji340t89u2";
        private const string passPhrase = "o6806642kbM7c5";
        private const int keysize = 256;
            
        private void setNotifyText(string mess)
        {
            notifyIcon1.Text = string.Format(notifTxt, appName, settingsForm.loginBox.Text, mess);
        }

        private static void SetButton(Button ourButt, string caption, byte type)
        {
            ourButt.Text = caption;

            switch (type)
            {
                case 1:
                    ourButt.Image = (Image)Properties.Resources.login;
                    break;
                case 2:
                    ourButt.Image = (Image)Properties.Resources.logout;
                    break;
                case 3:
                    ourButt.Image = (Image)Properties.Resources.cancel;
                    break;
                case 4:
                    ourButt.Image = (Image)Properties.Resources.host;
                    break;
                default:
                    break;
            }

        }


        public static void StartLoadImgTread(string imgUrl, PictureBox picbox)
        {
            if (imgUrl.Contains("http"))
            {
                ThreadStart threadStart = delegate() { loadImg(imgUrl, picbox, true, false); };
                Thread pTh = new Thread(threadStart);
                pTh.IsBackground = true;
                pTh.Start();
            }
        }

        static public void loadImg(string imgurl, PictureBox picbox, bool drawtext, bool doWhite)
        {
            try
            {

                if (imgurl == string.Empty)
                    return;

                if (drawtext)
                {
                    picbox.Image = Properties.Resources.working;
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
            catch (Exception exc)
            {
                Main.AddtoLog(exc.Message);
            }
        }



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


        //Acync file access
        public static void AddtoLog(string logstr)
        {
            if (!isLog)
                return;

            try
            {
                using (FileStream fs = new FileStream(logPath, FileMode.OpenOrCreate, FileSystemRights.AppendData,
                FileShare.Write, 4096, FileOptions.None))
                {
                    using (StreamWriter writer = new StreamWriter(fs))
                    {
                        writer.AutoFlush = true;
                        writer.WriteLine(DateTime.Now);
                        writer.WriteLine(logstr);
                        writer.WriteLine();
                        writer.Close();
                    }
                    fs.Close();
                }
            }
            catch (Exception)
            {
                //dummy
            }
          
        }

        public static void SaveBinary(string p, object o)
        {
            try
            {
                if (o != null)
                {
                    using (Stream stream = File.Create(p))
                    {
                        BinaryFormatter bin = new BinaryFormatter();
                        bin.Serialize(stream, o);
                    }
                }
            }
            catch (Exception e)
            {
                AddtoLog("Saving Binary Exception: " + e.Message);
            }
        }



        private static object LoadBinary(string p)
        {
            try
            {
                using (Stream stream = File.Open(p, FileMode.Open))
                {
                    BinaryFormatter bin = new BinaryFormatter();
                    var res = bin.Deserialize(stream);
                    return res;
                }
            }
            catch (Exception e)
            {
                AddtoLog("Error Opening " + p + ": " + e.Message);
                return null;
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


        private static void PlaySound(byte num, bool yes)
        {
            if (yes)
            {
                try
                {
                    SoundPlayer sp = new SoundPlayer();
                    switch (num)
                    {
                        case 0:
                            //for byuing
                            sp.Stream = Properties.Resources.ding;
                            break;
                        case 1:
                            // for error buying
                            sp.Stream = Properties.Resources.error;
                            break;
                        case 2:
                            //for resell
                            sp.Stream = Properties.Resources.success;
                            break;
                        case 3:
                            //for resell error
                            sp.Stream = Properties.Resources.error2;
                            break;
                    }

                    sp.Play();
                    sp.Dispose();
                }
                catch (Exception e)
                {
                    AddtoLog(e.Message);
                }
            }
        }


        public void ChangeStatusImage(bool isWork)
        {
            this.Invoke((MethodInvoker)delegate
            {
                if (isWork)
                    toolStripImage.Image = Properties.Resources.working;
                else
                    toolStripImage.Image = Properties.Resources.ready;

            });
        }

        public static string SendPostRequest(string req, string url, string refer, CookieContainer cookie)
        {
           
                var requestData = Encoding.UTF8.GetBytes(req);
                string content = string.Empty;

                try
                {
                    var request = (HttpWebRequest)
                        WebRequest.Create(url);

                    request.CookieContainer = cookie;
                    request.Method = "POST";
                    
                    //New
                    request.Proxy = null;
                    request.Timeout = 30000;

                    request.AutomaticDecompression = DecompressionMethods.GZip;
                    request.Host = Steam.host;
                    request.UserAgent = steamUA;

                    request.Accept = "text/javascript, text/html, application/xml, text/xml, application/json, */*";
                    request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";

                    request.Headers.Add("Accept-Encoding", "gzip, deflate");
                    request.Headers.Add("Accept-Language", "en-US,en;q=0.8,en-US;q=0.5,en;q=0.3");
                    request.Headers.Add("Cache-Control", "no-cache");

                    request.Referer = refer;

                    request.ContentLength = requestData.Length;

                    using (var s = request.GetRequestStream())
                    {
                        s.Write(requestData, 0, requestData.Length);
                    }

                    HttpWebResponse resp = (HttpWebResponse)request.GetResponse();

                    var stream = new StreamReader(resp.GetResponseStream());
                    content = stream.ReadToEnd();

                    //if (tolog)
                    //    AddtoLog(content);

                    cookie = request.CookieContainer;
                    resp.Close();
                    stream.Close();
                }
                catch (WebException e)
                {
                    if (e.Status == WebExceptionStatus.ProtocolError)
                    {
                        WebResponse resp = e.Response;
                        using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
                        {
                            content = sr.ReadToEnd();
                        }
                    }

                }

                return content;
        }


        public static string GetRequest(string url, CookieContainer cookie)
        {
            if (banTimer.Enabled)
                return "B";

            string content = string.Empty;

           // int hostNum = 0;
           // bool hostUsed = false;

            try
            {
                //if (UseHost && (hostList.Count != 0))
                //{
                //    hostNum = GetFreeIndex();
                //    if (hostNum != -1)
                //    {
                //        hostList[hostNum].InUsing = true;
                //        hostList[hostNum].WorkLoad++;
                //        url = url.Replace(SteamSite._host, hostList[hostNum].Host);
                //        hostUsed = true;
                //    }
                //}

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";

                //New
                request.Proxy = null;
                request.Timeout = 10000;
                request.ReadWriteTimeout = 10000;
                request.KeepAlive = true;
                request.AutomaticDecompression = DecompressionMethods.GZip;

                request.Host = Steam.host;
                request.Referer = Steam.market;
                request.UserAgent = steamUA;

                request.Accept = "text/javascript, text/html, application/xml, text/xml, application/json, */*";
                request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";

                request.Headers.Add("Accept-Encoding", "gzip, deflate");
                request.Headers.Add("Accept-Language", "en-US,en;q=0.8,en-US;q=0.5,en;q=0.3");
                request.Headers.Add("Cache-Control", "no-cache");

                request.CookieContainer = cookie;

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var stream = new StreamReader(response.GetResponseStream());
                    content = stream.ReadToEnd();
                    stream.Close();
                }
                else content = string.Empty;

                response.Close();

            }

            catch (WebException e)
            {
                if (e.Status == WebExceptionStatus.ProtocolError)
                {

                    HttpWebResponse resp = (HttpWebResponse)e.Response;
                    int statCode = (int)resp.StatusCode;

                    if (statCode == 403)
                    {
                        content = "403";
                    }
                    else
                    {
                        using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
                        {
                            content = sr.ReadToEnd();
                        }
                    }
                }

            }

            ////Free host
            //if (UseHost && (hostList.Count != 0) && hostUsed)
            //{
            //    hostList[hostNum].InUsing = false;
            //}


            if (content.Contains("error_ctn"))
            {
                banTimer.Enabled = true;
                return "B";
            }

            return content;

        }


        public static int GetFreeIndex()
        {
            int min = hostList[0].WorkLoad;
            int minIndex = 0;
            bool used = false;

            for (int i = 0; i < hostList.Count; i++)
            {
                if (hostList[i].WorkLoad < min)
                {
                    if (hostList[i].InUsing == false)
                    {
                        min = hostList[i].WorkLoad;
                        minIndex = i;
                        used = false;
                    }
                    else used = true;
                }
            }

            if (!used)
                return minIndex;
            else return -1;
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
            try
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
            catch (Exception)
            {
                return "Password";
            }
        }



        public static byte[] GetHash(string inputString)
        {
            HashAlgorithm algorithm = MD5.Create();  //or use SHA1.Create();
            return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
        }

        public static string GetHashString(string inputString)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in GetHash(inputString))
                sb.Append(b.ToString("X2"));

            return sb.ToString();
        }

        public static void StartCmdLine(string process, string param, bool wait)
        {
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo = new System.Diagnostics.ProcessStartInfo(process, param);
            p.Start();
            if (wait)
                p.WaitForExit();
        }

    }



    public class HostItem
    {
        public HostItem(string host, string pingas, bool inUsing, int workLoad)
        {
            this.Host = host;
            this.Pingas = pingas;
            this.InUsing = inUsing;
            this.WorkLoad = workLoad;
        }

        public string Host { set; get; }
        public string Pingas { set; get; }
        public bool InUsing { set; get; }
        public int WorkLoad { set; get; }
    }


    public class HostList : List<HostItem>
    {
        public void Add(string HostStr, string pingas)
        {
            try
            {
                this.Add(new HostItem(HostStr, pingas, false, 0));
            }
            catch (Exception)
            {
                // dummy
            }
           
        }
    }


    public class StrParam
    {
        public StrParam(string p1, string p2)
        {
            this.P1 = p1;
            this.P2 = p2;
        }

        public StrParam(string p1, string p2, string p3, string p4)
        {
            this.P1 = p1;
            this.P2 = p2;
            this.P3 = p3;
            this.P4 = p4;
        }

        public string P1 { set; get; }
        public string P2 { set; get; }
        public string P3 { set; get; }
        public string P4 { set; get; }
    }

    [Serializable]
    public class MainFormParams
    {
        public MainFormParams(Size size, Point location, FormWindowState state, int split1, int split2, int split3)
        {
            this.FrmSize = size;
            this.Location = location;
            this.FrmState = state;

            //Сплит - хуесос. Hello, Joyreactor!
            this.Split1 = split1;
            this.Split2 = split2;
            this.Split3 = split3;
        }

        public Size FrmSize { set; get; }
        public Point Location { set; get; }
        public FormWindowState FrmState { set; get; }
        public int Split1 { set; get; }
        public int Split2 { set; get; }
        public int Split3 { set; get; }
    }

    

    public static class FlashWindow
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FlashWindowEx(ref FLASHWINFO pwfi);

        [StructLayout(LayoutKind.Sequential)]
        private struct FLASHWINFO
        {
            public uint cbSize;
            public IntPtr hwnd;
            public uint dwFlags;
            public uint uCount;
            public uint dwTimeout;
        }

        public const uint FLASHW_ALL = 3;
        public const uint FLASHW_TIMERNOFG = 12;
  
        public static bool Flash(System.Windows.Forms.Form form)
        {
            if (Win2000OrLater)
            {
                FLASHWINFO fi = Create_FLASHWINFO(form.Handle, FLASHW_ALL | FLASHW_TIMERNOFG, uint.MaxValue, 0);
                return FlashWindowEx(ref fi);
            }
            return false;
        }

        private static FLASHWINFO Create_FLASHWINFO(IntPtr handle, uint flags, uint count, uint timeout)
        {
            FLASHWINFO fi = new FLASHWINFO();
            fi.cbSize = Convert.ToUInt32(Marshal.SizeOf(fi));
            fi.hwnd = handle;
            fi.dwFlags = flags;
            fi.uCount = count;
            fi.dwTimeout = timeout;
            return fi;
        }

        private static bool Win2000OrLater
        {
            get { return System.Environment.OSVersion.Version.Major >= 5; }
        }
    }
}
