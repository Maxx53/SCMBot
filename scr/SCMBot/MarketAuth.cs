using System;
using System.Net;
using System.ComponentModel;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Globalization;

namespace SCMBot
{
    public class MarketAuth
    {
        //Properies
        public string UserName { set; get; }
        public string Password { set; get; }
        public CookieContainer Cookies { set; get; }

        [DefaultValue(false)]
        public bool Logged { get; set; }

        //ReadOnly
        public string AccountName { get { return accountName; } }
        public string WalletStr { get { return walletStr; } }
        public string AvatarLink { get { return avatarLink; } }
        public string UserID { get { return userID; } }
        public string CurrencyCode { get { return currencyCode; } }
        public string CurrencyName { get { return currencyName; } }

        public string Country { get { return country; } }
        public string Language { get { return language; } }

        public string SessionID { get { return sessionID; } }

        public string JsonAddon { get { return jsonAddon; } }

        //Fields
        private string accountName;
        private string userID;
        private string walletStr;
        private string avatarLink;
        private string currencyCode;
        private string currencyName;
        private string country;
        private string language;
        private string sessionID;
        private string jsonAddon;

        private BackgroundWorker loginThread = new BackgroundWorker();

        public event LoginMessagesHandler LoginMessages;
        public delegate void LoginMessagesHandler(object obj, MyEventArgs e);

        
        public MarketAuth()
        {
            loginThread.WorkerSupportsCancellation = true;
            loginThread.DoWork += new DoWorkEventHandler(loginThread_DoWork);
            loginThread.RunWorkerCompleted += new RunWorkerCompletedEventHandler(loginThread_RunWorkerCompleted);
            Cookies = new CookieContainer();
        }


        private Steam.RespRSA GetRSA()
        {
            return JsonConvert.DeserializeObject<Steam.RespRSA>(SendPost("username=" + UserName, Steam.getRsaUrl, Steam.loginRef));
        }

        
        public bool GetNameBalance()
        {
            Main.AddtoLog("Getting account name and balance...");

            string markpage = SendGet(Steam.marketSSL);

            accountName = Regex.Match(markpage, "(?<=buynow_dialog_myaccountname\">)(.*)(?=</span>)").ToString().Trim();

            if (accountName == "")
            {
                return false;
            }
            else
            {
                userID = Regex.Match(markpage, "(?<=g_steamID = \")(.*)(?=\";)").ToString();
                avatarLink = Regex.Match(markpage, "(?<=avatarIcon\">" + @"\r\n\s+" + "<img src=\")(.*?)(?=\" srcset=\")", RegexOptions.Singleline).ToString();
                walletStr = Regex.Match(markpage, "(?<=marketWalletBalanceAmount\">)(.*)(?=</span>)").ToString();
                country = Regex.Match(markpage, "(?<=g_strCountryCode = \")(.*)(?=\";)").ToString();
                language = Regex.Match(markpage, "(?<=g_strLanguage = \")(.*)(?=\";)").ToString();
                currencyCode = Regex.Match(markpage, "(?<=wallet_currency\":)(.*)(?=,\"wallet_country)").ToString().Trim();

                //"?country={0}&language={1}&currency={2}";
                jsonAddon = string.Format(Steam.jsonAddonUrl, country, language, currencyCode);

                currencyName = Regex.Replace(walletStr, @"\d+[,.]*\d{1,2}", string.Empty).Trim();

                var ci = CultureInfo.InvariantCulture.Clone() as CultureInfo;
                ci.NumberFormat.NumberDecimalSeparator = ",";

                decimal walletBalance = Steam.ToDecimal(CleanPrice(walletStr));

                //?country=RU&language=russian&currency=5&count=20
                //string Addon = string.Format(jsonAddonUrl, country, strlang, Main.CurrCode);

                sessionID = GetSessId();

                return true;
            }
        }

        private string GetSessId()
        {
            //sessid sample MTMyMTg5MTk5Mw%3D%3D
            string resId = string.Empty;
            var stcook = Cookies.GetCookies(new Uri(Steam.site));

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


        public string CleanPrice(string input)
        {
            //return Regex.Replace(input, currencyName, string.Empty).Trim();
            return Regex.Match(input, @"\d+([,.]*\d{1,2})?").ToString();
        }

        public void Login()
        {
            if (loginThread.IsBusy != true)
            {
                loginThread.RunWorkerAsync();
            }
            else
                loginThread.CancelAsync();
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
            ThreadStart threadStart = delegate()
            {
                  string data = "sessionid=" + GetSessId();
                  SendPost(data, Steam.logoutUrl, Steam.marketSSL);
                  fireMessage(2, 0, "Logouted");
                  Logged = false;

            };
            Thread pTh = new Thread(threadStart);
            pTh.IsBackground = true;
            pTh.Start();
        }


        private void fireMessage(int code, int percent, string message)
        {
            LoginMessages(this, new MyEventArgs(code, percent, message));
        }

        private void loginThread_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            Logged = false;

            fireMessage(0, 10, "Start Login");

            if (GetNameBalance())
            {
                fireMessage(1, 0, "Already Logged");
                Logged = true;
                return;
            }


            string mailCode = string.Empty;
            string guardDesc = string.Empty;
            string capchaId = string.Empty;
            string capchaTxt = string.Empty;
            string mailId = string.Empty;
            string twoFactorCode = string.Empty;


        //Login cycle

        begin:

            if (worker.CancellationPending)
            {
                e.Cancel = true;
                return;
            }

            fireMessage(0, 20, "Accessing to server...");

            var rRSA = GetRSA();

            if (rRSA == null)
            {
                Main.AddtoLog("Network Problem");
                fireMessage(2, 0, "Network Problem");

                return;
            }

            if (worker.CancellationPending)
            {
                e.Cancel = true;
                return;
            }

            fireMessage(0, 40, "Sending Account Data...");

            string finalpass = Steam.EncryptPassword(Password, rRSA.Module, rRSA.Exponent);

            string MainReq = string.Format(Steam.loginReq, finalpass, UserName, mailCode, guardDesc, capchaId,
                                                                          capchaTxt, mailId, rRSA.TimeStamp, twoFactorCode);
            string BodyResp = SendPost(MainReq, Steam.doLoginUrl, Steam.loginRef);

            fireMessage(0, 60, "Processing Messages...");


            if (worker.CancellationPending)
            {
                e.Cancel = true;
                return;
            }

            //Checking login problem
            if (BodyResp.Contains("message"))
            {
                var rProcess = JsonConvert.DeserializeObject<Steam.RespProcess>(BodyResp);

                //Checking Incorrect Login
                if (rProcess.Message.Contains("Incorrect"))
                {
                    Main.AddtoLog("Incorrect login");
                    fireMessage(2, 0, "Incorrect login");
                    return;
                }
                else
                {
                    //Login correct, checking message type...

                    Dialog guardCheckForm = new Dialog();

                    if ((rProcess.isCaptcha) && (rProcess.Message.Contains("humanity")))
                    {
                        //Verifying humanity, loading capcha
                        guardCheckForm.capchgroupEnab = true;
                        guardCheckForm.codgroupEnab = false;
                        guardCheckForm.factorgroupEnab = false;

                        string newcap = Steam.capUrl + rProcess.Captcha_Id;
                        Main.loadImg(newcap, guardCheckForm.capchImg, false, false);
                    }
                    else
                        if (rProcess.isTwoFactor)
                        {
                            //Steam wants two factor code
                            guardCheckForm.capchgroupEnab = false;
                            guardCheckForm.codgroupEnab = false;
                            guardCheckForm.factorgroupEnab = true;
                        }
                        else
                            if (rProcess.isEmail)
                            {
                                //Steam guard wants email code
                                guardCheckForm.capchgroupEnab = false;
                                guardCheckForm.codgroupEnab = true;
                                guardCheckForm.factorgroupEnab = false;
                            }
                            else
                            {
                                //Whoops!
                                goto begin;
                            }

                    //Re-assign main request values
                    if (guardCheckForm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        mailCode = guardCheckForm.MailCode;
                        guardDesc = guardCheckForm.GuardDesc;
                        twoFactorCode = guardCheckForm.TwoFactorCode;
                        capchaId = rProcess.Captcha_Id;
                        capchaTxt = guardCheckForm.CapchaText;
                        mailId = rProcess.Email_Id;
                        guardCheckForm.Dispose();
                    }
                    else
                    {
                        Main.AddtoLog("Dialog has been cancelled");
                        fireMessage(2, 0, "Dialog has been cancelled");

                        guardCheckForm.Dispose();
                        return;

                    }

                    goto begin;
                }

            }
            else
            {
                if (worker.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }

                //No Messages, Success!
                var rFinal = JsonConvert.DeserializeObject<Steam.RespFinal>(BodyResp);

                fireMessage(0, 100, "Getting Info and Balance...");

                if (rFinal.Success && rFinal.isComplete)
                {
                    //Okay
                    if (GetNameBalance())
                    {

                        Main.AddtoLog("Login Success");

                        fireMessage(1, 0, "Login Success");
                       
                        Logged = true;
                    }
                    else
                        //Fail
                        goto begin;
                }
                else
                {
                    //Fail
                    goto begin;
                }
            }

        }


        private void loginThread_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                Logged = false;
                fireMessage(2, 0, "Login calcelled");
            }
        }

        public void ChangeLang(string lang)
        {
            ThreadStart threadStart = delegate()
            {
                SendPost(string.Format(Steam.langReq, lang, GetSessId()), Steam.langChangeUrl, Steam.loginRef);
                fireMessage(4, 0, "Language changed");
            };
            Thread pTh = new Thread(threadStart);
            pTh.IsBackground = true;
            pTh.Start();
        }

        public string SendPost(string data, string url, string refer)
        {
            Main.reqPool.WaitOne();

            fireMessage(3, 0, string.Empty);
            string result = Main.SendPostRequest(data, url, refer, Cookies);
            fireMessage(3, 1, string.Empty);

            Main.reqPool.Release();

            return result;
        }

        public string SendGet(string url)
        {
            Main.reqPool.WaitOne();

            fireMessage(3, 0, string.Empty);
            var res = Main.GetRequest(url, Cookies);
            fireMessage(3, 1, string.Empty);

            if (Main.ReqDelay > 0)
                Main.reqPool.WaitOne(Main.ReqDelay);

            Main.reqPool.Release();

            return res;
        }




    }

}
