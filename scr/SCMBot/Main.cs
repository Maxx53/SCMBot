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
        SteamSite steam = new SteamSite();

        public Main()
        {
            InitializeComponent();

        }


        private void Main_Load(object sender, EventArgs e)
        {
            steam.delegMessage += new eventDelegate(Event_Message);

            steam.cookieCont = ReadCookiesFromDisk("coockies.dat");
            
            if (checkBox2.Checked)
                loginButton.PerformClick(); 
        }


        private void Event_Message(object sender, string message, flag myflag)
        {
            switch (myflag)
            {
                case flag.Already_logged:

                    AddtoLog("Already Logged");
                    StatusLabel1.Text = "Already Logged";

                    label5.Text = message;
                    ProgressBar1.Visible = false;
                    break;

                case flag.Login_success:

                    StatusLabel1.Text = "Login Success";
                    ProgressBar1.Visible = false;
                    label5.Text = message;
                    break;

                case flag.Login_cancel:

                    MessageBox.Show("Login has been cancelled. Please try again.", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                    StatusLabel1.Text = "Login cancelled";

                    loginButton.Enabled = true;
                    ProgressBar1.Visible = false;
                    label5.Text = message;
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
            }

        }



        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            WriteCookiesToDisk("coockies.dat", steam.cookieCont);
        }



        private void loginButton_Click(object sender, EventArgs e)
        {
            
            steam.UserName = loginBox.Text;
            steam.Password = passwordBox.Text;
            steam.Login();

            ProgressBar1.Visible = true;
            StatusLabel1.Text = "Try Login...";

            this.Text = "SCM Bot alpha";

            loginButton.Enabled = false;

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

 

   }
}
