namespace SCMBot
{
    partial class Main
    {
        /// <summary>
        /// Требуется переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Обязательный метод для поддержки конструктора - не изменяйте
        /// содержимое данного метода при помощи редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.ListViewGroup listViewGroup1 = new System.Windows.Forms.ListViewGroup("In inventory", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup2 = new System.Windows.Forms.ListViewGroup("On Sale", System.Windows.Forms.HorizontalAlignment.Left);
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            this.loginBox = new System.Windows.Forms.TextBox();
            this.passwordBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.loginButton = new System.Windows.Forms.Button();
            this.label10 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripImage = new System.Windows.Forms.ToolStripStatusLabel();
            this.StatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.ProgressBar1 = new System.Windows.Forms.ToolStripProgressBar();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.labelFindCurr = new System.Windows.Forms.Label();
            this.buyNowButton = new System.Windows.Forms.Button();
            this.addtoScan = new System.Windows.Forms.Button();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.selectedItemToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fullSetToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.emptyTabToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.prevButton = new System.Windows.Forms.Button();
            this.label13 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.nextButton = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.labelStPrice = new System.Windows.Forms.Label();
            this.labelQuant = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.searchButton = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.button1 = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.SellButton = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.comboBox3 = new System.Windows.Forms.ComboBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.InventoryList = new System.Windows.Forms.ListView();
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label6 = new System.Windows.Forms.Label();
            this.pictureBox3 = new System.Windows.Forms.PictureBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.groupBox1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // loginBox
            // 
            this.loginBox.Location = new System.Drawing.Point(72, 17);
            this.loginBox.Name = "loginBox";
            this.loginBox.Size = new System.Drawing.Size(90, 20);
            this.loginBox.TabIndex = 15;
            this.loginBox.Text = "Name";
            // 
            // passwordBox
            // 
            this.passwordBox.Location = new System.Drawing.Point(72, 43);
            this.passwordBox.Name = "passwordBox";
            this.passwordBox.PasswordChar = '*';
            this.passwordBox.Size = new System.Drawing.Size(90, 20);
            this.passwordBox.TabIndex = 16;
            this.passwordBox.Text = "Password";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(33, 20);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(29, 13);
            this.label2.TabIndex = 17;
            this.label2.Text = "User";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 46);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 13);
            this.label3.TabIndex = 18;
            this.label3.Text = "Password";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.checkBox2);
            this.groupBox1.Controls.Add(this.loginButton);
            this.groupBox1.Controls.Add(this.loginBox);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.passwordBox);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Location = new System.Drawing.Point(9, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(266, 70);
            this.groupBox1.TabIndex = 20;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Login";
            // 
            // checkBox2
            // 
            this.checkBox2.AutoSize = true;
            this.checkBox2.Location = new System.Drawing.Point(172, 46);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(87, 17);
            this.checkBox2.TabIndex = 32;
            this.checkBox2.Text = "Login at start";
            this.checkBox2.UseVisualStyleBackColor = true;
            // 
            // loginButton
            // 
            this.loginButton.Image = global::SCMBot.Properties.Resources.login;
            this.loginButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.loginButton.Location = new System.Drawing.Point(172, 13);
            this.loginButton.Name = "loginButton";
            this.loginButton.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.loginButton.Size = new System.Drawing.Size(88, 27);
            this.loginButton.TabIndex = 8;
            this.loginButton.Text = "Login";
            this.loginButton.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.loginButton.UseVisualStyleBackColor = true;
            this.loginButton.Click += new System.EventHandler(this.loginButton_Click);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(91, 24);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(33, 13);
            this.label10.TabIndex = 35;
            this.label10.Text = "None";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(54, 43);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(40, 13);
            this.label8.TabIndex = 32;
            this.label8.Text = "Wallet:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(91, 43);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(33, 13);
            this.label5.TabIndex = 28;
            this.label5.Text = "None";
            // 
            // statusStrip1
            // 
            this.statusStrip1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripImage,
            this.StatusLabel1,
            this.ProgressBar1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 653);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(552, 22);
            this.statusStrip1.SizingGrip = false;
            this.statusStrip1.TabIndex = 36;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripImage
            // 
            this.toolStripImage.AutoSize = false;
            this.toolStripImage.Image = global::SCMBot.Properties.Resources.ready;
            this.toolStripImage.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripImage.Name = "toolStripImage";
            this.toolStripImage.Size = new System.Drawing.Size(17, 17);
            // 
            // StatusLabel1
            // 
            this.StatusLabel1.Name = "StatusLabel1";
            this.StatusLabel1.Size = new System.Drawing.Size(39, 17);
            this.StatusLabel1.Text = "Ready";
            // 
            // ProgressBar1
            // 
            this.ProgressBar1.Name = "ProgressBar1";
            this.ProgressBar1.Size = new System.Drawing.Size(100, 16);
            this.ProgressBar1.Visible = false;
            // 
            // comboBox1
            // 
            this.comboBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(6, 82);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(516, 21);
            this.comboBox1.TabIndex = 38;
            this.comboBox1.Text = "snow globe";
            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            this.comboBox1.KeyUp += new System.Windows.Forms.KeyEventHandler(this.comboBox1_KeyUp);
            // 
            // groupBox4
            // 
            this.groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox4.Controls.Add(this.labelFindCurr);
            this.groupBox4.Controls.Add(this.buyNowButton);
            this.groupBox4.Controls.Add(this.addtoScan);
            this.groupBox4.Controls.Add(this.prevButton);
            this.groupBox4.Controls.Add(this.label13);
            this.groupBox4.Controls.Add(this.label11);
            this.groupBox4.Controls.Add(this.nextButton);
            this.groupBox4.Controls.Add(this.pictureBox1);
            this.groupBox4.Controls.Add(this.labelStPrice);
            this.groupBox4.Controls.Add(this.labelQuant);
            this.groupBox4.Controls.Add(this.label12);
            this.groupBox4.Controls.Add(this.label9);
            this.groupBox4.Controls.Add(this.comboBox1);
            this.groupBox4.Controls.Add(this.searchButton);
            this.groupBox4.Location = new System.Drawing.Point(8, 194);
            this.groupBox4.MinimumSize = new System.Drawing.Size(300, 109);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(534, 109);
            this.groupBox4.TabIndex = 40;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Find Item";
            // 
            // labelFindCurr
            // 
            this.labelFindCurr.AutoSize = true;
            this.labelFindCurr.Location = new System.Drawing.Point(116, 38);
            this.labelFindCurr.Name = "labelFindCurr";
            this.labelFindCurr.Size = new System.Drawing.Size(29, 13);
            this.labelFindCurr.TabIndex = 52;
            this.labelFindCurr.Text = "units";
            // 
            // buyNowButton
            // 
            this.buyNowButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buyNowButton.Enabled = false;
            this.buyNowButton.Image = global::SCMBot.Properties.Resources.buy_now;
            this.buyNowButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buyNowButton.Location = new System.Drawing.Point(286, 20);
            this.buyNowButton.Name = "buyNowButton";
            this.buyNowButton.Padding = new System.Windows.Forms.Padding(5, 0, 0, 0);
            this.buyNowButton.Size = new System.Drawing.Size(80, 25);
            this.buyNowButton.TabIndex = 51;
            this.buyNowButton.Text = "Buy Now";
            this.buyNowButton.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.buyNowButton.UseVisualStyleBackColor = true;
            this.buyNowButton.Click += new System.EventHandler(this.buyNowButton_Click);
            // 
            // addtoScan
            // 
            this.addtoScan.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.addtoScan.ContextMenuStrip = this.contextMenuStrip1;
            this.addtoScan.Enabled = false;
            this.addtoScan.Image = global::SCMBot.Properties.Resources.add;
            this.addtoScan.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.addtoScan.Location = new System.Drawing.Point(372, 20);
            this.addtoScan.Name = "addtoScan";
            this.addtoScan.Padding = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.addtoScan.Size = new System.Drawing.Size(80, 25);
            this.addtoScan.TabIndex = 49;
            this.addtoScan.Text = "To Scan";
            this.addtoScan.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.addtoScan.UseVisualStyleBackColor = true;
            this.addtoScan.Click += new System.EventHandler(this.addtoScan_Click);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.selectedItemToolStripMenuItem,
            this.fullSetToolStripMenuItem,
            this.emptyTabToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(146, 70);
            // 
            // selectedItemToolStripMenuItem
            // 
            this.selectedItemToolStripMenuItem.Image = global::SCMBot.Properties.Resources.selected;
            this.selectedItemToolStripMenuItem.Name = "selectedItemToolStripMenuItem";
            this.selectedItemToolStripMenuItem.Size = new System.Drawing.Size(145, 22);
            this.selectedItemToolStripMenuItem.Text = "Selected Item";
            this.selectedItemToolStripMenuItem.Click += new System.EventHandler(this.selectedItemToolStripMenuItem_Click);
            // 
            // fullSetToolStripMenuItem
            // 
            this.fullSetToolStripMenuItem.Image = global::SCMBot.Properties.Resources.fullset;
            this.fullSetToolStripMenuItem.Name = "fullSetToolStripMenuItem";
            this.fullSetToolStripMenuItem.Size = new System.Drawing.Size(145, 22);
            this.fullSetToolStripMenuItem.Text = "Full Set";
            this.fullSetToolStripMenuItem.Click += new System.EventHandler(this.fullSetToolStripMenuItem_Click);
            // 
            // emptyTabToolStripMenuItem
            // 
            this.emptyTabToolStripMenuItem.Image = global::SCMBot.Properties.Resources.blank;
            this.emptyTabToolStripMenuItem.Name = "emptyTabToolStripMenuItem";
            this.emptyTabToolStripMenuItem.Size = new System.Drawing.Size(145, 22);
            this.emptyTabToolStripMenuItem.Text = "EmptyTab";
            this.emptyTabToolStripMenuItem.Click += new System.EventHandler(this.emptyTabToolStripMenuItem_Click);
            // 
            // prevButton
            // 
            this.prevButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.prevButton.Enabled = false;
            this.prevButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.prevButton.Location = new System.Drawing.Point(286, 52);
            this.prevButton.Name = "prevButton";
            this.prevButton.Size = new System.Drawing.Size(25, 25);
            this.prevButton.TabIndex = 48;
            this.prevButton.Text = "<";
            this.prevButton.UseVisualStyleBackColor = true;
            this.prevButton.Click += new System.EventHandler(this.prevButton_Click);
            // 
            // label13
            // 
            this.label13.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(315, 58);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(24, 13);
            this.label13.TabIndex = 47;
            this.label13.Text = "0/0";
            // 
            // label11
            // 
            this.label11.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(242, 58);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(38, 13);
            this.label11.TabIndex = 46;
            this.label11.Text = "Page: ";
            // 
            // nextButton
            // 
            this.nextButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.nextButton.Enabled = false;
            this.nextButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.nextButton.Location = new System.Drawing.Point(341, 52);
            this.nextButton.Name = "nextButton";
            this.nextButton.Size = new System.Drawing.Size(25, 25);
            this.nextButton.TabIndex = 45;
            this.nextButton.Text = ">";
            this.nextButton.UseVisualStyleBackColor = true;
            this.nextButton.Click += new System.EventHandler(this.nextButton_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox1.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.pictureBox1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pictureBox1.Location = new System.Drawing.Point(458, 12);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(64, 64);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pictureBox1.TabIndex = 44;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
            // 
            // labelStPrice
            // 
            this.labelStPrice.AutoSize = true;
            this.labelStPrice.Location = new System.Drawing.Point(79, 38);
            this.labelStPrice.Name = "labelStPrice";
            this.labelStPrice.Size = new System.Drawing.Size(31, 13);
            this.labelStPrice.TabIndex = 43;
            this.labelStPrice.Text = "none";
            // 
            // labelQuant
            // 
            this.labelQuant.AutoSize = true;
            this.labelQuant.Location = new System.Drawing.Point(79, 58);
            this.labelQuant.Name = "labelQuant";
            this.labelQuant.Size = new System.Drawing.Size(31, 13);
            this.labelQuant.TabIndex = 42;
            this.labelQuant.Text = "none";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(12, 38);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(58, 13);
            this.label12.TabIndex = 41;
            this.label12.Text = "Start price:";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(20, 58);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(49, 13);
            this.label9.TabIndex = 40;
            this.label9.Text = "Quantity:";
            // 
            // searchButton
            // 
            this.searchButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.searchButton.Image = global::SCMBot.Properties.Resources.search;
            this.searchButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.searchButton.Location = new System.Drawing.Point(372, 51);
            this.searchButton.Name = "searchButton";
            this.searchButton.Padding = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.searchButton.Size = new System.Drawing.Size(80, 25);
            this.searchButton.TabIndex = 39;
            this.searchButton.Text = "Search";
            this.searchButton.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.searchButton.UseVisualStyleBackColor = true;
            this.searchButton.Click += new System.EventHandler(this.searchButton_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Location = new System.Drawing.Point(8, 3);
            this.tabControl1.MinimumSize = new System.Drawing.Size(300, 191);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(537, 191);
            this.tabControl1.TabIndex = 41;
            this.tabControl1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.tabControl1_MouseDown);
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.pictureBox2);
            this.groupBox2.Controls.Add(this.label10);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.label8);
            this.groupBox2.Location = new System.Drawing.Point(286, 3);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(257, 70);
            this.groupBox2.TabIndex = 36;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Account Info";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(54, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(38, 13);
            this.label1.TabIndex = 36;
            this.label1.Text = "Name:";
            // 
            // pictureBox2
            // 
            this.pictureBox2.BackColor = System.Drawing.SystemColors.Window;
            this.pictureBox2.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pictureBox2.Location = new System.Drawing.Point(11, 24);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(32, 32);
            this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pictureBox2.TabIndex = 33;
            this.pictureBox2.TabStop = false;
            this.pictureBox2.Click += new System.EventHandler(this.pictureBox2_Click);
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(425, 44);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(96, 25);
            this.button1.TabIndex = 42;
            this.button1.Text = "Load Inventory";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(476, 71);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(33, 13);
            this.label4.TabIndex = 43;
            this.label4.Text = "None";
            // 
            // SellButton
            // 
            this.SellButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.SellButton.Location = new System.Drawing.Point(425, 216);
            this.SellButton.Name = "SellButton";
            this.SellButton.Size = new System.Drawing.Size(96, 24);
            this.SellButton.TabIndex = 45;
            this.SellButton.Text = "Sell Checked";
            this.SellButton.UseVisualStyleBackColor = true;
            this.SellButton.Click += new System.EventHandler(this.SellButton_Click);
            // 
            // textBox1
            // 
            this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox1.Location = new System.Drawing.Point(425, 190);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(96, 20);
            this.textBox1.TabIndex = 46;
            // 
            // comboBox3
            // 
            this.comboBox3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox3.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox3.FormattingEnabled = true;
            this.comboBox3.Items.AddRange(new object[] {
            "Trading Cards",
            "TF 2",
            "Dota 2"});
            this.comboBox3.Location = new System.Drawing.Point(425, 17);
            this.comboBox3.Name = "comboBox3";
            this.comboBox3.Size = new System.Drawing.Size(96, 21);
            this.comboBox3.TabIndex = 48;
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Controls.Add(this.InventoryList);
            this.groupBox3.Controls.Add(this.label6);
            this.groupBox3.Controls.Add(this.SellButton);
            this.groupBox3.Controls.Add(this.comboBox3);
            this.groupBox3.Controls.Add(this.button1);
            this.groupBox3.Controls.Add(this.label4);
            this.groupBox3.Controls.Add(this.pictureBox3);
            this.groupBox3.Controls.Add(this.textBox1);
            this.groupBox3.Location = new System.Drawing.Point(9, 3);
            this.groupBox3.MinimumSize = new System.Drawing.Size(300, 249);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(534, 249);
            this.groupBox3.TabIndex = 49;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "My Inventory (Test)";
            // 
            // InventoryList
            // 
            this.InventoryList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.InventoryList.CheckBoxes = true;
            this.InventoryList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader4,
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3});
            this.InventoryList.FullRowSelect = true;
            this.InventoryList.GridLines = true;
            listViewGroup1.Header = "In inventory";
            listViewGroup1.Name = "listViewGroup1";
            listViewGroup2.Header = "On Sale";
            listViewGroup2.Name = "listViewGroup2";
            this.InventoryList.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] {
            listViewGroup1,
            listViewGroup2});
            this.InventoryList.Location = new System.Drawing.Point(6, 17);
            this.InventoryList.MultiSelect = false;
            this.InventoryList.Name = "InventoryList";
            this.InventoryList.Size = new System.Drawing.Size(413, 222);
            this.InventoryList.TabIndex = 51;
            this.InventoryList.UseCompatibleStateImageBehavior = false;
            this.InventoryList.View = System.Windows.Forms.View.Details;
            this.InventoryList.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.InventoryList_ColumnClick);
            this.InventoryList.ColumnWidthChanging += new System.Windows.Forms.ColumnWidthChangingEventHandler(this.InventoryList_ColumnWidthChanging);
            this.InventoryList.Click += new System.EventHandler(this.InventoryList_Click);
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "";
            this.columnHeader4.Width = 23;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Type";
            this.columnHeader1.Width = 116;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Name";
            this.columnHeader2.Width = 155;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Cost";
            this.columnHeader3.Width = 96;
            // 
            // label6
            // 
            this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(427, 71);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(41, 13);
            this.label6.TabIndex = 50;
            this.label6.Text = "Count: ";
            // 
            // pictureBox3
            // 
            this.pictureBox3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox3.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.pictureBox3.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pictureBox3.Location = new System.Drawing.Point(425, 87);
            this.pictureBox3.Name = "pictureBox3";
            this.pictureBox3.Size = new System.Drawing.Size(96, 96);
            this.pictureBox3.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pictureBox3.TabIndex = 47;
            this.pictureBox3.TabStop = false;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(0, 79);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.tabControl1);
            this.splitContainer1.Panel1.Controls.Add(this.groupBox4);
            this.splitContainer1.Panel1MinSize = 191;
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.groupBox3);
            this.splitContainer1.Size = new System.Drawing.Size(552, 571);
            this.splitContainer1.SplitterDistance = 306;
            this.splitContainer1.TabIndex = 50;
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(552, 675);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.groupBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(560, 709);
            this.Name = "Main";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "SCM Bot alpha";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Main_FormClosing);
            this.Load += new System.EventHandler(this.Main_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.contextMenuStrip1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).EndInit();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button loginButton;
        private System.Windows.Forms.TextBox loginBox;
        private System.Windows.Forms.TextBox passwordBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.CheckBox checkBox2;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripProgressBar ProgressBar1;
        private System.Windows.Forms.ToolStripStatusLabel StatusLabel1;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Button searchButton;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label labelStPrice;
        private System.Windows.Forms.Label labelQuant;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Button nextButton;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Button prevButton;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.Button addtoScan;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button buyNowButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem selectedItemToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fullSetToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem emptyTabToolStripMenuItem;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button SellButton;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.PictureBox pictureBox3;
        private System.Windows.Forms.ComboBox comboBox3;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ListView InventoryList;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripImage;
        private System.Windows.Forms.Label labelFindCurr;
    }
}

