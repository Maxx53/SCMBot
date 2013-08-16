using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Threading;
using System.IO;

namespace SCMBot
{
    partial class SteamSite
    {
        public string UserName { set; get; }
        public string Password { set; get; }

        public string wishedPrice { set; get; }
        public string scanDelay { set; get; }
        public string pageLink { set; get; }

        public string reqTxt { set; get; }
        public string linkTxt { set; get; }

        public bool Logged { set; get; }
        public bool LoginProcess { set; get; }
        public bool toBuy { set; get; }

        public CookieContainer cookieCont { set; get; }

        public Semaphore Sem = new Semaphore(0, 1);

        private BackgroundWorker loginThread = new BackgroundWorker();
        private BackgroundWorker scanThread = new BackgroundWorker();
        private BackgroundWorker reqThread = new BackgroundWorker();


        public SteamSite()
        {
            loginThread.WorkerSupportsCancellation = true;
            loginThread.DoWork += new DoWorkEventHandler(loginThread_DoWork);

            scanThread.WorkerSupportsCancellation = true;
            scanThread.DoWork += new DoWorkEventHandler(scanThread_DoWork);

            reqThread.WorkerSupportsCancellation = true;
            reqThread.DoWork += new DoWorkEventHandler(reqThread_DoWork);
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
                SendPostRequest(string.Empty, _logout, _market, cookieCont, false);
                doMessage(flag.Logout_, string.Empty);
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
                scanThread.RunWorkerAsync();
            }
        }

        public void CancelScan()
        {
            if (scanThread.WorkerSupportsCancellation == true && scanThread.IsBusy)
            {
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

        private void reqThread_DoWork(object sender, DoWorkEventArgs e)

        {
            ParseSearchRes(SendPostRequest(string.Empty, linkTxt + reqTxt, _market, cookieCont, false), searchList);
            doMessage(flag.Search_success, string.Empty);
        }

        private void loginThread_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            LoginProcess = true;
            Logged = false;
            doMessage(flag.Rep_progress, "20");
            //if (worker.CancellationPending == true)
              //  return;

            string accInfo = GetNameBalance(cookieCont);
            if (accInfo != string.Empty)
            {
                doMessage(flag.Already_logged, accInfo);
                doMessage(flag.Rep_progress, "100");
                LoginProcess = false;
                Logged = true;
                return;
            }

        begin:

            string log_content = SendPostRequest("username=" + UserName, _getrsa, _ref, cookieCont, true);
            if (log_content == string.Empty)
            {
                e.Cancel = true;
                return;
            }

            doMessage(flag.Rep_progress, "40");
          //  if (worker.CancellationPending == true)
             //   return;

            Dictionary<string, string> serialLogin = ParseJson(log_content);

            string modval = serialLogin["publickey_mod"];
            string expval = serialLogin["publickey_exp"];
            string timeStamp = serialLogin["timestamp"];

            string finalpass = EncryptPassword(log_content, Password, modval, expval);

            string capchaId = string.Empty;
            string steamId = string.Empty;


            string firstTry = SendPostRequest(string.Format(loginReq, finalpass, UserName, string.Empty, string.Empty, capchaId,
                                                            string.Empty, steamId, timeStamp), _dologin, _ref, cookieCont, true);
            doMessage(flag.Rep_progress, "60");
           // if (worker.CancellationPending == true)
           //     return;
            serialLogin.Clear();

            Dictionary<string, string> serialCheck = ParseJson(firstTry);

            if (serialCheck.ContainsKey("message"))
            {

                Dialog guardCheckForm = new Dialog();

                if (serialCheck["message"] == "SteamGuard")
                {
                    guardCheckForm.capchgroupEnab = false;
                    steamId = serialCheck["emailsteamid"];
                }
                else if (serialCheck["message"] == "Error verifying humanity")
                {
                    guardCheckForm.codgroupEnab = false;
                    capchaId = serialCheck["captcha_gid"];
                    Main.loadImg(_capcha + capchaId, guardCheckForm.capchImg, false);
                }

                doMessage(flag.Rep_progress, "80");
            //    if (worker.CancellationPending == true)
             //       return;

                if (guardCheckForm.ShowDialog() == DialogResult.OK)
                {

                    string secondTry = SendPostRequest(string.Format(loginReq, finalpass, UserName, guardCheckForm.MailCode, guardCheckForm.GuardDesc, capchaId,
                                                           guardCheckForm.capchaText, steamId, timeStamp), _dologin, _ref, cookieCont, true);

                    Dictionary<string, string> serialFinal = ParseJson(secondTry);

                    if (serialFinal.ContainsKey("login_complete"))
                    {
                        string accInfo2 = GetNameBalance(cookieCont);

                        doMessage(flag.Login_success, accInfo2);
                        doMessage(flag.Rep_progress, "100");
                        Logged = true;
                        Main.AddtoLog("Login Success");
                    }
                    else if (serialFinal["message"] == "SteamGuard")
                    {
                        //TODO: Разобрать кашу, выкинуть goto
                        goto begin;
                    }

                    else
                    {
                        Main.AddtoLog("Login Problem");
                        e.Cancel = true;
                    }
                }
                else
                {
                    Main.AddtoLog("Login Guard Check Cancelled");
                    doMessage(flag.Login_cancel, string.Empty);
                    e.Cancel = true;
                }

                guardCheckForm.Dispose();

            }
            else if (serialCheck.ContainsKey("login_complete"))
            {
                string accInfo3 = GetNameBalance(cookieCont);

                doMessage(flag.Login_success, accInfo3);
                doMessage(flag.Rep_progress, "100");
                Main.AddtoLog("Login Success");
                Logged = true;
            }
            else
            {
                Main.AddtoLog("Login Guard Check Cancelled");
                doMessage(flag.Login_cancel, string.Empty);
                e.Cancel = true;
            }

            serialCheck.Clear();
            LoginProcess = false;
        }


        public void scanThread_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            //Небольшая проблема с форматом цены, обязательно указывать копейки после запятой или точки.
            int wished = Convert.ToInt32(Regex.Replace(wishedPrice, @"[d\.\,]+", string.Empty));

            string sessid = string.Empty;


            for (int i = 0; i < cookieCont.Count; i++)
            {
                string tempcook = cookieCont.GetCookies(new Uri("https://steamcommunity.com/"))[i].Value.ToString();
                if (tempcook.Length == 20)
                {
                    sessid = tempcook;
                    break;
                }
            }

            //TODO: Проверка наличия цифр, запятой или точки
            int delay = Convert.ToInt32(scanDelay);
            int prog = 1;

            while (worker.CancellationPending == false)
            {
                ParseLotList(SendPostRequest(string.Empty, pageLink, string.Empty, cookieCont, false), lotList);

                //Возьмем самый верхний лот со страницы. Он же первый в нашем списке лотов.
                string lul = lotList[0].Price;
                int current = Convert.ToInt32(lul);
                string prtoTxt = lul.Insert(lul.Length - 2, ",");

                if (current < wished)
                {
                    doMessage(flag.Price_htext, prtoTxt);

                    if (toBuy)
                    {
                        string subtotal = Convert.ToInt32(lotList[0].FeePrice).ToString();
                        string total = Convert.ToInt32(lotList[0].Price).ToString();
                        string walletball = BuyItem(cookieCont, sessid, lotList[0].SellerId, pageLink, total, subtotal);
                        doMessage(flag.Success_buy, walletball);
                    }
                }
                else
                    doMessage(flag.Price_text, prtoTxt);

                doMessage(flag.Scan_progress, prog.ToString());
                Sem.WaitOne(delay);
                prog++;
            }

            doMessage(flag.Scan_cancel, string.Empty);
        }

    }
}
