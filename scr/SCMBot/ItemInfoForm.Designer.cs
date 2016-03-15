namespace SCMBot
{
    partial class ItemInfoForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.sellOrdersListView = new System.Windows.Forms.ListView();
            this.columnHeader18 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader19 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.buyOrdersListView = new System.Windows.Forms.ListView();
            this.columnHeader16 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader17 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.toPriceButton = new System.Windows.Forms.Button();
            this.groupBox6.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox6
            // 
            this.groupBox6.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox6.Controls.Add(this.sellOrdersListView);
            this.groupBox6.Location = new System.Drawing.Point(213, 37);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(151, 409);
            this.groupBox6.TabIndex = 1;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "Sell Orders";
            // 
            // sellOrdersListView
            // 
            this.sellOrdersListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.sellOrdersListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader18,
            this.columnHeader19});
            this.sellOrdersListView.FullRowSelect = true;
            this.sellOrdersListView.Location = new System.Drawing.Point(6, 19);
            this.sellOrdersListView.MultiSelect = false;
            this.sellOrdersListView.Name = "sellOrdersListView";
            this.sellOrdersListView.Size = new System.Drawing.Size(133, 384);
            this.sellOrdersListView.TabIndex = 0;
            this.sellOrdersListView.UseCompatibleStateImageBehavior = false;
            this.sellOrdersListView.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader18
            // 
            this.columnHeader18.Text = "Price";
            // 
            // columnHeader19
            // 
            this.columnHeader19.Text = "Quantity";
            // 
            // groupBox5
            // 
            this.groupBox5.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox5.Controls.Add(this.buyOrdersListView);
            this.groupBox5.Location = new System.Drawing.Point(3, 37);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(204, 409);
            this.groupBox5.TabIndex = 1;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Buy Orders";
            // 
            // buyOrdersListView
            // 
            this.buyOrdersListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buyOrdersListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader16,
            this.columnHeader17});
            this.buyOrdersListView.FullRowSelect = true;
            this.buyOrdersListView.Location = new System.Drawing.Point(6, 19);
            this.buyOrdersListView.MultiSelect = false;
            this.buyOrdersListView.Name = "buyOrdersListView";
            this.buyOrdersListView.Size = new System.Drawing.Size(188, 384);
            this.buyOrdersListView.TabIndex = 0;
            this.buyOrdersListView.UseCompatibleStateImageBehavior = false;
            this.buyOrdersListView.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader16
            // 
            this.columnHeader16.Text = "Price";
            // 
            // columnHeader17
            // 
            this.columnHeader17.Text = "Quantity";
            // 
            // toPriceButton
            // 
            this.toPriceButton.Image = global::SCMBot.Properties.Resources.copy;
            this.toPriceButton.Location = new System.Drawing.Point(9, 8);
            this.toPriceButton.Name = "toPriceButton";
            this.toPriceButton.Size = new System.Drawing.Size(107, 23);
            this.toPriceButton.TabIndex = 2;
            this.toPriceButton.Text = "Copy to Price";
            this.toPriceButton.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.toPriceButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.toPriceButton.UseVisualStyleBackColor = true;
            this.toPriceButton.Click += new System.EventHandler(this.toPriceButton_Click);
            // 
            // ItemInfoForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(372, 451);
            this.Controls.Add(this.toPriceButton);
            this.Controls.Add(this.groupBox6);
            this.Controls.Add(this.groupBox5);
            this.Name = "ItemInfoForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "ItemInfoForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ItemInfoForm_FormClosing);
            this.Load += new System.EventHandler(this.ItemInfoForm_Load);
            this.groupBox6.ResumeLayout(false);
            this.groupBox5.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox6;
        public System.Windows.Forms.ListView sellOrdersListView;
        private System.Windows.Forms.ColumnHeader columnHeader18;
        private System.Windows.Forms.ColumnHeader columnHeader19;
        private System.Windows.Forms.GroupBox groupBox5;
        public System.Windows.Forms.ListView buyOrdersListView;
        private System.Windows.Forms.ColumnHeader columnHeader16;
        private System.Windows.Forms.ColumnHeader columnHeader17;
        private System.Windows.Forms.Button toPriceButton;

    }
}