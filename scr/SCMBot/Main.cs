using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.ComponentModel;
using System.IO;

// Внимание! Данная наработка - всего-лишь грубая реализация идеи.
// Код содержит множественные ошибки и костыли, бездумно копипастить не советую.
// По вопросам могу ответить, контактные данные ниже.
// email: demmaxx@gmail.com
// icq: 615485


namespace SCMBot
{



    public partial class Main : Form
    {
        SteamSite steam = new SteamSite();

        public Main()
        {
            InitializeComponent();

        }


        private void Main_Load(object sender, EventArgs e)
        {
            steam.delegMessage += new eventDelegate(Event_Message);

            steam.cookieCont = ReadCookiesFromDisk("coockies.dat");
            LoadSettings();
            
            if (checkBox2.Checked)
                loginButton.PerformClick(); 
        }


        private void LoadSettings()
        {
            var settings = Properties.Settings.Default;

            loginBox.Text = settings.lastLogin;
            checkBox2.Checked = settings.loginOnstart;
            passwordBox.Text = Decrypt(settings.lastPass);
            textBox1.Text = settings.delayVal.ToString();

        }


        private void SaveSettings()
        {
            var settings = Properties.Settings.Default;

            settings.lastLogin = loginBox.Text;
            settings.loginOnstart = checkBox2.Checked;
            settings.lastPass = Encrypt(passwordBox.Text);
            settings.delayVal = Convert.ToInt32(textBox1.Text);
            settings.Save();
        }


        public void GetAccInfo(string mess)
        {
            string[] accinfo = mess.Split(';');
            StartLoadImgTread(accinfo[2], pictureBox2);
            label5.Text = accinfo[1];
            label10.Text = accinfo[0];
            ProgressBar1.Visible = false;
            loginButton.Text = "Logout";
        }

        private void Event_Message(object sender, string message, flag myflag)
        {
            switch (myflag)
            {
                case flag.Already_logged:

                    AddtoLog("Already Logged");
                    StatusLabel1.Text = "Already Logged";
                    GetAccInfo(message);
                    break;

                case flag.Login_success:

                    AddtoLog("Login Success");
                    StatusLabel1.Text = "Login Success";
                    GetAccInfo(message);
                    break;

                case flag.Login_cancel:

                    MessageBox.Show("Login has been cancelled. Please try again.", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                    StatusLabel1.Text = "Login cancelled";

                    loginButton.Text = "Login";
                    ProgressBar1.Visible = false;
                    label5.Text = message;
                    break;

                case flag.Logout_:
                    StatusLabel1.Text = "Logouted";
                    loginButton.Text = "Login";
                    break;
                case flag.Price_htext:
                    AppendText(richTextBox1, message, true);
                    break;
                case flag.Price_text:
                    AppendText(richTextBox1, message, false);
                    break;

                case flag.Rep_progress:
                    ProgressBar1.Value = Convert.ToInt32(message);
                    break;

                case flag.Scan_progress:
                    StatusLabel1.Text = "Prices Loaded: "+ message;
                    break;

                case flag.Success_buy:
                    StatusLabel1.Text = "Item bought";
                    label5.Text = message;
                    break;

                case flag.Scan_cancel:
                    StatusLabel1.Text = "Scan Cancelled";
                    break;
                case flag.Search_success:
                    comboBox1.Items.Clear();
                    StatusLabel1.Text = "Finded: " + steam.searchList.Count.ToString();
                    for (int i = 0; i < steam.searchList.Count; i++)
                    {
                        var ourItem = steam.searchList[i];
                        comboBox1.Items.Add(string.Format("Type: {0}, Item: {1}", ourItem.Game, ourItem.Name));

                    }
                    comboBox1.DroppedDown = true;
                    break;
            }

        }


        public void StartLoadImgTread(string imgUrl, PictureBox picbox)
        {
            ThreadStart threadStart = delegate() { loadImg(imgUrl, picbox, true); };
            Thread pTh = new Thread(threadStart);
            pTh.IsBackground = true;
            pTh.Start();

        }


        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var ourItem = steam.searchList[comboBox1.SelectedIndex];
            labelQuant.Text = ourItem.Quant;
            labelStPrice.Text = ourItem.StartPrice;
            wishpriceBox.Text = ourItem.StartPrice;
            textBox5.Text = ourItem.Link;
            textBox5.Select(textBox5.Text.Length, 0);
            StartLoadImgTread(ourItem.ImgLink, pictureBox1);

        }


        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            WriteCookiesToDisk("coockies.dat", steam.cookieCont);
            SaveSettings();
        }


        private void loginButton_Click(object sender, EventArgs e)
        {
            if (!steam.Logged)
            {
                if (steam.LoginProcess)
                {
                    steam.CancelLogin();
                }
                else
                {
                    steam.UserName = loginBox.Text;
                    steam.Password = passwordBox.Text;
                    steam.Login();
                    ProgressBar1.Visible = true;
                    StatusLabel1.Text = "Try Login...";
                    loginButton.Text = "Cancel";
                }
            }
            else
                steam.Logout();

        }


        private void button4_Click(object sender, EventArgs e)
        {
            steam.scanDelay = textBox1.Text;
            steam.wishedPrice = wishpriceBox.Text;
            steam.pageLink = textBox5.Text;
            steam.ScanPrices();

            StatusLabel1.Text = "Scanning Prices...";

           // if (prload.IsBusy != true)
           // {
           //     prload.RunWorkerAsync();
          //  }
        }


        private void button5_Click(object sender, EventArgs e)
        {
            steam.CancelScan();
        }


        public void AddToFormTxt(string acc)
        {
            this.Text += string.Format(" ({0})", acc);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            steam.reqTxt = comboBox1.Text;
            steam.linkTxt = SteamSite._search;
            steam.reqLoad();
        }


   }
}
