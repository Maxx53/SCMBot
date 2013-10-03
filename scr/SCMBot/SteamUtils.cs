using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Security.Cryptography;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using Newtonsoft.Json;


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
        //const string _blist = _market + "buylisting/";
        //FIX
        public const string _mainsiteS = "https://steamcommunity.com/";
        const string _blist = _mainsiteS + "market/buylisting/";

        const string _lists = _market + "listings/";
        public const string _search = _market + "search?q=";
        const string _capcha = "https://steamcommunity.com/public/captcha.php?gid=";

        const string loginReq = "password={0}&username={1}&emailauth={2}&loginfriendlyname={3}&captchagid={4}&captcha_text={5}&emailsteamid={6}&rsatimestamp={7}";
        const string loginStr = "steamid={0}&token={1}&remember_login=false&webcookie={2}";
        const string buyReq = "sessionid={0}&currency=5&subtotal={1}&fee={2}&total={3}";

        const string _jsonInv = _mainsite + "id/{0}/inventory/json/{1}";
        public const string invImgUrl = "http://cdn.steamcommunity.com/economy/image/{0}/96fx96f";
        public const string _sellitem = _mainsiteS + "market/sellitem/";
        public const string sellReq = "sessionid={0}&appid={1}&contextid={2}&assetid={3}&amount=1&price={4}";

        private List<ScanItem> lotList = new List<ScanItem>();
        public List<SearchItem> searchList = new List<SearchItem>();
        public List<InventItem> inventList = new List<InventItem>();

        public class ScanItem
        {
            public ScanItem(string sellerId, string price, string subtotal)
            {
                this.SellerId = sellerId;
                this.Price = price;
                this.SubTotal = subtotal;
            }

            public string SellerId { set; get; }
            public string Price { set; get; }
            public string SubTotal { set; get; }
        }

        public class InventItem
        {
            public InventItem(string assetid, string name, string type, string price, string imglink)
            {
                this.Name = name;
                this.AssetId = assetid;
                this.Type = type;
                this.Price = price;
                this.ImgLink = imglink;
            }

            public string Name { set; get; }
            public string ImgLink { set; get; }
            public string Price { set; get; }
            public string Type { set; get; }
            public string AssetId { set; get; }

        }

        public class SearchItem
        {

            public SearchItem(string name, string game, string link, string quant, string startprice, string imglink)
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

        }


        //JSON Stuff...

        public class RespRSA
        {

            [JsonProperty("success")]
            public bool Success { get; set; }

            [JsonProperty("publickey_mod")]
            public string Module { get; set; }

            [JsonProperty("publickey_exp")]
            public string Exponent { get; set; }

            [JsonProperty("timestamp")]
            public string TimeStamp { get; set; }
        }

        public class RespProcess
        {
            [JsonProperty("success")]
            public bool Success { get; set; }

            [JsonProperty("emailauth_needed")]
            public bool isEmail { get; set; }

            [JsonProperty("captcha_needed")]
            public bool isCaptcha { get; set; }

            [JsonProperty("message")]
            public string Message { get; set; }

            [JsonProperty("captcha_gid")]
            public string Captcha_Id { get; set; }

            [JsonProperty("emailsteamid")]
            public string Email_Id { get; set; }
        }

        public class RespFinal
        {
            [JsonProperty("success")]
            public bool Success { get; set; }

            [JsonProperty("login_complete")]
            public bool isComplete { get; set; }
        }


        public class InventoryData
        {
            [JsonProperty("rgInventory")]
            public IDictionary<string, InvItem> myInvent { get; set; }

            [JsonProperty("rgDescriptions")]
            public IDictionary<string, ItemDescr> invDescr { get; set; }
        }


        public class InvItem
        {
            [JsonProperty("id")]
            public string assetid { get; set; }

            [JsonProperty("classid")]
            public string classid { get; set; }
        }


        public class ItemDescr
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("icon_url")]
            public string IconUrl { get; set; }

            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("market_hash_name")]
            public string MarketName { get; set; }
        }


        public class WalletInfo
        {
            [JsonProperty("wallet_info")]
            public Wallet WalletRes { get; set; }
        }

        public class Wallet
        {
            [JsonProperty("wallet_balance")]
            public string Balance { get; set; }
            [JsonProperty("success")]
            public int Success { get; set; }
        }


        //End JSON

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
            string content = string.Empty;

            try
            {
                var request = (HttpWebRequest)
                    WebRequest.Create(url);

                request.CookieContainer = cookie;
                request.Method = "POST";
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
                Main.AddtoLog(content);

                cookie = request.CookieContainer;
                resp.Close();
                stream.Close();
                return content;
            }

            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Main.AddtoLog(e.GetType() + ". " + e.Message);
                return content;
            }

        }



        public static string GetRequest(string url, CookieContainer cookie)
        {
             string content = string.Empty;

             try
             {
                 HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

                 request.Method = "GET";
                 request.Accept = "application/json";
                 request.CookieContainer = cookie;

                 HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                 var stream = new StreamReader(response.GetResponseStream());
                 content = stream.ReadToEnd();

                 response.Close();
                 stream.Close();

                 return content;
             }
             catch (Exception e)
             {
                 MessageBox.Show(e.Message, "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                 Main.AddtoLog(e.GetType() + ". " + e.Message);
                 return content;
             }
        }



        public static string EncryptPassword(string password, string modval, string expval)
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


        //steam utils

        public static string GetNameBalance(CookieContainer cock)
        {
            Main.AddtoLog("Getting account name and balance...");
            string markpage = GetRequest(_market, cock);
       
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
                accName = parseName.Substring(0, nlength - 6);
            }

            string parseAmount = Regex.Match(markpage, "(?<=marketWalletBalanceAmount\">)(.*)(?<=</span>)").ToString();

            int blength = parseAmount.Length;
            if (blength != 0)
            {
                parseAmount = parseAmount.Substring(0, parseAmount.IndexOf(" "));
            }
            return string.Format("{0};{1};{2}", accName, parseAmount, parseImg);
        }


        private static string prFormat(string input)
        {
            string result = input;

            if (input.Length < 3)
            {
                result = input + "00";
            }

            return result;
        }


        public static string GetSessId(CookieContainer coock)
        {
            //sessid sample MTMyMTg5MTk5Mw%3D%3D
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


        static string BuyItem(CookieContainer cock, string sessid, string itemId, string link, string total, string subtotal)
        {
            int int_total = Convert.ToInt32(prFormat(total));
            int int_sub = Convert.ToInt32(prFormat(subtotal));
            string fee = (int_total - int_sub).ToString();

            string data = string.Format(buyReq, sessid, int_sub, fee, int_total);

            //buy
            //29.08.2013 Steam Update Issue!
            //FIX: using SSL - https:// in url
            string buyres = SendPostRequest(data, _blist + itemId, link, cock, true);
            if (buyres != string.Empty)
            {
                var AfterBuy = JsonConvert.DeserializeObject<WalletInfo>(buyres);

                if (AfterBuy.WalletRes.Success == 1)
                {
                    string balance = AfterBuy.WalletRes.Balance;
                    balance = balance.Insert(balance.Length - 2, ",");
                    return balance;

                }
                else return string.Empty;
            }
            else return string.Empty;

        }

       public static void ParseLotList(string content, List<ScanItem> lst)
        {
            lst.Clear();

            if (content == string.Empty)
                return;

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
                    lst.Add(new ScanItem(sellid, parts[0], parts[1]));
                    
                    //Remove this to parse all 10 items
                    return;
                }
            }
            else MessageBox.Show("Не удалось загрузить список предметов!", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error); ;

        }


        public static string ParseSearchRes(string content, List<SearchItem> lst)
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

                    //Заполняем список 
                    lst.Add(new SearchItem(ItemName, ItemGame, ItemUrl, ItemQuan, ItemPrice, ItemImg));
                }

                totalfind = Regex.Match(content, "(?<=searchResults_total\">)(.*)(?=</span>)").ToString();
            }
            else 
                MessageBox.Show("Не удалось найти!", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error); ;
           
            return totalfind;
        }


        public string ParseInventory(string content)
        {
            inventList.Clear();

            var rgDescr = JsonConvert.DeserializeObject<InventoryData>(content);


            foreach (InvItem prop in rgDescr.myInvent.Values)
            {
                var ourItem = rgDescr.invDescr[prop.classid + "_0"];
                //parse cost by url (_lists + 753/ + ourItem.MarketName)
                //or (_search + name)

                inventList.Add(new InventItem(prop.assetid, ourItem.Name, ourItem.Type, "None ", ourItem.IconUrl));
            }

            return inventList.Count.ToString();
        }

    }
}
