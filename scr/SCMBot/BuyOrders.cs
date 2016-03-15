using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.Threading;
using System.ComponentModel;

namespace SCMBot
{
    public class BuyOrders
    {
        //pointer
        private MarketAuth Auth;
        public event OrdersMessagesHandler OrdersMessages;
        public delegate void OrdersMessagesHandler(object obj, MyEventArgs e);

        private BackgroundWorker scanThread = new BackgroundWorker();
        private Semaphore Sem = new Semaphore(0, 1);

        private BackgroundWorker removeOrdersThread = new BackgroundWorker();
        private Semaphore Sem2 = new Semaphore(0, 1);


        private BackgroundWorker placeOrderThread = new BackgroundWorker();
        private BackgroundWorker getOrdersThread = new BackgroundWorker();

        public int ScanDelay { get; set; }
        public int ResellDelay { get; set; }
        public bool Scanning { get; set; }


        public List<MyOrder> myOrders = new List<MyOrder>();
        public OrderHistory orderHistory = new OrderHistory();

        public Steam.OrdersInfo orderInfo = new Steam.OrdersInfo();

        public class ItemInfo
        {
            public ItemInfo(decimal price, int quantity, string link)
            {
                this.Quantity = quantity;
                this.Price = price;
                this.Link = link;
            }

            public decimal Price { set; get; }
            public int Quantity { set; get; }
            public string Link { set; get; }
        }

        public class MyOrder
        {
            public MyOrder(string orderId, int quantity, decimal price, string name, string link, string imgLink)
            {
                this.OrderId = orderId;
                this.ImgLink = imgLink;
                this.Name = name;
                this.Item = new ItemInfo(price, quantity, link);
            }

            public string Name { set; get; }
            public string OrderId { set; get; }
            public string ImgLink { set; get; }

            public ItemInfo Item { set; get; }
        }

        public class HistoryItem
        {
            public HistoryItem(ItemInfo item, int resellType, decimal resellValue)
            {
                this.Name = Steam.GetNameFromUrl(item.Link);
                this.Item = item;
                this.ResellType = resellType;
                this.ResellValue = resellValue;
            }

            [JsonIgnore]
            public string Name { set; get; }
            public ItemInfo Item { set; get; }
            public int ResellType { set; get; }
            public decimal ResellValue { set; get; }
        }

        public class OrderHistory : List<HistoryItem>
        {
            public OrderHistory()
            {
               //dummy
            }

            public int AddItem(string link, decimal price, int quantity, int resellType, decimal resellValue)
            {
               return AddItem(new HistoryItem(new ItemInfo(price, quantity, link), resellType, resellValue));
            }

            public int AddItem(HistoryItem newItem)
            {
                bool found = false;

                for (int i = 0; i < this.Count; i++)
                {
                    //If found
                    if (this[i].Item.Link == newItem.Item.Link)
                    {
                        //Replace
                        this[i] = newItem;
                        found = true;
                        return i;
                    }
                }

                if (!found)
                {
                    this.Add(newItem);
                    return -1;
                }

                return -1;
            }

            private bool CustomContains(string source, string input)
            {
                if (source.IndexOf(input, 0, StringComparison.CurrentCultureIgnoreCase) != -1)
                    return true;
                else
                    return false;
            }

            public int GetIndex(string link)
            {
                //hell hea
                return this.IndexOf(this.Where(p => p.Item.Link == link).FirstOrDefault());
            }

            public HistoryItem GetByLink(string input, bool contains)
            {
                 //some linq
                if (contains)
                    return this.FirstOrDefault(m => CustomContains(m.Item.Link, input));
                else
                    return this.FirstOrDefault(m => m.Item.Link == input);
            }


            public int RemoveItem(string link)
            {
                for (int i = 0; i < this.Count; i++)
                {
                    //If found
                    if (this[i].Item.Link == link)
                    {
                        this.RemoveAt(i);
                        return i;
                    }
                }

                return 0;
            }
        }

         
        public BuyOrders(MarketAuth marketauth)
        {
            Auth = marketauth;

            scanThread.WorkerSupportsCancellation = true;
            scanThread.DoWork += new DoWorkEventHandler(scanThread_DoWork);
            scanThread.RunWorkerCompleted += new RunWorkerCompletedEventHandler(scanThread_RunWorkerCompleted);

            removeOrdersThread.WorkerSupportsCancellation = true;
            removeOrdersThread.DoWork += new DoWorkEventHandler(removeOrdersThread_DoWork);
            removeOrdersThread.RunWorkerCompleted += new RunWorkerCompletedEventHandler(removeOrdersThread_RunWorkerCompleted);

            placeOrderThread.DoWork += new DoWorkEventHandler(placeOrderThread_DoWork);
            placeOrderThread.RunWorkerCompleted += new RunWorkerCompletedEventHandler(placeOrderThread_RunWorkerCompleted);

            getOrdersThread.DoWork += new DoWorkEventHandler(getOrdersThread_DoWork);
            getOrdersThread.RunWorkerCompleted += new RunWorkerCompletedEventHandler(getOrdersThread_RunWorkerCompleted);

        }

        private void fireMessage(int code, int percent, string message)
        {
            OrdersMessages(this, new MyEventArgs(code, percent, message));
        }


        public void GetMyOrders()
        {
            if (getOrdersThread.IsBusy != true)
            {
                getOrdersThread.RunWorkerAsync();
                fireMessage(2, 0, "Getting order list...");
            }
        }

        public void PlaceBuyOrder(int index, string link, decimal price, int quantity, int resType, decimal resVal)
        {
            if (placeOrderThread.IsBusy != true)
            {
                var item = new HistoryItem(new ItemInfo(price, quantity, link), resType, resVal);
                var keypair = new KeyValuePair<int, HistoryItem>(index, item);
                placeOrderThread.RunWorkerAsync(keypair);
            }
        }


        public void CancelOrders(int[] indexes)
        {
            if (removeOrdersThread.IsBusy != true)
            {
                removeOrdersThread.RunWorkerAsync(indexes);
            }
            else
            {
                removeOrdersThread.CancelAsync();
                Sem2.Release();
            }
        }


        public void StartScan()
        {
            if (scanThread.IsBusy != true)
            {
                scanThread.RunWorkerAsync();
                Scanning = true;
                fireMessage(2, 0, "Scan Started");
            }
            else
            {
                scanThread.CancelAsync();
                Sem.Release();
            }

        }


        //Workers Stuff
        
        private void getOrdersThread_DoWork(object sender, DoWorkEventArgs e)
        {
            UpdateOrders();
        }

        private void UpdateOrders()
        {
            myOrders.Clear();

            var marketHTML = Auth.SendGet(Steam.market);

            MatchCollection matches = Regex.Matches(marketHTML, "(?<=_row\" id=\"mybuyorder_)(.*?)(?=CancelMarketBuyOrder)", RegexOptions.Singleline);
            if (matches.Count != 0)
            {
                foreach (Match match in matches)
                {
                    string currmatch = match.Groups[1].Value;

                    string ordId = Regex.Match(currmatch, "(?<=^)(.*?)(?=\">)").ToString();
                    int ordQuantity = Convert.ToInt32(Regex.Match(currmatch, "(?<=buyorder_qty\">)(.*?)(?=@</span>)").ToString().Trim());
                    string ordPrice = Regex.Match(currmatch, @"(?<=@</span>\r\n)(.*?)(?=</span>\r\n\s+</span>\r\n\s+</div>)").ToString().Trim();
                    string[] itemLinkName = Regex.Match(currmatch, "(?<=listing_item_name_link\" href=\")(.*?)(?=</a></span><br/>)").ToString().Split(new string[] { "\">" }, StringSplitOptions.None);
                    string itemImage = Regex.Match(currmatch, "(?<=1x, )(.*?)(?= 2x\")").ToString();
                    myOrders.Add(new MyOrder(ordId, ordQuantity, Steam.ToDecimal(Auth.CleanPrice(ordPrice)), itemLinkName[1], itemLinkName[0], itemImage));
                }
            }

            fireMessage(1, 0, string.Empty);
        }


        private void getOrdersThread_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            fireMessage(2, 0, "Orders Loaded: " + myOrders.Count.ToString());
        }

        private void placeOrderThread_DoWork(object sender, DoWorkEventArgs e)
        {
            var keypair = (KeyValuePair<int, HistoryItem>)e.Argument;
            var index = keypair.Key;
            var item = keypair.Value;

            if (index >= 0)
            {
                fireMessage(2, 0, "Removing order № " + (index + 1).ToString());
                CancelOrder(index);
            }


            fireMessage(2, 0, "Placing order...");

            orderHistory.AddItem(item);

            try
            {
                var finalPrice = Convert.ToString(item.Item.Price * item.Item.Quantity);
                var appName = Steam.GetAppName(item.Item.Link);
                var fullreq = string.Format(Steam.createOrderReq, Auth.SessionID, Auth.CurrencyCode, appName.Key, appName.Value, Steam.GetSweetPrice(finalPrice), item.Item.Quantity);
                var RespJSON = Auth.SendPost(fullreq, Steam.createOrderUrl, item.Item.Link);

                var Resp = JsonConvert.DeserializeObject<Steam.CreateOrderResp>(RespJSON);

                switch (Resp.Success)
                {
                    case 1: e.Result = "Order placed!";
                        break;
                    case 29: e.Result = "You already have order on this item!";
                        break;
                    case 78: e.Result = "Order price is too much for your wallet!";
                        break;
                    case 82: e.Result = "You have 7-day trade and market restriction!";
                        break;
                    default: e.Result = Resp.Message;
                        break;
                }
            }
            catch (Exception)
            {
                fireMessage(2, 0, "Error placing order");
            }
            finally
            {
                UpdateOrders();
            }           
        }


        private void placeOrderThread_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            fireMessage(2, 0, e.Result.ToString());
        }


        private void removeOrdersThread_DoWork(object sender, DoWorkEventArgs e)
        {

            if (myOrders.Count != 0)
            {
                BackgroundWorker worker = sender as BackgroundWorker;

                var remIndexes = (int[])e.Argument;

                fireMessage(2, 0, "Removing orders...");

                int incr = (100 / remIndexes.Length);


                for (int i = 0; i < remIndexes.Length; i++)
                {
                    if (worker.CancellationPending)
                    {
                        e.Cancel = true;
                        return;
                    }

                    fireMessage(4, (incr * (i + 1)), string.Empty);

                    CancelOrder(remIndexes[i]);

                    Sem2.WaitOne(1000);
                }


                UpdateOrders();
            }
            else fireMessage(2, 0, "Load oder list first!");
        }

        private bool CancelOrder(int index)
        {
            var result = Auth.SendPost(string.Format(Steam.cancelOrderReq, Auth.SessionID, myOrders[index].OrderId), Steam.cancelOrderUrl, Steam.market);
            return result.Contains(":1}");
        }

        private void removeOrdersThread_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                fireMessage(5, 0, "Removing cancelled");
            }
            else
                fireMessage(5, 0, "Removing orders complete");
        }


        private void scanThread_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            bool bought = false;

            while (worker.CancellationPending == false)
            {
                fireMessage(2, 0, "Scanning my orders: " + myOrders.Count.ToString());

                bought = false;

                try
                {
                    if (myOrders.Count != 0)
                    {
                        foreach (var item in myOrders)
                        {
                            var statusJSON = Auth.SendGet(string.Format(Steam.getStatusUrl, Auth.SessionID, item.OrderId));

                            var Resp = JsonConvert.DeserializeObject<Steam.BuyStatus>(statusJSON);

                            if (Resp.Success == 1)
                            {
                                if (Resp.Purchased == 1)
                                {
                                    fireMessage(7, 0, "Item bought");

                                    var history = orderHistory.GetByLink(item.Item.Link, false);
                                    if (history != null)
                                    {
                                        var processPrice = GetResellPrice(history.ResellType, history.ResellValue, history.Item.Price);

                                        foreach (var infos in Resp.Purchases)
                                        {
                                           StartSellThread(string.Format(Steam.sellReq, Auth.SessionID, infos.App, infos.Context, infos.AssetId, processPrice));
                                        }
                                    }

                                    bought = true;
                                }
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    fireMessage(2, 0, "Scan Error: " + ex.Message);
                }
                finally
                {
                    if (bought)
                        UpdateOrders();

                    Sem.WaitOne(ScanDelay);
                }
            }
        }

        private void scanThread_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Scanning = false;
            fireMessage(3, 0, "Scan Cancelled");
        }

        //Workers Stuff End

        
        public void GetItemInfo(string link)
        {
            ThreadStart threadStart = delegate()
            {

                var pageHTML = Auth.SendGet(link);

                var name = Steam.GetNameFromUrl(link);
               
                string nameID = Regex.Match(pageHTML, @"(?<=OrderSpread\( )(.*?)(?= \);)").ToString();
                var finalLink = string.Format(Steam.orderGraphUrl, Auth.Country, Auth.Language, Auth.CurrencyCode, nameID);

                var infoJSON = Auth.SendGet(finalLink);
                orderInfo = JsonConvert.DeserializeObject<Steam.OrdersInfo>(infoJSON);
                fireMessage(0, 0, name);

            };
            Thread pTh = new Thread(threadStart);
            pTh.IsBackground = true;
            pTh.Start();
        }

      
        private void StartSellThread(string req)
        {
            ThreadStart threadStart = delegate()
            {
                Thread.Sleep(ResellDelay);
                var sellResp = Auth.SendPost(req, Steam.sellUrl, Steam.market);
                Console.WriteLine(sellResp);

                fireMessage(2, 0, "Item placed on sell");

            };
            Thread pTh = new Thread(threadStart);
            pTh.IsBackground = true;
            pTh.Start();
        }

        private string GetResellPrice(int type, decimal value, decimal buyprice)
        {

            decimal working = 0;

            switch (type)
            {
                case -1:
                case 0: working = value;
                    break;
                case 1: working = buyprice + value;
                    break;
                case 2: 
                    working = buyprice + buyprice * (value / 100);
                    break;
            }

            return Steam.GetSweetPrice(working.ToString());
        }



        public void GetActualPrice(string link)
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
                        if (priceOver.Volume != null)
                        resVolume = Convert.ToInt32(Regex.Replace(priceOver.Volume, ",", string.Empty));
                        actual = Auth.CleanPrice(priceOver.Lowest);
                    }

                }
                catch (Exception)
                {
                    //
                }
                finally
                {
                    fireMessage(6, resVolume, actual);
                }

            };
            Thread pTh = new Thread(threadStart);
            pTh.IsBackground = true;
            pTh.Start();
        }



        internal void GetCurrRate()
        {
            ThreadStart threadStart = delegate()
            {
                decimal rate = 0;

                try
                {               
                    var link = "http://steamcommunity.com/market/listings/730/AWP%20%7C%20Asiimov%20(Battle-Scarred)/render?count=1&start=20&currency=";

                    var dollarJson = Auth.SendGet(link + "1");
                    var ourJson = Auth.SendGet(link + Auth.CurrencyCode.ToString());
                    var dollarText = Regex.Match(dollarJson, @"(?<=without_fee\\"">\\r\\n\\t\\t\\t\\t\\t\\t\$)(.*?)(?=\\t)").ToString();
                    var ourText = Regex.Match(ourJson, @"(?<=without_fee\\"">\\r\\n\\t\\t\\t\\t\\t\\t)(.*?)(?=\\t)").ToString();

                    var rub = Regex.Match(ourText, @"\d+[,.]*\d{1,2}").ToString();

                    var r = Steam.ToDecimal(rub);
                    var d = Steam.ToDecimal(dollarText);

                    rate = System.Math.Round(r / d, 2);
                }
                catch (Exception)
                {
                    // dummy
                }
                finally
                {
                    fireMessage(8, 0, rate.ToString() + " " + Auth.CurrencyName);
                }
            };

            Thread pTh = new Thread(threadStart);
            pTh.IsBackground = true;
            pTh.Start();
   
        }
    }
}
