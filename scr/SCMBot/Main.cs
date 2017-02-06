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
using System.Net.NetworkInformation;
using Newtonsoft.Json;


// Внимание! Данная наработка - всего-лишь грубая реализация идеи.
// Код содержит множественные ошибки и костыли, бездумно копипастить не советую.
// Это альфа-версия, совершенно сырой продукт. Не предназначено для продажи, уважай чужой труд!
// Если моя работа вам помогла, материальная помощь приветствуется. 
// Это простимулирует меня на дальнейшее совершенствование кода.
// По вопросам могу ответить, контактные данные ниже.
// email: demmaxx@gmail.com
// site: http://maxx53.ru


namespace SCMBot
{
    public partial class Main : Form
    {
        public static MarketAuth auth = new MarketAuth();

        public static Search search = new Search(auth);

        public static Inventory inventory = new Inventory("AppTypes.txt", auth);

        public static BuyOrders orders = new BuyOrders(auth);

        private SearchPagePos sppos;
        string lastSrch;
        int lastSelec = -1;
       // bool addonComplete = false;
        bool isExit = false;

        static bool isLog = true;
        public static bool isHTML = true;

        static public Semaphore reqPool;


        private SettingsFrm settingsForm = new SettingsFrm();
        private HostStatsFrm hostsStatForm = new HostStatsFrm();

        private ItemInfoForm infoFrm = new ItemInfoForm();

        Properties.Settings settings = Properties.Settings.Default;

        private ImageList StatImgLst;
        private List<Inventory.InventItem> filteredInvList = new List<Inventory.InventItem>();

        public static HostList hostList = new HostList();

        private Size lastFrmSize;
        private Point lastFrmPos;
      //  private bool ResComboClicked = false;
        private bool sortDirect = true;

        public static int walletVal = 0;
        public static decimal stopfundsVal = 0;
        public static string jsonAddon;
        public static int ReqDelay = 100;


        public static System.Timers.Timer banTimer = new System.Timers.Timer(120000);

        private bool numChanging = false;

        public Main()
        {

            InitializeComponent();

            StatImgLst = new ImageList();
            StatImgLst.Images.Add("Img1", Properties.Resources.ready);
            StatImgLst.Images.Add("Img2", Properties.Resources.warning);
            StatImgLst.Images.Add("Img3", Properties.Resources.clock);
            StatImgLst.Images.Add("Img4", Properties.Resources.warning);
            StatImgLst.ColorDepth = ColorDepth.Depth32Bit;

            auth.LoginMessages += new MarketAuth.LoginMessagesHandler(marketAuth_LoginMessages);
            inventory.InventoryMessages += new Inventory.InventoryMessagesHandler(steam_inv_InventoryMessages);
            orders.OrdersMessages += new BuyOrders.OrdersMessagesHandler(orders_OrdersMessages);
            search.SearchMessages += new Search.SearchMessagesHandler(search_SearchMessages);

            // Only raise the event the first time Interval elapses.
            banTimer.AutoReset = false;
            banTimer.Enabled = false;


            //Some WebRequest optimizations
            ServicePointManager.Expect100Continue = false;
            ServicePointManager.UseNagleAlgorithm = false;
            ServicePointManager.DefaultConnectionLimit = 50;
         }

        void search_SearchMessages(object obj, MyEventArgs e)
        {
            this.Invoke((MethodInvoker)delegate
            {
                UpdateSearchUI(e.Code, e.Value, e.Message);
            });
        }

        void orders_OrdersMessages(object obj, MyEventArgs e)
        {
            this.Invoke((MethodInvoker)delegate
            {
                UpdateOrdersUI(e.Code, e.Value, e.Message);
            });
        }

        private void steam_inv_InventoryMessages(object obj, MyEventArgs e)
        {
            this.Invoke((MethodInvoker)delegate
            {
                UpdateInventoryUI(e.Code, e.Value, (string)e.Message);
            });
        }


        private void marketAuth_LoginMessages(object obj, MyEventArgs e)
        {
            this.Invoke((MethodInvoker)delegate
            {
                UpdateLoginUI(e.Code, e.Value, (string)e.Message);
            });
        }


        private void UpdateSearchUI(int code, int value, string message)
        {

            switch (code)
            {
                case 0:

                    if (sppos.CurrentPos == 1)
                    {
                        sppos.PageCount = value / settings.searchRes;

                        if (value % settings.searchRes != 0)
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

                    for (int i = 0; i < search.searchList.Count; i++)
                    {
                        var ourItem = search.searchList[i];

                        string[] row = { string.Empty, ourItem.Game, ourItem.Name, ourItem.StartPrice.ToString(), ourItem.Quant };
                        var lstItem = new ListViewItem(row);
                        FoundList.Items.Add(lstItem);
                    }


                    searchButton.Enabled = true;
                    SetColumnWidths(FoundList, true);
                    StatusLabel1.Text = message;

                    break;

                case 1:
                    StatusLabel1.Text = message;
                    searchButton.Enabled = true;
                    break;
            }
        }


        private void UpdateOrdersUI(int code, int value, string message)
        {
            switch (code)
            {
                //Orders Info
                case 0:


                    infoFrm.buyOrdersListView.Items.Clear();
                    infoFrm.sellOrdersListView.Items.Clear();

                    foreach (var item in orders.orderInfo.BuyGraph)
                    {
                        string[] row = { item[0], item[1] };
                        var lstItem = new ListViewItem(row);

                        infoFrm.buyOrdersListView.Items.Add(lstItem);
                    }

                    foreach (var item in orders.orderInfo.SellGraph)
                    {
                        string[] row = { item[0], item[1] };
                        var lstItem = new ListViewItem(row);

                        infoFrm.sellOrdersListView.Items.Add(lstItem);
                    }


                    getInfoButton.Enabled = true;
                    StatusLabel1.Text = "Order Info loaded!";

                    infoFrm.Text = message;

                    if (infoFrm.buyOrdersListView.Items.Count != 0)
                    {
                        infoFrm.buyOrdersListView.Items[0].Selected = true;
                    }

                    if (infoFrm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        if (infoFrm.SelectedPrice > priceNumeric.Minimum)
                        {
                            priceNumeric.Value = infoFrm.SelectedPrice;
                            addHistoryButton.PerformClick();
                        }
                    }

             
                    break;

                //MyOrders
                case 1:

                    myOrdersListView.Items.Clear();

                    if (orders.myOrders.Count != 0)
                    {
                        foreach (var item in orders.myOrders)
                        {
                            string[] row = { string.Empty, item.Name, item.Item.Price.ToString(), item.Item.Quantity.ToString() };
                            var lstItem = new ListViewItem(row);
                            myOrdersListView.Items.Add(lstItem);
                        }
                    }

                    SetColumnWidths(myOrdersListView, true);
                  //  StatusLabel1.Text = message;
                    break;

                    //Message
                case 2:
                    StatusLabel1.Text = message;
                    break;

                case 3:

                OrderScanButton.Image = (Image)Properties.Resources.start;
                OrderScanButton.Text = "Start";
                StatusLabel1.Text = message;
                    break;
                case 4:

                    //cancel progress

                    ProgressBar1.Value = value;
                   // StatusLabel1.Text = message;

                    break;
                case 5:

                    //orders cancelled
                    ProgressBar1.Visible = false;
                    StatusLabel1.Text = message;

                cancelOrderButton.Image = (Image)Properties.Resources.cancel;
                cancelOrderButton.Text = "Remove";

                    break;

                    //Actual
                case 6:
                    priceNumeric.Value = Steam.ToDecimal(message);
                    volumeLabel.Text = value.ToString();
                    getActualButton.Enabled = true;
                    break;

                case 7:
                    StatusLabel1.Text = message;
                    PlaySound(0, settings.playSnd);
                    FlashWindow.Flash(this);
                    break;
                case 8:
                    currRateLabel.Text = message;
                    getCurrRateButton.Enabled = true;
                    break;

            }
        }


        private void SetAccountInfo()
        {
            StartLoadImgTread(auth.AvatarLink, pictureBox2);
            label5.Text = auth.WalletStr;

            label10.Text = auth.AccountName;
            
            SetButton(loginButton, Strings.Logout, 2);
          
            toOrderButton.Enabled = true;

            setNotifyText(Strings.LoginSucc);
        }

        private void UpdateLoginUI(int code, int value, string message)
        {
            switch (code)
            {
                //Progress
                case 0:
                    ProgressBar1.Value = value;
                    StatusLabel1.Text = message;

                    break;

                //Success
                case 1:
                    SetAccountInfo();

                    StatusLabel1.Text = message;
                    ProgressBar1.Visible = false;
                    ProgressBar1.Value = 0;
                    SetButton(loginButton, Strings.Logout, 2);

                    break;
                 //Errors
                case 2:

                    //MessageBox.Show(Strings.ErrLogin + message, Strings.Attention, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    loginButton.Enabled = true;

                    StatusLabel1.Text = message;
                    SetButton(loginButton, Strings.Login, 1); 
                    ProgressBar1.Visible = false;
                    ProgressBar1.Value = 0;

                    break;

                    //change image
                case 3:
                    if (value == 0)
                       toolStripImage.Image = Properties.Resources.working;
                    else
                        toolStripImage.Image = Properties.Resources.ready;
                    break;

                case 4:
                    StatusLabel1.Text = message;
                    break;
            }
        }


        private void UpdateInventoryUI(int code, int value, string message)
        {
            switch (code)
            {
                //Loaded
                case 0:

                    InventoryList.Items.Clear();
                    if (value == 0)
                    {
                        SetInvFilter();
                        label4.Text = filteredInvList.Count.ToString();
                        loadInventoryButton.Enabled = true;

                        FillInventoryList();

                        SellButton.Enabled = true;

                        if (!inventory.isOnSale)
                            setPriceButton.Enabled = true;
                        else setPriceButton.Enabled = false;
                    }
                    else
                    {
                        setPriceButton.Enabled = false;
                    }

                    StatusLabel1.Text = message;
                    loadInventoryButton.Enabled = true;
                    break;

                //Empty
                case 1:

                    InventoryList.Items[value].SubItems[3].Text = message;
                   

                    decimal convPrice = Steam.ToDecimal(message);
                    filteredInvList[value].Price = convPrice;
                    paysNumeric.Value = convPrice;

                    paysNumeric_ValueChanged(null, null);
                    paysNumeric.Enabled = true;
                    receiveNumeric.Enabled = true;
                    setPriceButton.Enabled = true;

                   
                    break;
                //sell complete
                case 2:

                    StatusLabel1.Text = message;
                    ProgressBar1.Visible = false;
                    loadInventoryButton.PerformClick();
                    break;

                case 3:
                    //Progress
                    ProgressBar1.Value = value;
                    StatusLabel1.Text = message;
                    break;

            }
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

            for (int i = 0; i < inventory.appTypes.Count; i++)
            {
                inventoryComboBox.Items.Add(inventory.appTypes[i].Name);
            }

            inventoryComboBox.Items.Add("On Sale");

            //Hotfix
            var cook = (CookieContainer)LoadBinary(cockPath);

            if (cook == null)
                cook =  new CookieContainer();

            auth.Cookies = cook;

            filterTypeBox.SelectedIndex = 1;

            //InventoryList.ListViewItemSorter = itemComparer;
            
            //Add sorter?
            //FounList.ListViewItemSorter = itemComparer;
           
            LoadSettings(true);

            setNotifyText(Strings.NotLogged);

            openFileDialog1.FileName = "scmbot_list_";
            string AppPath = Path.GetDirectoryName(Application.ExecutablePath);
            openFileDialog1.InitialDirectory = AppPath;
            saveFileDialog1.InitialDirectory = AppPath;

            LoadHosts(hostsPath);


            LoadHistory();
            resellTypeComboBox.SelectedIndex = 0;
            itemNameCombo.DataSource = orders.orderHistory;
            itemNameCombo.DisplayMember = "Name";

            PerformAutoLogin();
       }


        private void LoadHistory()
        {
            if (File.Exists(historyPath))
            {
                Application.DoEvents();
                var plines = File.ReadAllText(historyPath);

                try
                {
                    orders.orderHistory = JsonConvert.DeserializeObject<BuyOrders.OrderHistory>(plines);
                }
                catch (Exception)
                {
                    MessageBox.Show("Can't load file: " + historyPath, Strings.Attention, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    orders.orderHistory = new BuyOrders.OrderHistory();
                }
            }
        }

        private void SaveHistory()
        {
            Application.DoEvents();
            JsonSerializerSettings jsonSettings = new JsonSerializerSettings();
            jsonSettings.NullValueHandling = NullValueHandling.Ignore;
            jsonSettings.Formatting = Newtonsoft.Json.Formatting.Indented;

            string json = JsonConvert.SerializeObject(orders.orderHistory, jsonSettings);
            File.WriteAllText(historyPath, json);
        }

        private void PerformAutoLogin()
        {
            if (settings.loginOnstart && loginButton.Enabled == true)
                loginButton.PerformClick();
        }


        private void LoadHosts(string path)
        {
            return;
            if (File.Exists(path))
            {
                var plines = File.ReadAllLines(path);

                SetButton(loginButton, "Wait...", 4);
                loginButton.Enabled = false;

                ThreadStart readThread = delegate()
                {

                    for (int i = 0; i < plines.Length; i++)
                    {
                        string ipPattern = @"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b";
                        Match match = Regex.Match(plines[i], ipPattern);
                        if (match.Success)
                        {
                            this.Invoke((MethodInvoker)delegate { StatusLabel1.Text = "Loading hosts... " + (i + 1).ToString() + " of " + plines.Length.ToString(); });
                            Ping ping = new Ping();
                            PingReply pingReply = ping.Send(plines[i], 2000);
                            //Don't add dead hosts
                            if (pingReply.RoundtripTime != 0)
                            {
                                hostList.Add(plines[i], pingReply.RoundtripTime.ToString());
                            }
                        }
                    }

                    this.Invoke((MethodInvoker)delegate
                    {
                        StatusLabel1.Text = "Hosts loaded: " + hostList.Count.ToString();
                        SetButton(loginButton, Strings.Login, 1);
                        loginButton.Enabled = true;

                        PerformAutoLogin();
                    
                    });

                };
                Thread pTh = new Thread(readThread);
                pTh.IsBackground = true;
                pTh.Start();
            }
            else
                usingProxyStatuslStrip.Enabled = false;
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
            settingsForm.loginBox.Text = settings.lastLogin;
            label3.Text = string.Format("({0})", settings.lastLogin);

            label3Stretch();

            isLog = settings.keepLog;
            settingsForm.keepLogBox.Checked = settings.keepLog;

            settingsForm.checkBox2.Checked = settings.loginOnstart;
            settingsForm.searchResBox.Text = settings.searchRes.ToString();
            settingsForm.ignoreBox.Checked = settings.ignoreWarn;
            settingsForm.actualBox.Checked = settings.loadActual;
            settingsForm.reqCountNumeric.Value = settings.reqCount;

            settingsForm.stopFundsBox.Text = settings.StopFunds.ToString();
            stopfundsVal = settings.StopFunds;

            settingsForm.playSndCheckBox.Checked = settings.playSnd;

            settingsForm.resDelayBox.Text = settings.resellDelay.ToString();

            scanDelayNumeric.Value = (decimal)settings.delayVal;
            resellDelayNumeric.Value = (decimal)settings.resellDelay;

            inventoryComboBox.SelectedIndex = settings.InvType;
            sellDelayBox.Text = settings.sellDelay.ToString();
            settingsForm.reqDelayBox.Text = settings.reqDelay.ToString();
            ReqDelay = settings.reqDelay;

            settingsForm.hideInventBox.Checked = settings.hideInvent;
            splitContainer1.Panel2Collapsed = settings.hideInvent;
            minimizeOnClosingToolStripMenuItem.Checked = settings.minOnClose;

            lastFrmPos = this.Location;
            lastFrmSize = this.Size;

            banTimer.Interval = settings.UnBanInterval;

            if (settings.formParams == null)
            {
                settings.formParams = new MainFormParams(this.Size, this.Location, this.WindowState,
                splitContainer1.SplitterDistance, splitContainer2.SplitterDistance, 0);
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
                   // splitContainer3.SplitterDistance = settings.formParams.Split3;
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


            if (reqPool != null)
                reqPool.Dispose();

            int num = settings.reqCount;
            if (num <= 0)
                num = 5;
            reqPool = new Semaphore(num, num);
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
            settings.resellDelay = Convert.ToInt32(settingsForm.resDelayBox.Text);
            settings.reqCount = (int)settingsForm.reqCountNumeric.Value;

            settings.sellDelay = Convert.ToInt32(sellDelayBox.Text);
            settings.reqDelay = Convert.ToInt32(settingsForm.reqDelayBox.Text);
            ReqDelay = settings.reqDelay;

            isLog = settingsForm.keepLogBox.Checked;
            settings.keepLog = isLog;

            settings.searchRes = Convert.ToInt32(settingsForm.searchResBox.Text);
            settings.ignoreWarn = settingsForm.ignoreBox.Checked;
            settings.loadActual = settingsForm.actualBox.Checked;

            settings.playSnd = settingsForm.playSndCheckBox.Checked;

            settings.delayVal = (int)scanDelayNumeric.Value;
            settings.resellDelay = (int)resellDelayNumeric.Value;

            settings.InvType = inventoryComboBox.SelectedIndex;

          //  settings.StopFunds = Convert.ToInt32(Steam.GetSweetPrice(settingsForm.stopFundsBox.Text));
            stopfundsVal = settings.StopFunds;

            settings.Language = settingsForm.intLangComboBox.SelectedItem.ToString();

            settings.formParams = new MainFormParams(lastFrmSize, lastFrmPos, this.WindowState, 
            splitContainer1.SplitterDistance, splitContainer2.SplitterDistance, 0);


            splitContainer1.Panel2Collapsed = settings.hideInvent;
            label3.Text = string.Format("({0})", settings.lastLogin);

            label3Stretch();

            //You need password crypting
            settings.lastPass = Encrypt(settingsForm.passwordBox.Text);

            settings.Save();
        }
    

//=================================================== Settings Stuff ====================== End ==============================


        private void FillInventoryList()
        {
            InventoryList.Items.Clear();

            for (int i = 0; i < filteredInvList.Count; i++)
            {
                var ourItem = filteredInvList[i];

                string priceRes;
                if (ourItem.Price == 0)
                    priceRes = Strings.None;
                else if (ourItem.Price == 1)
                    priceRes = Strings.NFS;
                else
                    priceRes = ourItem.Price.ToString();

                string[] row = { string.Empty, ourItem.Type, ourItem.Name, priceRes };

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
                filteredInvList = new List<Inventory.InventItem>(inventory.inventList);
            }
            else
            {
                switch (filterTypeBox.SelectedIndex)
                {
                    case 0:
                        filteredInvList = inventory.inventList.Where(x => (x.Type.StartsWith(textBox2.Text, StringComparison.OrdinalIgnoreCase))).ToList();
                        break;
                    case 1:
                        filteredInvList = inventory.inventList.Where(x => (x.Name.StartsWith(textBox2.Text, StringComparison.OrdinalIgnoreCase))).ToList();
                        break;
                    case 2:
                        filteredInvList = inventory.inventList.Where(x => (x.Price.ToString().StartsWith(textBox2.Text, StringComparison.OrdinalIgnoreCase))).ToList();
                        break;

                    default:
                        filteredInvList = new List<Inventory.InventItem>(inventory.inventList);
                        break;
                }
            }
        }



        bool searchRight()
        {
            return (FoundList.Items.Count != 0 && search.searchList.Count != 0 && FoundList.CheckedItems.Count > -1);
        }

        private void searchFirstMess()
        {
            MessageBox.Show(Strings.TryToFind, Strings.Attention, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        

        private void toOrder_Click(object sender, EventArgs e)
        {
            if (searchRight())
            {
                if (FoundList.CheckedItems.Count != 0)
                {
                    //dah..

                    int lastIndex = 0;

                    for (int i = 0; i < FoundList.CheckedItems.Count; i++)
                    {
                        var ourItem = search.searchList[FoundList.CheckedItems[i].Index];
                        var resIndex = orders.orderHistory.AddItem(ourItem.Link, ourItem.StartPrice, 1, 0, ourItem.StartPrice);

                        if (i == FoundList.CheckedItems.Count - 1)
                            lastIndex = resIndex;
                    }

                    UpdateItemLinkCombo(lastIndex);
                  

                }
                else
                    MessageBox.Show("Check items to add", Strings.Attention, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else 
                searchFirstMess(); 

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
            var sLink = string.Format(Steam.searchUrl, lastSrch, sppos.CurrentPos - 1, settings.searchRes);
            search.LoadSearch(sLink);
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
                SaveBinary(cockPath, auth.Cookies);
                SaveHistory();
                SaveSettings(true);
            }
        }


        private void loginButton_Click(object sender, EventArgs e)
        {

            if (!auth.Logged)
            {
                ProgressBar1.Visible = true;
                StatusLabel1.Text = Strings.tryLogin;
                SetButton(loginButton, Strings.Cancel, 3);

                auth.UserName = settings.lastLogin;
                auth.Password = Decrypt(settings.lastPass);
                auth.Login();
            }
            else
            {
                loginButton.Enabled = false;
                auth.Logout();
            }

        }


        public void AddToFormTxt(string acc)
        {
            this.Text += string.Format(" ({0})", acc);
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            if (auth.Logged)
                StartCmdLine(string.Format("{0}profiles/{1}", Steam.site, auth.UserID), string.Empty, false);
        }
        
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (searchRight() && (search.searchList.Count == FoundList.Items.Count) && (FoundList.SelectedIndices.Count !=0)) 
            StartCmdLine(search.searchList[FoundList.SelectedIndices[0]].Link, string.Empty, false);

        }

        private void loadInventoryButton_Click(object sender, EventArgs e)
        {
            if (auth.Logged)
            {
                loadInventoryButton.Enabled = false;
                SellButton.Enabled = false;
                setPriceButton.Enabled = false;

                bool isOnSale = (inventoryComboBox.SelectedIndex == inventoryComboBox.Items.Count - 1);


                if (isOnSale)
                {
                    SellButton.Text = Strings.RemSell;
                }
                else
                {
                    SellButton.Text = Strings.Sell;
                }

                inventory.appTypeIndex = inventoryComboBox.SelectedIndex;

                inventory.loadInventory();

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


            inventory.toSellList.Clear();

            for (int i = 0; i < InventoryList.CheckedItems.Count; i++)
            {
                var ouritem = filteredInvList[InventoryList.CheckedItems[i].Index];
                if ((ouritem.Marketable) && (ouritem.Price != 0))
                {
                    inventory.toSellList.Add(new Inventory.ItemToSell(ouritem.AssetId, Steam.GetSweetPrice(Steam.CalcWithFee(ouritem.Price).ToString())));
                }
            }

            if (inventory.toSellList.Count != 0)
            {

                ProgressBar1.Value = 0;
                ProgressBar1.Visible = true;

                inventory.ItemSell();
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
            if (auth.Logged && inventory.inventList.Count != 0 && e.Column != 0)
            {
                filteredInvList.Clear();

                //Sorry for copypaste

                if (sortDirect)
                {
                    switch (e.Column)
                    {
                        case 1:

                            filteredInvList = inventory.inventList.OrderBy(foo => foo.Type).ToList();
                            break;

                        case 2:
                            filteredInvList = inventory.inventList.OrderBy(foo => foo.Name).ToList();
                            break;

                        case 3:
                            filteredInvList = inventory.inventList.OrderBy(foo => foo.Price).ToList();
                            break;
                    }
                }
                else
                {
                    switch (e.Column)
                    {
                        case 1:

                            filteredInvList = inventory.inventList.OrderByDescending(foo => foo.Type).ToList();
                            break;

                        case 2:
                            filteredInvList = inventory.inventList.OrderByDescending(foo => foo.Name).ToList();
                            break;

                        case 3:
                            filteredInvList = inventory.inventList.OrderByDescending(foo => foo.Price).ToList();
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
                    auth.ChangeLang(lang);
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
            if (FoundList.SelectedItems.Count == 1)
            {
                var ourItem = search.searchList[FoundList.SelectedIndices[0]];
                StartLoadImgTread(string.Format(Steam.fndImgUrl, ourItem.ImgLink), pictureBox1);
            }
            else
                pictureBox1.Image = null;
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

        private void setPriceButton_Click(object sender, EventArgs e)
        {
            if (!inventory.isOnSale)
            {
                for (int i = 0; i < InventoryList.SelectedIndices.Count; i++)
                {
                    var ourItem = filteredInvList[InventoryList.SelectedIndices[i]];
                    ourItem.Price = paysNumeric.Value;
                    InventoryList.SelectedItems[i].SubItems[3].Text = paysNumeric.Value.ToString();
                }

            }
        }




        private void DonateBox_Click(object sender, EventArgs e)
        {
            StartCmdLine(Strings.donateLink, string.Empty, false);
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



        private void pictureBox3_Click(object sender, EventArgs e)
        {
            if ((filteredInvList.Count != 0) && (InventoryList.SelectedIndices.Count != 0))
            {
                var ourItem = filteredInvList[InventoryList.SelectedIndices[0]];
                StartCmdLine(ourItem.PageLnk, string.Empty, false);
            }
        }


   

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if (auth.Logged && inventory.inventList.Count != 0)
            {
                SetInvFilter();
                FillInventoryList();
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
        }

      
        private void usingProxyStatusToolStripMenuItem_Click(object sender, EventArgs e)
        {
            hostsStatForm.Show();
        }



        private void getInfoButton_Click(object sender, EventArgs e)
        {
            if (auth.Logged)
                if (checkLink(linkTextBox.Text))
                {
                    getInfoButton.Enabled = false;
                    orders.GetItemInfo(linkTextBox.Text);
                }
        }


        private bool checkLink(string link)
        {
            if (link.Contains("market/listings/"))
                return true;
            else
            {
                MessageBox.Show("Wrong link format!", Strings.Attention, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
        }

        private void placeOrderButton_Click(object sender, EventArgs e)
        {
            if (auth.Logged)
            {
                if (checkLink(linkTextBox.Text))
                    orders.PlaceBuyOrder(-1, linkTextBox.Text, priceNumeric.Value, (int)quantityNumeric.Value, resellTypeComboBox.SelectedIndex, resellValueNumeric.Value);
            }
         }

        private void LoadOrdersButton_Click(object sender, EventArgs e)
        {
            if (auth.Logged)
                orders.GetMyOrders();
        }

        private void cancelOrderButton_Click(object sender, EventArgs e)
        {
            if (auth.Logged)
            {
                if (myOrdersListView.CheckedIndices.Count > 0)
                {

                    int[] indexes = new int[myOrdersListView.CheckedIndices.Count];

                    for (int i = 0; i < indexes.Length; i++)
                    {
                        indexes[i] = myOrdersListView.CheckedIndices[i];
                    }

                    ProgressBar1.Visible = true;
                    cancelOrderButton.Image = (Image)Properties.Resources.stop;
                    cancelOrderButton.Text = "Cancel";

                    orders.CancelOrders(indexes);
                }
                else MessageBox.Show("Check your orders to cancel", Strings.Attention, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void myOrdersListView_Click(object sender, EventArgs e)
        {
            if (myOrdersListView.SelectedIndices.Count != 0)
            {
                //todo interface?
                var selItem = orders.myOrders[myOrdersListView.SelectedIndices[0]];
                linkTextBox.Text = selItem.Item.Link;
                itemNameCombo.SelectedIndex = orders.orderHistory.GetIndex(selItem.Item.Link);
                priceNumeric.Value = selItem.Item.Price;
                quantityNumeric.Value = selItem.Item.Quantity;
                StartLoadImgTread(selItem.ImgLink, orderPictureBox);
                volumeLabel.Text = "None";
            }
 
        }

        private void replaceOrderButton_Click(object sender, EventArgs e)
        {
            if (auth.Logged)
            {
                if (myOrdersListView.SelectedIndices.Count != 0)
                {
                    orders.PlaceBuyOrder(myOrdersListView.SelectedIndices[0], linkTextBox.Text, priceNumeric.Value, (int)quantityNumeric.Value, resellTypeComboBox.SelectedIndex, resellValueNumeric.Value);
                }
                else MessageBox.Show("Select Item To Replace", Strings.Attention, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void orderPictureBox_Click(object sender, EventArgs e)
        {
            if (myOrdersListView.SelectedIndices.Count != 0)
            {
                var selItem = orders.myOrders[myOrdersListView.SelectedIndices[0]];
                StartCmdLine(selItem.Item.Link, string.Empty, false);
            }
        }

        private void SetOrdersContorls(BuyOrders.HistoryItem item)
        {
            if (item != null)
            {
                linkTextBox.Text = item.Item.Link;
                itemNameCombo.SelectedIndex = orders.orderHistory.GetIndex(item.Item.Link);
                priceNumeric.Value = item.Item.Price;
                quantityNumeric.Value = item.Item.Quantity;
                resellTypeComboBox.SelectedIndex = item.ResellType;
                resellValueNumeric.Value = item.ResellValue;
                volumeLabel.Text = "None";
            }
        }

        private void OrderScanButton_Click(object sender, EventArgs e)
        {
                if (!orders.Scanning)
                {
                    orders.ResellDelay = (int)resellDelayNumeric.Value;
                    orders.ScanDelay = (int)scanDelayNumeric.Value;

                    OrderScanButton.Image = (Image)Properties.Resources.stop;
                    OrderScanButton.Text = "Stop";
                }

                orders.StartScan();
        }


        private void addHistoryButton_Click(object sender, EventArgs e)
        {
            if (checkLink(linkTextBox.Text))
            {
               if (orders.orderHistory.AddItem(linkTextBox.Text, priceNumeric.Value, (int)quantityNumeric.Value, resellTypeComboBox.SelectedIndex, resellValueNumeric.Value) == -1)
                   UpdateItemLinkCombo(itemNameCombo.Items.Count);
            }
        }

        private void UpdateItemLinkCombo(int pos)
        {
            itemNameCombo.DataSource = null;
            itemNameCombo.DataSource = orders.orderHistory;
            itemNameCombo.DisplayMember = "Name";

            if (itemNameCombo.Items.Count != 0)
            {
                if (pos >= 0)
                    itemNameCombo.SelectedIndex = pos;
                else
                    itemNameCombo.SelectedIndex = itemNameCombo.Items.Count - 1;
            }
        }

        private static int IndexAfterRemove(int old, int listCount)
        {
            //Need optimization

            int temp = 0;

            if ((old != 0) && (old != listCount))
                temp = old - 1;
            else
            {
                if (old != 0)
                {
                    if (old != listCount - 1)
                        temp = old - 1;
                    else
                        temp = old;
                }
                else
                    if (old != listCount)
                        temp = old;
            }

            return temp;
        }


        private void itemNameCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (itemNameCombo.SelectedIndex >= 0)
            {
                var histResult = orders.orderHistory[itemNameCombo.SelectedIndex];
                SetOrdersContorls(histResult);
            }
        }

        private void getActualButton_Click(object sender, EventArgs e)
        {
            if (auth.Logged)
            if (checkLink(linkTextBox.Text))
            {
                getActualButton.Enabled = false;
                orders.GetActualPrice(linkTextBox.Text);

            }
        }

        private void getCurrRateButton_Click(object sender, EventArgs e)
        {
            getCurrRateButton.Enabled = false;
            orders.GetCurrRate();
        }

        private void removeHistroryButton_Click(object sender, EventArgs e)
        {
            if (checkLink(linkTextBox.Text))
            {
               var oldPos = orders.orderHistory.RemoveItem(linkTextBox.Text);
               UpdateItemLinkCombo(IndexAfterRemove(oldPos, orders.orderHistory.Count));
            }
        }

        private void InventoryList_Click(object sender, EventArgs e)
        {
            if (InventoryList.SelectedItems.Count == 1)
            {

                var lit = InventoryList.SelectedItems[0];

                if (lastSelec != lit.Index)
                {
                    var ourItem = filteredInvList[InventoryList.SelectedIndices[0]];
                    StartLoadImgTread(string.Format(Steam.fndImgUrl, ourItem.ImgLink), pictureBox3);

                    var currPr = InventoryList.SelectedItems[0].SubItems[3].Text;

                    if (currPr == Strings.None)
                    {
                        if (settings.loadActual)
                        {
                            inventory.GetActualPrice(ourItem.PageLnk, lit.Index);
                            paysNumeric.Enabled = false;
                            receiveNumeric.Enabled = false;
                            receiveNumeric.Enabled = false;
                        }
                    }
                    else
                        if (currPr == Strings.NFS)
                        {
                            receiveNumeric.Enabled = false;
                            paysNumeric.Enabled = false;
                            setPriceButton.Enabled = false;
                        }
                        else
                        {
                            paysNumeric.Value = ourItem.Price;
                            receiveNumeric.Enabled = true;
                            paysNumeric.Enabled = true;
                            setPriceButton.Enabled = true;
                        }

                    paysNumeric_ValueChanged(null, null);
                    lastSelec = lit.Index;
                }

            }
            else pictureBox3.Image = null;
        }


        private void paysNumeric_ValueChanged(object sender, EventArgs e)
        {
            if (!numChanging)
            {
                numChanging = true;
                receiveNumeric.Value = Steam.CalcWithFee(paysNumeric.Value);
                numChanging = false;
            }
        }

        private void receiveNumeric_ValueChanged(object sender, EventArgs e)
        {
            if (!numChanging)
            {
                numChanging = true;
                paysNumeric.Value = Steam.AddFee(receiveNumeric.Value);
                numChanging = false;
            }
        }

        private void linkTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                var histResult = orders.orderHistory.GetByLink(linkTextBox.Text, true);
                SetOrdersContorls(histResult);
            }
        }
   }
}
