using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.Text.RegularExpressions;

// Внимание! Данная наработка - всего-лишь грубая реализация идеи.
// Код содержит множественные ошибки и костыли, бездумно копипастить не советую.
// По вопросам могу ответить, контактные данные ниже.
// email: demmaxx@gmail.com
// icq: 615485


namespace SCMBot
{
    public partial class Main : Form
    {
        SteamSite steam_srch = new SteamSite();
        SearchPagePos sppos;
        string lastSrch;
        bool addonComplete = false;
        ItemComparer itemComparer = new ItemComparer();

        List<ScanItem> ScanItLst = new List<ScanItem>();
        List<SteamSite> SteamLst = new List<SteamSite>();

        public Main()
        {
            InitializeComponent();
            steam_srch.delegMessage += new eventDelegate(Event_Message);
         }


        private void Main_Load(object sender, EventArgs e)
        {
            
            steam_srch.cookieCont = ReadCookiesFromDisk("coockies.dat");

            InventoryList.ListViewItemSorter = itemComparer;

            LoadSettings();
            
            if (checkBox2.Checked)
                loginButton.PerformClick();

            //temp
            comboBox3.SelectedIndex = 0;
        }

        private void addTabItem(string link, string price, string tabId, string imgLink)
        {
            if (tabId == string.Empty)
            tabControl1.TabPages.Add("Item " + (tabControl1.TabCount + 1).ToString());
            else
                tabControl1.TabPages.Add(tabId);

            ScanItem scanIt = new ScanItem();
            scanIt.Height = tabControl1.Height - 20;
            scanIt.Width = tabControl1.Width - 5;
            scanIt.Anchor = (AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom | AnchorStyles.Left);
            tabControl1.TabPages[tabControl1.TabCount - 1].Controls.Add(scanIt);
            scanIt.linkValue = link;
            scanIt.wishedValue = price;
            scanIt.ImgLink = imgLink;
            scanIt.ButtonClick += new EventHandler(ScanItemButton_Click);
            ScanItLst.Add(scanIt);

            SteamSite ScanSite = new SteamSite();
            ScanSite.delegMessage += new eventDelegate(Event_Message);
            ScanSite.cookieCont = steam_srch.cookieCont;
            SteamLst.Add(ScanSite);
        }

        private void LoadSettings()
        {
            var settings = Properties.Settings.Default;

            loginBox.Text = settings.lastLogin;
            checkBox2.Checked = settings.loginOnstart;
            //If you need password crypting
            //passwordBox.Text = Decrypt(settings.lastPass);
            passwordBox.Text = settings.lastPass;
            comboBox2.SelectedIndex = settings.curComb;
        }


        private void SaveSettings()
        {
            var settings = Properties.Settings.Default;

            settings.lastLogin = loginBox.Text;
            settings.loginOnstart = checkBox2.Checked;
            //If you need password crypting
            //settings.lastPass = Encrypt(passwordBox.Text);
            settings.lastPass = passwordBox.Text;
            settings.curComb = comboBox2.SelectedIndex;
            settings.Save();
        }

        private static void SetButton(Button ourButt, string caption)
        {
            ourButt.Text = caption;
            ourButt.Image = (Image)Properties.Resources.ResourceManager.GetObject(caption.ToLower());
        }

        public void GetAccInfo(string mess)
        {
            if (mess != string.Empty)
            {
                string[] accinfo = mess.Split(';');
                StartLoadImgTread(accinfo[2], pictureBox2);
                label5.Text = accinfo[1];
                label10.Text = accinfo[0];
                ProgressBar1.Visible = false;
                SetButton(loginButton, "Logout");
            }
            else
                MessageBox.Show("wtf?");

        }

        public string GetPriceFormat(string mess)
        {
            return string.Format("{0} {1} {2}", DateTime.Now.ToString("HH:mm:ss"), mess, "units");
        }
        


        private void Event_Message(object sender, string message, int searchId, flag myflag)
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

                    SetButton(loginButton, "Login");
                    ProgressBar1.Visible = false;
                    label5.Text = message;
                    break;

                case flag.Logout_:
                    StatusLabel1.Text = "Logouted";
                    SetButton(loginButton, "Login");
                    break;
                case flag.Price_htext:
                    ScanItLst[searchId].lboxAdd(GetPriceFormat(message), 1);
                    break;
                case flag.Price_btext:
                    ScanItLst[searchId].lboxAdd(GetPriceFormat(message), 2);
                    break;
                case flag.Price_text:
                    ScanItLst[searchId].lboxAdd(GetPriceFormat(message), 0);
                    break;

                case flag.Rep_progress:
                    ProgressBar1.Value = Convert.ToInt32(message);
                    break;

                case flag.Scan_progress:
                    //StatusLabel1.Text = "Prices Loaded: "+ message;
                    //Nonsense
                    break;

                case flag.Success_buy:
                    StatusLabel1.Text = "Item bought";
                    label5.Text = message;
                    buyNowButton.Enabled = true;
                    break;

                case flag.Scan_cancel:
                    StatusLabel1.Text = "Scan Cancelled";
                    break;

                case flag.Items_Sold:
                    StatusLabel1.Text = "Items on Sale";
                    ProgressBar1.Visible = false;
                    steam_srch.loadInventory();
                    break;

                case flag.Sell_progress:
                    ProgressBar1.Value = Convert.ToInt32(message);
                    break;
                case flag.Search_success:

                    comboBox1.Items.Clear();
                    StatusLabel1.Text = string.Format("Found: {0}, Shown: {1}", message, steam_srch.searchList.Count.ToString());

                    if (sppos.CurrentPos == 1)
                    {
                        int found = Convert.ToInt32(message);
                        sppos.PageCount = found / 10;
                        if (found % 10 != 0)
                            sppos.PageCount++;

                    }

                    label13.Text = string.Format("{0}/{1}", sppos.CurrentPos.ToString(), sppos.PageCount.ToString());
                   
                    if (sppos.PageCount > 1)
                    {
                        prevButton.Enabled = true;
                        nextButton.Enabled = true;
                    }
                    else
                    {
                        prevButton.Enabled = false;
                        nextButton.Enabled = false;
                    }

                    for (int i = 0; i < steam_srch.searchList.Count; i++)
                    {
                        var ourItem = steam_srch.searchList[i];
                        comboBox1.Items.Add(string.Format("Type: {0}, Item: {1}", ourItem.Game, ourItem.Name));

                    }

                    if (addonComplete)
                    {
                        for (int i = 0; i < steam_srch.searchList.Count; i++)
                        {
                            var currItem = steam_srch.searchList[i];
                            addTabItem(currItem.Link, currItem.StartPrice, currItem.Name, currItem.ImgLink);
                        }
                        addonComplete = false;
                    }
                    else
                    comboBox1.DroppedDown = true;

                    searchButton.Enabled = true;

                    break;

                case flag.Inventory_Loaded:
                    InventoryList.Items.Clear();
                    label4.Text = message;
                    InventoryList.Groups[0].Header = string.Format("In Inventory ({0})", steam_srch.inventList.Count);
                    for (int i = 0; i < steam_srch.inventList.Count; i++)
                    {
                        var ourItem = steam_srch.inventList[i];
                        string[] row = { string.Empty, ourItem.Type, ourItem.Name, ourItem.Price };
                        var lstItem = new ListViewItem(row);
                        lstItem.Group = InventoryList.Groups[0];
                        InventoryList.Items.Add(lstItem);
                    }

                    SetColumnWidths(InventoryList, true);
                    break;
            }

        }


        private void addtoScan_Click(object sender, EventArgs e)
        {
            contextMenuStrip1.Show(this.addtoScan, new Point(0, this.addtoScan.Height)); 
        }


        bool searchRight()
        {
            return (comboBox1.Items.Count != 0 && steam_srch.searchList.Count != 0 && comboBox1.SelectedIndex > -1);
        }

        private void searchFirstMess()
        {
            MessageBox.Show("Try to find and choose an Item first", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void fullSetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //if (fullsetFinded)
            //{}
            if (searchRight())
            {
                comboBox1.Text = string.Format("\"{0}\"", steam_srch.searchList[comboBox1.SelectedIndex].Game);
                searchButton.PerformClick();

                addonComplete = true;
            }
            else
                searchFirstMess();
        }


        private void buyNowButton_Click(object sender, EventArgs e)
        {
            if (searchRight())
            {
                var ourItem = steam_srch.searchList[comboBox1.SelectedIndex];
                steam_srch.BuyNow = true;
                steam_srch.pageLink = ourItem.Link;
                steam_srch.ScanPrices();
                buyNowButton.Enabled = false;
                StatusLabel1.Text = "Buying \"" + ourItem.Name + "\" in process...";
            }
            else 
                searchFirstMess(); 

        }

        private void selectedItemToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (searchRight())
            {
                var ourItem = steam_srch.searchList[comboBox1.SelectedIndex];
                addTabItem(ourItem.Link, ourItem.StartPrice, ourItem.Name, ourItem.ImgLink);
                tabControl1.SelectedIndex = tabControl1.TabCount - 1;
            }
            else
                searchFirstMess();
        }


        private void emptyTabToolStripMenuItem_Click(object sender, EventArgs e)
        {
            addTabItem(string.Empty, string.Empty, string.Empty, string.Empty);
        }


        private void doSearch(byte type)
        {
            switch (type)
            {
                case 0:
                    sppos = new SearchPagePos(0, 1);
                    lastSrch = comboBox1.Text;
                    break;
                case 1:
                    if (sppos.CurrentPos < sppos.PageCount)
                        sppos.CurrentPos++;
                    else sppos.CurrentPos = 1;

                    break;
                case 2:
                    if (sppos.CurrentPos > 1)
                        sppos.CurrentPos--;
                    else sppos.CurrentPos = sppos.PageCount;
                    break;
                default:
                    break;
            }

            steam_srch.reqTxt = string.Format("{0}&start={1}0", lastSrch, sppos.CurrentPos - 1);
            steam_srch.linkTxt = SteamSite._search;
            steam_srch.reqLoad();
        }

        private void searchButton_Click(object sender, EventArgs e)
        {
            doSearch(0);
            searchButton.Enabled = false;
        }


        private void nextButton_Click(object sender, EventArgs e)
        {
            doSearch(1);
            nextButton.Enabled = false; 

        }

        private void prevButton_Click(object sender, EventArgs e)
        {
            doSearch(2);
            prevButton.Enabled = false;
        }

        public static void StartLoadImgTread(string imgUrl, PictureBox picbox)
        {
            ThreadStart threadStart = delegate() { loadImg(imgUrl, picbox, true, false); };
            Thread pTh = new Thread(threadStart);
            pTh.IsBackground = true;
            pTh.Start();

        }


        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var ourItem = steam_srch.searchList[comboBox1.SelectedIndex];
            labelQuant.Text = ourItem.Quant;
            labelStPrice.Text = ourItem.StartPrice;

            //addTabItem(ourItem.Link, ourItem.StartPrice, ourItem.Name, ourItem.ImgLink);

           // tabControl1.SelectedIndex = tabControl1.TabCount - 1;

            StartLoadImgTread(ourItem.ImgLink, pictureBox1);

        }


        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            WriteCookiesToDisk("coockies.dat", steam_srch.cookieCont);
            SaveSettings();
        }


        private void loginButton_Click(object sender, EventArgs e)
        {
            if (!steam_srch.Logged)
            {
                if (steam_srch.LoginProcess)
                {
                    //TODO: adequate login cancellation!
                    steam_srch.CancelLogin();
                }
                else
                {
                    steam_srch.UserName = loginBox.Text;
                    steam_srch.Password = passwordBox.Text;
                    steam_srch.Login();
                    ProgressBar1.Visible = true;
                    StatusLabel1.Text = "Try Login...";
                    SetButton(loginButton, "Cancel");
                }

            }
            else
                steam_srch.Logout();

        }

        public static string GetCurrency(int comboItm)
        {
            //1 for USD, 2 for GBP, 3 for EUR, 5 for RUB
            string output = "5";

            switch (comboItm)
            {
                case 0:
                    output = "1";
                    break;
                case 1:
                    output = "2";
                    break;
                case 2:
                    output = "3";
                    break;
                case 3:
                    output = "5";
                    break;
            }
            return output;
        }

        public void ScanItemButton_Click(object sender, EventArgs e)
        {
            var scanItem = ScanItLst[tabControl1.SelectedIndex];
            var steam = SteamLst[tabControl1.SelectedIndex];

            if (!steam.scaninProg)
            {
                int num;
                
                if (Int32.TryParse(scanItem.delayValue, out num) && scanItem.wishedValue != string.Empty && scanItem.linkValue.Contains(SteamSite._market))
                {
                    steam.scanDelay = scanItem.delayValue;
                    steam.wishedPrice = scanItem.wishedValue;
                    steam.pageLink = scanItem.linkValue;
                    steam.toBuy = scanItem.tobuyValue;
                    steam.scanID = tabControl1.SelectedIndex;
                    steam.currency = GetCurrency(comboBox2.SelectedIndex);
                    steam.ScanPrices();

                    StatusLabel1.Text = "Scanning Prices...";
                    scanItem.ButtonText = "Stop";
               }
               else
               {
                    MessageBox.Show("Check your values and try again.", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
               }
            
            }

            else
            {
                steam.CancelScan();
                scanItem.ButtonText = "Start";
            }
        }
        

        public void AddToFormTxt(string acc)
        {
            this.Text += string.Format(" ({0})", acc);
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            if (steam_srch.Logged)
            StartCmdLine(string.Format("{0}/id/{1}", SteamSite._mainsite, label10.Text), string.Empty, false);
        }
        
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            //if (textBox5.Text.Contains(SteamSite._mainsite))
                //StartCmdLine(textBox5.Text, string.Empty, false);
        }

        private void comboBox1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
               //e.SuppressKeyPress = true;
                searchButton.PerformClick();
            }
        }



        private void tabControl1_MouseDown(object sender, MouseEventArgs e)
        {
            var tabControl = sender as TabControl;
            TabPage tabPageCurrent = null;
            if (e.Button == MouseButtons.Middle)
            {
                for (var i = 0; i < tabControl.TabCount; i++)
                {
                    if (!tabControl.GetTabRect(i).Contains(e.Location))
                        continue;
                    tabPageCurrent = tabControl.TabPages[i];
                    break;
                }
                if (tabPageCurrent != null)
                {
                    var k = tabControl.TabPages.IndexOf(tabPageCurrent);
                    SteamLst[k].CancelScan();
                    SteamLst.RemoveAt(k);
                    ScanItLst[k].Dispose();
                    ScanItLst.RemoveAt(k);
                    tabControl.TabPages.Remove(tabPageCurrent);
                    //Update id's
                    for (int i = 0; i < SteamLst.Count; i++)
                    {
                        SteamLst[i].scanID = i;
                    }
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {

            if (steam_srch.Logged)
            {
                steam_srch.invApp = comboBox3.SelectedIndex;
                steam_srch.loadInventory();
            } else
                MessageBox.Show("Please, Login.", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private static string SpliteRemove(string input, char ch)
        {
            string[] arr = input.Split(ch);
            return arr[0] + arr[1].Remove(2, arr[1].Length - 2);
        }

        private static string ConvertToPrice(string input)
        {
            //Sorry for this

            string res = string.Empty;
            input = Regex.Replace(input, @"[^\.\,\d]+", string.Empty);

            if (input != string.Empty)
                if (input.IndexOf(',') != -1)
                {
                    res = SpliteRemove(input, ',');
                }
                else if (input.IndexOf('.') != -1)
                {
                    res = SpliteRemove(input, '.');
                }

                else
                    if (input[0] != 0)
                        res = input + "00";

            return Convert.ToInt32(res).ToString();
        }


        private void SellButton_Click(object sender, EventArgs e)
        {

            if ((steam_srch.inventList.Count != 0) && (textBox1.Text != string.Empty))
            {
                string truePrice = ConvertToPrice(textBox1.Text);
                if (truePrice != string.Empty)
                {
                    StatusLabel1.Text = "Adding to Sale..";
                    ProgressBar1.Value = 0;
                    ProgressBar1.Visible = true;
                    steam_srch.invApp = comboBox3.SelectedIndex;
                    steam_srch.toSellList.Clear();



                    for (int i = 0; i < InventoryList.CheckedItems.Count; i++)
                    {
                        var viewName = InventoryList.CheckedItems[i].SubItems[2].Text;
                        var ouritem = steam_srch.inventList.Find(item => item.Name == viewName);

                        steam_srch.toSellList.Add(new SteamSite.ItemToSell(ouritem.AssetId, truePrice));
                    }

                    steam_srch.ItemSell();
                }
                else MessageBox.Show("Price format is invalid!", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            MessageBox.Show("You need to load Inventory fist.", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void InventoryList_ColumnWidthChanging(object sender, ColumnWidthChangingEventArgs e)
        {
            if (e.ColumnIndex == 0)
            {
                e.NewWidth = this.InventoryList.Columns[0].Width;
                e.Cancel = true;
            }
        }

        private void InventoryList_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            itemComparer.ColumnIndex = e.Column;
            ((ListView)sender).Sort();
        }

        private void InventoryList_Click(object sender, EventArgs e)
        {
            if (InventoryList.SelectedIndices.Count != 0)
            {
                var ourItem = steam_srch.inventList[InventoryList.SelectedIndices[0]];
                StartLoadImgTread(string.Format(SteamSite.invImgUrl, ourItem.ImgLink), pictureBox3);
            }
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            steam_srch.currency = GetCurrency(comboBox2.SelectedIndex);
        }


   }
}
