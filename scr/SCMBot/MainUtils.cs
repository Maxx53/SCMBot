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
using System.ComponentModel;
using System.Media;
using System.Runtime.InteropServices;
using System.Security.AccessControl;

namespace SCMBot
{
    public delegate void eventDelegate(object sender, object data, int searchId, flag myflag, bool isMain);
    
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
    }


    public partial class Main
    {
        const string logPath = "logfile.txt";
        const string proxyPath = "proxy.txt";

        const string appName = "SCM Bot alpha";
        const string notifTxt = "{0}\r\n{1} {2}";

        string aboutApp = appName + "\r\n" + Strings.aboutBody;

        const string homePage = "https://github.com/Maxx53/SCMBot";
        const string helpPage = homePage + "/wiki";

        public const string cockPath = "coockies.dat";
        const string steamUA = "Mozilla/5.0 (Windows NT 5.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/35.0.1916.153 Safari/537.36 OPR/22.0.1471.70";
        
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
                default:
                    break;
            }

        }


        public static void StartLoadImgTread(string imgUrl, PictureBox picbox)
        {
            if (imgUrl.Contains("http://"))
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


        public static string SendPostRequest(string req, string url, string refer, CookieContainer cookie, bool tolog)
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
                    //KeepAlive is True by default
                    //request.KeepAlive = true;

                    //LOL, really?
                    request.UserAgent = steamUA;

                    request.Referer = refer;
                    request.ContentType = "application/x-www-form-urlencoded";
                    request.ContentLength = requestData.Length;

                    using (var s = request.GetRequestStream())
                    {
                        s.Write(requestData, 0, requestData.Length);
                    }

                    HttpWebResponse resp = (HttpWebResponse)request.GetResponse();

                    var stream = new StreamReader(resp.GetResponseStream());
                    content = stream.ReadToEnd();

                    if (tolog)
                        AddtoLog(content);

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




        public static int GetFreeIndex()
        {
            int min = proxyList[0].WorkLoad;
            int minIndex = 0;
            bool used = false;

            for (int i = 0; i < proxyList.Count; i++)
            {
                if (proxyList[i].WorkLoad < min)
                {
                    if (proxyList[i].InUsing == false)
                    {
                        min = proxyList[i].WorkLoad;
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


        public static string GetRequest(string url, CookieContainer cookie, bool UseProxy, bool keepAlive)
        {
                string content = string.Empty;
                int proxyNum = 0;

                bool proxyUsed = false;

                try
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                    request.Method = "GET";

                    //New
                    request.Proxy = null;
                    request.Timeout = 30000;
                
                    //KeepAlive is True by default
                    //request.KeepAlive = keepAlive;

                    //LOL, really?
                    request.UserAgent = steamUA;

                    request.Accept = "application/json";
                    request.CookieContainer = cookie;


                    if (UseProxy && (proxyList.Count != 0))
                    {
                        proxyNum = GetFreeIndex();
                        if (proxyNum != -1)
                        {
                            proxyList[proxyNum].InUsing = true;
                            proxyList[proxyNum].WorkLoad++;
                            request.Proxy = proxyList[proxyNum].Proxy;
                            proxyUsed = true;
                        }
                    }


                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    var stream = new StreamReader(response.GetResponseStream());
                    content = stream.ReadToEnd();

                    response.Close();
                    stream.Close();

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

                //Free proxy
                if (UseProxy && (proxyList.Count != 0) && proxyUsed)
                {
                    proxyList[proxyNum].InUsing = false;
                }

                return content;

        }


        //That's all folks. Not precisely.
        public static string CalcWithFee(string strInput)
        {

            int intres = 0;
            int input = Convert.ToInt32(strInput);

            //Magic
            double temp = input / 1.15;

            if (input > 10)
                intres = Convert.ToInt32(Math.Ceiling(temp));
            else
                if (input < 4)
                {
                    if (input == 3)
                        intres = 1;
                    else
                        intres = 0;
                }
                else
                    intres = Convert.ToInt32(temp) - 1;

            return intres.ToString();
        }



        public static string AddFee(string strInput)
        {

            int intres = 0;
            int input = Convert.ToInt32(strInput);
            double temp = 0;

            if (input < 20)
                temp = 2;
            else
            {
                temp = input * 0.15;
            }

            intres = input + Convert.ToInt32(temp);

            return intres.ToString();
        }

        private string GetScanErrMess(string message)
        {
            string mess = string.Empty;

            switch (message)
            {
                case "0": mess = "Content empty";
                    break;
                case "1": mess = "Json without data";
                    break;
                case "2": mess = "Json is not valid";
                    break;
                case "3": mess = "Parsing fail";
                    break;
                case "4": mess = "Wait to Relogin";
                    break;
                case "5": mess = "Too much requests per second";
                    break;
                case "6": mess = "Item is not supported in html source!";
                    break;
                default: mess = "Unknown error";
                    break;
            }
            return mess;
        }

        static bool isScanValid(saveTab item, bool isMain)
        {
            if (isMain)
                return (item.Price != string.Empty && item.Link.Contains(SteamSite._market) && item.Name != string.Empty);
            else
                return (item.Price != string.Empty && item.Name != string.Empty);
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



    public class ProxyItem
    {
        public ProxyItem(WebProxy proxy, bool inUsing, int workLoad)
        {
            this.Proxy = proxy;
            this.InUsing = inUsing;
            this.WorkLoad = workLoad;
        }

        public WebProxy Proxy { set; get; }
        public bool InUsing { set; get; }
        public int WorkLoad { set; get; }
    }


    public class ProxyList : List<ProxyItem>
    {
        public void Add(string ProxyStr)
        {
            try
            {
                this.Add(new ProxyItem(new WebProxy(ProxyStr, false), false, 0));
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

    [Serializable]
    public class saveTabLst : List<saveTab>
    {
        public int Position { set; get; }
    }

    [Serializable]
    public class saveTab
    {
        public saveTab(string name, string link, string imglink, string price, int delay, int buyQnt, bool toBuy, int resellType, string resellPrice, status statId)
        {
            this.Name = name;
            this.Price = price;
            this.Link = link;
            this.ImgLink = imglink;
            this.Delay = delay;
            this.BuyQnt = buyQnt;
            this.ToBuy = toBuy;
            this.ResellType = resellType;
            this.ResellPrice = resellPrice;
            this.StatId = statId;
        }

        public saveTab(string link)
        {
            this.Link = link;
        }

        //copy constructor
        public saveTab(saveTab tocopy)
        {
            this.Name = tocopy.Name;
            this.Price = tocopy.Price;
            this.Link = tocopy.Link;
            this.ImgLink = tocopy.ImgLink;
            this.Delay = tocopy.Delay;
            this.BuyQnt = tocopy.BuyQnt;
            this.ToBuy = tocopy.ToBuy;
            this.ResellType = tocopy.ResellType;
            this.ResellPrice = tocopy.ResellPrice;
            this.StatId = tocopy.StatId;
        }

        public string Name { set; get; }
        public string ImgLink { set; get; }
        public string Link { set; get; }
        public string Price { set; get; }
        public int BuyQnt { set; get; }
        public int Delay { set; get; }
        public bool ToBuy { set; get; }
        public int ResellType { get; set; }
        public string ResellPrice { get; set; }
        public bool HeadSet { get; set; }
        public status StatId { get; set; }
    }


    public class ScanItemList : List<MainScanItem>
    {
        public void UpdateIds()
        {
            for (int i = 0; i < this.Count; i++)
            {
                this[i].Steam.scanID = i;
            }

        }
    }


    public class MainScanItem
    {
        public SteamSite Steam = new SteamSite();
        public BindingList<LogItem> LogCont = new BindingList<LogItem>();

        public class LogItem
        {
            public LogItem(int id, string rawPrice, DateTime time, bool addcur, string curr)
            {
                this.Id = id;

                this.RawPrice = rawPrice;
                this.Time = time;
                this.AddCurr = addcur;

                if (addcur)
                    this.Text = string.Format("{0} {1} {2}", time.ToString("HH:mm:ss"), DoFracture(rawPrice), curr);
                else
                    this.Text = string.Format("{0} {1}", time.ToString("HH:mm:ss"), rawPrice); 
            }

            public override string ToString()
            {
                return this.Text;
            }


            public static string DoFracture(string input)
            {
                string prtoTxt = "0,";

                switch (input.Length)
                {
                    case 0:
                        prtoTxt = "0";
                        break;
                    case 1:
                        prtoTxt += "0" + input;
                        break;
                    case 2:
                        prtoTxt += input;
                        break;
                    default:
                        prtoTxt = input.Insert(input.Length - 2, ",");
                        break;
                }
                return prtoTxt;
            }


            public int Id { get; set; }
            public string Text { get; set; }

            public string RawPrice { get; set; }
            public DateTime Time { get; set; }
            public bool AddCurr { get; set; }
        }

        public MainScanItem(saveTab scanParams, CookieContainer cookie, eventDelegate deleg, int currency, bool ignoreWarn, int resDel)
        {
            Steam.scanInput = scanParams;
            Steam.NotSetHead = (scanParams.Name == string.Empty);
            Steam.delegMessage += deleg;
            Steam.cookieCont = cookie;
            Steam.currencies.Current = currency;
            Steam.IgnoreWarn = ignoreWarn;
            Steam.resellDelay = resDel;
        }

       // public byte StatId { get; set; }
  
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
