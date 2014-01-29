using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Security.Cryptography;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.Threading;
using System.IO;


namespace SCMBot
{

    public partial class SteamSite
    {
        //================================ Consts ======================= Begin! ============================================

        public const string _mainsite = "http://steamcommunity.com/";
        public const string _mainsiteS = "https://steamcommunity.com/";

        const string _comlog = "https://steamcommunity.com/login/";
        const string _ref = _comlog + "home/?goto=market%2F";
        const string _getrsa = _comlog + "getrsakey/";
        const string _dologin = _comlog + "dologin/";
        const string _logout = _comlog + "logout/";
        public const string _market = _mainsite + "market/";
        //const string _blist = _market + "buylisting/";
        //FIX

        const string _blist = _mainsiteS + "market/buylisting/";

        const string _lists = _market + "listings/";
        public const string _search = _market + "search?q=";
        const string _capcha = "https://steamcommunity.com/public/captcha.php?gid=";
        const string _refrcap = "https://steamcommunity.com/actions/RefreshCaptcha/?count=1";

        const string _lang_chg = _market + "?l=";

        const string loginReq = "password={0}&username={1}&emailauth={2}&loginfriendlyname={3}&captchagid={4}&captcha_text={5}&emailsteamid={6}&rsatimestamp={7}";
        const string loginStr = "steamid={0}&token={1}&remember_login=false&webcookie={2}";
        //Currency FIX
        //1 = USD, 2 = GBP, 3 = EUR, 5 = RUB
        const string buyReq = "sessionid={0}&currency={4}&subtotal={1}&fee={2}&total={3}";

        //New url format
        //const string _jsonInv = _mainsite + "id/{0}/inventory/json/{1}";
        //Old Url format, recommended
        const string _jsonInv = _mainsite + "profiles/{0}/inventory/json/{1}";

        public const string imgUri = "http://cdn.steamcommunity.com/economy/image/";

        //public const string invImgUrl = imgUri + "{0}/96fx96f";
        public const string fndImgUrl = imgUri + "{0}/62fx62f";
        public const string _sellitem = _mainsiteS + "market/sellitem/";
        public const string sellReq = "sessionid={0}&appid={1}&contextid={2}&assetid={3}&amount=1&price={4}";
        public const string removeSell = _market + "removelisting/";

        public const string searchPageReq = "{0}&start={1}0";

        //================================ Consts ======================= End ===============================================

        public event eventDelegate delegMessage;

        private List<ScanItem> lotList = new List<ScanItem>();
        public List<SearchItem> searchList = new List<SearchItem>();
        public List<InventItem> inventList = new List<InventItem>();


        public class ScanItem
        {
            public ScanItem(string sellerId, string price, string subtotal, AppType appType)
            {
                this.SellerId = sellerId;
                this.Price = price;
                this.SubTotal = subtotal;
                this.Type = appType;
            }

            public string SellerId { set; get; }
            public string Price { set; get; }
            public string SubTotal { set; get; }
            public AppType Type { set; get; }
        }


        public class InventItem
        {
            public InventItem(string assetid, string name, string type, string price, string imglink, string marketName, bool onSale, bool marketable)
            {
                this.Name = name;
                this.AssetId = assetid;
                this.Type = type;
                this.Price = price;
                this.ImgLink = imglink;
                this.OnSale = onSale;
                this.MarketName = marketName;

                //Dummy
                this.Marketable = marketable;
            }

            public string Name { set; get; }
            public string ImgLink { set; get; }
            public string Price { set; get; }
            public string Type { set; get; }
            public string AssetId { set; get; }
            public string MarketName { set; get; }
            public bool OnSale { set; get; }

            //Dummy
            public bool Marketable { set; get; }
        }

        public class BuyResponse
        {
            public BuyResponse(bool succsess, string mess)
            {
                this.Succsess = succsess;
                this.Mess = mess;
            }

            public bool Succsess { set; get; }
            public string Mess { set; get; }
        }


        public class CurrencyInfo
        {
            public CurrencyInfo(string asciiName, string trueName, string index)
            {
                this.AsciiName = asciiName;
                this.TrueName = trueName;
                this.Index = index;
            }

            public string AsciiName { set; get; }
            public string TrueName { set; get; }
            public string Index { set; get; }
        }

        public class CurrInfoLst : List<CurrencyInfo>
        {
            public CurrInfoLst()
            {
                //1 for USD, 2 for GBP, 3 for EUR, 5 for RUB

                this.Add(new CurrencyInfo("&#36;", "$", "1"));
                this.Add(new CurrencyInfo("p&#1091;&#1073;.", "руб.", "5"));
                this.Add(new CurrencyInfo("&#163;", "£", "2"));
                this.Add(new CurrencyInfo("&#8364;", "€", "3"));
                
                //Brasilian Real? I guess...
                this.Add(new CurrencyInfo("R&#36;", "R$", "4"));
               
                this.Current = 0;
                this.NotSet = true;
            }

            public int Current { set; get; }
            public bool NotSet { set; get; }
            
            public string GetCurrentName()
            {
                return this[this.Current].TrueName;
            }
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

            [JsonProperty("bad_captcha")]
            public bool isBadCap { get; set; }
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

            //FIX
            [JsonProperty("instanceid")]
            public string instanceid { get; set; }
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

            [JsonProperty("marketable")]
            public bool Marketable { get; set; }
            
        }

        public class InfoMessage
        {
            [JsonProperty("message")]
            public string Message { get; set; }
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

        private string SendPost(string data, string url, string refer, bool tolog)
        {
            Main.reqPool.WaitOne();

            doMessage(flag.StripImg, 0, string.Empty);
            var res = Main.SendPostRequest(data, url, refer, cookieCont, tolog);
            doMessage(flag.StripImg, 1, string.Empty);
            
            Main.reqPool.Release();

            return res;

        }

        private string SendGet(string url, CookieContainer cok)
        {
            Main.reqPool.WaitOne();

            doMessage(flag.StripImg, 0, string.Empty);
            var res = Main.GetRequest(url, cookieCont);
            doMessage(flag.StripImg, 1, string.Empty);
            
            //MessageBox.Show("blocked");

            Main.reqPool.Release();

            return res;
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


        public static string GetSweetPrice(string input)
        {
            string res = string.Empty;

            var match = input.IndexOfAny(".,".ToCharArray());

            if ((match == -1) | (match == input.Length - 1))
            {
                res = input + "00";
            }
            else
            {
                //Укорачиваем
                if (input.Length > match + 3)
                {
                    res = input.Substring(0, match + 3);
                }
                else
                    //Удлинняем
                    if (input.Length == match)
                    {
                        res = input + "00";
                    }
                    else if (input.Length == match + 2)
                    {
                        res = input + "0";
                    }
                    else res = input;
            }

            return Regex.Replace(res, @"[d\.\,]+", string.Empty);

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

        private static string GetCurrencyType(string input, CurrInfoLst currLst)
        {
            string res = "0";
            for (int i = 0; i < currLst.Count; i++)
            {
                if (input.Contains(currLst[i].AsciiName))
                {
                    currLst.Current = i;
                    currLst.NotSet = false;
                    res = currLst[i].Index;
                    break;
                }
            }
            return res;
        }

        public string GetNameBalance(CookieContainer cock, CurrInfoLst currLst)
        {
            Main.AddtoLog("Getting account name and balance...");
            string markpage = SendGet(_market, cock);

            //For testring purposes!
            //string markpage = File.ReadAllText(@"C:\dollars.html");

            //Fix to getting name regex
            string parseName = Regex.Match(markpage, "(?<=buynow_dialog_myaccountname\">)(.*)(?=</span>)").ToString().Trim();
            
            if (parseName == "")
            {
                return string.Empty;
            }

            //accName = parseName;
            //Set profileId for old Url format
            accName = Regex.Match(markpage, "(?<=g_steamID = \")(.*)(?=\";)").ToString();

            string parseImg = Regex.Match(markpage, "(?<=headerUserAvatarIcon\" src=\")(.*)(?=<div id=\"global_action_menu\">)", RegexOptions.Singleline).ToString();
            parseImg = parseImg.Substring(0, parseImg.Length - 46);

            string parseAmount = Regex.Match(markpage, "(?<=marketWalletBalanceAmount\">)(.*)(?=</span>)").ToString();

            string curInd = GetCurrencyType(parseAmount, currLst);
            parseAmount = parseAmount.Replace(currLst[currLst.Current].AsciiName, currLst[currLst.Current].TrueName);

            return string.Format("{0}|{1}|{2}|{3}", parseName, parseAmount, parseImg, curInd);
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



        private BuyResponse BuyItem(CookieContainer cock, string sessid, string itemId, string link, string total, string subtotal, string currStr)
        {
            //For test purpose
            //return new BuyResponse(true, "100");

            int int_total = Convert.ToInt32(prFormat(total));
            int int_sub = Convert.ToInt32(prFormat(subtotal));
            string fee = (int_total - int_sub).ToString();

            string data = string.Format(buyReq, sessid, int_sub, fee, int_total, currStr);

            //buy
            //29.08.2013 Steam Update Issue!
            //FIX: using SSL - https:// in url
            string buyres = SendPost(data, _blist + itemId, link, true);

           

            if (buyres.Contains("message"))
            {
                //Already buyed!
                var ErrBuy = JsonConvert.DeserializeObject<InfoMessage>(buyres);
                return new BuyResponse(false, ErrBuy.Message);
            }
            else
            
            if (buyres != string.Empty)
            {
                var AfterBuy = JsonConvert.DeserializeObject<WalletInfo>(buyres);

                if (AfterBuy.WalletRes.Success == 1)
                {
                    string balance = AfterBuy.WalletRes.Balance;
                    balance = balance.Insert(balance.Length - 2, ",");
                    return new BuyResponse(true, balance);

                }
                else return new BuyResponse(false, Strings.UnknownErr);
            }
            else return new BuyResponse(false, "Unknown Error");

        }

        private static string fixNotFracture(string input)
        {
            string output = input.Trim();

            if (input.IndexOfAny(",.".ToCharArray()) == -1)
            {
                output = input + "00";
            }

            if (output.Contains(","))
                output = output.Replace(",", string.Empty);
            else
            if (output.Contains("."))
                output = output.Replace(".", string.Empty);

            return output;
        }

        public static void ParseLotList(string content, List<ScanItem> lst, CurrInfoLst currLst)
        {
            lst.Clear();

            //For testring purposes!
            //content = File.ReadAllText(@"C:\dollars.html");

            if (content == string.Empty)
                return;

            MatchCollection matches = Regex.Matches(content, "BuyMarketListing(.*?)market_listing_seller\">", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline);

            if (matches.Count != 0)
            {
                //Парсим все лоты (авось пригодится в будущем), но использовать будем пока только первый.
                foreach (Match match in matches)
                {
                    string currmatch = match.Groups[1].Value;

                    //Чистим результат от тегов
                    //Оставляем цифры, пробелы, точки и запятые, разделяющие цены
                    currmatch = Regex.Replace(currmatch, "<[^>]+>", string.Empty).Trim();

                    //Удаляем ascii кода нашей текущей валюты
                    currmatch = Regex.Replace(currmatch, currLst[currLst.Current].AsciiName, string.Empty);

                    currmatch = Regex.Replace(currmatch, @"[^\.\,\d\ ]+", string.Empty);

                    //Отделяем номер лота
                    string sellid = currmatch.Substring(2, 19);
                    
                    //Отделяем тип приложения
                    string appDirty = currmatch.Substring(24, 30);
                    string[] app = Regex.Split(appDirty, ", ");
                    var appRes = new AppType(app[0], app[1]);


                    //Отделяем строку, содержащую цены
                    string amount = currmatch.Substring(43, currmatch.Length - 43).Trim();

                    string[] parts = Regex.Split(amount, " +");
 
                    string _price = fixNotFracture(parts[0]);
                    string _subtot = fixNotFracture(parts[1]);

                    //Заполняем список лотов
                    lst.Add(new ScanItem(sellid, _price, _subtot, appRes));

                    //Remove this to parse all 10 items
                    return;
                }
            }
            
        }


        public static string ParseSearchRes(string content, List<SearchItem> lst, CurrInfoLst currLst)
        {
            lst.Clear();
            string totalfind = "0";

            //content = File.ReadAllText(@"C:\dollar2.html");
            MatchCollection matches = Regex.Matches(content, "(?<=market_listing_row_link\" href)(.*?)(?<=</a>)", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline);
            if (matches.Count != 0)
            {

                foreach (Match match in matches)
                {
                    string currmatch = match.Groups[1].Value;

                    string ItemUrl = Regex.Match(currmatch, "(?<==\")(.*)(?=\">)").ToString();

                    string ItemQuan = Regex.Match(currmatch, "(?<=num_listings_qty\">)(.*)(?=</span>)").ToString();


                    string ItemPrice = Regex.Match(currmatch, "(?<=<br/>)(.*)(?=<div class=\"market_listing)", RegexOptions.Singleline).ToString();
                   
                    //Удаляем ascii кода нашей текущей валюты
                    if (currLst.NotSet)
                    {
                        //If not loggen in then
                        GetCurrencyType(ItemPrice, currLst);
                        ItemPrice = Regex.Replace(ItemPrice, currLst[currLst.Current].AsciiName, string.Empty);
                        currLst.NotSet = true;
                    }
                    else
                    {
                        ItemPrice = Regex.Replace(ItemPrice, currLst[currLst.Current].AsciiName, string.Empty);

                    }

                    ItemPrice = Regex.Replace(ItemPrice, @"[^\d\,\.]+", string.Empty);

                    string ItemName = Regex.Match(currmatch, "(?<=style=\"color:)(.*)(?=</span>)").ToString();
                    ItemName = ItemName.Remove(0, ItemName.IndexOf(">") + 1);

                    string ItemGame = Regex.Match(currmatch, "(?<=game_name\">)(.*)(?=</span>)").ToString();

                    string ItemImg = Regex.Match(currmatch, "(?<=com/economy/image/)(.*)(/62fx62f)", RegexOptions.Singleline).ToString();

                    //Заполняем список 
                    lst.Add(new SearchItem(ItemName, ItemGame, ItemUrl, ItemQuan, ItemPrice, ItemImg));
                }

                totalfind = Regex.Match(content, "(?<=searchResults_total\">)(.*)(?=</span>)").ToString();
            }
            else
                MessageBox.Show(Strings.SearchErr, Strings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);

            return totalfind;
        }


        //Fixed
        public int ParseInventory(string content)
        {
            inventList.Clear();
            try
            {
                var rgDescr = JsonConvert.DeserializeObject<InventoryData>(content);

                foreach (InvItem prop in rgDescr.myInvent.Values)
                {
                    var ourItem = rgDescr.invDescr[prop.classid + "_" + prop.instanceid];

                    //parse cost by url (_lists + 753/ + ourItem.MarketName)
                    string price = Strings.None;
                    
                    if (!ourItem.Marketable)
                        price = Strings.NFS;


                    //Careful, this action takes time!
                    //string cont = GetRequest(_lists + GetUrlApp(invApp, false).App + "/" + ourItem.MarketName, cookieCont);
                    //var tempLst = new List<ScanItem>();
                    //ParseLotList(cont, tempLst, currencies);
                    //if (tempLst.Count != 0)
                    //  price = tempLst[0].Price;

                    inventList.Add(new InventItem(prop.assetid, ourItem.Name, ourItem.Type, price, ourItem.IconUrl, ourItem.MarketName, false, ourItem.Marketable));
                }
            }
            catch (Exception e)
            {
                Main.AddtoLog(e.Message);
            }

            return inventList.Count;
        }


        public int ParseOnSale(string content, CurrInfoLst currLst)
        {
            inventList.Clear();
            string parseBody = Regex.Match(content, "(?<=section market_home_listing_table\">)(.*)(?=<div id=\"tabContentsMyMarketHistory)", RegexOptions.Singleline).ToString();

            MatchCollection matches = Regex.Matches(parseBody, "(?<=market_recent_listing_row listing_)(.*?)(?=	</div>\r\n</div>)", RegexOptions.Singleline);
            if (matches.Count != 0)
            {
                foreach (Match match in matches)
                {
                    string currmatch = match.Groups[1].Value;

                    string ImgLink = Regex.Match(currmatch, "(?<=economy/image/)(.*)(?=/38fx38f)").ToString();

                    //If you need:
                    //string assetid = Regex.Match(currmatch, "(?<='mylisting', ')(.*)(?=\" class=\"item_market)").ToString();
                    //assetid = assetid.Substring(assetid.Length - 11, 9); 

                    string listId = Regex.Match(currmatch, "(?<=mylisting_)(.*)(?=_image\" src=)").ToString();

                    string captainPrice = Regex.Match(currmatch, "(?<=market_listing_price\">)(.*)(?=			</span>)", RegexOptions.Singleline).ToString().Trim();

                    captainPrice = Regex.Replace(captainPrice, currLst[currLst.Current].AsciiName, string.Empty);


                    string[] LinkName = Regex.Match(currmatch, "(?<=_name_link\" href=\")(.*)(?=</a></span><br/>)").ToString().Split(new string[] { "\">" }, StringSplitOptions.None);
                   
                    string ItemType = Regex.Match(currmatch, "(?<=_listing_game_name\">)(.*)(?=</span>)").ToString();

                    inventList.Add(new InventItem(listId, LinkName[1], ItemType, captainPrice, ImgLink, string.Empty, true, true));

                }

            }
            else
                //TODO. Add correct error processing
            MessageBox.Show(Strings.OnSaleErr, Strings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);

            return matches.Count;
        }



    }
}
