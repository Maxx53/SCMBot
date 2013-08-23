using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Security.Cryptography;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace SCMBot
{

    public partial class SteamSite
    {

        public event eventDelegate delegMessage;

        public const string _mainsite = "http://steamcommunity.com/";
        const string _comlog = "https://steamcommunity.com/login/";
        const string _ref = _comlog + "home/?goto=market%2F";
        const string _getrsa = _comlog + "getrsakey/";
        const string _dologin = _comlog + "dologin/";
        const string _logout = _comlog + "logout/";
        public const string _market = _mainsite + "market/";
        const string _blist = _market + "buylisting/";
        const string _lists = _market + "listings/";
        public const string _search = _market + "search?q=";
        const string _capcha = "https://steamcommunity.com/public/captcha.php?gid=";

        const string loginReq = "password={0}&username={1}&emailauth={2}&loginfriendlyname={3}&captchagid={4}&captcha_text={5}&emailsteamid={6}&rsatimestamp={7}";
        const string loginStr = "steamid={0}&token={1}&remember_login=false&webcookie={2}";

        private List<MutliString> lotList = new List<MutliString>();
        public List<MutliString> searchList = new List<MutliString>();

        public class MutliString
        {
            public MutliString(string sellerId, string price, string feeprice)
            {
                this.SellerId = sellerId;
                this.Price = price;
                this.FeePrice = feeprice;
            }

            public MutliString(string name, string game, string link, string quant, string startprice, string imglink)
            {
                this.Name = name;
                this.Game = game;
                this.Link = link;
                this.Quant = quant;
                this.StartPrice = startprice;
                this.ImgLink = imglink;
            }

            public string Name { set; get; }
            public string Game { set; get; }
            public string Link { set; get; }
            public string ImgLink { set; get; }
            public string Quant { set; get; }
            public string StartPrice { set; get; }
            public string SellerId { set; get; }
            public string Price { set; get; }
            public string FeePrice { set; get; }

        }


        protected void doMessage(flag myflag, int searchId, string message)
        {

            if (delegMessage != null)
            {
                Control target = delegMessage.Target as Control;

                if (target != null && target.InvokeRequired)
                {
                    target.Invoke(delegMessage, new object[] { this, message, searchId, myflag });
                }
                else
                {
                    delegMessage(this, message, searchId, myflag);
                }
            }
        }


        static byte[] HexToByte(string hex)
        {
            if (hex.Length % 2 == 1)
            {
                Main.AddtoLog("HexToByte: The binary key cannot have an odd number of digits");
                return null;
            }

            byte[] arr = new byte[hex.Length >> 1];
            int l = hex.Length;

            for (int i = 0; i < (l >> 1); ++i)
            {
                arr[i] = (byte)((GetHexVal(hex[i << 1]) << 4) + (GetHexVal(hex[(i << 1) + 1])));
            }

            return arr;
        }

        static int GetHexVal(char hex)
        {
            int val = (int)hex;
            return val - (val < 58 ? 48 : 55);
        }


        public static string SendPostRequest(string req, string url, string refer, CookieContainer cookie, bool tolog)
        {
            var requestData = Encoding.UTF8.GetBytes(req);

            try
            {
                var request = (HttpWebRequest)
                    WebRequest.Create(url);

                request.AllowAutoRedirect = false;
                request.KeepAlive = true;
                request.CookieContainer = cookie;
                request.Method = "POST";
                request.Referer = refer;
                request.ContentType = "application/x-www-form-urlencoded";
                request.UserAgent = "Mozilla/5.0 (Windows NT 5.1; rv:22.0) Gecko/20100101 Firefox/22.0";
                request.ContentLength = requestData.Length;

                using (var s = request.GetRequestStream())
                {
                    s.Write(requestData, 0, requestData.Length);
                }

                HttpWebResponse resp = (HttpWebResponse)request.GetResponse();

                string content = string.Empty;

                var stream = new StreamReader(resp.GetResponseStream());
                content = stream.ReadToEnd();

                if (tolog)
                Main.AddtoLog(content);

                cookie = request.CookieContainer;
                resp.Close();

                return content;
            }

            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Main.AddtoLog(e.GetType() + ". " + e.Message);
                return string.Empty;
            }

        }

        public static string EncryptPassword(string steam_rsa, string password, string modval, string expval)
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            RSAParameters rsaParams = new RSAParameters();
            rsaParams.Modulus = HexToByte(modval);
            rsaParams.Exponent = HexToByte(expval);
            rsa.ImportParameters(rsaParams);

            byte[] bytePassword = Encoding.ASCII.GetBytes(password);
            byte[] encodedPassword = rsa.Encrypt(bytePassword, false);
            string encryptedPass = Convert.ToBase64String(encodedPassword);

            return Uri.EscapeDataString(encryptedPass);
        }

        //Спасибо хабру
        static Dictionary<string, string> ParseJson(string res)
        {
            var lines = res.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            var ht = new Dictionary<string, string>(20);
            var st = new Stack<string>(20);

            for (int i = 0; i < lines.Length; ++i)
            {
                var line = lines[i];
                var pair = line.Split(":".ToCharArray(), 2, StringSplitOptions.RemoveEmptyEntries);

                if (pair.Length == 2)
                {
                    var key = ClearString(pair[0]);
                    var val = ClearString(pair[1]);

                    if (val == "{")
                    {
                        st.Push(key);
                    }
                    else
                    {
                        if (st.Count > 0)
                        {
                            key = string.Join("_", st) + "_" + key;
                        }

                        if (ht.ContainsKey(key))
                        {
                            ht[key] += "&" + val;
                        }
                        else
                        {
                            ht.Add(key, val);
                        }
                    }
                }
                else if (line.IndexOf('}') != -1 && st.Count > 0)
                {
                    st.Pop();
                }
            }

            return ht;
        }


        static string ClearString(string str)
        {
            str = str.Trim();

            var ind0 = str.IndexOf("\"");
            var ind1 = str.LastIndexOf("\"");

            if (ind0 != -1 && ind1 != -1)
            {
                str = str.Substring(ind0 + 1, ind1 - ind0 - 1);
            }
            else if (str[str.Length - 1] == ',')
            {
                str = str.Substring(0, str.Length - 1);
            }

            return str;
        }

        //steam utils

        private static string GetSessId(CookieContainer coock)
        {
            string resId = string.Empty;
            var stcook = coock.GetCookies(new Uri(_mainsite));

            for (int i = 0; i < stcook.Count; i++)
            {
                string cookname = stcook[i].Name.ToString();

                if (cookname == "sessionid")
                {
                    resId = stcook[i].Value.ToString();
                    break;
                }
            }
            return resId;
        }

        public static string GetNameBalance(CookieContainer cock)
        {
            Main.AddtoLog("Getting account name and balance...");
            string markpage = SendPostRequest(string.Empty, _market, string.Empty, cock, false);


            string parseName = Regex.Match(markpage, "(?<=steamcommunity.com/id/)(.*)(?<=\"><img)").ToString().Trim();
            if (parseName == "")
            {
                return string.Empty;
            }
            string parseImg = Regex.Match(markpage, "(?<=headerUserAvatarIcon\" src=\")(.*)(?=<div id=\"global_action_menu\">)", RegexOptions.Singleline).ToString();
            parseImg = parseImg.Substring(0, parseImg.Length - 46);

            int nlength = parseName.Length;
            if (nlength != 0)
            {
                parseName = parseName.Substring(0, nlength - 6);
            }

            string parseAmount = Regex.Match(markpage, "(?<=marketWalletBalanceAmount\">)(.*)(?<=</span>)").ToString();

            int blength = parseAmount.Length;
            if (blength != 0)
            {
                parseAmount = parseAmount.Substring(0, parseAmount.IndexOf(" "));
            }
            return string.Format("{0};{1};{2}", parseName, parseAmount, parseImg);
        }


        static string BuyItem(CookieContainer cock, string sessid, string itemId, string link, string total, string subtotal)
        {
            string fee = (Convert.ToInt32(total) - Convert.ToInt32(subtotal)).ToString();

            string data = "sessionid=" + sessid + "&currency=5&subtotal=" + subtotal + "&fee=" + fee + "&total=" + total;

            //buy
            string buyres = SendPostRequest(data, _blist + itemId, _lists + link, cock, true);

            Dictionary<string, string> serialBuy = ParseJson(buyres);

            if (serialBuy["success"] == "1")
            {
                string balance = serialBuy["wallet_balance"];
                balance = balance.Insert(balance.Length - 2, ",");
                return balance;

            }
            else return string.Empty;

        }

        public static void ParseLotList(string content, List<MutliString> lst)
        {
            lst.Clear();

            MatchCollection matches = Regex.Matches(content, "BuyMarketListing(.*?)market_listing_seller\">", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline);

            if (matches.Count != 0)
            {
                //Парсим все лоты (авось пригодится в будущем), но использовать будем пока только первый.
                foreach (Match match in matches)
                {
                    string currmatch = match.Groups[1].Value;

                    //Чистим результат от тегов, символов ascii
                    //Оставляем цифры, пробелы, точки и запятые, разделяющие цены
                    currmatch = Regex.Replace(currmatch, "<[^>]+>", string.Empty).Trim();
                    currmatch = Regex.Replace(currmatch, "&#(.*?);", " ");
                    currmatch = Regex.Replace(currmatch, @"[^\.\,\d\ ]+", string.Empty);

                    //Отделяем номер лота
                    string sellid = currmatch.Substring(2, 19);

                    //Отделяем строку, содержащую цены
                    string amount = currmatch.Substring(43, currmatch.Length - 43);

                    //Чистим цены, оставляем цифры и пробелы
                    amount = Regex.Replace(amount, @"[^\d\ ]+", string.Empty).Trim();

                    //Разделяем, с содержанием цен
                    string[] parts = Regex.Split(amount, " +");

                    //Заполняем список лотов
                    lst.Add(new MutliString(sellid, parts[0], parts[1]));
                }
            }
            else MessageBox.Show("Не удалось загрузить список предметов!", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error); ;

        }



        public static string ParseSearchRes(string content, List<MutliString> lst)
        {
            lst.Clear();
            string totalfind = "0";

            MatchCollection matches = Regex.Matches(content, "(?<=market_listing_row_link\" href)(.*?)(?<=</a>)", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline);
            if (matches.Count != 0)
            {

                foreach (Match match in matches)
                {
                    string currmatch = match.Groups[1].Value;

                    string ItemUrl = Regex.Match(currmatch, "(?<==\")(.*)(?=\">)").ToString();
                
                    string ItemQuan = Regex.Match(currmatch, "(?<=num_listings_qty\">)(.*)(?=</span>)").ToString();

                    
                    string ItemPrice = Regex.Match(currmatch, "(?<=<br/>)(.*)(?=<div class=\"market_listing)", RegexOptions.Singleline).ToString();
                    ItemPrice = Regex.Replace(ItemPrice, "&#(.*?);", string.Empty);
                    ItemPrice = Regex.Replace(ItemPrice, @"[^\d\,]+", string.Empty);

                    string ItemName = Regex.Match(currmatch, "(?<=style=\"color:)(.*)(?=</span>)").ToString();
                    ItemName = ItemName.Remove(0, ItemName.IndexOf(">")+1);

                    string ItemGame = Regex.Match(currmatch, "(?<=game_name\">)(.*)(?=</span>)").ToString();
                    
                    string ItemImg = Regex.Match(currmatch, "(?<=_image\" src=\")(.*)(?=\" alt)", RegexOptions.Singleline).ToString();
                    //<span id="searchResults_total">33</span>

                    //Заполняем список 
                    lst.Add(new MutliString(ItemName, ItemGame, ItemUrl, ItemQuan, ItemPrice, ItemImg));
                }

                totalfind = Regex.Match(content, "(?<=searchResults_total\">)(.*)(?=</span>)").ToString();
            }
            else 
                MessageBox.Show("Не удалось найти!", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error); ;
           
            return totalfind;
        }
      

    }
}
