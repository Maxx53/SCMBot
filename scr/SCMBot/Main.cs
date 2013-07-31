using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.ComponentModel;

// Внимание! Данная наработка - всего-лишь грубая реализация идеи.
// Код содержит множественные ошибки и костыли, бездумно копипастить не советую.
// По вопросам могу ответить, контактные данные ниже.
// email: demmaxx@gmail.com
// icq: 615485


namespace SCMBot
{

    public partial class Main : Form
    {

        CookieContainer cookie = new CookieContainer();
        List<LotData> lotList = new List<LotData>();
        private BackgroundWorker bw = new BackgroundWorker();
        private BackgroundWorker prload = new BackgroundWorker();

        const string _comlog = "https://steamcommunity.com/login/";
        const string _ref = _comlog + "home/?goto=market%2F";
        const string _getrsa = _comlog + "getrsakey/";
        const string _dologin = _comlog + "dologin/";
        const string _market = "http://steamcommunity.com/market/";
        const string _blist = _market + "buylisting/";
        const string _lists = _market + "listings/";
        const string _capcha = "https://steamcommunity.com/public/captcha.php?gid=";
        const string loginReq = "password={0}&username={1}&emailauth={2}&loginfriendlyname={3}&captchagid={4}&captcha_text={5}&emailsteamid={6}&rsatimestamp={7}";
        const string loginStr = "steamid={0}&token={1}&remember_login=false&webcookie={2}";
        const string _logFile = "logfile.txt";
        const string appName = "SCM Bot alpha";

        public class LotData
        {
            public LotData(string sellerId, string price, string feeprice)
            {
                this.SellerId = sellerId;
                this.Price = price;
                this.FeePrice = feeprice;
            }
            public string SellerId { set; get; }
            public string Price { set; get; }
            public string FeePrice { set; get; }
        }

        public Main()
        {
            InitializeComponent();

            bw.WorkerReportsProgress = true;
            bw.WorkerSupportsCancellation = true;
            bw.DoWork += new DoWorkEventHandler(bw_DoWork);
            bw.ProgressChanged += new ProgressChangedEventHandler(bw_ProgressChanged);
            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);


            prload.WorkerSupportsCancellation = true;
            prload.DoWork += new DoWorkEventHandler(prload_DoWork);
 
        }


        private void Main_Load(object sender, EventArgs e)
        {
            cookie = ReadCookiesFromDisk("coockies.dat");
            if (checkBox2.Checked)
                loginButton.PerformClick();
        }


        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            WriteCookiesToDisk("coockies.dat", cookie);
        }


        public void ParseLotList(string content)
        {
            lotList.Clear();

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
                    lotList.Add(new LotData(sellid, parts[0], parts[1]));
                }
            }
            else MessageBox.Show("Не удалось загрузить список предметов!", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error); ;

        }


        private void loginButton_Click(object sender, EventArgs e)
        {
            loginButton.Text = "Try Login...";
            AddtoLog("Try Login...");
            this.Text = "SCM Bot alpha";

            loginButton.Enabled = false;

            if (bw.IsBusy != true)
            {
                bw.RunWorkerAsync();
            }
        }


        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            ProcessLogin(loginBox.Text, passwordBox.Text, cookie, sender, e);
        }
        

        private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if ((e.Cancelled == true))
            {
                MessageBox.Show("Login has been cancelled. Please try again!", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                loginButton.Text = "Login";
                loginButton.Enabled = true;
                progressBar1.Value = 0;
            }

            else if (!(e.Error == null))
            {
                MessageBox.Show(e.Error.Message, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            else
            {
                progressBar1.Visible = false;
            }
            
        }


        private void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
        }


        private void button4_Click(object sender, EventArgs e)
        {
            if (prload.IsBusy != true)
            {
                prload.RunWorkerAsync();
            }
        }


        private void button5_Click(object sender, EventArgs e)
        {
            if (prload.WorkerSupportsCancellation == true)
            {
                prload.CancelAsync();
                
            }
        }


        public static void AppendText(RichTextBox box, string text, Color color)
        {
            box.SelectionStart = box.TextLength;
            box.SelectionLength = 0;

            box.SelectionColor = color;
            box.AppendText(text);
            box.SelectionColor = box.ForeColor;

            box.SelectionStart = box.Text.Length + 1;
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


        public void prload_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
      
            //Небольшая проблема с форматом цены, обязательно указывать копейки после запятой или точки.
            int wished = Convert.ToInt32(Regex.Replace(wishpriceBox.Text, @"[d\.\,]+", string.Empty));
           
            string sessid = string.Empty;

                
            for (int i = 0; i < cookie.Count; i++)
            {
                string tempcook = cookie.GetCookies(new Uri("https://steamcommunity.com/"))[i].Value.ToString();
                if (tempcook.Length == 20)
                {
                    sessid = tempcook;
                    break;
                }

            }

            //TODO: Проверка наличия цифр, запятой или точки
            int delay = Convert.ToInt32(textBox1.Text);

            while (worker.CancellationPending == false)
            {
                ParseLotList(SendPostRequest(string.Empty, _lists + textBox5.Text, string.Empty, cookie, false));

                //Возьмем самый верхний лот со страницы. Он же первый в нашем списке лотов.
                string lul = lotList[0].Price;
                int current = Convert.ToInt32(lul);
                if (current < wished)
                {
                    this.Invoke(new MethodInvoker(delegate { AppendText(richTextBox1, DateTime.Now.ToString("HH:mm:ss") + " " + lul.Insert(lul.Length - 2, ",") + " ед.\r\n", Color.Red); }));
                    if (checkBox1.Checked)
                    {
                         string subtotal = Convert.ToInt32(lotList[0].FeePrice).ToString();
                         string total = Convert.ToInt32(lotList[0].Price).ToString();
                         string walletball = BuyItem(cookie, sessid, lotList[0].SellerId, _lists + textBox5.Text, total, subtotal);
                         this.Invoke(new MethodInvoker(delegate { label5.Text = walletball; }));
                    }
                }
                else
                    this.Invoke(new MethodInvoker(delegate { AppendText(richTextBox1, DateTime.Now.ToString("HH:mm:ss") + " " + lul.Insert(lul.Length - 2, ",") + " ед.\r\n", Color.Black); }));

                Thread.Sleep(delay);
            }

        }


        public static void GetNameBalance(CookieContainer cock, out string name, out string balance)
        {
            AddtoLog("Getting account name and balance...");
            string markpage = SendPostRequest(string.Empty, _market, string.Empty, cock, false);
            
            name = string.Empty;
            balance = string.Empty;

                string parseName = Regex.Match(markpage, "(?<=steamcommunity.com/id/)(.*)(?<=\"><img)").ToString();

                int nlength = parseName.Length;
                if (nlength != 0)
                {
                    parseName = parseName.Substring(0, nlength - 6);
                    name = parseName;
                }

                string parseAmount = Regex.Match(markpage, "(?<=marketWalletBalanceAmount\">)(.*)(?<=</span>)").ToString();

                int blength = parseAmount.Length;
                if (blength != 0)
                {
                    parseAmount = parseAmount.Substring(0, parseAmount.IndexOf(" "));
                    balance = parseAmount;
                }
         
        }


        public void AddToFormTxt(string acc)
        {
            this.Text += string.Format(" ({0})", acc);
        }

        public void ProcessLogin(string login, string pass, CookieContainer cock, object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            worker.ReportProgress(20);

            string acc = string.Empty;
            string balance = string.Empty;

            GetNameBalance(cock, out acc, out balance);

            if (acc != string.Empty)
            {
                AddtoLog("Already Logged");
                this.Invoke(new MethodInvoker(delegate
                {
                    AddToFormTxt(acc);
                    label5.Text = balance;
                }));

                return;
            }

            begin:

            string log_content = SendPostRequest("username=" + login, _getrsa, _ref, cock, true);
            if (log_content == string.Empty)
            {
                e.Cancel = true;
                return;
            }

            worker.ReportProgress(40);

            Dictionary<string, string> serialLogin = ParseJson(log_content);

            string modval = serialLogin["publickey_mod"];
            string expval = serialLogin["publickey_exp"];
            string timeStamp = serialLogin["timestamp"];

            string finalpass = EncryptPassword(log_content, pass, modval, expval);

            string capchaId = string.Empty;
            string steamId = string.Empty;


            string firstTry = SendPostRequest(string.Format(loginReq, finalpass, login, string.Empty, string.Empty, capchaId,
                                                            string.Empty, steamId, timeStamp), _dologin, _ref, cock, true);
            worker.ReportProgress(60);
            serialLogin.Clear();

            Dictionary<string, string> serialCheck = ParseJson(firstTry);

            if (serialCheck.ContainsKey("message"))
            {

                Dialog guardCheckForm = new Dialog();


                if (serialCheck["message"] == "SteamGuard")
                {
                   // guardCheckForm.capchgroupEnab = false;
                    steamId = serialCheck["emailsteamid"];

                }
                else if (serialCheck["message"] == "Error verifying humanity")
                {
                   // guardCheckForm.codgroupEnab = false;
                    capchaId = serialCheck["captcha_gid"];
                    guardCheckForm.capchImg = LoadImage(_capcha + capchaId);
                }
                //else MessageBox.Show(serialCheck["message"]);

                worker.ReportProgress(80);

                if (guardCheckForm.ShowDialog() == DialogResult.OK)
                {

                    string secondTry = SendPostRequest(string.Format(loginReq, finalpass, login, guardCheckForm.MailCode, guardCheckForm.GuardDesc, capchaId,
                                                           guardCheckForm.capchaText, steamId, timeStamp), _dologin, _ref, cock, true);

                    Dictionary<string, string> serialFinal = ParseJson(secondTry);

                    if (serialFinal.ContainsKey("login_complete"))
                    {
                        worker.ReportProgress(100);


                        string ball = string.Empty;
                        string name = string.Empty;
                        GetNameBalance(cock, out name, out ball);

                        this.Invoke(new MethodInvoker(delegate
                        {
                            label5.Text = ball;
                            AddToFormTxt(name);
                        }));
                        AddtoLog("Login Success");
                    }
                    else if (serialFinal["message"] == "SteamGuard")

                    {
                        //TODO: Разобрать кашу, выкинуть goto
                        goto begin;
                    }

                    else
                    {
                        AddtoLog("Login Problem");
                        e.Cancel = true;
                    }
                }
                else
                {
                    AddtoLog("Login Guard Check Cancelled");
                    e.Cancel = true;
                }

                guardCheckForm.Dispose();

            }
            else if (serialCheck.ContainsKey("login_complete"))
            {
                worker.ReportProgress(100);

                string ball = string.Empty;
                string name = string.Empty;
                GetNameBalance(cock, out name, out ball);

                this.Invoke(new MethodInvoker(delegate
                {
                    label5.Text = ball;
                    AddToFormTxt(name);
                }));
                AddtoLog("Login Success");
            }
            else
            {
                AddtoLog("Login Guard Check Cancelled");
                e.Cancel = true;
            }

            serialCheck.Clear();
        }

   }
}
