namespace SCMBot
{
    partial class Dialog
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
            this.okButton = new System.Windows.Forms.Button();
            this.codgroupBox = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.guardBox = new System.Windows.Forms.TextBox();
            this.mailcodeBox = new System.Windows.Forms.TextBox();
            this.capchgroupBox = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.capchaBox = new System.Windows.Forms.TextBox();
            this.capchapicBox = new System.Windows.Forms.PictureBox();
            this.codgroupBox.SuspendLayout();
            this.capchgroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.capchapicBox)).BeginInit();
            this.SuspendLayout();
            // 
            // okButton
            // 
            this.okButton.Location = new System.Drawing.Point(153, 195);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(86, 28);
            this.okButton.TabIndex = 2;
            this.okButton.Text = "ОК";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // codgroupBox
            // 
            this.codgroupBox.Controls.Add(this.label2);
            this.codgroupBox.Controls.Add(this.label1);
            this.codgroupBox.Controls.Add(this.guardBox);
            this.codgroupBox.Controls.Add(this.mailcodeBox);
            this.codgroupBox.Location = new System.Drawing.Point(12, 12);
            this.codgroupBox.Name = "codgroupBox";
            this.codgroupBox.Size = new System.Drawing.Size(227, 77);
            this.codgroupBox.TabIndex = 23;
            this.codgroupBox.TabStop = false;
            this.codgroupBox.Text = "Enter access code";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 51);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(60, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "Description";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 25);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 13);
            this.label1.TabIndex = 7;
            this.label1.Text = "Email Code";
            // 
            // guardBox
            // 
            this.guardBox.Location = new System.Drawing.Point(81, 48);
            this.guardBox.Name = "guardBox";
            this.guardBox.Size = new System.Drawing.Size(135, 20);
            this.guardBox.TabIndex = 6;
            this.guardBox.Text = "scmarket bot";
            // 
            // mailcodeBox
            // 
            this.mailcodeBox.Location = new System.Drawing.Point(80, 22);
            this.mailcodeBox.Name = "mailcodeBox";
            this.mailcodeBox.Size = new System.Drawing.Size(136, 20);
            this.mailcodeBox.TabIndex = 5;
            // 
            // capchgroupBox
            // 
            this.capchgroupBox.Controls.Add(this.label3);
            this.capchgroupBox.Controls.Add(this.capchaBox);
            this.capchgroupBox.Controls.Add(this.capchapicBox);
            this.capchgroupBox.Location = new System.Drawing.Point(12, 95);
            this.capchgroupBox.Name = "capchgroupBox";
            this.capchgroupBox.Size = new System.Drawing.Size(227, 93);
            this.capchgroupBox.TabIndex = 24;
            this.capchgroupBox.TabStop = false;
            this.capchgroupBox.Text = "Enter capcha text";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 68);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(68, 13);
            this.label3.TabIndex = 25;
            this.label3.Text = "Capcha Text";
            // 
            // capchaBox
            // 
            this.capchaBox.Location = new System.Drawing.Point(81, 65);
            this.capchaBox.Name = "capchaBox";
            this.capchaBox.Size = new System.Drawing.Size(135, 20);
            this.capchaBox.TabIndex = 24;
            // 
            // capchapicBox
            // 
            this.capchapicBox.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.capchapicBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.capchapicBox.Location = new System.Drawing.Point(10, 19);
            this.capchapicBox.Name = "capchapicBox";
            this.capchapicBox.Size = new System.Drawing.Size(206, 40);
            this.capchapicBox.TabIndex = 23;
            this.capchapicBox.TabStop = false;
            // 
            // Form2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(252, 228);
            this.Controls.Add(this.capchgroupBox);
            this.Controls.Add(this.codgroupBox);
            this.Controls.Add(this.okButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "Form2";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Steam Guard Check";
            this.codgroupBox.ResumeLayout(false);
            this.codgroupBox.PerformLayout();
            this.capchgroupBox.ResumeLayout(false);
            this.capchgroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.capchapicBox)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.GroupBox codgroupBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox guardBox;
        private System.Windows.Forms.TextBox mailcodeBox;
        private System.Windows.Forms.GroupBox capchgroupBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox capchaBox;
        private System.Windows.Forms.PictureBox capchapicBox;
    }
}