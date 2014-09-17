using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Security.Cryptography;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using Newtonsoft.Json;


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

        public const string _lists = _market + "listings/";


        //Todo: JSON
        public const string _search = _market + "search/render/?query={0}&start={1}&count={2}";
        //public const string _search = _market + "search?q=";


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

        //Fix
        public const string imgUri = "http://steamcommunity-a.akamaihd.net/economy/image/";

        //public const string invImgUrl = imgUri + "{0}/96fx96f";
        public const string fndImgUrl = imgUri + "{0}/62fx62f";
        public const string _sellitem = _mainsiteS + "market/sellitem/";
        public const string sellReq = "sessionid={0}&appid={1}&contextid={2}&assetid={3}&amount=1&price={4}";
        public const string removeSell = _market + "removelisting/";

        public const string searchPageReq = "{0}&start={1}0";

        public const string recentMarket = _market + "recent/";

        //New, update fix
        public const string priceOverview = _market + "priceoverview/?{0}&appid={1}&market_hash_name={2}";
        public const string jsonAddonUrl = "?country={0}&language={1}&currency={2}";

        //For html parsing, bulding own json!
        public string  buildJson = "\"success\":true,\"results_html\":\"\",\"listinginfo\":{0},\"assets\":{1}";

        //================================ Consts ======================= End ===============================================

        public event eventDelegate delegMessage;

        private List<ScanItem> lotList = new List<ScanItem>();
        public List<SearchItem> searchList = new List<SearchItem>();
        public List<InventItem> inventList = new List<InventItem>();


        public class ScanItem
        {
            public ScanItem(string listringId, int price, int fee, AppType appType, string itemName)
            {
                this.ListringId = listringId;
                this.Price = price;
                this.Fee = fee;
                this.Type = appType;
                this.ItemName = itemName;
            }

            public string ListringId { set; get; }
            public int Price { set; get; }
            public int Fee { set; get; }
            public AppType Type { set; get; }
            public string ItemName { set; get; }
        }


        public class InventItem
        {
            public InventItem(string assetid, string name, string type, string price, string imglink, string marketName, bool onSale, bool marketable, string pageLink)
            {
                this.Name = name;
                this.AssetId = assetid;
                this.Type = type;
                this.Price = price;
                this.ImgLink = imglink;
                this.OnSale = onSale;
                this.MarketName = marketName;
                this.PageLnk = pageLink;
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
            public string PageLnk { set; get; }

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
                //1 for USD, 2 for GBP, 3 for EUR, 5 for RUB, 7 for BRL, 11 for MYR, 13 for SGD, 14 for THB

                this.Add(new CurrencyInfo("&#36;", "$", "1"));
                this.Add(new CurrencyInfo("p&#1091;&#1073;.", "руб.", "5"));
                this.Add(new CurrencyInfo("&#163;", "£", "2"));
                this.Add(new CurrencyInfo("&#8364;", "€", "3"));
                
                //Fixed, thanks to Brasilian guy.
                this.Add(new CurrencyInfo("&#82;&#36;", "R$", "7"));
                //Fixed, thanks to Malaysian guy.
                this.Add(new CurrencyInfo("RM", "RM", "11"));
                //Fixed, thanks to Singaporian guy.
                this.Add(new CurrencyInfo("S&#36;", "S$ ", "13"));
                //Fixed, thanks to Thai guy.
                this.Add(new CurrencyInfo("#x0e3f;", "฿", "14"));
               
                this.NotSet = true;
                this.Current = 0;
            }

            public int Current { set; get; }
            public bool NotSet { set; get; }
            
            public string GetName()
            {
                return this[Current].TrueName;
            }

            public string GetCode()
            {
                return this[Current].Index;
            }

            public string GetAscii()
            {
                return this[Current].AsciiName;
            }

            public void GetType(string input)
            {
                for (int i = 0; i < this.Count; i++)
                {
                    if (input.Contains(this[i].AsciiName))
                    {
                        Current = i;
                        NotSet = false;
                        //break;
                    }
                }
            }


            public string ReplaceAscii(string parseAmount)
            {
                return parseAmount.Replace(GetAscii(), GetName());
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
            //Fix for Resell feature
            [JsonProperty("market_name")]
            public string Name { get; set; }

            [JsonProperty("name")]
            public string SimpleName { get; set; }

            [JsonProperty("icon_url")]
            public string IconUrl { get; set; }

            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("market_hash_name")]
            public string MarketName { get; set; }

            [JsonProperty("marketable")]
            public bool Marketable { get; set; }

            //new
            [JsonProperty("appid")]
            public string AppId { get; set; }
            
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


        public class PageBody
        {
            [JsonProperty("success")]
            public bool Success { get; set; }
            [JsonProperty("results_html")]
            public string HtmlRes { get; set; }

            [JsonProperty(PropertyName = "listinginfo", Required = Required.Default)]
            public IDictionary<string, ListingInfo> Listing { get; set; }

            [JsonProperty("assets")]
            public IDictionary<string, IDictionary<string, IDictionary<string, ItemInfo>>> Assets { get; set; }
        }

  
        public class ListingInfo
        {
            [JsonProperty("listingid")]
            public string listingid { get; set; }

            [JsonProperty("converted_price")]
            public int price { get; set; }

            [JsonProperty("converted_fee")]
            public int fee { get; set; }

            [JsonProperty("asset")]
            public ItemAsset asset { get; set; }

            [JsonProperty("steamid_lister")]
            public string userId { get; set; }
        }

        public class ItemAsset
        {
            [JsonProperty("appid")]
            public string appid { get; set; }

            [JsonProperty("contextid")]
            public string contextid { get; set; }

            [JsonProperty("id")]
            public string id { get; set; }
        }

        public class Assets
        {
            [JsonProperty(PropertyName = "listinginfo", Required = Required.Default)]
            public IDictionary<string, ListingInfo> Listing { get; set; }

            [JsonProperty("converted_fee")]
            public string fee { get; set; }

            [JsonProperty("asset")]
            public ItemAsset asset { get; set; }
        }

        public class ItemInfo
        {
            [JsonProperty("market_name")]
            public string name { get; set; }

            [JsonProperty(PropertyName = "fraudwarnings", Required = Required.Default)]
            public object warnings { get; set; }

            //Not Useful Yet...
            [JsonProperty("type")]
            public string type { get; set; }

            [JsonProperty("tradable")]
            public bool tradable { get; set; }

            [JsonProperty("icon_url")]
            public string icon_url { get; set; }
        }

        public class SearchBody
        {
            [JsonProperty("success")]
            public bool Success { get; set; }
            [JsonProperty("results_html")]
            public string HtmlRes { get; set; }
            [JsonProperty("total_count")]
            public string TotalCount { get; set; }
        }
        
        public class PriceOverview
        {
            [JsonProperty("success")]
            public bool Success { get; set; }
            [JsonProperty("lowest_price")]
            public string Lowest { get; set; }
            [JsonProperty("volume")]
            public string Volume { get; set; }
            [JsonProperty("median_price")]
            public string Median { get; set; }
        }


        //End JSON

        protected void doMessage(flag myflag, int searchId, object message, bool isMain)
        {
            try
            {

                if (delegMessage != null)
                {
                    Control target = delegMessage.Target as Control;

                    if (target != null && target.InvokeRequired)
                    {
                        target.Invoke(delegMessage, new object[] { this, message, searchId, myflag, isMain });
                    }
                    else
                    {
                        delegMessage(this, message, searchId, myflag, isMain);
                    }
                }

            }
            catch (Exception e)
            {
                Main.AddtoLog(e.Message);
            }
        }

        private string SendPost(string data, string url, string refer, bool tolog)
        {
            Main.reqPool.WaitOne();

            doMessage(flag.StripImg, 0, string.Empty, true);
            var res = Main.SendPostRequest(data, url, refer, cookieCont, tolog);
            doMessage(flag.StripImg, 1, string.Empty, true);

            Main.reqPool.Release();

            return res;

        }

        private string SendGet(string url, CookieContainer cok, bool UseProxy, bool keepAlive)
        {
            Main.reqPool.WaitOne();

            doMessage(flag.StripImg, 0, string.Empty, true);
            var res = Main.GetRequest(url, cookieCont, UseProxy, keepAlive);
            doMessage(flag.StripImg, 1, string.Empty, true);
            
            //MessageBox.Show("blocked");
            if (Main.ReqDelay > 0)
                Main.reqPool.WaitOne(Main.ReqDelay);

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


        public static AppType GetUrlApp(int appIndx, bool isGetInv)
        {
            string app = "753";
            string cont = "6";

            switch (appIndx)
            {
                case 0: //Trading Cards
                    app = "753";
                    cont = "6";
                    break;
                case 1:  //TF2
                    app = "440";
                    cont = "2";
                    break;
                case 2:  //DOTA2
                    app = "570";
                    cont = "2";
                    break;
                case 3: //CS:GO
                    app = "730";
                    cont = "2";
                    break;
                case 4: //BattleBlock Theater
                    app = "238460";
                    cont = "2";
                    break;
                case 5: //Warframe
                    app = "230410";
                    cont = "2";
                    break;
                case 6: //Sins of a Dark Age
                    app = "251970";
                    cont = "1";
                    break;
                case 7: //Path of Exile
                    app = "238960";
                    cont = "1";
                    break;
            }
            if (isGetInv)
                return new AppType(string.Format("{0}/{1}", app, cont), string.Empty);
            else return new AppType(app, cont);
        }




        public StrParam GetNameBalance(CookieContainer cock, CurrInfoLst currLst)
        {
            Main.AddtoLog("Getting account name and balance...");
            
            string markpage = SendGet(_market, cock, false, true);

            //For testring purposes!
            //string markpage = System.IO.File.ReadAllText(@"C:\sing.htm");

            //Fix to getting name regex
            string parseName = Regex.Match(markpage, "(?<=buynow_dialog_myaccountname\">)(.*)(?=</span>)").ToString().Trim();
            
            if (parseName == "")
            {
                return null;
            }

            //accName = parseName;
            //Set profileId for old Url format
            myUserId = Regex.Match(markpage, "(?<=g_steamID = \")(.*)(?=\";)").ToString();

            //30.05.14 Update
            string parseImg = Regex.Match(markpage, "(?<=avatarIcon\"><img src=\")(.*)(?=\" alt=\"\"></span>)", RegexOptions.Singleline).ToString();
           
            string parseAmount = Regex.Match(markpage, "(?<=marketWalletBalanceAmount\">)(.*)(?=</span>)").ToString();

            string country = Regex.Match(markpage, "(?<=g_strCountryCode = \")(.*)(?=\";)").ToString();
            string strlang = Regex.Match(markpage, "(?<=g_strLanguage = \")(.*)(?=\";)").ToString();

            currLst.GetType(parseAmount);

            parseAmount = currLst.ReplaceAscii(parseAmount);
            
            //?country=RU&language=russian&currency=5&count=20
            string Addon = string.Format(jsonAddonUrl, country, strlang, currLst.GetCode());

            return new StrParam(parseName, parseAmount, parseImg, Addon);
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



        private BuyResponse BuyItem(CookieContainer cock, string sessid, string itemId, string link, string subtotal, string fee, string total)
        {
            string data = string.Format(buyReq, sessid, subtotal, fee, total, currencies.GetCode());

            //buy
            //29.08.2013 Steam Update Issue!
            //FIX: using SSL - https:// in url
            string buyres = SendPost(data, _blist + itemId, link, true);
            
            //testing purposes
            //string buyres = File.ReadAllText(@"C:\x.txt");
            
            try
            {
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
                    else return new BuyResponse(false, Strings.UnknownErr);
            }
            catch (Exception)
            {
                return new BuyResponse(false, Strings.UnknownErr);
            }

        }

        private static bool isStillLogged(CookieContainer cook)
        {
            var stcook = cook.GetCookies(new Uri(_mainsite));

            for (int i = 0; i < stcook.Count; i++)
            {
                if (stcook[i].Name.Contains("steamLogin"))
                {
                    return true;
                }
            }
            return false;
        }

        public byte ParseLotList(string content, List<ScanItem> lst, CurrInfoLst currLst, bool full, bool ismain)
        {

            lst.Clear();

            //Smart ass!
            if (Main.isHTML && ismain)
            {
                string jsonAssets = Regex.Match(content, @"(?<=g_rgAssets \= )(.*)(?=;
	var g_rgCurrency)", RegexOptions.Singleline).ToString();

                if (jsonAssets == string.Empty)
                    return 6;

                string jsonListInfo = Regex.Match(content, @"(?<=g_rgListingInfo \= )(.*)(?=;
	var g_plotPriceHistory)", RegexOptions.Singleline).ToString();

                content = "{" + string.Format(buildJson, jsonListInfo, jsonAssets) + "}";
            }
            else
            {

                if (content == string.Empty)
                {
                    //Content empty
                    return 0;
                }
                else if (content == "403")
                {
                    //403 Forbidden
                    return 5;
                }
                else if (content[0] != '{')
                {
                    //Json is not valid
                    return 2;
                }
            }

            try
            {
                //"success":false
                if (content.Substring(11, 1) == "f")
                    return 1;

                var pageJS = JsonConvert.DeserializeObject<PageBody>(content);

                if (pageJS.Listing.Count != 0)
                {
                    foreach (ListingInfo ourItem in pageJS.Listing.Values)
                    {
                        var ourItemInfo = pageJS.Assets[ourItem.asset.appid][ourItem.asset.contextid][ourItem.asset.id];
                        bool isNull = false;


                        if (ourItem.userId == myUserId)
                        {
                            continue;
                        }

                        if ((IgnoreWarn) && (ourItemInfo.warnings != null))
                        {
                            //Renamed Item or Descriprtion
                            Main.AddtoLog(string.Format("{0}: {1}", ourItemInfo.name, ourItemInfo.warnings.ToString()));
                            continue;
                        }

                        if (ourItem.price != 0)
                        {
                            //Damn, Mr.Crowley... WTF!?
                            if (NotSetHead && !full)
                            {
                                doMessage(flag.SetHeadName, scanID, new StrParam(ourItemInfo.name, ourItemInfo.icon_url), true);
                                scanInput.Name = ourItemInfo.name;
                                NotSetHead = false;
                            }

                            lst.Add(new ScanItem(ourItem.listingid, ourItem.price, ourItem.fee, new AppType(ourItem.asset.appid, ourItem.asset.contextid), ourItemInfo.name));
                            isNull = false;
                        }
                        else
                        {
                            isNull = true;
                        }

                        //If we load 1st lot and it's not null
                        if (!full && !isNull)
                            //Fine!
                            return 7;
                    }
                }
                else return 1;

            }
            catch(Exception e)
            {
                //Parsing fail
                Main.AddtoLog("Err Source: " + e.Message);
                return 3;
            }

            if (lst.Count == 0)
                return 0;
            else
                //Fine!
                return 7;
        }


        public static string ParseSearchRes(string content, List<SearchItem> lst, CurrInfoLst currLst)
        {
            lst.Clear();
            string totalFind = "0";

            try
            {
                var searchJS = JsonConvert.DeserializeObject<SearchBody>(content);

                if (searchJS.Success)
                {
                    totalFind = searchJS.TotalCount;

                    //content = File.ReadAllText(@"C:\dollar2.html");
                    MatchCollection matches = Regex.Matches(searchJS.HtmlRes, "(?<=market_listing_row_link\" href)(.*?)(?<=</a>)", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline);
                    if (matches.Count != 0)
                    {

                        foreach (Match match in matches)
                        {
                            string currmatch = match.Groups[1].Value;

                            //Fix for Steam update 5/01/14 4:00 PM PST
                            string ItemUrl = Regex.Match(currmatch, "(?<==\")(.*)(?=\" id)").ToString();

                            string ItemQuan = Regex.Match(currmatch, "(?<=num_listings_qty\">)(.*)(?=</span>)").ToString();

                            //Fix for Steam update 3/26/14 4:00 PM PST
                            string ItemPrice = Regex.Match(currmatch, "(?<=<span style=\"color:)(.*)(?=<div class=\"market_listing_right_cell)", RegexOptions.Singleline).ToString();

                            //Удаляем ascii кода нашей текущей валюты
                            if (currLst.NotSet)
                            {
                                currLst.GetType(ItemPrice);
                                //If not loggen in then
                                ItemPrice = Regex.Replace(ItemPrice, currLst.GetAscii(), string.Empty);
                                //currLst.NotSet = true;
                            }
                            else
                            {
                                ItemPrice = Regex.Replace(ItemPrice, currLst.GetAscii(), string.Empty);

                            }

                            ItemPrice = Regex.Replace(ItemPrice, @"[^\d\,\.]+", string.Empty);

                            //Fix fot Steam update 3/26/14 4:00 PM PST
                            string ItemName = Regex.Match(currmatch, "(?<=listing_item_name\" style=\"color:)(.*)(?=</span>)").ToString();
                            ItemName = ItemName.Remove(0, ItemName.IndexOf(">") + 1);

                            string ItemGame = Regex.Match(currmatch, "(?<=game_name\">)(.*)(?=</span>)").ToString();

                            string ItemImg = Regex.Match(currmatch, "(?<=net/economy/image/)(.*)(/62fx62f)", RegexOptions.Singleline).ToString();

                            //Заполняем список 
                            lst.Add(new SearchItem(ItemName, ItemGame, ItemUrl, ItemQuan, ItemPrice, ItemImg));
                        }

                    }
                    else
                        MessageBox.Show(Strings.SearchErr, Strings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception e)
            {
                Main.AddtoLog(e.Message);
                MessageBox.Show("Error parsing search results.", Strings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return totalFind;
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
                    string price = "0";
                    
                    if (!ourItem.Marketable)
                        price = "1";

                    //fix for special symbols in Item Name
                    string markname = string.Empty;


                    if ((ourItem.MarketName == null) && (ourItem.Name == string.Empty))
                    {
                        ourItem.Name = ourItem.SimpleName;
                        ourItem.MarketName = ourItem.SimpleName;
                    }

                    //BattleBlock Theater Fix
                    markname = Uri.EscapeDataString(ourItem.MarketName);
                    string pageLnk = string.Format("{0}/{1}/{2}", _lists, ourItem.AppId, markname);

                    inventList.Add(new InventItem(prop.assetid, ourItem.Name, ourItem.Type, price, ourItem.IconUrl, ourItem.MarketName, false, ourItem.Marketable, pageLnk));
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

                    string appidRaw = Regex.Match(currmatch, "(?<=market_listing_item_name_link)(.*)(?=</a></span>)").ToString();
                    string pageLnk = Regex.Match(appidRaw, "(?<=href=\")(.*)(?=\">)").ToString();

                    //phuckin' shit
                    string captainPrice = Regex.Match(currmatch, @"(?<=>
						)(.*)(?=					</span>
					<br>)", RegexOptions.Singleline).ToString();

                    captainPrice = GetSweetPrice(Regex.Replace(captainPrice, currLst.GetAscii(), string.Empty).Trim());
                   
                    string[] LinkName = Regex.Match(currmatch, "(?<=_name_link\" href=\")(.*)(?=</a></span><br/>)").ToString().Split(new string[] { "\">" }, StringSplitOptions.None);
                   
                    string ItemType = Regex.Match(currmatch, "(?<=_listing_game_name\">)(.*)(?=</span>)").ToString();

                    inventList.Add(new InventItem(listId, LinkName[1], ItemType, captainPrice, ImgLink, string.Empty, true, true, pageLnk));

                }

            }
          //  else
                //TODO. Add correct error processing
           // MessageBox.Show(Strings.OnSaleErr, Strings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);

            return matches.Count;
        }

    }
}
