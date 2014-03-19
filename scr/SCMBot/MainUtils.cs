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
using System.Collections.Specialized;

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
        Sell_progress = 15,
        Error_buy = 16,
        StripImg = 17,
        Send_cancel = 18,
        Error_scan = 19,
        Lang_Changed = 20,
        InvPrice = 21,
        Resold = 22,
        SetHeadName = 23
    }


    public partial class Main
    {
        const string logPath = "logfile.txt";
        const string appName = "SCM Bot alpha";
        const string notifTxt = "{0}\r\n{1} {2}";

        string aboutApp = appName + "\r\n" + Strings.aboutBody;

        const string homePage = "https://github.com/Maxx53/SteamCMBot";
        const string helpPage = homePage + "/wiki";
        const string donateLink = "https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business=demmaxx@gmail.com&lc=RU&item_name=SteamCMBot%20Donate&currency_code=RUB&bn=PP-DonationsBF";

        const string cockPath = "coockies.dat";
        
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
            if (imgurl == string.Empty)
                return;

            if (drawtext)
            {
                picbox.Image = Properties.Resources.working;
            }

            try
            {
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
            catch (Exception)
            {

               // throw;
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
                    request.Timeout = 10000;
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
       
            //catch (Exception e)
            //{
            //     MessageBox.Show(e.Message, "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //      Main.AddtoLog(e.GetType() + ". " + e.Message);
            //     return content;
            //  }

        }



        public static string GetRequest(string url, CookieContainer cookie)
        {
                string content = string.Empty;

                try
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                    request.Method = "GET";
                    request.Timeout = 10000;
                    request.Accept = "application/json";
                    request.CookieContainer = cookie;

                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    var stream = new StreamReader(response.GetResponseStream());
                    content = stream.ReadToEnd();

                    response.Close();
                    stream.Close();

                }

                catch (Exception e)
                {
                    content = e.Message;
                    AddtoLog(e.Message);
                }
                return content;

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
                default: mess = "Unknown error";
                    break;
            }
            return mess;
        }

        static bool isScanValid(saveTab item)
        {
            return (item.Price != string.Empty && item.Link.Contains(SteamSite._market) && (item.ScanPage | item.ScanRecent));
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


    [Serializable]
    public class saveTabLst : List<saveTab>
    {
        public int Position { set; get; }
    }

    [Serializable]
    public class saveTab
    {
        public saveTab(string name, string link, string imglink, string price, int delay, int buyQnt, bool toBuy, int resellType, string resellPrice, bool scanPage, bool scanRecent)
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
            this.ScanPage = scanPage;
            this.ScanRecent = scanRecent;
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
        public bool ScanPage { get; set; }
        public bool ScanRecent { get; set; }
        public bool HeadSet { get; set; }

    }


    public class ScanItemList : List<ScanItem>
    {
        public void UpdateIds()
        {
            for (int i = 0; i < this.Count; i++)
            {
                this[i].Steam.scanID = i;
            }

        }
    }


    public class ScanItem
    {
        public SteamSite Steam = new SteamSite();
        public BindingList<LogItem> LogCont = new BindingList<LogItem>();

        public class LogItem
        {
            public LogItem(int id, string text)
            {
                this.Id = id;
                this.Text = text;
            }

            public override string ToString()
            {
                return this.Text;
            }

            public int Id { get; set; }
            public string Text { get; set; }
        }

        public ScanItem(saveTab scanParams, CookieContainer cookie, eventDelegate deleg, int currency, bool ignoreWarn)
        {
            this.ScanParams = scanParams;
            Steam.delegMessage += deleg;
            Steam.cookieCont = cookie;
            Steam.currencies.Current = currency;
            Steam.IgnoreWarn = ignoreWarn;
        }

        public void ReadParams()
        {

            Steam.ResellType = ScanParams.ResellType;
            Steam.ResellPrice = ScanParams.ResellPrice;
            Steam.scanDelay = ScanParams.Delay.ToString();
            Steam.wishedPrice = ScanParams.Price;
            Steam.pageLink = ScanParams.Link;
            Steam.toBuy = ScanParams.ToBuy;
            Steam.BuyQuant = ScanParams.BuyQnt;

            Steam.NotSetHead = (ScanParams.Name == string.Empty);
            Steam.scanName = ScanParams.Name;
            Steam.scanPage = ScanParams.ScanPage;
            Steam.scanRecent = ScanParams.ScanRecent;
        }

        public saveTab ScanParams { set; get; }
        public byte StatId { get; set; }
  
    }

}
