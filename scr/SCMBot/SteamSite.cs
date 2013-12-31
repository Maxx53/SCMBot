using System;
using System.ComponentModel;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Threading;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace SCMBot
{
    partial class SteamSite
    {
        public string UserName { set; get; }
        public string Password { set; get; }

        public string wishedPrice { set; get; }
        public string scanDelay { set; get; }
        public string pageLink { set; get; }
        public int scanID { set; get; }

        public string currency { set; get; }

        public string reqTxt { set; get; }
        public string linkTxt { set; get; }
   
        public bool Logged { set; get; }
        public bool LoginProcess { set; get; }
        public bool scaninProg { set; get; }
        public bool toBuy { set; get; }
        public bool BuyNow { set; get; }
       
        public int BuyQuant { get; set; }

        public int invApp { get; set; }
        
        public static string accName;
        public int buyCounter = 0;

        public CookieContainer cookieCont { set; get; }

        public Semaphore Sem = new Semaphore(0, 1);

        private BackgroundWorker loginThread = new BackgroundWorker();
        private BackgroundWorker scanThread = new BackgroundWorker();
        private BackgroundWorker reqThread = new BackgroundWorker();
        private BackgroundWorker sellThread = new BackgroundWorker();

        private BackgroundWorker getInventory = new BackgroundWorker();

        public List<ItemToSell> toSellList = new List<ItemToSell>();

        public CurrInfoLst currencies = new CurrInfoLst();

        public SteamSite()
        {
            loginThread.WorkerSupportsCancellation = true;
            loginThread.DoWork += new DoWorkEventHandler(loginThread_DoWork);

            scanThread.WorkerSupportsCancellation = true;
            scanThread.DoWork += new DoWorkEventHandler(scanThread_DoWork);

            reqThread.WorkerSupportsCancellation = true;
            reqThread.DoWork += new DoWorkEventHandler(reqThread_DoWork);


            getInventory.WorkerSupportsCancellation = true;
            getInventory.DoWork += new DoWorkEventHandler(getInventory_DoWork);

            sellThread.WorkerSupportsCancellation = true;
            sellThread.DoWork += new DoWorkEventHandler(sellThread_DoWork);
         }

        public class AppType
        {
            public AppType(string app, string context)
            {
                this.App = app;
                this.Context = context;
            }
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

        public void Login()
        {
            if (loginThread.IsBusy != true)
            {
                loginThread.RunWorkerAsync();
            }
        }

        public void CancelLogin()
        {
            if (loginThread.IsBusy == true)
            {
                loginThread.CancelAsync();
            }
        }

        public void Logout()
        {
            ThreadStart threadStart = delegate() {
                GetRequest(_logout, cookieCont);
                doMessage(flag.Logout_, 0, string.Empty);
                Logged = false;
            };
            Thread pTh = new Thread(threadStart);
            pTh.IsBackground = true;
            pTh.Start();
        }



        public void ScanPrices()
        {
            if (scanThread.IsBusy != true)
            {
                scaninProg = true;
                scanThread.RunWorkerAsync();
            }
        }

        public void CancelScan()
        {
            if (scanThread.WorkerSupportsCancellation == true && scanThread.IsBusy)
            {
                scaninProg = false;
                Sem.Release();
                scanThread.CancelAsync();
            }
        }


        public void reqLoad()
        {
            if (reqThread.IsBusy != true)
            {
                reqThread.RunWorkerAsync();
            }
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


        static AppType GetUrlApp(int appIndx, bool isGetInv)
        {
            string app = "753";
            string cont = "2";

            switch (appIndx)
            {
                case 0: 
                    app = "753";
                    cont = "6";
                    break;
                case 1: 
                    app = "440";
                    cont = "2";
                    break;

                case 2:
                    app = "570";
                    cont = "2";
                    break;
            }
            if (isGetInv)
            return new AppType(string.Format("{0}/{1}",app,cont),string.Empty);
            else return new AppType( app, cont);


        }

        private void getInventory_DoWork(object sender, DoWorkEventArgs e)
        {

           ParseInventory(GetRequest(string.Format(_jsonInv, accName, GetUrlApp(invApp, true).App), 
               cookieCont));

           doMessage(flag.Inventory_Loaded, 0, ParseOnSale(GetRequest(_market, cookieCont), currencies));

        }


        private void sellThread_DoWork(object sender, DoWorkEventArgs e)
        {
            var cunt = toSellList.Count;

            if (cunt != 0)
            {
                var appReq = GetUrlApp(invApp, false);

                int incr = (100 / cunt);

                for (int i = 0; i < cunt; i++)
                {
                    var req = string.Format(sellReq, GetSessId(cookieCont), appReq.App, appReq.Context, toSellList[i].AssetId, toSellList[i].Price);
                    SendPostRequest(req, _sellitem, _market, cookieCont, false);
                    doMessage(flag.Sell_progress, 0, (incr * (i + 1)).ToString());
                }

                doMessage(flag.Items_Sold, 0, string.Empty);
            }
        }


        private void reqThread_DoWork(object sender, DoWorkEventArgs e)
        {
            doMessage(flag.Search_success, 0, ParseSearchRes(GetRequest(linkTxt + reqTxt, cookieCont), searchList, currencies));
        }


        private void loginThread_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            LoginProcess = true;
            Logged = false;
            doMessage(flag.Rep_progress, 0, "20");
            //if (worker.CancellationPending == true)
            //  return;

            string accInfo = GetNameBalance(cookieCont, currencies);

            if (accInfo != string.Empty)
            {
                doMessage(flag.Already_logged, 0, accInfo);
                doMessage(flag.Rep_progress, 0, "100");
                LoginProcess = false;
                Logged = true;
                return;
            }



            string log_content = SendPostRequest("username=" + UserName, _getrsa, _ref, cookieCont, true);
            if (log_content == string.Empty)
            {
                e.Cancel = true;
                return;
            }

            doMessage(flag.Rep_progress, 0, "40");
            //  if (worker.CancellationPending == true)
            //   return;

            var rRSA = JsonConvert.DeserializeObject<RespRSA>(log_content);
            string firstTry = string.Empty;
            string finalpass = EncryptPassword(Password, rRSA.Module, rRSA.Exponent);

        begin:

            if (rRSA.Success)
            {
                firstTry = SendPostRequest(string.Format(loginReq, finalpass, UserName, string.Empty, string.Empty, string.Empty,
                                                                string.Empty, string.Empty, rRSA.TimeStamp), _dologin, _ref, cookieCont, true);
                doMessage(flag.Rep_progress, 0, "60");
                // if (worker.CancellationPending == true)
                //     return;
            }
            else
            {
                return;
                //el probleme, comondante
            }

            var rProcess = JsonConvert.DeserializeObject<RespProcess>(firstTry);

            if (firstTry.Contains("message"))
            {
                if (rProcess.Message == "Incorrect login")
                {
                    Main.AddtoLog("Incorrect login");
                    doMessage(flag.Login_cancel, 0, "Incorrect login");
                    e.Cancel = true;
                    LoginProcess = false;
                    return;
                }

                Dialog guardCheckForm = new Dialog();

                if (rProcess.isEmail)
                {
                    guardCheckForm.capchgroupEnab = false;
                }
                else if (rProcess.isCaptcha)
                {
                    string newcap = string.Empty;
                    
                  //  Not much, brb
                  //  if (rProcess.isBadCap)
                  //  {
                  //      MessageBox.Show("cap is bad");
                  //      newcap = GetRequest(_refrcap, cookieCont);
                  //      newcap = _capcha + newcap.Substring(8, 20);
                  //   }
                  //   else
                  //   {
                            newcap =  _capcha + rProcess.Captcha_Id;
                  //   }

                    guardCheckForm.codgroupEnab = false;
                    Main.loadImg(newcap, guardCheckForm.capchImg, false, false);
                }

                doMessage(flag.Rep_progress, 0, "80");
                //    if (worker.CancellationPending == true)
                //       return;

                if (guardCheckForm.ShowDialog() == DialogResult.OK)
                {

                    string secondTry = SendPostRequest(string.Format(loginReq, finalpass, UserName, guardCheckForm.MailCode, guardCheckForm.GuardDesc, rProcess.Captcha_Id,
                                                           guardCheckForm.capchaText, rProcess.Email_Id, rRSA.TimeStamp), _dologin, _ref, cookieCont, true);

                    MessageBox.Show(rProcess.Captcha_Id);
                    MessageBox.Show(string.Format(loginReq, finalpass, UserName, guardCheckForm.MailCode, guardCheckForm.GuardDesc, rProcess.Captcha_Id,
                                                           guardCheckForm.capchaText, rProcess.Email_Id, rRSA.TimeStamp));
                    MessageBox.Show(guardCheckForm.capchaText);
                    var rFinal = JsonConvert.DeserializeObject<RespFinal>(secondTry);

                    if (rFinal.Success && rFinal.isComplete)
                    {
                        string accInfo2 = GetNameBalance(cookieCont, currencies);
                        doMessage(flag.Login_success, 0, accInfo2);
                        doMessage(flag.Rep_progress, 0, "100");
                        Logged = true;
                        Main.AddtoLog("Login Success");
                    }
                    else
                    {
                        //TODO: Разобрать кашу, выкинуть goto
                        goto begin;
                    }

                }
                else
                {
                    Main.AddtoLog("Login Guard Check Cancelled");
                    doMessage(flag.Login_cancel, 0, "Login Cancelled");
                    e.Cancel = true;
                }

                guardCheckForm.Dispose();

            }

            else if (rProcess.Success)
            {
                string accInfo3 = GetNameBalance(cookieCont, currencies);

                doMessage(flag.Login_success, 0, accInfo3);
                doMessage(flag.Rep_progress, 0, "100");
                Main.AddtoLog("Login Success");
                Logged = true;
            }
            else
            {
                Main.AddtoLog("Login Guard Check Cancelled");
                doMessage(flag.Login_cancel, 0, string.Empty);
                e.Cancel = true;
                Logged = false;
            }

            LoginProcess = false;
        }


        private static string GetSweetPrice(string input)
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

        public void scanThread_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            string sessid = GetSessId(cookieCont);


            if (BuyNow)
            {
                ParseLotList(GetRequest(pageLink, cookieCont), lotList, currencies);

                if (lotList.Count == 0)
                {
                    doMessage(flag.Price_text, scanID, "Error");
                }
                else
                {
                    var buyresp = BuyItem(cookieCont, sessid, lotList[0].SellerId, pageLink, lotList[0].Price, lotList[0].SubTotal, currency);
                    BuyNow = false;
                    if (buyresp.Succsess)
                    {
                        doMessage(flag.Success_buy, scanID, buyresp.Mess);
                    }
                    else
                    {
                        doMessage(flag.Error_buy, scanID, buyresp.Mess);
                    }
                }
                return;
            }
            else
            {
                wishedPrice = GetSweetPrice(wishedPrice);
            }

            int wished = Convert.ToInt32(wishedPrice);

            //TODO: Проверка наличия цифр, запятой или точки
            int delay = Convert.ToInt32(scanDelay);
            int prog = 1;

            buyCounter = 0;

            while (worker.CancellationPending == false)
            {
                ParseLotList(GetRequest(pageLink, cookieCont), lotList, currencies);

                if (lotList.Count == 0)
                {
                    doMessage(flag.Price_text, scanID, "Error");
                    continue;
                }

                //Возьмем самый верхний лот со страницы. Он же первый в нашем списке лотов.
                string total = lotList[0].Price;
                int current = Convert.ToInt32(total);
                string prtoTxt = total.Insert(total.Length - 2, ",");

                if (current < wished)
                {
                    if (toBuy)
                    {
                        var buyresp = BuyItem(cookieCont, sessid, lotList[0].SellerId, pageLink, lotList[0].Price, lotList[0].SubTotal, currency);

                        if (buyresp.Succsess)
                        {
                            doMessage(flag.Success_buy, scanID, buyresp.Mess);
                            doMessage(flag.Price_btext, scanID, prtoTxt);
                            buyCounter++;

                            if (buyCounter == BuyQuant)
                            {
                                doMessage(flag.Send_cancel, scanID, string.Empty);
                            }

                        }
                        else
                        {
                            doMessage(flag.Error_buy, scanID, buyresp.Mess);
                        }

                    }
                    else doMessage(flag.Price_htext, scanID, prtoTxt);
                }
                else
                    doMessage(flag.Price_text, scanID, prtoTxt);

                doMessage(flag.Scan_progress, scanID, prog.ToString());
                Sem.WaitOne(delay);
                prog++;
            }

            doMessage(flag.Scan_cancel, scanID, string.Empty);

        }


        }

    }


