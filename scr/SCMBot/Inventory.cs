using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Threading;

namespace SCMBot
{
    public class Inventory
    {
        [DefaultValue(0)]
        public int appTypeIndex { get; set; }

        private BackgroundWorker sellThread = new BackgroundWorker();
        private BackgroundWorker getInventory = new BackgroundWorker();
        public List<AppType> appTypes = new List<AppType>();
        public List<InventItem> inventList = new List<InventItem>();
        public List<ItemToSell> toSellList = new List<ItemToSell>();

        public event InventoryMessagesHandler InventoryMessages;
        public delegate void InventoryMessagesHandler(object obj, MyEventArgs e);

        public int sellDelay { get; set; }

        public bool isDelayRand { get; set; }

        public bool isOnSale
        {
            get { return appTypeIndex >= appTypes.Count; }
        }

        //pointer
        private MarketAuth Auth;


        public class AppType
        {
            public AppType(string name, string app, string context)
            {
                this.Name = name;
                this.App = app;
                this.Context = context;
            }

            public string GetForUrl()
            {
                return string.Format("{0}/{1}", App, Context);
            }

            public string Name { set; get; }
            public string App { set; get; }
            public string Context { set; get; }
        }

        public class ItemToSell
        {
            public ItemToSell(string assetid, string price)
            {
                this.AssetId = assetid;
                this.Price = price;
            }

            public string AssetId { set; get; }
            public string Price { set; get; }
        }


        public class InventItem
        {
            public InventItem(string assetid, string name, string type, decimal price, string imglink, string marketName, bool onSale, bool marketable, string pageLink)
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
            public decimal Price { set; get; }
            public string Type { set; get; }
            public string AssetId { set; get; }
            public string MarketName { set; get; }
            public bool OnSale { set; get; }
            public string PageLnk { set; get; }

            //Dummy
            public bool Marketable { set; get; }
        }


       
        public Inventory(string path, MarketAuth marketauth)
        {
            Auth = marketauth;

            getInventory.WorkerSupportsCancellation = true;
            getInventory.DoWork += new DoWorkEventHandler(getInventory_DoWork);

            sellThread.WorkerSupportsCancellation = true;
            sellThread.DoWork += new DoWorkEventHandler(sellThread_DoWork);

            LoadAppTypes(path);
        }

        public void LoadAppTypes(string path)
        {
            if (File.Exists(path))
            {
                appTypes.Clear();

                var plines = File.ReadAllLines(path);

                for (int i = 0; i < plines.Length; i++)
                {
                   var keyVal = plines[i].Split('=');
                   if (keyVal.Length != 2) continue;

                   var name = keyVal[0];

                   var appCont = keyVal[1].Split('/');
                   if (appCont.Length != 2) continue;

                   var app = appCont[0];
                   var cont = appCont[1];

                   appTypes.Add(new AppType(name, app, cont));
                }
            }
        }

        private void fireMessage(int code, int percent, string message)
        {
            InventoryMessages(this, new MyEventArgs(code, percent, message));
        }

        public void loadInventory()
        {

            if (getInventory.IsBusy != true)
            {
                getInventory.RunWorkerAsync();
            }
        }

        public void ItemSell()
        {
            if (sellThread.IsBusy != true)
            {
                sellThread.RunWorkerAsync();
            }
        }

        private void getInventory_DoWork(object sender, DoWorkEventArgs e)
        {
            //if index in list
            if (!isOnSale)
            {
                var url = string.Format(Steam.invJsonUrl, Auth.UserID, appTypes[appTypeIndex].GetForUrl());
                ParseInventory(Auth.SendGet(url));
            }
            else
            {
                ParseOnSale(Auth.SendGet(Steam.market));
            }

            if (inventList.Count > 0)
                fireMessage(0, 0, "Inventory Loaded");
            else
                fireMessage(0, 1, "Inventory Section is empty");

        }


        private void sellThread_DoWork(object sender, DoWorkEventArgs e)
        {
            var cunt = toSellList.Count;

            if (cunt != 0)
            {
                int incr = (100 / cunt);

                bool isSleep = false;

                if (cunt > 0)
                    isSleep = true;


                Random random = new Random();
                int min = sellDelay / 2;
                int max = sellDelay * 2;

                for (int i = 0; i < cunt; i++)
                {
                    if (isOnSale)
                    {
                        var req = "sessionid=" + Auth.SessionID;
                        Auth.SendPost(req, Steam.removeSellUrl + toSellList[i].AssetId, Steam.market);
                    }
                    else
                    {
                        if (toSellList[i].Price != Strings.None)
                        {
                            var appReq = appTypes[appTypeIndex];
                            var req = string.Format(Steam.sellReq, Auth.SessionID, appReq.App, appReq.Context, toSellList[i].AssetId, toSellList[i].Price);
                            Auth.SendPost(req, Steam.sellUrl, Steam.market);
                        }
                    }

                    fireMessage(3, (incr * (i + 1)), "Selling progress");


                    if ((isSleep) && (i != cunt - 1))
                    {
                        if (isDelayRand)
                        {
                            Thread.Sleep(random.Next(min, max));
                        }
                        else
                            Thread.Sleep(sellDelay);
                    }
                }

                fireMessage(2, 0, "Item(s) sold");
            }
            else
            {
                fireMessage(2, 0, "Nothing to sell");
            }
        }

        //Fixed
        public void ParseInventory(string content)
        {
            inventList.Clear();

            try
            {
                var rgDescr = JsonConvert.DeserializeObject<Steam.InvData>(content);

                foreach (Steam.InvItem prop in rgDescr.myInvent.Values)
                {
                    var ourItem = rgDescr.invDescr[prop.classid + "_" + prop.instanceid];

                    //parse cost by url (_lists + 753/ + ourItem.MarketName)
                    decimal price = 0;

                    if (!ourItem.Marketable)
                        price = 1;

                    //fix for special symbols in Item Name
                    string markname = string.Empty;


                    if ((ourItem.MarketName == null) && (ourItem.Name == string.Empty))
                    {
                        ourItem.Name = ourItem.SimpleName;
                        ourItem.MarketName = ourItem.SimpleName;
                    }

                    //BattleBlock Theater Fix
                    markname = Uri.EscapeDataString(ourItem.MarketName);
                    string pageLnk = string.Format("{0}{1}/{2}", Steam.listingsUrl, ourItem.AppId, markname);

                    inventList.Add(new InventItem(prop.assetid, ourItem.Name, ourItem.Type, price, ourItem.IconUrl, ourItem.MarketName, false, ourItem.Marketable, pageLnk));
                }
           }
            catch (Exception e)
            {
                Main.AddtoLog(e.Message);
            }
        }

        public void ParseOnSale(string content)
        {
            inventList.Clear();
            string parseBody = Regex.Match(content, "(?<=my_listing_section market_home_listing_table\">)(.*)(?=div class=\"my_listing_section market_content_block)", RegexOptions.Singleline).ToString();

            MatchCollection matches = Regex.Matches(parseBody, "(?<=market_recent_listing_row listing_)(.*?)(?=javascript:RemoveMarketListing)", RegexOptions.Singleline);
            if (matches.Count != 0)
            {
                foreach (Match match in matches)
                {
                    string currmatch = match.Groups[1].Value;

                    string ImgLink = Regex.Match(currmatch, "(?<=economy/image/)(.*)(?=/38fx38f\")").ToString();

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


                    string[] LinkName = Regex.Match(currmatch, "(?<=_name_link\" href=\")(.*)(?=</a></span><br/>)").ToString().Split(new string[] { "\">" }, StringSplitOptions.None);

                    string ItemType = Regex.Match(currmatch, "(?<=_listing_game_name\">)(.*)(?=</span>)").ToString();

                    inventList.Add(new InventItem(listId, LinkName[1], ItemType, Steam.ToDecimal(Auth.CleanPrice(captainPrice)), ImgLink, string.Empty, true, true, pageLnk));

                }

            }
            //  else
            //TODO. Add correct error processing
            // MessageBox.Show(Strings.OnSaleErr, Strings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }



        public void GetActualPrice(string link, int index)
        {
            ThreadStart threadStart = delegate()
            {
                int resVolume = 0;
                string actual = "0.03";

                try
                {
                    var appName = Steam.GetAppName(link);
                    var actualJSON = Auth.SendGet(string.Format(Steam.overviewUrl, Auth.JsonAddon, appName.Key, appName.Value));
                    var priceOver = JsonConvert.DeserializeObject<Steam.PriceOverview>(actualJSON);

                    if (priceOver.Success)
                    {
                        resVolume = Convert.ToInt32(Regex.Replace(priceOver.Volume, ",", string.Empty));
                        actual = Auth.CleanPrice(priceOver.Lowest);
                    }

                }
                catch (Exception)
                {
                    //dummy
                }
                finally
                {
                    fireMessage(1, index, actual);
                }

            };
            Thread pTh = new Thread(threadStart);
            pTh.IsBackground = true;
            pTh.Start();
        }
    }
}
