using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using System.Text;
using System.Security.Cryptography;
using System.Globalization;

namespace SCMBot
{
    public class Steam
    {

        public const string host = "steamcommunity.com";
        public const string site = "http://" + host + "/";
        public const string siteSSL = "https://" + host + "/";
        public const string market = site + "market/";
        public const string marketSSL = siteSSL + "market/";
 
        // Login Urls ===============================================
        public const string loginUrl = siteSSL + "login/";
        public const string loginRef = loginUrl + "home/?goto=0";
        public const string getRsaUrl = loginUrl + "getrsakey/";
        public const string doLoginUrl = loginUrl + "dologin/";
        public const string logoutUrl = loginUrl + "logout/";

        public const string capUrl = siteSSL + "public/captcha.php?gid=";
        public const string refCapUrl = siteSSL + "actions/RefreshCaptcha/?count=1";
        public const string langChangeUrl = siteSSL + "actions/SetLanguage/";
        
        // Login Reqs ===============================================
        public const string loginReq = "donotcache={9}&password={0}&username={1}&twofactorcode={8}&emailauth={2}&loginfriendlyname={3}&captchagid={4}&captcha_text={5}&emailsteamid={6}&rsatimestamp={7}&remember_login=true";
        public const string rsaReq = "donotcache={0}&username={1}";
        public const string langReq = "language={0}&sessionid={1}";
        public const string jsonAddonUrl = "?country={0}&language={1}&currency={2}";






        // Orders Urls ===============================================
        public const string createOrderUrl = marketSSL + "createbuyorder/";
        public const string orderGraphUrl = market + "itemordershistogram?country={0}&language={1}&currency={2}&item_nameid={3}&two_factor=0";
        public const string cancelOrderUrl = market + "cancelbuyorder/";
        public const string getStatusUrl = market + "getbuyorderstatus/?sessionid={0}&buy_orderid={1}";
        public const string overviewUrl = market + "priceoverview/?{0}&appid={1}&market_hash_name={2}";

        // Orders Reqs ===============================================
        public const string createOrderReq = "sessionid={0}&currency={1}&appid={2}&market_hash_name={3}&price_total={4}&quantity={5}";
        public const string cancelOrderReq = "sessionid={0}&buy_orderid={1}";



        // Other Urls ========================================================
        public const string buyListringUrl = marketSSL + "buylisting/";
        public const string searchUrl = marketSSL + "search/render/?query={0}&start={1}&count={2}";

        public const string recentMarket = marketSSL + "recent/";
        public const string imgUrl = "http://steamcommunity-a.akamaihd.net/economy/image/";
        public const string fndImgUrl = imgUrl + "{0}/62fx62f";

        // Other Reqs ========================================================
        public const string buyReq = "sessionid={0}&currency={4}&subtotal={1}&fee={2}&total={3}&quantity=1";
        public const string searchPageReq = "{0}&start={1}0";


        // Inventory Uls =====================================================

        public const string invJsonUrl = site + "profiles/{0}/inventory/json/{1}";
        public const string listingsUrl = market + "listings/";
        public const string sellUrl = siteSSL + "market/sellitem/";
        public const string removeSellUrl = market + "removelisting/";

        //Inventory Reqs ======================================================
        public const string sellReq = "sessionid={0}&appid={1}&contextid={2}&assetid={3}&amount=1&price={4}";


        //JSON Stuff

        // Login =============================================================

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

            [JsonProperty("requires_twofactor")]
            public bool isTwoFactor { get; set; }
        }

        public class RespFinal
        {
            [JsonProperty("success")]
            public bool Success { get; set; }

            [JsonProperty("login_complete")]
            public bool isComplete { get; set; }
        }


        // Orders ===========================================================

        public class CreateOrderResp
        {
            [JsonProperty("success")]
            public int Success { get; set; }
            [JsonProperty("message")]
            public string Message { get; set; }
            [JsonProperty(PropertyName = "buy_orderid", Required = Required.Default)]
            public string orderID { get; set; }
        }

        public class BuyStatus
        {
            [JsonProperty("success")]
            public int Success { get; set; }
            [JsonProperty("active")]
            public int Active { get; set; }
            [JsonProperty("purchased")]
            public int Purchased { get; set; }
            [JsonProperty("quantity")]
            public int Quantity { get; set; }

            [JsonProperty(PropertyName = "purchases", Required = Required.Default)]
            public IList<PurchaseInfo> Purchases { get; set; }
        }

        public class PurchaseInfo
        {
            [JsonProperty("assetid")]
            public string AssetId { get; set; }
            [JsonProperty("appid")]
            public int App { get; set; }
            [JsonProperty("contextid")]
            public int Context { get; set; }
            [JsonProperty("price_total")]
            public decimal PriceTotal { get; set; }
        }

        public class OrdersInfo
        {
            [JsonProperty("success")]
            public bool Success { get; set; }

            //0-price, 1-quantity, 3-message
            [JsonProperty("buy_order_graph")]
            public IList<IList<string>> BuyGraph { get; set; }

            [JsonProperty("sell_order_graph")]
            public IList<IList<string>> SellGraph { get; set; }
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


        //Inventory ==========================================================
        public class InvData
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

        //Search ============================================================

        public class SearchBody
        {
            [JsonProperty("success")]
            public bool Success { get; set; }
            [JsonProperty("results_html")]
            public string HtmlRes { get; set; }
            [JsonProperty("total_count")]
            public int TotalCount { get; set; }
        }
        

        //End JSONs

        // Methods ===========================================================

        public static string EncryptPassword(string password, string modval, string expval)
        {
            RNGCryptoServiceProvider secureRandom = new RNGCryptoServiceProvider();
            byte[] encryptedPasswordBytes;
            using (var rsaEncryptor = new RSACryptoServiceProvider())
            {
                var passwordBytes = Encoding.ASCII.GetBytes(password);
                var rsaParameters = rsaEncryptor.ExportParameters(false);
                rsaParameters.Exponent = HexStringToByteArray(expval);
                rsaParameters.Modulus = HexStringToByteArray(modval);
                rsaEncryptor.ImportParameters(rsaParameters);
                encryptedPasswordBytes = rsaEncryptor.Encrypt(passwordBytes, false);
            }

            return Uri.EscapeDataString(Convert.ToBase64String(encryptedPasswordBytes));
        }


        public static byte[] HexStringToByteArray(string hex)
        {
            int hexLen = hex.Length;
            byte[] ret = new byte[hexLen / 2];
            for (int i = 0; i < hexLen; i += 2)
            {
                ret[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }
            return ret;
        }

        public static long GetNoCacheTime()
        {
            return ((long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds);
        }

        public static string GetSweetPrice(string input)
        {
            input = input.Trim();
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

        public static KeyValuePair<string, string> GetAppName(string url)
        {
            var parsUrl = Regex.Match(url, "(?<=market/listings/)(.*?)(?=$|\\?)").ToString().Split('/');
            return new KeyValuePair<string, string>(parsUrl[0], parsUrl[1]);
        }

        public static string GetNameFromUrl(string url)
        {
           return Uri.UnescapeDataString(GetAppName(url).Value);
        }

        public static decimal ToDecimal(string input)
        {
            //goddammit... goddammit!
            try
            {
                if (input.Contains(","))
                {
                    var val = decimal.Parse(input.Replace(",", string.Empty));
                    return val / 100;
                }
                else
                    if (input.Contains("."))
                    {
                        var val = decimal.Parse(input.Replace(".", string.Empty));
                        return val / 100;
                    }
                    else
                        return decimal.Parse(input);
            }
            catch (Exception ex)
            {
                Main.AddtoLog("String: \"" + input + "\"\r\n" + ex.Message);
                return (decimal)1;
            }
        }


        //That's all folks. Not precisely.
        public static decimal CalcWithFee(decimal input)
        {
            double res = 0;
            double dInput = (double)input;
            //Magic
            double temp = dInput / 1.15;

            if (dInput > 0.1)
                res = Math.Round(temp, 2, MidpointRounding.AwayFromZero) + 0.01;
            else
                if (dInput < 0.04)
                {
                    if (dInput == 0.03)
                        res = 0.01;
                    else
                        res = 0;
                }
                else
                    res = temp - 0.01;

            return (decimal)res;
        }



        public static decimal AddFee(decimal input)
        {
            double res = 0;
            double dInput = (double)input;
            double temp = 0;

            if (dInput < 0.2)
                temp = 0.02;
            else
            {
                temp = dInput * 0.15;
            }

            res = dInput + temp;

            return (decimal)res;

        }

    }
}
