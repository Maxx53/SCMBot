using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.Text.RegularExpressions;

// Внимание! Данная наработка - всего-лишь грубая реализация идеи.
// Код содержит множественные ошибки и костыли, бездумно копипастить не советую.
// Это альфа-версия, совершенно сырой продукт. Не предназначено для продажи, уважай чужой труд!
// Если моя работа вам помогла, материальная помощь приветствуется. 
// Это простимулирует меня на дальнейшее совершенствование кода.
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
        bool isExit = false;
        ItemComparer itemComparer = new ItemComparer();

        List<ScanItem> ScanItLst = new List<ScanItem>();
        SteamSiteLst SteamLst = new SteamSiteLst();
        Settings settingsForm = new Settings();

        public Main()
        {
            InitializeComponent();
            steam_srch.delegMessage += new eventDelegate(Event_Message);
         }

        private void setNotifyText(string mess)
        {
            notifyIcon1.Text = string.Format(notifTxt, appName, settingsForm.loginBox.Text, mess);
        }

        private void Main_Load(object sender, EventArgs e)
        {

            
            steam_srch.cookieCont = ReadCookiesFromDisk("coockies.dat");

            InventoryList.ListViewItemSorter = itemComparer;
            //InventoryList.ListViewItemSorter = itemComparer;
            LoadSettings(true);

            if (settingsForm.checkBox2.Checked)
                loginButton.PerformClick();

            //temp
            comboBox3.SelectedIndex = 0;
            setNotifyText("is not logged");
        }

        private void addTabItem(string link, string price, string tabId, string imgLink, int Delay, int BuyQnt, bool ToBuy)
        {
            string tabName = string.Empty;

            if (tabId == string.Empty)
            {
                tabName = "Item " + (tabControl1.TabCount + 1).ToString();
            }
            else
            {
                tabName = tabId;
            }

            tabControl1.TabPages.Add(tabName);

            ScanItem scanIt = new ScanItem();
            scanIt.ItemName = tabName;
            scanIt.Height = tabControl1.Height - 20;
            scanIt.Width = tabControl1.Width - 5;
            scanIt.Anchor = (AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom | AnchorStyles.Left);
            tabControl1.TabPages[tabControl1.TabCount - 1].Controls.Add(scanIt);
            scanIt.linkValue = link;
            scanIt.wishedValue = price;
            scanIt.ImgLink = imgLink;
            scanIt.delayValue = Delay.ToString();
            scanIt.tobuyQuant = BuyQnt;
            scanIt.tobuyValue = ToBuy;
            scanIt.ButtonClick += new EventHandler(ScanItemButton_Click);
            ScanItLst.Add(scanIt);

            SteamSite ScanSite = new SteamSite();
            ScanSite.delegMessage += new eventDelegate(Event_Message);
            ScanSite.cookieCont = steam_srch.cookieCont;
            SteamLst.Add(ScanSite);
        }

        private void LoadSettings(bool loadtabs)
        {
            var settings = Properties.Settings.Default;

            settingsForm.loginBox.Text = settings.lastLogin;
            settingsForm.checkBox2.Checked = settings.loginOnstart;
            minimizeOnClosingToolStripMenuItem.Checked = settings.minOnClose;

            if (loadtabs)
            {
                LoadTabs(settings.saveTabs);

                if (settings.firstRun == true)
                {
                    MessageBox.Show("It's first time launch. Please, check settings.\r\nYou can find it in system tray anytime.", "Attention!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    settings.firstRun = false;
                    settings.Save();
                    settingsForm.ShowDialog(this);
                }
            }

            //If you need password crypting
            //passwordBox.Text = Decrypt(settings.lastPass);
            settingsForm.passwordBox.Text = settings.lastPass;
        }


        private void LoadTabs(saveTabLst lst)
        {
            if (lst != null)
            {
                for (int i = 0; i < lst.Count; i++)
                {
                    var ourItem = lst[i];
                    addTabItem(ourItem.Link, ourItem.Price, ourItem.Name, ourItem.ImgLink, ourItem.Delay, ourItem.BuyQnt, ourItem.ToBuy);
                }

                tabControl1.SelectedIndex = lst.Position;
            }          
        }

        private void SaveSettings(bool savetabs)
        {
            var settings = Properties.Settings.Default;

            settings.lastLogin = settingsForm.loginBox.Text;
            settings.loginOnstart = settingsForm.checkBox2.Checked;
            settings.minOnClose = minimizeOnClosingToolStripMenuItem.Checked;

            if (savetabs)
                SaveTabs(settings.saveTabs);

            //If you need password crypting
            //settings.lastPass = Encrypt(passwordBox.Text);
            settings.lastPass = settingsForm.passwordBox.Text;
            settings.Save();
        }


        private void SaveTabs(saveTabLst lst)
        {
            if (lst != null)
            {
                lst.Clear();

                if (ScanItLst.Count != 0)
                {
                    for (int i = 0; i < ScanItLst.Count; i++)
                    {
                        var ourItem = ScanItLst[i];
                        lst.Add(new saveTab(ourItem.ItemName, ourItem.linkValue, ourItem.ImgLink, ourItem.wishedValue,
                                                   Convert.ToInt32(ourItem.delayValue), ourItem.tobuyQuant, ourItem.tobuyValue));
                    }
                    lst.Position = tabControl1.SelectedIndex;
                }
            }
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
                string[] accinfo = mess.Split('|');
                StartLoadImgTread(accinfo[2], pictureBox2);
                label5.Text = accinfo[1];
                label10.Text = accinfo[0];
                steam_srch.currency = accinfo[3];
                ProgressBar1.Visible = false;
                SetButton(loginButton, "Logout");
                buyNowButton.Enabled = true;
                addtoScan.Enabled = true;
                SteamLst.CurrencyName = steam_srch.currencies.GetCurrentName();
                setNotifyText("is logged");
            }
            else
                MessageBox.Show("Error loading account information!");

        }

        public string GetPriceFormat(string mess, bool addcurr, string currName)
        {
            string curr = string.Empty;
            if (addcurr)
                curr = currName;
            return string.Format("{0} {1} {2}", DateTime.Now.ToString("HH:mm:ss"), mess, curr);
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

                    MessageBox.Show("Error while Login: " + message, "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                    StatusLabel1.Text = message;

                    SetButton(loginButton, "Login");
                    ProgressBar1.Visible = false;
                    label5.Text = message;
                    break;

                case flag.Logout_:
                    StatusLabel1.Text = "Logouted";
                    SetButton(loginButton, "Login");
                    setNotifyText("is not logged");
                    break;

                case flag.Send_cancel:
                    var scanItem = ScanItLst[searchId];
                    var steam = SteamLst[searchId];
                    steam.CancelScan();
                    scanItem.ButtonText = "Start";
                    break;
                case flag.StripImg:
                    if (searchId == 0)
                        toolStripImage.Image = Properties.Resources.working;
                    else
                        toolStripImage.Image = Properties.Resources.ready;
                    break;

                case flag.Lang_Changed:
                    StatusLabel1.Text = "Language changed to " + message;
                    break;

                case flag.Price_htext:
                    ScanItLst[searchId].lboxAdd(GetPriceFormat(message, true, SteamLst.CurrencyName), 1);
                    break;
                case flag.Price_btext:
                    ScanItLst[searchId].lboxAdd(GetPriceFormat(message, true, SteamLst.CurrencyName), 2);
                    break;
                case flag.Price_text:
                    ScanItLst[searchId].lboxAdd(GetPriceFormat(message, true, SteamLst.CurrencyName), 0);
                    break;

                case flag.Rep_progress:
                    ProgressBar1.Value = Convert.ToInt32(message);
                    break;

                case flag.Scan_progress:
                    StatusLabel1.Text = "Scanning Prices...";
                    break;

                case flag.Success_buy:
                    StatusLabel1.Text = "Item bought";
                    label5.Text = message;
                    buyNowButton.Enabled = true;
                    break;

                case flag.Error_buy:
                    StatusLabel1.Text = "Error while buying!";
                    ScanItLst[searchId].lboxAdd(GetPriceFormat(message, false, string.Empty), 1);
                    buyNowButton.Enabled = true;
                    break;
                case flag.Error_scan:
                    StatusLabel1.Text = "Error while scanning!";
                    ScanItLst[searchId].lboxAdd(GetPriceFormat(message, false, string.Empty), 1);
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


                    string curr = steam_srch.currencies.GetCurrentName();
                    if (steam_srch.currencies.NotSet)
                    {
                        SteamLst.CurrencyName = curr;
                    }

                    FoundList.Items.Clear();

                    for (int i = 0; i < steam_srch.searchList.Count; i++)
                    {
                        var ourItem = steam_srch.searchList[i];

                        string[] row = { string.Empty, ourItem.Game, ourItem.Name, ourItem.StartPrice + " " + curr, ourItem.Quant };
                        var lstItem = new ListViewItem(row);
                        FoundList.Items.Add(lstItem);
                    }

                    if (addonComplete)
                    {
                        for (int i = 0; i < steam_srch.searchList.Count; i++)
                        {
                            var currItem = steam_srch.searchList[i];
                            addTabItem(currItem.Link, currItem.StartPrice, currItem.Name, currItem.ImgLink, 3000, 1, false);
                        }
                        addonComplete = false;
                    }
                    else
                        SetColumnWidths(FoundList, true);


                    searchButton.Enabled = true;

                    break;

                case flag.Inventory_Loaded:
                    InventoryList.Items.Clear();
                    label4.Text = steam_srch.inventList.Count.ToString();
                    int saleCont = Convert.ToInt32(message);
                    InventoryList.Groups[0].Header = string.Format("In Inventory ({0})", steam_srch.inventList.Count - saleCont);
                    InventoryList.Groups[1].Header = string.Format("On Sale ({0})", saleCont);
                    for (int i = 0; i < steam_srch.inventList.Count; i++)
                    {
                        var ourItem = steam_srch.inventList[i];
                        string[] row = { string.Empty, ourItem.Type, ourItem.Name, ourItem.Price };
                        var lstItem = new ListViewItem(row);
                        lstItem.Group = InventoryList.Groups[Convert.ToInt32(ourItem.OnSale)];
                        InventoryList.Items.Add(lstItem);
                    }

                    SetColumnWidths(InventoryList, true);
                    button1.Enabled = true;
                    break;
            }

        }


        private void addtoScan_Click(object sender, EventArgs e)
        {
            contextMenuStrip1.Show(this.addtoScan, new Point(0, this.addtoScan.Height)); 
        }


        bool searchRight()
        {
            return (FoundList.Items.Count != 0 && steam_srch.searchList.Count != 0 && FoundList.CheckedItems.Count > -1);
        }

        private void searchFirstMess()
        {
            MessageBox.Show("Try to find and choose an Item first", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void fullSetToolStripMenuItem_Click(object sender, EventArgs e)
        {

            if (searchRight())
            {
                searchBox.Text = string.Format("\"{0}\"", steam_srch.searchList[FoundList.SelectedItems[0].Index].Game);
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
                var ourItem = steam_srch.searchList[FoundList.SelectedItems[0].Index];
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


                for (int i = 0; i < FoundList.CheckedItems.Count; i++)
                {
                    var viewName = FoundList.CheckedItems[i].SubItems[2].Text;
                    var ourItem = steam_srch.searchList.Find(item => item.Name == viewName);
                    addTabItem(ourItem.Link, ourItem.StartPrice, ourItem.Name, ourItem.ImgLink, 3000, 1, false);
                    tabControl1.SelectedIndex = tabControl1.TabCount - 1;
                }

            }
            else
                searchFirstMess();
        }


        private void emptyTabToolStripMenuItem_Click(object sender, EventArgs e)
        {
            addTabItem(string.Empty, string.Empty, string.Empty, string.Empty, 3000, 1, false);
        }


        private void doSearch(byte type)
        {
            switch (type)
            {
                case 0:
                    sppos = new SearchPagePos(0, 1);
                    lastSrch = searchBox.Text;
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


        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (minimizeOnClosingToolStripMenuItem.Checked && !isExit)
            {
                e.Cancel = true;
                this.Hide();
            }
            else
            {
                WriteCookiesToDisk("coockies.dat", steam_srch.cookieCont);
                SaveSettings(true);
            }
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
                    steam_srch.UserName = settingsForm.loginBox.Text;
                    steam_srch.Password = settingsForm.passwordBox.Text;
                    steam_srch.Login();
                    ProgressBar1.Visible = true;
                    StatusLabel1.Text = "Try Login...";
                    SetButton(loginButton, "Cancel");
                }

            }
            else
            {
                steam_srch.Logout();
                buyNowButton.Enabled = false;
                addtoScan.Enabled = false;
            }

        }


        public void ScanItemButton_Click(object sender, EventArgs e)
        {
            if (steam_srch.Logged)
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
                        steam.BuyQuant = scanItem.tobuyQuant;
                        steam.scanID = tabControl1.SelectedIndex;
                        steam.currency = steam_srch.currency;
                        steam.currencies.Current = steam_srch.currencies.Current;
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
            else MessageBox.Show("Please login first.", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);

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
            if (searchRight()) 
            StartCmdLine(steam_srch.searchList[1].Link, string.Empty, false);

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
                button1.Enabled = false;
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
            if (SellButton.Text == "Sell Checked")
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
            else
            {
                //You need to POST this url (SteamSite.removeSell + ourItem.AssetId)
                //Post data needs only sessionId
                MessageBox.Show("Feature is not working yet.", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
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
                if (ourItem.OnSale)
                {
                    SellButton.Text = "Remove Sell";
                }
                else
                {
                    SellButton.Text = "Sell Checked";
                } 
                StartLoadImgTread(string.Format(SteamSite.invImgUrl, ourItem.ImgLink), pictureBox3);
                
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(aboutApp, "About...", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == System.Windows.Forms.DialogResult.Yes)
            StartCmdLine(homePage, string.Empty, false);
        }

        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StartCmdLine(helpPage, string.Empty, false);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
           isExit = true;
           this.Close();
        }

        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            this.Show();
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (settingsForm.ShowDialog() == DialogResult.OK)
            {
                string lang = settingsForm.comboBox2.Text;
                if (lang != string.Empty && settingsForm.isLangChg)
                {
                    steam_srch.ChangeLng(lang);
                    settingsForm.isLangChg = false;
                }

                SaveSettings(false);
            }
            else
            {
                LoadSettings(false);
            }
        }

        private void splitContainer2_Paint(object sender, PaintEventArgs e)
        {
            SplitContainer s = sender as SplitContainer;
            if (s != null)
            {
                int x1 = 5;
                int x2 = s.Width - 5;
                int y1 = s.SplitterDistance;
                e.Graphics.DrawLine(Pens.Silver, x1, y1, x2, y1);
                e.Graphics.DrawLine(Pens.Silver, x1, y1 + 3, x2, y1 + 3);
            }
        }

        private void FoundList_ColumnWidthChanging(object sender, ColumnWidthChangingEventArgs e)
        {
            if (e.ColumnIndex == 0)
            {
                e.NewWidth = this.InventoryList.Columns[0].Width;
                e.Cancel = true;
            }
        }

        private void FoundList_Click(object sender, EventArgs e)
        {
            if (FoundList.SelectedIndices.Count != 0)
            {
                var ourItem = steam_srch.searchList[FoundList.SelectedIndices[0]];
                StartLoadImgTread(string.Format(SteamSite.fndImgUrl, ourItem.ImgLink), pictureBox1);
            }
        }

        private void searchBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                searchButton.PerformClick();
            }
        }

   }
}
