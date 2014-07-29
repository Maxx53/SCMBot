using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.ComponentModel;
using System.Net;
using System.Windows.Forms.DataVisualization.Charting;
using System.Collections.Generic;
using System.Linq;
using System.IO;
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
        public static SteamSite steam_srch = new SteamSite();
        private SearchPagePos sppos;
        string lastSrch;
        int lastSelec = -1;
        bool addonComplete = false;
        bool isExit = false;
        bool isFirstTab = true;
        bool relog = false;

        static bool isLog = true;
        public static bool isHTML = true;
        //ItemComparer itemComparer = new ItemComparer();

        static public Semaphore reqPool;

        private ScanItemList scanItems = new ScanItemList();

        private SettingsFrm settingsForm = new SettingsFrm();
        private ProxyStatsFrm proxyStatForm = new ProxyStatsFrm();
        private GraphFrm graphFrm = new GraphFrm();

        Properties.Settings settings = Properties.Settings.Default;

        private ImageList StatImgLst;
        private List<SteamSite.InventItem> filteredInvList = new List<SteamSite.InventItem>();

        public static ProxyList proxyList = new ProxyList();

        private Size lastFrmSize;
        private Point lastFrmPos;
        private bool ResComboClicked = false;
        private bool sortDirect = true;

        public static int walletVal = 0;
        public static int stopfundsVal = 0;
        public static string jsonAddon;
        public static int ReqDelay = 100;

        public Main()
        {

            InitializeComponent();

            StatImgLst = new ImageList();
            StatImgLst.Images.Add("Img1", Properties.Resources.ready);
            StatImgLst.Images.Add("Img2", Properties.Resources.warning);
            StatImgLst.Images.Add("Img3", Properties.Resources.clock);
            StatImgLst.ColorDepth = ColorDepth.Depth32Bit;
            scanListView.SmallImageList = StatImgLst;
            recentListView.SmallImageList = StatImgLst;

            steam_srch.delegMessage += new eventDelegate(Event_Message);
           
            //Some WebRequest optimizations
            ServicePointManager.Expect100Continue = false;
            ServicePointManager.UseNagleAlgorithm = false;
            ServicePointManager.DefaultConnectionLimit = 1000;
         }

 
        private void Main_Load(object sender, EventArgs e)
        {
            settingsForm.intLangComboBox.DataSource = new System.Globalization.CultureInfo[]
            {
              System.Globalization.CultureInfo.GetCultureInfo("ru-RU"),
              System.Globalization.CultureInfo.GetCultureInfo("en-US"),
              System.Globalization.CultureInfo.GetCultureInfo("fr-FR")
            };

            settingsForm.intLangComboBox.DisplayMember = "NativeName";
            settingsForm.intLangComboBox.ValueMember = "Name";

            //Hotfix
            var cook = (CookieContainer)LoadBinary(cockPath);

            if (cook == null)
                cook =  new CookieContainer();

            steam_srch.cookieCont = cook;
            steam_srch.scanID = 0;
            filterTypeBox.SelectedIndex = 1;

            //InventoryList.ListViewItemSorter = itemComparer;
            
            //Add sorter?
            //FounList.ListViewItemSorter = itemComparer;
           
            LoadSettings(true);

            if (settings.loginOnstart)
                loginButton.PerformClick();

            setNotifyText(Strings.NotLogged);


            openFileDialog1.FileName = "scmbot_list_";
            string AppPath = Path.GetDirectoryName(Application.ExecutablePath);
            openFileDialog1.InitialDirectory = AppPath;
            saveFileDialog1.InitialDirectory = AppPath;


            if (File.Exists(proxyPath))
            {
                var plines = File.ReadAllLines(proxyPath);

                for (int i = 0; i < plines.Length; i++)
                {
                    string proxyPattern = @"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}:\d{1,5}";
                    Match match = Regex.Match(plines[i], proxyPattern);
                    if (match.Success)
                    {
                        proxyList.Add(plines[i]);
                    }
                }
                StatusLabel1.Text = "Proxy servers loaded: " + proxyList.Count.ToString();
            }
            else
                usingProxyStatuslStrip.Enabled = false;


            ListViewHelper.EnableDoubleBuffer(scanListView);
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
            scanListView.Focus();
            BindToControls(scanListView);
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

            label3Stretch();

            isLog = settings.keepLog;
            settingsForm.keepLogBox.Checked = settings.keepLog;

            settingsForm.checkBox2.Checked = settings.loginOnstart;
            settingsForm.logCountBox.Text = settings.logCount.ToString();
            settingsForm.searchResBox.Text = settings.searchRes;
            settingsForm.ignoreBox.Checked = settings.ignoreWarn;
            settingsForm.actualBox.Checked = settings.loadActual;

            isHTML = settings.scanHTML;
            settingsForm.scanHtlmBox.Checked = settings.scanHTML;

            settingsForm.stopFundsBox.Text = MainScanItem.LogItem.DoFracture(settings.StopFunds.ToString());
            stopfundsVal = settings.StopFunds;

            settingsForm.playSndCheckBox.Checked = settings.playSnd;

            settingsForm.resDelayBox.Text = settings.resellDelay.ToString();

            steam_srch.mainDelay = settings.delayVal;
            steam_srch.resellDelay = settings.resellDelay;

            comboBox3.SelectedIndex = settings.InvType;
            sellDelayBox.Text = settings.sellDelay.ToString();
            settingsForm.reqDelayBox.Text = settings.reqDelay.ToString();
            ReqDelay = settings.reqDelay;

            settingsForm.hideInventBox.Checked = settings.hideInvent;
            splitContainer1.Panel2Collapsed = settings.hideInvent;
            minimizeOnClosingToolStripMenuItem.Checked = settings.minOnClose;

            lastFrmPos = this.Location;
            lastFrmSize = this.Size;

            if (settings.formParams == null)
            {
                settings.formParams = new MainFormParams(this.Size, this.Location, this.WindowState,
                splitContainer1.SplitterDistance, splitContainer2.SplitterDistance, splitContainer3.SplitterDistance);
            }
            else
            {
                this.Size = settings.formParams.FrmSize;
                this.Location = settings.formParams.Location;
                this.WindowState = settings.formParams.FrmState;
                try
                {
                    splitContainer1.SplitterDistance = settings.formParams.Split1;
                    splitContainer2.SplitterDistance = settings.formParams.Split2;
                    splitContainer3.SplitterDistance = settings.formParams.Split3;
                }
                catch (Exception)
                {
                    AddtoLog(string.Format("Splitter Distance Error: Splitter1={0}, Splitter2={1}, Splitter3={3}", 
                        settings.formParams.Split1, settings.formParams.Split2, settings.formParams.Split3));
                }

            }

            if (!String.IsNullOrEmpty(settings.Language))
            {
                switch (settings.Language)
                {
                    case "ru-RU": settingsForm.intLangComboBox.SelectedIndex = 0;
                        break;
                    case "en-US": settingsForm.intLangComboBox.SelectedIndex = 1;
                        break;
                    case "fr-FR": settingsForm.intLangComboBox.SelectedIndex = 2;
                        break;
                    default: settingsForm.intLangComboBox.SelectedIndex = 1;
                        break;
                }
            }

            //You need password crypting
            settingsForm.passwordBox.Text = Decrypt(settings.lastPass);
           
            if (loadtabs)
            {
                LoadTabs(settings.saveTabs);

                steam_srch.recentInputList = settings.saveRecent;
                LoadRecent(steam_srch.recentInputList);

                if (reqPool != null)
                    reqPool.Dispose();

                 reqPool = new Semaphore(1, 1);
            }
        }

        private void label3Stretch()
        {
            int xValue = 97;
            int widthValue = 590;

            if (label3.Size.Width > 70)
            {
                xValue += label3.Size.Width - 70;
                widthValue -= label3.Size.Width - 70;
            }

            groupBox2.Location = new Point(xValue, groupBox3.Location.Y);
            groupBox2.Size = new System.Drawing.Size(widthValue, groupBox2.Size.Height);
        }



        private void SaveSettings(bool savetabs)
        {
            settings.lastLogin = settingsForm.loginBox.Text;
            settings.loginOnstart = settingsForm.checkBox2.Checked;
            settings.minOnClose = minimizeOnClosingToolStripMenuItem.Checked;
            settings.hideInvent = settingsForm.hideInventBox.Checked;
            settings.logCount = Convert.ToInt32(settingsForm.logCountBox.Text);
            settings.resellDelay = Convert.ToInt32(settingsForm.resDelayBox.Text);
           
            settings.sellDelay = Convert.ToInt32(sellDelayBox.Text);
            settings.reqDelay = Convert.ToInt32(settingsForm.reqDelayBox.Text);
            ReqDelay = settings.reqDelay;

            isLog = settingsForm.keepLogBox.Checked;
            settings.keepLog = isLog;

            settings.searchRes = settingsForm.searchResBox.Text;
            settings.ignoreWarn = settingsForm.ignoreBox.Checked;
            settings.loadActual = settingsForm.actualBox.Checked;

            settings.playSnd = settingsForm.playSndCheckBox.Checked;


            isHTML = settingsForm.scanHtlmBox.Checked;
            settings.scanHTML = isHTML;
            
            settings.delayVal = steam_srch.mainDelay;

            settings.InvType = comboBox3.SelectedIndex;
            settings.LastCurr = steam_srch.currencies.Current;

            settings.StopFunds = Convert.ToInt32(SteamSite.GetSweetPrice(settingsForm.stopFundsBox.Text));
            stopfundsVal = settings.StopFunds;

            settings.Language = settingsForm.intLangComboBox.SelectedItem.ToString();

            settings.formParams = new MainFormParams(lastFrmSize, lastFrmPos, this.WindowState, 
            splitContainer1.SplitterDistance, splitContainer2.SplitterDistance, splitContainer3.SplitterDistance);


            splitContainer1.Panel2Collapsed = settings.hideInvent;
            label3.Text = string.Format("({0})", settings.lastLogin);

            label3Stretch();

            //You need password crypting
            settings.lastPass = Encrypt(settingsForm.passwordBox.Text);

            if (savetabs)
            {
                settings.saveTabs = new saveTabLst();
                SaveTabs(settings.saveTabs);
                settings.saveRecent = steam_srch.recentInputList;
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
                      var scanItem = new MainScanItem(ourItem, steam_srch.cookieCont, new eventDelegate(Event_Message), settings.LastCurr, settings.ignoreWarn, settings.resellDelay);

                      
                    
                    if (isScanValid(ourItem, true))
                      {
                          ourItem.StatId = status.Ready;
                          setStatImg(i, ourItem.StatId, scanListView);
                      }
                      else
                      {
                          ourItem.StatId = status.Warning;
                          setStatImg(i, ourItem.StatId, scanListView);
                      }


                    //new
                     scanItems.Add(scanItem);

                
                }

                if (scanListView.Items.Count != 0)
                {
                    if (scanListView.Items.Count > lst.Position)
                    {
                        scanListView.Items[lst.Position].Selected = true;
                        scanListView.Focus();
                        BindToControls(scanListView);
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
                    lst.Add(scanItems[i].Steam.scanInput);
                }

                if (scanListView.SelectedIndices.Count != 0)
                    lst.Position = scanListView.SelectedIndices[0];
                else lst.Position = 0;

            }
        }



        private void LoadRecent(saveTabLst lst)
        {
            if (lst != null)
            {
                for (int i = 0; i < lst.Count; i++)
                {
                    var ourItem = lst[i];

                    string[] row = { string.Empty, ourItem.Name, ourItem.Price };
                    var lstItem = new ListViewItem(row);
                    recentListView.Items.Add(lstItem);
                   
                    if (isScanValid(ourItem, false))
                    {
                        setStatImg(i, ourItem.StatId, recentListView);
                    }
                    else
                    {
                        setStatImg(i, ourItem.StatId, recentListView);
                    }


                }

                int recentPos = 0;

                if (recentListView.Items.Count != recentPos)
                {
                    if (recentListView.Items.Count > recentPos)
                    {
                        recentListView.Items[recentPos].Selected = true;
                        BindToControls(recentListView);
                    }
                    SetColumnWidths(recentListView, true);
                }
                else panel1.Enabled = false;
            }
            else
                steam_srch.recentInputList = new saveTabLst();
        }

//=================================================== Settings Stuff ====================== End ==============================




        public void GetAccInfo(StrParam mess)
        {
            if (mess != null)
            {
                StartLoadImgTread(mess.P3, pictureBox2);
                label5.Text = mess.P2;

                jsonAddon = mess.P4;

                walletVal = Convert.ToInt32(SteamSite.GetSweetPrice(Regex.Replace(mess.P2, @"[^\d.,]+", string.Empty)));

                label10.Text = mess.P1;
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


        private void AddToScanLog(string message, int scanId, byte color, bool addcurr, bool ismain)
        {
           if (ismain)
            {
                cutLog(scanItems[scanId].LogCont, settings.logCount);
                scanItems[scanId].LogCont.Add(new MainScanItem.LogItem(color, message, DateTime.Now, addcurr, steam_srch.currencies.GetName()));
                ScrollLbox(scanId, scanListView, true);

            }
            else
            {
                cutLog(steam_srch.logContainer, settings.logCount);
                if (message == "Not found")
                    addcurr = false;
                steam_srch.logContainer.Add(new MainScanItem.LogItem(color, message, DateTime.Now, addcurr, steam_srch.currencies.GetName()));
                ScrollLbox(scanId, recentListView, false);
            }

        }


        public void Event_Message(object sender, object data, int searchId, flag myflag, bool isMain)
        {
            if (data == null)
                return;

            string message = data.ToString();

            switch (myflag)
            {
                case flag.Already_logged:

                    AddtoLog(Strings.AlreadyLogged);
                    StatusLabel1.Text = Strings.AlreadyLogged;
                    GetAccInfo((StrParam)data);
                    break;

                case flag.Login_success:
                    relog = false;
                    AddtoLog(Strings.LoginSucc);
                    StatusLabel1.Text = Strings.LoginSucc;
                    GetAccInfo((StrParam)data);
                    break;

                case flag.Login_cancel:

                    MessageBox.Show(Strings.ErrLogin + message, Strings.Attention, MessageBoxButtons.OK, MessageBoxIcon.Warning);

                    StatusLabel1.Text = message;

                    SetButton(loginButton, Strings.Login, 1);
                    ProgressBar1.Visible = false;
                    break;

                case flag.Logout_:
                    StatusLabel1.Text = Strings.Logouted;
                    SetButton(loginButton, Strings.Login, 1);
                    setNotifyText(Strings.NotLogged);
                    break;

                case flag.Send_cancel:

                    if (isMain)
                    {
                        var scanItem = scanItems[searchId];
                        scanItem.Steam.CancelScan();

                        var ourItem = scanItem.Steam.scanInput;
                        ourItem.StatId = status.Ready;
                         setStatImg(searchId, ourItem.StatId, scanListView);

                        if (scanListView.SelectedIndices[0] == searchId)
                        {
                            BindToControls(scanListView);
                        }
                    }
                    else
                    {
                        var ourItem = steam_srch.recentInputList[searchId];
                       
                        if (isLastProc())
                                    steam_srch.CancelListed();
                        ourItem.StatId = status.Ready;

                        setStatImg(searchId, ourItem.StatId, recentListView);

                        if (recentListView.SelectedIndices[0] == searchId)
                        {
                            BindToControls(recentListView);
                        }
                    }

                    break;

                case flag.ReLogin:
                    if (relog == false)
                    {
                        relog = true;
                        steam_srch.Logged = false;
                        steam_srch.Login();
                        ProgressBar1.Visible = true;
                        StatusLabel1.Text = Strings.Relogin;
                        SetButton(loginButton, Strings.Cancel, 3);
                    }

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
                    AddToScanLog(message, searchId, 1, true, isMain);
                    break;

                case flag.Price_btext:
                    AddToScanLog(message, searchId, 2, true, isMain);
                    break;

                case flag.Price_text:
                    AddToScanLog(message, searchId, 0, true, isMain);
                   break;

                case flag.Rep_progress:
                    ProgressBar1.Value = Convert.ToInt32(message);
                    break;
                case flag.SetHeadName:

                    var info = (StrParam)data;

                        var item = scanItems[searchId].Steam.scanInput;
                        item.Name = info.P1;
                        item.ImgLink = info.P2;

                        scanListView.Items[searchId].SubItems[1].Text = info.P1;
                        SetColumnWidths(scanListView, true);

                        if (scanListView.SelectedIndices[0] == searchId)
                        {
                            BindToControls(scanListView);
                        }

                    break;
                    
                case flag.Scan_progress:
                    StatusLabel1.Text = Strings.ScanPrice;
                    break;

                case flag.Success_buy:
                    PlaySound(0, settings.playSnd);
                    FlashWindow.Flash(this);

                    StatusLabel1.Text = Strings.Bought;
                    label5.Text = message;
                    buyNowButton.Enabled = true;
                    break;

                case flag.Error_buy:
                    PlaySound(1, settings.playSnd);
                    StatusLabel1.Text = Strings.BuyError;
                    AddToScanLog(message, searchId, 1, false, isMain);

                    buyNowButton.Enabled = true;
                    break;
                case flag.Error_scan:
                    StatusLabel1.Text = Strings.ScanError;
                    string mess = GetScanErrMess(message);
                    
                    if (isMain)
                        AddtoLog(scanItems[searchId].Steam.scanInput.Name + ": " + mess);
                    else
                        AddtoLog("Recent Scan Error: " + mess);

                    AddToScanLog(mess, searchId, 1, false, isMain);
                    buyNowButton.Enabled = true;
                    break;

                case flag.Scan_cancel:
                    StatusLabel1.Text = Strings.ScanCancel;
                    break;
                case flag.Resold:
                    PlaySound(2, settings.playSnd);
                    FlashWindow.Flash(this);

                    StatusLabel1.Text = string.Format("Item \"{0}\" resold!", message);
                    break;

                case flag.ResellErr:
                    PlaySound(3, settings.playSnd);
                    StatusLabel1.Text = string.Format("Resell Error, Item: \"{0}\"", message);
                    break;

                case flag.InvPrice:
                    var lowprice = ((StrParam)data).P1;
                    InventoryList.Items[searchId].SubItems[3].Text = lowprice;
                    filteredInvList[searchId].Price = SteamSite.GetSweetPrice(lowprice);
                    textBox1.Text = lowprice;
                    textBox1_KeyUp(this, null);

                    textBox1.ReadOnly = false;

                    //sold in the last 24 hours
                    //((StrParam)data).P2;

                    break;
                case flag.ActPrice:
                    var lowprice2 = ((StrParam)data).P1;
                 

                    if (isFirstTab)
                    {
                        scanItems[searchId].Steam.scanInput.ResellPrice = lowprice2;
                        scanItems[searchId].Steam.scanInput.ResellType = 2;
                        if (scanListView.SelectedIndices[0] == searchId)
                        {
                            BindToControls(scanListView);
                       }
                    }
                    else
                    {

                        steam_srch.recentInputList[searchId].ResellPrice = lowprice2;
                        steam_srch.recentInputList[searchId].ResellType = 2;

                       if (recentListView.SelectedIndices[0] == searchId)
                        {
                            BindToControls(recentListView);
                        }
    
                    }

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
                        StatusLabel1.Text = "Set prices before Selling!";

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
                            addScanItem(currItem, 3000, 0, false, 0, status.Ready);
                        }
                        scanItems.UpdateIds();
                        addonComplete = false;
                    }
                    else
                        SetColumnWidths(FoundList, true);


                    searchButton.Enabled = true;

                    break;

                case flag.Inventory_Loaded:

                    SetInvFilter();

                    label4.Text = filteredInvList.Count.ToString();
                    button1.Enabled = true;

                    if (searchId == 0)
                    {
                        FillInventoryList();

                        SellButton.Enabled = true;
                        if (comboBox3.SelectedIndex != 8)
                        button2.Enabled = true;
                        else button2.Enabled = false;
                    }
                    else
                    {
                        InventoryList.Items.Clear();
                        button2.Enabled = false;
                        MessageBox.Show("Inventory section is empty!", Strings.Attention, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                    break;
            }

        }

        private void FillInventoryList()
        {
            InventoryList.Items.Clear();

            for (int i = 0; i < filteredInvList.Count; i++)
            {
                var ourItem = filteredInvList[i];

                string priceRes;
                if (ourItem.Price == "0")
                    priceRes = Strings.None;
                else if (ourItem.Price == "1")
                    priceRes = Strings.NFS;
                else
                    priceRes = MainScanItem.LogItem.DoFracture(ourItem.Price);

                string[] row = { "", ourItem.Type, ourItem.Name, priceRes };

                var lstItem = new ListViewItem(row);
                InventoryList.Items.Add(lstItem);
            }
            SetColumnWidths(InventoryList, true);
        }

        private void SetInvFilter()
        {
            filteredInvList.Clear();

            if (textBox2.Text == string.Empty)
            {
                filteredInvList = new List<SteamSite.InventItem>(steam_srch.inventList);
            }
            else
            {
                switch (filterTypeBox.SelectedIndex)
                {
                    case 0:
                        filteredInvList = steam_srch.inventList.Where(x => (x.Type.StartsWith(textBox2.Text, StringComparison.OrdinalIgnoreCase))).ToList();
                        break;
                    case 1:
                        filteredInvList = steam_srch.inventList.Where(x => (x.Name.StartsWith(textBox2.Text, StringComparison.OrdinalIgnoreCase))).ToList();
                        break;
                    case 2:
                        filteredInvList = steam_srch.inventList.Where(x => (x.Price.StartsWith(textBox2.Text, StringComparison.OrdinalIgnoreCase))).ToList();
                        break;

                    default:
                        filteredInvList = new List<SteamSite.InventItem>(steam_srch.inventList);
                        break;
                }
            }
        }


        public static void GetDownLbox(ListBox lbox)
        {
            lbox.SelectedIndex = lbox.Items.Count - 1;
            lbox.SelectedIndex = -1;
        }

        public void ScrollLbox(int input, ListView lst, bool ismain)
        {

            if (ismain)
            {
                if (lst.SelectedIndices.Count != 0)
                {
                    if (lst.SelectedIndices[0] == input)
                    {
                        GetDownLbox(logListBox);
                    }
                }
            }
            else
            {
                GetDownLbox(logListBox);
            }
           
        }

        private void cutLog(System.ComponentModel.BindingList<MainScanItem.LogItem> bindingList, int limit)
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
                if (isFirstTab)
                {
                    if (FoundList.SelectedIndices.Count != 0)
                    {
                        searchBox.Text = string.Format("\"{0}\"", steam_srch.searchList[FoundList.SelectedItems[0].Index].Game);
                        searchButton.PerformClick();

                        addonComplete = true;
                    }
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
                    steam_srch.scanInput = new saveTab(ourItem.Link);
                    steam_srch.ScanPrices();
                    buyNowButton.Enabled = false;
                    StatusLabel1.Text = string.Format(Strings.buyingProc, ourItem.Name);
                }
            }
            else 
                searchFirstMess(); 

        }


        void addtoPage()
        {
            for (int i = 0; i < FoundList.CheckedItems.Count; i++)
            {
                var viewName = FoundList.CheckedItems[i].SubItems[2].Text;
                var ourItem = steam_srch.searchList.Find(item => item.Name == viewName);

                addScanItem(ourItem, 3000, 0, false, 0, status.Ready);
            }
            scanItems.UpdateIds();
        }

        void addtoRecent()
        {
            for (int i = 0; i < FoundList.CheckedItems.Count; i++)
            {
                var viewName = FoundList.CheckedItems[i].SubItems[2].Text;
                var ourItem = steam_srch.searchList.Find(item => item.Name == viewName);
                addRecentItem(ourItem, status.Ready);
            }

        }


        private void selectedItemToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (searchRight())
            {
                if (isFirstTab)
                {
                    addtoPage();
                   
                }
                else
                {

                    addtoRecent();
                }
            }
            else
                searchFirstMess();
        }


        private void checkedToBothSourcesToolStripMenuItem_Click(object sender, EventArgs e)
        {
                  addtoPage();
                  addtoRecent();
        }

        private void addScanItem(SteamSite.SearchItem ourItem, int delay, int buyQuant, bool tobuy, int resellType, status Stat)
        {
            string[] row = { string.Empty, ourItem.Name, ourItem.StartPrice };
            var lstItem = new ListViewItem(row);
            scanListView.Items.Add(lstItem);

            var ourTab = new saveTab(ourItem.Name, ourItem.Link, ourItem.ImgLink, ourItem.StartPrice, delay, buyQuant, tobuy, resellType, ourItem.StartPrice, Stat);
            scanItems.Add(new MainScanItem(ourTab, steam_srch.cookieCont, new eventDelegate(Event_Message), steam_srch.currencies.Current, settings.ignoreWarn, settings.resellDelay));
            setStatImg(scanListView.Items.Count - 1, (status)Convert.ToByte(!isScanValid(ourTab, true)), scanListView);
            SetColumnWidths(scanListView, true);
        }

        private void addRecentItem(SteamSite.SearchItem ourItem, status Stat)
        {
            string[] row = { string.Empty, ourItem.Name, ourItem.StartPrice };
            var lstItem = new ListViewItem(row);
            recentListView.Items.Add(lstItem);

            var ourTab = new saveTab(ourItem.Name, ourItem.Link, ourItem.ImgLink, ourItem.StartPrice, steam_srch.mainDelay, 0, false, 0, ourItem.StartPrice, Stat);
            steam_srch.recentInputList.Add(ourTab);

            setStatImg(recentListView.Items.Count - 1, (status)Convert.ToByte(!isScanValid(ourTab, false)), recentListView);
            SetColumnWidths(recentListView, true);
        }


        private void emptyTabToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (isFirstTab)
            {
                addScanItem(new SteamSite.SearchItem(string.Empty, string.Empty, string.Empty, "1", "0", string.Empty), 3000, 0, false, 0, status.Warning);
                scanItems.UpdateIds();
            }
            else
            {
                addRecentItem(new SteamSite.SearchItem(string.Empty, string.Empty, string.Empty, "1", "0", string.Empty), status.Warning);
            }
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
            FlashWindow.Flash(this);

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
                SaveBinary(cockPath, steam_srch.cookieCont);
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
                    steam_srch.Password = Decrypt(settings.lastPass);
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
            if (searchRight() && (steam_srch.searchList.Count == FoundList.Items.Count) && (FoundList.SelectedIndices.Count !=0)) 
            StartCmdLine(steam_srch.searchList[FoundList.SelectedIndices[0]].Link, string.Empty, false);

        }

        private void button1_Click(object sender, EventArgs e)
        {

            if (steam_srch.Logged)
            {
                lastSelec = -1;

                button1.Enabled = false;
                if (comboBox3.SelectedIndex == 8)
                {
                    steam_srch.LoadOnSale = true;
                    SellButton.Text = Strings.RemSell;
                    steam_srch.isRemove = true;
                    textBox1.Clear();
                    textBox3.Clear();
                    textBox1.ReadOnly = true;
                    textBox3.ReadOnly = true;
                    button2.Enabled = false;
                }
                else
                {
                    steam_srch.LoadOnSale = false;
                    steam_srch.invApp = comboBox3.SelectedIndex;
                    SellButton.Text = Strings.Sell;
                    steam_srch.isRemove = false;
                    textBox1.ReadOnly = false;
                    textBox3.ReadOnly = false;
                }

                SellButton.Enabled = false;
                InventoryList.Items.Clear();
                steam_srch.loadInventory();

            } else
                MessageBox.Show(Strings.LoginFirst, Strings.Attention, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
       

        private void SellButton_Click(object sender, EventArgs e)
        {

            if (filteredInvList.Count == 0)
            {
                MessageBox.Show(Strings.LoadInvFirst, Strings.Attention, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (InventoryList.CheckedItems.Count == 0)
            {
                MessageBox.Show(Strings.CheckIt, Strings.Attention, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }


            steam_srch.invApp = comboBox3.SelectedIndex;
            steam_srch.toSellList.Clear();

            for (int i = 0; i < InventoryList.CheckedItems.Count; i++)
            {
                var ouritem = filteredInvList[InventoryList.CheckedItems[i].Index];
                if ((ouritem.Marketable) && (ouritem.Price != "0"))
                {
                    steam_srch.toSellList.Add(new SteamSite.ItemToSell(ouritem.AssetId, CalcWithFee(ouritem.Price)));
                }
            }

            if (steam_srch.toSellList.Count != 0)
            {

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

                steam_srch.sellDelay = Convert.ToInt32(sellDelayBox.Text);
                steam_srch.isDelayRand = randomDelayBox.Checked;
                steam_srch.ItemSell();
            }
            else
                MessageBox.Show("Set prices first!", Strings.Attention, MessageBoxButtons.OK, MessageBoxIcon.Warning);
  
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
            if (steam_srch.Logged && steam_srch.inventList.Count != 0 && e.Column != 0)
            {
                filteredInvList.Clear();

                //Sorry for copypaste

                if (sortDirect)
                {
                    switch (e.Column)
                    {
                        case 1:

                            filteredInvList = steam_srch.inventList.OrderBy(foo => foo.Type).ToList();
                            break;

                        case 2:
                            filteredInvList = steam_srch.inventList.OrderBy(foo => foo.Name).ToList();
                            break;

                        case 3:
                            filteredInvList = steam_srch.inventList.OrderBy(foo => foo.Price).ToList();
                            break;
                    }
                }
                else
                {
                    switch (e.Column)
                    {
                        case 1:

                            filteredInvList = steam_srch.inventList.OrderByDescending(foo => foo.Type).ToList();
                            break;

                        case 2:
                            filteredInvList = steam_srch.inventList.OrderByDescending(foo => foo.Name).ToList();
                            break;

                        case 3:
                            filteredInvList = steam_srch.inventList.OrderByDescending(foo => foo.Price).ToList();
                            break;
                    }

                }
                

                FillInventoryList();
                sortDirect = !sortDirect;
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
                Pen silverPen = new Pen(Color.Silver, 4);

                e.Graphics.DrawLine(silverPen, x1, y1, x2, y1);
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

                Pen silverPen = new Pen(Color.Silver, 4);

                e.Graphics.DrawLine(silverPen, y1, x1, y1, x2);
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


        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (comboBox3.SelectedIndex != 8)
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


                for (int i = 0; i < InventoryList.SelectedIndices.Count; i++)
                {
                    var ourItem = filteredInvList[InventoryList.SelectedIndices[i]];
                    ourItem.Price = truePrice;
                    InventoryList.SelectedItems[i].SubItems[3].Text = textBox1.Text;
                }

            }
        }


       



        private void setButtText(status stat)
        {
            switch (stat)
            {
                case status.Ready:
                    scanButton.Text = Strings.Start;
                    scanButton.Image = Properties.Resources.start;
                    scanButton.Enabled = true;
                    break;
                case status.Warning:
                    scanButton.Text = Strings.Warning;
                    scanButton.Image = Properties.Resources.warning;
                    scanButton.Enabled = false;
                    break;
                case status.InProcess:
                    scanButton.Text = Strings.Stop;
                    scanButton.Image = Properties.Resources.stop;
                    scanButton.Enabled = true;
                    break;
            }

        }


        private void linkBinding_BindingComplete(object sender, BindingCompleteEventArgs e)
        {
            //e.Binding.PropertyName;
            if (isFirstTab)
            {

                var Item = scanItems[scanListView.SelectedIndices[0]];

                var ourItem = Item.Steam.scanInput;

                scanListView.SelectedItems[0].SubItems[2].Text = ourItem.Price;

                if (Item.Steam.scaninProg)
                {
                    return;
                }

                if (isScanValid(ourItem, true))
                {
                    ourItem.StatId = status.Ready;
                    setStatImg(scanListView.SelectedIndices[0], ourItem.StatId, scanListView);
                }
                else
                {
                    ourItem.StatId = status.Warning;
                    setStatImg(scanListView.SelectedIndices[0], ourItem.StatId, scanListView);
                }

                setButtText(ourItem.StatId);
            }
            else
            {
                var Item = steam_srch.recentInputList[recentListView.SelectedIndices[0]];

                recentListView.SelectedItems[0].SubItems[2].Text = Item.Price;

                if (steam_srch.scaninProg)
                {
                    return;
                }

                if (isScanValid(Item, false))
                {
                    Item.StatId = status.Ready;
                    setStatImg(recentListView.SelectedIndices[0], status.Ready, recentListView);
                }
                else
                {
                    Item.StatId = status.Warning;
                    setStatImg(recentListView.SelectedIndices[0], status.Warning, recentListView);
                }
                setButtText(Item.StatId);
            }
        }

        private void nameBinding_BindingComplete(object sender, BindingCompleteEventArgs e)
        {
            if (isFirstTab)
            {
                var Item = scanItems[scanListView.SelectedIndices[0]];

                var ourItem = Item.Steam.scanInput;

                scanListView.SelectedItems[0].SubItems[1].Text = ourItem.Name;
                SetColumnWidths(scanListView, true);
            }
            else
            {
                var Item = steam_srch.recentInputList[recentListView.SelectedIndices[0]];
                recentListView.SelectedItems[0].SubItems[1].Text = Item.Name;
                SetColumnWidths(recentListView, true);
            }

        }	

        private void scanButton_Click(object sender, EventArgs e)
        {
            //Nope. TODO: Clean this shit!
            if (isFirstTab)
            {

                int id = scanListView.SelectedIndices[0];
                var ourItem = scanItems[id];
                var steamItem = ourItem.Steam;
                var paramItem = steamItem.scanInput;

                if (!steamItem.scaninProg)
                {

                    if (isScanValid(paramItem, true))
                    {

                        startScan(false);

                    }
                    else
                    {
                        paramItem.StatId = status.Warning;
                        MessageBox.Show(Strings.CheckVal, Strings.Attention, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }

                }

                else
                {
                    stopScan(false);
                }

                setButtText(paramItem.StatId);
                setStatImg(id, paramItem.StatId, scanListView);
            }
            else
            {
                if (steam_srch.Logged)
                {
                    int indx = recentListView.SelectedIndices[0];

                    //First run
                    if (!steam_srch.scaninProg)
                    {
                        if (steam_srch.recentInputList.Count != 0)
                        {

                            if (recentListView.SelectedIndices.Count !=0)
                            {
                                steam_srch.recentInputList[indx].StatId = status.InProcess;
                                setStatImg(indx, steam_srch.recentInputList[indx].StatId, recentListView);
                                steam_srch.ScanNewListed();
                            }

                        }
                        else MessageBox.Show("Add some Items to list.", Strings.Attention, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else
                    {
                        if (recentListView.SelectedIndices.Count != 0)
                        {
                            if (steam_srch.recentInputList[indx].StatId == status.InProcess)
                            {
                                if (isLastProc())
                                    steam_srch.CancelListed();

                                steam_srch.recentInputList[indx].StatId = status.Ready;
                            }
                            else if (steam_srch.recentInputList[indx].StatId == status.Ready)
                            {
                                steam_srch.recentInputList[indx].StatId = status.InProcess;
                            }

                            setStatImg(indx, steam_srch.recentInputList[indx].StatId, recentListView);
                        }
                       
                    }

                    setButtText(steam_srch.recentInputList[recentListView.SelectedIndices[0]].StatId);


                }
                else
                    MessageBox.Show(Strings.LoginFirst, Strings.Attention, MessageBoxButtons.OK, MessageBoxIcon.Warning);


            }
        }

        private bool isLastProc()
        {
            int inproc = 0;
            for (int i = 0; i < steam_srch.recentInputList.Count; i++)
            {
                if (steam_srch.recentInputList[i].StatId == status.InProcess)
                    inproc++;
            }

            if (inproc == 1)
                return true;
            else
                return false;
        }


        private void startScan(bool all)
        {

            if (steam_srch.Logged)
            {

                if (isFirstTab)
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
                        var paramItem = steamItem.scanInput;

                        if (steamItem.scaninProg)
                            continue;

                        if (paramItem.StatId == status.Ready)
                        {

                            steamItem.ScanPrices();

                            paramItem.StatId = status.InProcess;
                            setStatImg(id, paramItem.StatId, scanListView);
                        }
                    }

                    StatusLabel1.Text = Strings.ScanPrice;

                    if (all)
                    {
                        DoSelect(false);
                    }

                }
                else
                {


                    bool isReady = false;
                    if (steam_srch.recentInputList.Count != 0)
                    {
                        if (all)
                        {

                            for (int i = 0; i < steam_srch.recentInputList.Count; i++)
                            {
                                if (steam_srch.recentInputList[i].StatId == status.Ready)
                                {
                                    isReady = true;
                                    steam_srch.recentInputList[i].StatId = status.InProcess;
                                    setStatImg(i, steam_srch.recentInputList[i].StatId, recentListView);
                                }
                            }
                        }
                        else
                        {
                            for (int i = 0; i < recentListView.SelectedIndices.Count; i++)
                            {
                                int indx = recentListView.SelectedIndices[i];
                                if (steam_srch.recentInputList[indx].StatId == status.Ready)
                                {
                                    isReady = true;
                                    steam_srch.recentInputList[indx].StatId = status.InProcess;
                                    setStatImg(indx, steam_srch.recentInputList[indx].StatId, recentListView);
                                }
                            }

                        }


                    }
                    else MessageBox.Show("Add some Items to list.", Strings.Attention, MessageBoxButtons.OK, MessageBoxIcon.Warning);

                    if ((isReady) && (!steam_srch.scaninProg))
                    {
                           steam_srch.ScanNewListed();
                    }

                }
            }
            else MessageBox.Show(Strings.LoginFirst, Strings.Attention, MessageBoxButtons.OK, MessageBoxIcon.Warning);


        }


        private void stopScan(bool all)
        {

            if (steam_srch.Logged)
            {
                if (isFirstTab)
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
                        var paramItem = steamItem.scanInput;

                        if (steamItem.scaninProg)
                        {
                            steamItem.CancelScan();
                            paramItem.StatId = status.Ready;
                            setStatImg(id, 0, scanListView);
                        }
                    }

                    if (all)
                    {
                        DoSelect(false);
                    }
                }
                else
                {
                    bool isReady = false;
                    if (steam_srch.recentInputList.Count != 0)
                    {

                        if (all)
                        {
                            for (int i = 0; i < steam_srch.recentInputList.Count; i++)
                            {
                                if (steam_srch.recentInputList[i].StatId == status.InProcess)
                                {
                                    isReady = true;
                                    steam_srch.recentInputList[i].StatId = status.Ready;
                                    setStatImg(i, steam_srch.recentInputList[i].StatId, recentListView);
                                }
                            }

                        }
                        else
                        {
                            for (int i = 0; i < recentListView.SelectedIndices.Count; i++)
                            {
                                int indx = recentListView.SelectedIndices[i];
                                if (steam_srch.recentInputList[indx].StatId == status.InProcess)
                                {
                                    isReady = true;
                                    steam_srch.recentInputList[indx].StatId = status.Ready;
                                    setStatImg(indx, steam_srch.recentInputList[indx].StatId, recentListView);
                                }
                            }
                        }
                    }
                    else MessageBox.Show("Add some Items to list.", Strings.Attention, MessageBoxButtons.OK, MessageBoxIcon.Warning);

                    if ((isReady) && (steam_srch.scaninProg))
                    {
                            steam_srch.CancelListed();
                    }
                }

            }
            else MessageBox.Show(Strings.LoginFirst, Strings.Attention, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }



        private void setStatImg(int id, status stat, ListView lst)
        {
            lst.Items[id].ImageIndex = (byte)stat;
        }

        private void logListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            try
            {
                BindingList<MainScanItem.LogItem> logItem;

                if (isFirstTab)
                {
                    if ((scanListView.SelectedIndices.Count == 0) | (e.Index < 0))
                        return;

                    logItem = scanItems[scanListView.SelectedIndices[0]].LogCont;
                }
                else
                {
                    logItem = steam_srch.logContainer;
                }

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
            if (isFirstTab)
            {

                if ((scanListView.SelectedItems.Count != 0) && (scanItems.Count != 0))
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
                    BindToControls(scanListView);
                    SetColumnWidths(scanListView, true);
                }
            }
            else
            {
                if (recentListView.SelectedItems.Count != 0)
                {
                    for (int i = 0; i < recentListView.Items.Count; i++)
                    {
                        if (recentListView.Items[i].Selected)
                        {
                            var Items = steam_srch.recentInputList;

                            if (isLastProc())
                                steam_srch.CancelListed();

                            Items.RemoveAt(i);
                            recentListView.Items[i].Remove();
                            i--;
                        }
                    }

                    //TODO - Select last standing Item
                    BindToControls(recentListView);
                    SetColumnWidths(recentListView, true);
                }
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


        private void BindToControls(ListView lst)
        {
            if (lst.SelectedIndices.Count == 1)
            {

                try
                {

                    panel1.Enabled = true;

                    saveTab ourItem;
                    logListBox.DataBindings.Clear();

                    if (isFirstTab)
                    {
                        var Item = scanItems[scanListView.SelectedIndices[0]];
                        ourItem = Item.Steam.scanInput;

                        logListBox.DataSource = Item.LogCont;
                        logListBox.DisplayMember = "Text";

                        delayTextBox.DataBindings.Clear();
                        delayTextBox.DataBindings.Add("Text", ourItem, "Delay");

                        setButtText(Item.Steam.scanInput.StatId);

                    }
                    else
                    {
                        ourItem = steam_srch.recentInputList[recentListView.SelectedIndices[0]];

                        logListBox.DataSource = steam_srch.logContainer;
                        logListBox.DisplayMember = "Text";

                        delayTextBox.DataBindings.Clear();
                        delayTextBox.DataBindings.Add("Text", steam_srch, "mainDelay");

                        setButtText(ourItem.StatId);
                    }

                    //Add event for all controls? I don't sure..

                    linkTextBox.DataBindings.Clear();
                    Binding linkBinding = linkTextBox.DataBindings.Add("Text", ourItem, "Link", true, DataSourceUpdateMode.OnPropertyChanged);
                    linkBinding.BindingComplete += new BindingCompleteEventHandler(linkBinding_BindingComplete);

                    wishpriceBox.DataBindings.Clear();
                    Binding wishedBinding = wishpriceBox.DataBindings.Add("Text", ourItem, "Price", true, DataSourceUpdateMode.OnPropertyChanged);
                    wishedBinding.BindingComplete += new BindingCompleteEventHandler(linkBinding_BindingComplete);

                    nameTextBox.DataBindings.Clear();
                    Binding nameBinding = nameTextBox.DataBindings.Add("Text", ourItem, "Name", true, DataSourceUpdateMode.OnPropertyChanged);
                    nameBinding.BindingComplete += new BindingCompleteEventHandler(nameBinding_BindingComplete);

                    buyCheckBox.DataBindings.Clear();
                    buyCheckBox.DataBindings.Add("Checked", ourItem, "ToBuy");

                    buyUpDown.DataBindings.Clear();
                    buyUpDown.DataBindings.Add("Value", ourItem, "BuyQnt");

                    resellComboBox.DataBindings.Clear();
                    resellComboBox.DataBindings.Add("SelectedIndex", ourItem, "ResellType");

                    resellPriceBox.DataBindings.Clear();
                    resellPriceBox.DataBindings.Add("Text", ourItem, "ResellPrice");



                    if (logListBox.Items.Count != 0)
                    {
                        GetDownLbox(logListBox);
                    }

                    if (ourItem.ImgLink != string.Empty)
                        StartLoadImgTread(string.Format(SteamSite.fndImgUrl, ourItem.ImgLink), pictureBox4);
                    else
                        pictureBox4.Image = null;

                }
                catch (Exception)
                {
                    //dummy
                   // throw;
                }

            }


            else
            {
                panel1.Enabled = false;
            }


        }

        private void scanListView_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                bool block = !(scanListView.SelectedIndices.Count == 0);

                startSelectedMenuItem.Enabled = block;
                stopSelectedMenuItem.Enabled = block;
                deleteMenuItem.Enabled = block;

                bool block2 = !(scanListView.Items.Count == 0);

                startAllMenuItem.Enabled = block2;
                stopAllMenuItem.Enabled = block2;
                exportListToolStripMenuItem.Enabled = block2;
            }
           
        }

        private void splitContainer3_SplitterMoved(object sender, SplitterEventArgs e)
        {
            SetColumnWidths(scanListView, true);
        }

        private void DonateBox_Click(object sender, EventArgs e)
        {
            StartCmdLine(Strings.donateLink, string.Empty, false);
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            string Link = string.Empty;

            if (isFirstTab)
            {
                Link = scanItems[scanListView.SelectedIndices[0]].Steam.scanInput.Link;
            }
            else
            {
                Link = steam_srch.recentInputList[recentListView.SelectedIndices[0]].Link;

            }

            if (Link.Contains("http://"))
                StartCmdLine(Link, string.Empty, false);
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            isFirstTab = !Convert.ToBoolean(((TabControl)sender).SelectedIndex);
            if (isFirstTab)
            {
                scanListView.Focus();
                BindToControls(scanListView);

                button4.Visible = true;
                logListBox.Height -= 20;
            }
            else
            {
                recentListView.Focus();
                BindToControls(recentListView);

                button4.Visible = false;
                logListBox.Height += 20;
            }
            logListBox.Refresh();
        }

        private void recentListView_MouseUp(object sender, MouseEventArgs e)
        {
            //damn you, copypaste!
            if (e.Button == MouseButtons.Right)
            {
                bool block = !(recentListView.SelectedIndices.Count == 0);

                startSelectedMenuItem.Enabled = block;
                stopSelectedMenuItem.Enabled = block;
                deleteMenuItem.Enabled = block;

                if (recentListView.Items.Count == 0)
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
           
        }

        private void scanListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindToControls((ListView)sender);
        }

        private void FoundList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (FoundList.SelectedItems.Count == 1)
            {
                var ourItem = steam_srch.searchList[FoundList.SelectedIndices[0]];
                StartLoadImgTread(string.Format(SteamSite.fndImgUrl, ourItem.ImgLink), pictureBox1);
            }
            else
                pictureBox1.Image = null;
        }

        private void InventoryList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (InventoryList.SelectedItems.Count == 1)
            {

                var lit = InventoryList.SelectedItems[0];

                if (lastSelec != lit.Index)
                {
                    var ourItem = filteredInvList[InventoryList.SelectedIndices[0]];
                    StartLoadImgTread(string.Format(SteamSite.fndImgUrl, ourItem.ImgLink), pictureBox3);

                    var currPr = InventoryList.SelectedItems[0].SubItems[3].Text;

                    if (currPr == Strings.None)
                    {
                        if (settings.loadActual)
                        {
                            steam_srch.GetPriceTread(SteamSite.GetUrlApp(steam_srch.invApp, false).App, Uri.EscapeDataString(ourItem.MarketName), lit.Index, true);
                            textBox1.Text = Strings.Loading;
                            textBox1.ReadOnly = true;
                        }
                    }
                    else
                        if (currPr == Strings.NFS)
                            textBox1.ReadOnly = true;
                        else
                            textBox1.Text = currPr;

                    lastSelec = lit.Index;
                }

                textBox1_KeyUp(this, null);
            }
            else pictureBox3.Image = null;
        }


        public static StrParam GetFromLink(string url)
        {
            string markname = string.Empty;

            string appId = Regex.Match(url, "(?<=listings/)(.*)(?=/)").ToString();

            if (url.Contains('?'))
                markname = Regex.Match(url, "(?<=" + appId + @"/)(.*)(?=\?)").ToString();
            else
                markname = url.Substring(url.IndexOf(appId) + 4);

            return new StrParam(appId, markname);
        }


        private void resellComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (settings.loadActual)
            {
                if (ResComboClicked)
                {
                    ResComboClicked = false;

                    if ((resellComboBox.SelectedIndex == 2) && (steam_srch.Logged))
                    {
                        if (isFirstTab)
                        {
                            var ourItem = scanItems[scanListView.SelectedIndices[0]].Steam.scanInput;
                            var param = GetFromLink(ourItem.Link);
                            steam_srch.GetPriceTread(param.P1, param.P2, scanListView.SelectedIndices[0], false);
                        }
                        else
                        {
                            var ourItem = steam_srch.recentInputList[recentListView.SelectedIndices[0]];
                            var param = GetFromLink(ourItem.Link);
                            steam_srch.GetPriceTread(param.P1, param.P2, recentListView.SelectedIndices[0], false);
                        }

                        resellPriceBox.Text = Strings.Loading;
                    }
                }
            }
        }


        private void pictureBox3_Click(object sender, EventArgs e)
        {
            if ((filteredInvList.Count != 0) && (InventoryList.SelectedIndices.Count != 0))
            {
                var ourItem = filteredInvList[InventoryList.SelectedIndices[0]];
                StartCmdLine(ourItem.PageLnk, string.Empty, false);
            }
           

        }


        private void button3_Click_1(object sender, EventArgs e)
        {
            //Buggy?
            int sel = 0;

            if (isFirstTab)
            {
                sel = scanListView.SelectedIndices[0];
                var tocopy = new MainScanItem(new saveTab(scanItems[sel].Steam.scanInput), steam_srch.cookieCont, new eventDelegate(Event_Message), steam_srch.currencies.Current, settings.ignoreWarn, settings.resellDelay);
                tocopy.Steam.scanInput.Name += " Copy";
                scanItems.Insert(sel + 1, tocopy);

                scanListView.Items.Insert(sel, (ListViewItem)scanListView.Items[sel].Clone());
                scanListView.Items[sel + 1].SubItems[1].Text = tocopy.Steam.scanInput.Name;
                SetColumnWidths(scanListView, true);

                scanItems.UpdateIds();

                BindToControls(scanListView);
                
            }
            else
            {
                sel = recentListView.SelectedIndices[0];
                var tocopy = new saveTab(steam_srch.recentInputList[sel]);
                tocopy.Name += " Copy";
                steam_srch.recentInputList.Insert(sel + 1, tocopy);
                recentListView.Items.Insert(sel, (ListViewItem)recentListView.Items[sel].Clone());
                recentListView.Items[sel + 1].SubItems[1].Text = tocopy.Name;
                SetColumnWidths(recentListView, true);
               
                BindToControls(recentListView);
            }

        }


        private void ClearGraph()
        {
            graphFrm.chart1.Series.Clear();
            graphFrm.chart1.ChartAreas.Clear();

            graphFrm.chart1.Series.Add("line");
            graphFrm.chart1.Series[0].ChartType = SeriesChartType.Line;

            graphFrm.chart1.ChartAreas.Add("back");

            graphFrm.chart1.ChartAreas[0].AxisX.LabelStyle.Format = "HH:mm:ss";
            graphFrm.chart1.ChartAreas[0].AxisX.Interval = 0;
            graphFrm.chart1.ChartAreas[0].AxisX.IntervalType = DateTimeIntervalType.Seconds;

            graphFrm.chart1.ChartAreas[0].AxisY.LabelStyle.Format = "customY";

            graphFrm.chart1.Series[0].BorderWidth = 2;
            graphFrm.chart1.Series[0].MarkerStyle = MarkerStyle.Circle;

            graphFrm.currency = steam_srch.currencies.GetName();
        }


        private void button4_Click(object sender, EventArgs e)
        {
            if (isFirstTab && (scanListView.SelectedIndices.Count != 0))
            {
                ClearGraph();

                var sel = scanListView.SelectedIndices[0];
                var logLst = scanItems[sel].LogCont;

                if (logLst.Count == 0)
                {
                    MessageBox.Show(Strings.LogEmpty, Strings.Attention, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }


                graphFrm.Text = string.Format(Strings.GraphFor, scanItems[sel].Steam.scanInput.Name);

                for (int i = 0; i < logLst.Count; i++)
                {
                    if (logLst[i].AddCurr)
                        graphFrm.chart1.Series[0].Points.AddXY(logLst[i].Time, logLst[i].RawPrice);
                }

                graphFrm.chart1.ChartAreas[0].RecalculateAxesScale();

                int wished = Convert.ToInt32(SteamSite.GetSweetPrice(scanItems[sel].Steam.scanInput.Price));

                if (graphFrm.chart1.ChartAreas[0].AxisY.Maximum > wished)
                {
                    StripLine stripOut = new StripLine();
                    stripOut.IntervalOffset = wished;
                    stripOut.StripWidth = graphFrm.chart1.ChartAreas[0].AxisY.Maximum - wished;
                    stripOut.BackColor = Color.FromArgb(64, Color.Red);
                    graphFrm.chart1.ChartAreas[0].AxisY.StripLines.Add(stripOut);
                }
                else
                {
                    graphFrm.chart1.ChartAreas[0].AxisY.Maximum = wished;
                }

                StripLine stripWished = new StripLine();
                stripWished.IntervalOffset = 0;
                stripWished.StripWidth = wished;
                stripWished.BackColor = Color.FromArgb(64, Color.Green);

                graphFrm.chart1.ChartAreas[0].AxisY.StripLines.Add(stripWished);

                graphFrm.Show();
             
            }
        }

        private void scanListView_DragDrop(object sender, DragEventArgs e)
        {
            if (scanListView.SelectedItems.Count == 0)
            {
                return;
            }

            Point cp = scanListView.PointToClient(new Point(e.X, e.Y));
            ListViewItem dragToItem = scanListView.GetItemAt(cp.X, cp.Y);

            if (dragToItem == null)
            {
                return;
            }

            int dragIndex = dragToItem.Index;

            ListViewItem[] sel = new ListViewItem[scanListView.SelectedItems.Count];
            MainScanItem[] vsel = new MainScanItem[scanListView.SelectedItems.Count];

            for (int i = 0; i < scanListView.SelectedItems.Count; i++)
            {
                sel[i] = scanListView.SelectedItems[i];
                vsel[i] = scanItems[scanListView.SelectedIndices[i]];
            }

            for (int i = 0; i < sel.GetLength(0); i++)
            {
                ListViewItem dragItem = sel[i];
                MainScanItem vdragItem = vsel[i];

                int itemIndex = dragIndex;

                if (itemIndex == dragItem.Index)
                {
                    return;
                }

                bool isStart = false;
                if (vdragItem.Steam.scaninProg)
                {
                    vdragItem.Steam.CancelScan();
                    isStart = true;
                }


                if (dragItem.Index < itemIndex)
                    itemIndex++;
                else
                    itemIndex = dragIndex + i;


                MainScanItem insertVItem = new MainScanItem(new saveTab(vdragItem.Steam.scanInput), steam_srch.cookieCont, new eventDelegate(Event_Message), steam_srch.currencies.Current, settings.ignoreWarn, settings.resellDelay);
                insertVItem.LogCont = vdragItem.LogCont;

                if (isStart)
                {
                    insertVItem.Steam.ScanPrices();
                }

                scanItems.Insert(itemIndex, insertVItem);
                scanItems.Remove(vdragItem);
                scanItems.UpdateIds();

                ListViewItem insertItem = (ListViewItem)dragItem.Clone();
                scanListView.Items.Insert(itemIndex, insertItem);
                scanListView.Items.Remove(dragItem);
                insertItem.Selected = true;
            }

        }

        private void scanListView_ItemDrag(object sender, ItemDragEventArgs e)
        {
           ((ListView)sender).DoDragDrop(((ListView)sender).SelectedItems, DragDropEffects.Move);
        }

        private void scanListView_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if (steam_srch.Logged && steam_srch.inventList.Count != 0)
            {
                SetInvFilter();
                FillInventoryList();
            }
        }

        private void recentListView_DragDrop(object sender, DragEventArgs e)
        {
            if (recentListView.SelectedItems.Count == 0)
            {
                return;
            }

            Point cp = recentListView.PointToClient(new Point(e.X, e.Y));
            ListViewItem dragToItem = recentListView.GetItemAt(cp.X, cp.Y);

            if (dragToItem == null)
            {
                return;
            }

            int dragIndex = dragToItem.Index;

            ListViewItem[] sel = new ListViewItem[recentListView.SelectedItems.Count];
            saveTab[] vsel = new saveTab[recentListView.SelectedItems.Count];

            for (int i = 0; i < recentListView.SelectedItems.Count; i++)
            {
                sel[i] = recentListView.SelectedItems[i];
                vsel[i] = steam_srch.recentInputList[recentListView.SelectedIndices[i]];
            }

            for (int i = 0; i < sel.GetLength(0); i++)
            {
                ListViewItem dragItem = sel[i];
                saveTab vdragItem = vsel[i];

                int itemIndex = dragIndex;

                if (itemIndex == dragItem.Index)
                {
                    return;
                }


                if (dragItem.Index < itemIndex)
                    itemIndex++;
                else
                    itemIndex = dragIndex + i;


                saveTab insertVItem = new saveTab(vdragItem);

                steam_srch.recentInputList.Insert(itemIndex, insertVItem);
                steam_srch.recentInputList.Remove(vdragItem);

                ListViewItem insertItem = (ListViewItem)dragItem.Clone();
                recentListView.Items.Insert(itemIndex, insertItem);
                recentListView.Items.Remove(dragItem);
                insertItem.Selected = true;
            }

        }

        private void filterTypeBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBox2_TextChanged(sender, e);
        }

        private void Main_LocationChanged(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Normal)
            {
                lastFrmPos = this.Location;
            }
        }

        private void Main_ResizeEnd(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Normal)
            {
                lastFrmSize = this.Size;
            }
        }

        private void Main_Resize(object sender, EventArgs e)
        {
            splitContainer1.Refresh();
            splitContainer2.Refresh();
            splitContainer3.Refresh();
        }

        private void exportListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog1.FileName = "scmbot_list_" + DateTime.Now.ToString("dd_MM_yyyy");

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if (isFirstTab)
                {
                    saveTabLst lst = new saveTabLst();
                    SaveTabs(lst);
                    SaveBinary(saveFileDialog1.FileName, lst);
                }
                else
                {
                    SaveBinary(saveFileDialog1.FileName, steam_srch.recentInputList);
                }
            }
        }


        private void importListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {

                try
                {
                    saveTabLst lst = (saveTabLst)LoadBinary(openFileDialog1.FileName);
                    if (lst != null)
                    {

                        if (isFirstTab)
                        {
                            scanItems.Clear();
                            scanListView.Items.Clear();
                            LoadTabs(lst);
                        }
                        else
                        {
                            recentListView.Items.Clear();
                            steam_srch.recentInputList.Clear();
                            steam_srch.recentInputList = lst;
                            LoadRecent(steam_srch.recentInputList);
                        }
                    }
                    else
                        MessageBox.Show("Bin-file is not correct!", Strings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);

                }
                catch (Exception)
                {
                    //dummy
                }
            }
        }

        private void resellComboBox_Click(object sender, EventArgs e)
        {
            ResComboClicked = true;
        }

        private void usingProxyStatusToolStripMenuItem_Click(object sender, EventArgs e)
        {
            proxyStatForm.Show();
        }


        //Not Used Yet!
        private void scanListView_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            saveTabLst lst = new saveTabLst();
            SaveTabs(lst);

            scanItems.Clear();
            scanListView.Items.Clear();

            switch (e.Column)
            {
                case 0:
                    lst.Sort(delegate(saveTab p1, saveTab p2) { return p1.StatId.CompareTo(p2.StatId); });
                    break;

                case 1:
                    lst.Sort(delegate(saveTab p1, saveTab p2){return p1.Name.CompareTo(p2.Name);});
                    break;

                case 2:
                    lst.Sort(delegate(saveTab p1, saveTab p2) { return p1.Price.CompareTo(p2.Price); });
                    break;
            }

            LoadTabs(lst);

        }




        private void textBox1_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (textBox1.Text != string.Empty)
                    textBox3.Text = MainScanItem.LogItem.DoFracture(CalcWithFee(SteamSite.GetSweetPrice(textBox1.Text)));
            }
            catch (Exception)
            {

            }

        }

        private void textBox3_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (textBox3.Text != string.Empty)
                    textBox1.Text = MainScanItem.LogItem.DoFracture(AddFee(SteamSite.GetSweetPrice(textBox3.Text)));
            }
            catch (Exception)
            {

            }
        }


   }
}
