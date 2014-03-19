using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;

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
        int lastSelec = -1;
        bool addonComplete = false;
        bool isExit = false;
        ItemComparer itemComparer = new ItemComparer();

        static public Semaphore reqPool;

        ScanItemList scanItems = new ScanItemList();

        Settings settingsForm = new Settings();
        Properties.Settings settings = Properties.Settings.Default;

        ImageList StatImgLst;


        public Main()
        {

            InitializeComponent();

            StatImgLst = new ImageList();
            StatImgLst.Images.Add("Img1", Properties.Resources.ready);
            StatImgLst.Images.Add("Img2", Properties.Resources.warning);
            StatImgLst.Images.Add("Img3", Properties.Resources.clock);
            StatImgLst.ColorDepth = ColorDepth.Depth32Bit;
            scanListView.SmallImageList = StatImgLst;

            steam_srch.delegMessage += new eventDelegate(Event_Message);
         }

 
        private void Main_Load(object sender, EventArgs e)
        {

            settingsForm.intLangComboBox.DataSource = new System.Globalization.CultureInfo[]
            {
              System.Globalization.CultureInfo.GetCultureInfo("ru-RU"),
              System.Globalization.CultureInfo.GetCultureInfo("en-US")
            };


            settingsForm.intLangComboBox.DisplayMember = "NativeName";
            settingsForm.intLangComboBox.ValueMember = "Name";

            steam_srch.cookieCont = ReadCookiesFromDisk(cockPath);

            InventoryList.ListViewItemSorter = itemComparer;
            
            //Add sorter?
            //FounList.ListViewItemSorter = itemComparer;
           
            LoadSettings(true);

            if (settings.loginOnstart)
                loginButton.PerformClick();

            setNotifyText(Strings.NotLogged);
        }

        private void Main_Shown(object sender, EventArgs e)
        {
            if (settings.firstRun == true)
            {
                MessageBox.Show(Strings.FirstTime, Strings.Attention, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                settings.firstRun = false;
                settings.Save();
                settingsToolStripMenuItem.PerformClick();
            }
        }

//=================================================== Settings Stuff ====================== Start ============================

        private void LoadSettings(bool loadtabs)
        {
            //#if(DEBUG)

            //Verify that the Property has the required attribute for Binary serialization.
            System.Reflection.PropertyInfo binarySerializeProperty = settings.GetType().GetProperty("saveTabs");
            object[] customAttributes = binarySerializeProperty.GetCustomAttributes(typeof(System.Configuration.SettingsSerializeAsAttribute), false);
            if (customAttributes.Length != 1)
            {
                // ooops!
                // add attribute to settings.designer.cs
                // [global::System.Configuration.SettingsSerializeAs(System.Configuration.SettingsSerializeAs.Binary)]
                // right before "public global::SCMBot.saveTabLst saveTabs..."

                throw new ApplicationException("SettingsSerializeAsAttribute required for saveTabs property");
            }

            //#endif

            settingsForm.loginBox.Text = settings.lastLogin;
            label3.Text = string.Format("({0})", settings.lastLogin);

            settingsForm.checkBox2.Checked = settings.loginOnstart;
            settingsForm.logCountBox.Text = settings.logCount.ToString();
            settingsForm.searchResBox.Text = settings.searchRes;
            settingsForm.numThreadsBox.Value = settings.numThreads;
            settingsForm.ignoreBox.Checked = settings.ignoreWarn;


            comboBox3.SelectedIndex = settings.InvType;


            settingsForm.hideInventBox.Checked = settings.hideInvent;
            splitContainer1.Panel2Collapsed = settings.hideInvent;
            minimizeOnClosingToolStripMenuItem.Checked = settings.minOnClose;

            if (!String.IsNullOrEmpty(settings.Language))
            {
                settingsForm.intLangComboBox.SelectedValue = settings.Language;
            }

            //If you need password crypting
            //passwordBox.Text = Decrypt(settings.lastPass);
            settingsForm.passwordBox.Text = settings.lastPass;

            if (loadtabs)
            {
                LoadTabs(settings.saveTabs);

                if (reqPool != null)
                    reqPool.Dispose();

                int num = settings.numThreads;
                if (num <= 0)
                    num = 5;
                 reqPool = new Semaphore(num, num);
            }
        }


        private void SaveSettings(bool savetabs)
        {
            settings.lastLogin = settingsForm.loginBox.Text;
            settings.loginOnstart = settingsForm.checkBox2.Checked;
            settings.minOnClose = minimizeOnClosingToolStripMenuItem.Checked;
            settings.hideInvent = settingsForm.hideInventBox.Checked;
            settings.numThreads = (int)settingsForm.numThreadsBox.Value;
            settings.logCount = Convert.ToInt32(settingsForm.logCountBox.Text);
            settings.searchRes = settingsForm.searchResBox.Text;
            settings.ignoreWarn = settingsForm.ignoreBox.Checked;

            settings.InvType = comboBox3.SelectedIndex;
            settings.LastCurr = steam_srch.currencies.Current;

            settings.Language = settingsForm.intLangComboBox.SelectedItem.ToString();

            splitContainer1.Panel2Collapsed = settings.hideInvent;
            label3.Text = string.Format("({0})", settings.lastLogin);

            //If you need password crypting
            //settings.lastPass = Encrypt(passwordBox.Text);
            settings.lastPass = settingsForm.passwordBox.Text;



            if (savetabs)
            {
                settings.saveTabs = new saveTabLst();
                SaveTabs(settings.saveTabs);
            }
            settings.Save();
        }



        private void LoadTabs(saveTabLst lst)
        {
            if (lst != null)
            {
                for (int i = 0; i < lst.Count; i++)
                {
                    var ourItem = lst[i];


                      string[] row = {string.Empty, ourItem.Name, ourItem.Price };
                      var lstItem = new ListViewItem(row);

                      scanListView.Items.Add(lstItem);
                      var scanItem = new ScanItem(ourItem, steam_srch.cookieCont, new eventDelegate(Event_Message), settings.LastCurr, settings.ignoreWarn);

                      if (isScanValid(ourItem))
                      {
                          scanItem.StatId = 0;
                          setStatImg(i, 0);
                      }
                      else
                      {
                          scanItem.StatId = 1;
                          setStatImg(i, 1);
                      }


                    //new
                     scanItems.Add(scanItem);

                
                }

                if (scanListView.Items.Count != 0)
                {
                    if (scanListView.Items.Count > lst.Position)
                    {
                        scanListView.Items[lst.Position].Selected = true;
                        BindToControls();
                    }
                    scanItems.UpdateIds();
                    SetColumnWidths(scanListView, true);
                }
                else panel1.Enabled = false;
            }
        }

        private void SaveTabs(saveTabLst lst)
        {
            if (scanItems.Count != 0)
          {
               for (int i = 0; i < scanItems.Count; i++)
                {
                        lst.Add(scanItems[i].ScanParams);
               }

               if (scanListView.SelectedIndices.Count != 0)
                   lst.Position = scanListView.SelectedIndices[0];
               else lst.Position = 0;

            }
        }

//=================================================== Settings Stuff ====================== End ==============================




        public void GetAccInfo(string mess)
        {
            if (mess != string.Empty)
            {
                string[] accinfo = mess.Split('|');
                StartLoadImgTread(accinfo[2], pictureBox2);
                label5.Text = accinfo[1];
                label10.Text = accinfo[0];
                ProgressBar1.Visible = false;
                SetButton(loginButton, Strings.Logout, 2);
                buyNowButton.Enabled = true;
                addtoScan.Enabled = true;

                setNotifyText(Strings.NotLogged);

                UpdateScanCurrs();
            }
            else
                MessageBox.Show(Strings.ErrAccInfo, Strings.Attention, MessageBoxButtons.OK, MessageBoxIcon.Error);

        }

        private void UpdateScanCurrs()
        {
            for (int i = 0; i < scanItems.Count; i++)
            {
                scanItems[i].Steam.currencies.Current = steam_srch.currencies.Current;
            } 
        }

        public string GetPriceFormat(string mess, bool addcurr, string currName)
        {
            string curr = string.Empty;
            if (addcurr)
                curr = currName;
            return string.Format("{0} {1} {2}", DateTime.Now.ToString("HH:mm:ss"), mess, curr);
        }


        private void AddToScanLog(string message, int scanId, byte color, bool addcurr)
        {
            cutLog(scanItems[scanId].LogCont, settings.logCount);
            scanItems[scanId].LogCont.Add(new ScanItem.LogItem(color, GetPriceFormat(message, addcurr, steam_srch.currencies.GetName())));
            ScrollLbox(scanId);
        }


        public void Event_Message(object sender, string message, int searchId, flag myflag)
        {
            switch (myflag)
            {
                case flag.Already_logged:

                    AddtoLog(Strings.AlreadyLogged);
                    StatusLabel1.Text = Strings.AlreadyLogged;
                    GetAccInfo(message);
                    break;

                case flag.Login_success:

                    AddtoLog(Strings.LoginSucc);
                    StatusLabel1.Text = Strings.LoginSucc;
                    GetAccInfo(message);
                    break;

                case flag.Login_cancel:

                    MessageBox.Show(Strings.ErrLogin + message, Strings.Attention, MessageBoxButtons.OK, MessageBoxIcon.Warning);

                    StatusLabel1.Text = message;

                    SetButton(loginButton, Strings.Login, 1);
                    ProgressBar1.Visible = false;
                    label5.Text = message;
                    break;

                case flag.Logout_:
                    StatusLabel1.Text = Strings.Logouted;
                    SetButton(loginButton, Strings.Login, 1);
                    setNotifyText(Strings.NotLogged);
                    break;

                case flag.Send_cancel:

                    var scanItem = scanItems[searchId];
                    scanItem.Steam.CancelScan();
                    break;

                case flag.StripImg:
                    if (searchId == 0)
                        toolStripImage.Image = Properties.Resources.working;
                    else
                        toolStripImage.Image = Properties.Resources.ready;
                    break;

                case flag.Lang_Changed:
                    StatusLabel1.Text = Strings.ChangeLang + message;
                    break;

                case flag.Price_htext:
                    AddToScanLog(message, searchId, 1, true);
                    break;

                case flag.Price_btext:
                    AddToScanLog(message, searchId, 2, true);
                    break;

                case flag.Price_text:
                    AddToScanLog(message, searchId, 0, true);
                   break;

                case flag.Rep_progress:
                    ProgressBar1.Value = Convert.ToInt32(message);
                    break;
                case flag.SetHeadName:
                    scanListView.Items[searchId].SubItems[1].Text = message;
                    SetColumnWidths(scanListView, true);
                    break;
                    
                case flag.Scan_progress:
                    StatusLabel1.Text = Strings.ScanPrice;
                    break;

                case flag.Success_buy:
                    StatusLabel1.Text = Strings.Bought;
                    label5.Text = message;
                    buyNowButton.Enabled = true;
                    break;

                case flag.Error_buy:
                    StatusLabel1.Text = Strings.BuyError;
                    AddToScanLog(message, searchId, 1, false);

                    buyNowButton.Enabled = true;
                    break;
                case flag.Error_scan:
                    StatusLabel1.Text = Strings.ScanError;
                    string mess = GetScanErrMess(message);
                    AddtoLog(scanItems[searchId].Steam.scanName + ": " + mess);
                    AddToScanLog(mess, searchId, 1, false);
                    buyNowButton.Enabled = true;
                    break;

                case flag.Scan_cancel:
                    StatusLabel1.Text = Strings.ScanCancel;
                    break;
                case flag.Resold:
                    StatusLabel1.Text = string.Format("Item \"{0}\" resold!", message);
                    break;
                case flag.InvPrice:
                    string sweet = SteamSite.DoFracture(message);
                    InventoryList.Items[searchId].SubItems[3].Text = sweet;
                    textBox1.Text = sweet;
                    textBox1.ReadOnly = false;
                    break;
                case flag.Items_Sold:
                    if (searchId != 1)
                    {
                        if (steam_srch.isRemove)
                        {
                            StatusLabel1.Text = Strings.SellRemoved;
                        }
                        else
                        {
                            StatusLabel1.Text = Strings.SaleItems;
                        }
                        steam_srch.loadInventory();
                    }
                    else
                        StatusLabel1.Text = "Nothing to sell!";

                    ProgressBar1.Visible = false;
                    break;

                case flag.Sell_progress:
                    ProgressBar1.Value = Convert.ToInt32(message);
                    break;
                case flag.Search_success:
                    int searchLim = Convert.ToInt32(settings.searchRes);
                    StatusLabel1.Text = string.Format(Strings.FoundShown, message, steam_srch.searchList.Count.ToString());

                    if (sppos.CurrentPos == 1)
                    {
                        int found = Convert.ToInt32(message);
                        sppos.PageCount = found / searchLim;
                        if (found % searchLim != 0)
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

                   
                    FoundList.Items.Clear();

                    for (int i = 0; i < steam_srch.searchList.Count; i++)
                    {
                        var ourItem = steam_srch.searchList[i];

                        string[] row = { string.Empty, ourItem.Game, ourItem.Name, ourItem.StartPrice + " " + steam_srch.currencies.GetName(), ourItem.Quant };
                        var lstItem = new ListViewItem(row);
                        FoundList.Items.Add(lstItem);
                    }

                    if (addonComplete)
                    {
                        for (int i = 0; i < steam_srch.searchList.Count; i++)
                        {
                            var currItem = steam_srch.searchList[i];
                            addScanItem(currItem, 3000, 0, false, 0, true, false);
                        }
                        scanItems.UpdateIds();
                        addonComplete = false;
                    }
                    else
                        SetColumnWidths(FoundList, true);


                    searchButton.Enabled = true;

                    break;

                case flag.Inventory_Loaded:
                    InventoryList.Items.Clear();
                    label4.Text = steam_srch.inventList.Count.ToString();
                    button1.Enabled = true;

                    if (searchId == 0)
                    {
                        for (int i = 0; i < steam_srch.inventList.Count; i++)
                        {
                            var ourItem = steam_srch.inventList[i];
                            
                            string priceRes;
                            if (ourItem.Price == "0")
                                priceRes = Strings.None;
                            else if (ourItem.Price == "1")
                                priceRes = Strings.NFS;
                            else
                                priceRes = ourItem.Price;

                            string[] row = { string.Empty, ourItem.Type, ourItem.Name, priceRes };
                            var lstItem = new ListViewItem(row);
                            InventoryList.Items.Add(lstItem);
                        }
                        SetColumnWidths(InventoryList, true);
                        SellButton.Enabled = true;
                        button2.Enabled = true;
                    }
                    else
                    {
                        button2.Enabled = false;
                        MessageBox.Show("Inventory section is empty!", Strings.Attention, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                    break;
            }

        }

        public void ScrollLbox(int input)
        {
            if (scanListView.SelectedIndices.Count != 0)
            {
                if (scanListView.SelectedIndices[0] == input)
                {
                    logListBox.SelectedIndex = logListBox.Items.Count - 1;
                    logListBox.SelectedIndex = -1;
                }
            }
        }

        private void cutLog(System.ComponentModel.BindingList<ScanItem.LogItem> bindingList, int limit)
        {
            if (bindingList.Count > limit)
            {
                bindingList.Clear();
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
            MessageBox.Show(Strings.TryToFind, Strings.Attention, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void fullSetToolStripMenuItem_Click(object sender, EventArgs e)
        {

            if (searchRight())
            {
                if (FoundList.SelectedIndices.Count != 0)
                {
                    searchBox.Text = string.Format("\"{0}\"", steam_srch.searchList[FoundList.SelectedItems[0].Index].Game);
                    searchButton.PerformClick();

                    addonComplete = true;
                }
            }
            else
                searchFirstMess();
        }

        private void buyNowButton_Click(object sender, EventArgs e)
        {
            if (searchRight())
            {
                if (FoundList.SelectedIndices.Count != 0)
                {
                    var ourItem = steam_srch.searchList[FoundList.SelectedItems[0].Index];
                    steam_srch.BuyNow = true;
                    steam_srch.pageLink = ourItem.Link;
                    steam_srch.ScanPrices();
                    buyNowButton.Enabled = false;
                    StatusLabel1.Text = string.Format(Strings.buyingProc, ourItem.Name);
                }
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

                    addScanItem(ourItem, 3000, 0, false, 0, true, false);
                }
                scanItems.UpdateIds();

            }
            else
                searchFirstMess();
        }

        private void addScanItem(SteamSite.SearchItem ourItem, int delay, int buyQuant, bool tobuy, int resellType, bool scanPage, bool scanRecent)
        {
            string[] row = { string.Empty, ourItem.Name, ourItem.StartPrice };
            var lstItem = new ListViewItem(row);
            scanListView.Items.Add(lstItem);
            var ourTab = new saveTab(ourItem.Name, ourItem.Link, ourItem.ImgLink, ourItem.StartPrice, delay, buyQuant, tobuy, resellType, ourItem.StartPrice, scanPage, scanRecent);
            scanItems.Add(new ScanItem(ourTab, steam_srch.cookieCont, new eventDelegate(Event_Message), steam_srch.currencies.Current, settings.ignoreWarn));
            setStatImg(scanListView.Items.Count - 1, Convert.ToByte(!isScanValid(ourTab)));
            SetColumnWidths(scanListView, true);
        }


        private void emptyTabToolStripMenuItem_Click(object sender, EventArgs e)
        {
            addScanItem(new SteamSite.SearchItem(string.Empty, string.Empty, string.Empty, "1", "0", string.Empty), 3000, 0, false, 0, true, false);
            scanItems.UpdateIds();
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
            
            //search/render/?query={0}&start={1}&count={2}
            steam_srch.linkTxt = string.Format(SteamSite._search, lastSrch, sppos.CurrentPos - 1, settings.searchRes);
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


        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (minimizeOnClosingToolStripMenuItem.Checked && !isExit)
            {
                e.Cancel = true;
                this.Hide();
            }
            else
            {
                WriteCookiesToDisk(cockPath, steam_srch.cookieCont);
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
                    steam_srch.UserName = settings.lastLogin;
                    steam_srch.Password = settings.lastPass;
                    steam_srch.Login();
                    ProgressBar1.Visible = true;
                    StatusLabel1.Text = Strings.tryLogin;
                    SetButton(loginButton, Strings.Cancel, 3);
                }

            }
            else
            {
                steam_srch.Logout();
                buyNowButton.Enabled = false;
                addtoScan.Enabled = false;
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
            if (searchRight()) 
            StartCmdLine(steam_srch.searchList[1].Link, string.Empty, false);

        }

        private void button1_Click(object sender, EventArgs e)
        {

            if (steam_srch.Logged)
            {
                lastSelec = -1;

                button1.Enabled = false;
                if (comboBox3.SelectedIndex == 4)
                {
                    steam_srch.LoadOnSale = true;
                }
                else
                {
                    steam_srch.LoadOnSale = false;
                    steam_srch.invApp = comboBox3.SelectedIndex;
                }
                steam_srch.loadInventory();

            } else
                MessageBox.Show(Strings.LoginFirst, Strings.Attention, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
       

        private void SellButton_Click(object sender, EventArgs e)
        {

            if (steam_srch.inventList.Count == 0)
            {
                MessageBox.Show(Strings.LoadInvFirst, Strings.Attention, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (InventoryList.CheckedItems.Count == 0)
            {
                MessageBox.Show(Strings.CheckIt, Strings.Attention, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (steam_srch.isRemove)
            {
                StatusLabel1.Text = Strings.RemSellStat;
            }
            else
            {
                StatusLabel1.Text = Strings.AddSell;
            }

            ProgressBar1.Value = 0;
            ProgressBar1.Visible = true;
            steam_srch.invApp = comboBox3.SelectedIndex;
            steam_srch.toSellList.Clear();

            for (int i = 0; i < InventoryList.CheckedItems.Count; i++)
            {
                var ouritem = steam_srch.inventList[InventoryList.CheckedItems[i].Index];
                if (ouritem.Marketable)
                steam_srch.toSellList.Add(new SteamSite.ItemToSell(ouritem.AssetId, ouritem.Price));
            }


            steam_srch.ItemSell();
  
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

                var lit = InventoryList.SelectedItems[0];

                if (lastSelec != lit.Index)
                {
                    var ourItem = steam_srch.inventList[InventoryList.SelectedIndices[0]];
                    StartLoadImgTread(string.Format(SteamSite.fndImgUrl, ourItem.ImgLink), pictureBox3);

                        var currPr = InventoryList.SelectedItems[0].SubItems[3].Text;

                        if (currPr == Strings.None)
                        {
                            steam_srch.GetPriceTread(ourItem.MarketName, lit.Index);
                            textBox1.Text = Strings.Loading;
                            textBox1.ReadOnly = true;
                        }
                        else
                            if (currPr == Strings.NFS)
                                textBox1.ReadOnly = true;
                        else
                            textBox1.Text = currPr;

                    lastSelec = lit.Index;
                }
                
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(aboutApp, Strings.AboutTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Information) == System.Windows.Forms.DialogResult.Yes)
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
                int y2 = y1 + 3;
                e.Graphics.DrawLine(Pens.Silver, x1, y1, x2, y1);
                e.Graphics.DrawLine(Pens.Silver, x1, y2, x2, y2);
            }
        }


        private void splitContainer3_Paint(object sender, PaintEventArgs e)
        {
            SplitContainer s = sender as SplitContainer;
            if (s != null)
            {
                int x1 = 5;
                int x2 = s.Height - 5;
                int y1 = s.SplitterDistance;
                int y2 = y1 + 3;
                e.Graphics.DrawLine(Pens.Silver, y1, x1, y1, x2);
                e.Graphics.DrawLine(Pens.Silver, y2, x1, y2, x2);
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

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox3.SelectedIndex == 4)
            {
                SellButton.Text = Strings.RemSell;
                steam_srch.isRemove = true;
                textBox1.Clear();
                textBox1.ReadOnly = true;
                button2.Enabled = false;
            }
            else
            {
                SellButton.Text = Strings.Sell;
                steam_srch.isRemove = false;
                textBox1.ReadOnly = false;
            }
            SellButton.Enabled = false;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string truePrice = string.Empty;
            if (textBox1.Text == string.Empty)
            {
                MessageBox.Show(Strings.WrongPrice, Strings.Attention, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            else
            {
                truePrice = SteamSite.GetSweetPrice(textBox1.Text);
                if (truePrice == string.Empty)
                {
                    MessageBox.Show(Strings.WrongPrice, Strings.Attention, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
              
            
            for (int i = 0; i < InventoryList.SelectedItems.Count; i++)
            {
                var ourItem = steam_srch.inventList[InventoryList.SelectedIndices[i]];
                ourItem.Price = truePrice;
                InventoryList.SelectedItems[i].SubItems[3].Text = textBox1.Text;
            }

            
        }

        private void setButtText(bool isScaning)
        {
            if (isScaning)
            {
                scanButton.Text = Strings.Stop;
                scanButton.Image = Properties.Resources.stop;
            }
            else
            {
                scanButton.Text = Strings.Start;
                scanButton.Image = Properties.Resources.start;
            }
        }




        private void scanListView_Click(object sender, EventArgs e)
        {
         

        }



        private void linkBinding_BindingComplete(object sender, BindingCompleteEventArgs e)
        {
            //e.Binding.PropertyName;


                var Item = scanItems[scanListView.SelectedIndices[0]];
                var ourItem = Item.ScanParams;

                scanListView.SelectedItems[0].SubItems[2].Text = ourItem.Price;


                if (isScanValid(ourItem))
                {
                    Item.StatId = 0;
                    setStatImg(scanListView.SelectedIndices[0], 0);
                }
                else
                {
                    Item.StatId = 1;
                    setStatImg(scanListView.SelectedIndices[0], 1);
                }
        }	


        private void scanButton_Click(object sender, EventArgs e)
        {
            //Nope. TODO: Clean this shit!

                int id = scanListView.SelectedIndices[0];
                var ourItem = scanItems[id];
                var steamItem = ourItem.Steam;
                var paramItem = ourItem.ScanParams;

                if (!steamItem.scaninProg)
                {

                    if (isScanValid(paramItem))
                    {

                        startScan(false);

                    }
                    else
                    {
                        ourItem.StatId = 1;
                        MessageBox.Show(Strings.CheckVal, Strings.Attention, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }

                }

                else
                {
                    stopScan(false);
                }

                setButtText(steamItem.scaninProg);
                setStatImg(id, ourItem.StatId);
        }



        private void startScan(bool all)
        {

            if (steam_srch.Logged)
            {
                if (all)
                {
                    DoSelect(true);
                }

                for (int i = 0; i < scanListView.SelectedIndices.Count; i++)
                {
                    int id = scanListView.SelectedIndices[i];
                    var ourItem = scanItems[id];
                    var steamItem = ourItem.Steam;
                    var paramItem = ourItem.ScanParams;

                    if (steamItem.scaninProg)
                        continue;

                    if (ourItem.StatId == 0)
                    {

                        ourItem.ReadParams();
                        steamItem.ScanPrices();

                        ourItem.StatId = 2;
                        setStatImg(id, 2);
                    }
                }

                StatusLabel1.Text = Strings.ScanPrice;

                if (all)
                {
                    DoSelect(false);
                }

            }
            else MessageBox.Show(Strings.LoginFirst, Strings.Attention, MessageBoxButtons.OK, MessageBoxIcon.Warning);


        }


        private void stopScan(bool all)
        {

            if (steam_srch.Logged)
            {
                if (all)
                {
                    DoSelect(true);
                }


                for (int i = 0; i < scanListView.SelectedIndices.Count; i++)
                {
                    int id = scanListView.SelectedIndices[i];
                    var ourItem = scanItems[id];
                    var steamItem = ourItem.Steam;
                    var paramItem = ourItem.ScanParams;

                    if (!steamItem.scaninProg)
                        continue;

                    if (ourItem.StatId == 2)
                    {
                        steamItem.CancelScan();
                        ourItem.StatId = 0;
                        setStatImg(id, 0);
                    }
                }

                if (all)
                {
                    DoSelect(false);
                }

            }
            else MessageBox.Show(Strings.LoginFirst, Strings.Attention, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }



        private void setStatImg(int id, byte p)
        {
            scanListView.Items[id].ImageIndex = p;
        }

        private void logListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            try
            {
                if ((scanListView.SelectedIndices.Count == 0) | (e.Index < 0))
                    return;


                var logItem = scanItems[scanListView.SelectedIndices[0]].LogCont;

                if (logItem.Count == 0)
                    return;


                e.DrawBackground();
                Brush myBrush = Brushes.Black;

                if (e.Index < logItem.Count)
                {

                    switch (logItem[e.Index].Id)
                    {
                        case 0:
                            myBrush = Brushes.Black;
                            break;
                        case 1:
                            myBrush = Brushes.Red;
                            break;
                        case 2:
                            myBrush = Brushes.Green;
                            break;
                    }

                    e.Graphics.DrawString(((ListBox)sender).Items[e.Index].ToString(), e.Font, myBrush, e.Bounds, StringFormat.GenericDefault);
                    e.DrawFocusRectangle();
                }

            }
            catch (Exception)
            {
                throw;
            }

        }

        private void logListBox_ItemsChanged(object sender, EventArgs e)
        {
            logListBox.SelectedIndex = logListBox.Items.Count - 1;
            logListBox.SelectedIndex = -1;
        }


        private void deleteSelected()
        {
            if (scanListView.SelectedItems.Count != 0)
            {
                for (int i = 0; i < scanListView.Items.Count; i++)
                {
                    if (scanListView.Items[i].Selected)
                    {
                        var Item = scanItems[i];

                        if (Item.Steam.scaninProg)
                            Item.Steam.CancelScan();

                        scanItems.RemoveAt(i);
                        scanListView.Items[i].Remove();
                        i--;
                    }
                }

                scanItems.UpdateIds();

                //TODO - Select last standing Item
                BindToControls();
                SetColumnWidths(scanListView, true);
            }
        }

        private void удалитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            deleteSelected();

        }

        private void DoSelect(bool p)
        {
            foreach (ListViewItem item in scanListView.Items)
            {
                item.Selected = p;
            }

        }

        private void scanListView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                deleteSelected();
                e.Handled = true;
            }
        }

 
        private void startSelectedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            startScan(false);
        }

        private void startAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            startScan(true);
        }

        private void stopSelectedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            stopScan(false);
        }

        private void stopAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            stopScan(true);
        }


        private void BindToControls()
        {
            if (scanListView.SelectedIndices.Count == 1)
            {

                panel1.Enabled = true;

                var Item = scanItems[scanListView.SelectedIndices[0]];
                var ourItem = Item.ScanParams;
                var logItem = Item.LogCont;

                //Add event for all controls? I don't sure..

                linkTextBox.DataBindings.Clear();
                Binding linkBinding = linkTextBox.DataBindings.Add("Text", ourItem, "Link", true, DataSourceUpdateMode.OnPropertyChanged);
                linkBinding.BindingComplete += new BindingCompleteEventHandler(linkBinding_BindingComplete);

                wishpriceBox.DataBindings.Clear();
                Binding wishedBinding = wishpriceBox.DataBindings.Add("Text", ourItem, "Price", true, DataSourceUpdateMode.OnPropertyChanged);
                wishedBinding.BindingComplete += new BindingCompleteEventHandler(linkBinding_BindingComplete);


                delayTextBox.DataBindings.Clear();
                delayTextBox.DataBindings.Add("Text", ourItem, "Delay");

                buyCheckBox.DataBindings.Clear();
                buyCheckBox.DataBindings.Add("Checked", ourItem, "ToBuy");

                buyUpDown.DataBindings.Clear();
                buyUpDown.DataBindings.Add("Value", ourItem, "BuyQnt");

                pageCheckBox.DataBindings.Clear();
                pageCheckBox.DataBindings.Add("Checked", ourItem, "ScanPage");

                recentCheckBox.DataBindings.Clear();
                recentCheckBox.DataBindings.Add("Checked", ourItem, "ScanRecent");

                resellComboBox.DataBindings.Clear();
                resellComboBox.DataBindings.Add("SelectedIndex", ourItem, "ResellType");

                resellPriceBox.DataBindings.Clear();
                resellPriceBox.DataBindings.Add("Text", ourItem, "ResellPrice");

                logListBox.DataSource = logItem;
                logListBox.DisplayMember = "Text";

                if (logListBox.Items.Count != 0)
                {
                    logListBox.SelectedIndex = logListBox.Items.Count - 1;
                    logListBox.SelectedIndex = -1;
                }

                if (ourItem.ImgLink != string.Empty)
                    StartLoadImgTread(string.Format(SteamSite.fndImgUrl, ourItem.ImgLink), pictureBox4);
                else
                    pictureBox4.Image = null;

                setButtText(Item.Steam.scaninProg);

                // linkTextBox.Text = ourItem.Link;
                // wishpriceBox.Text = ourItem.Price;
                // delayTextBox.Text = ourItem.Delay.ToString();
                // buyCheckBox.Checked = ourItem.ToBuy;
                // buyUpDown.Value = ourItem.BuyQnt;
                // pageCheckBox.Checked = ourItem.ScanPage;
                // recentCheckBox.Checked = ourItem.ScanRecent;
                // resellComboBox.SelectedIndex = ourItem.ResellType;
                // resellPriceBox.Text = ourItem.ResellVal;
            }
            else panel1.Enabled = false;
        }

        private void scanListView_MouseClick(object sender, MouseEventArgs e)
        {
            BindToControls();
        }

        private void scanListView_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                bool block = !(scanListView.SelectedIndices.Count == 0);

                startSelectedMenuItem.Enabled = block;
                stopSelectedMenuItem.Enabled = block;
                deleteMenuItem.Enabled = block;

                if (scanListView.Items.Count == 0)
                {
                    startAllMenuItem.Enabled = false;
                    stopAllMenuItem.Enabled = false;
                }
                else
                {
                    startAllMenuItem.Enabled = true;
                    stopAllMenuItem.Enabled = true;
                }
            }
            else
                if (scanListView.SelectedIndices.Count == 0)
                    panel1.Enabled = false;
                else
                    panel1.Enabled = true;
           
        }

        private void splitContainer3_SplitterMoved(object sender, SplitterEventArgs e)
        {
            SetColumnWidths(scanListView, true);
        }

        private void DonateBox_Click(object sender, EventArgs e)
        {
            StartCmdLine(donateLink, string.Empty, false);
        }

  
   }
}
