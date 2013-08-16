using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SCMBot
{

    public partial class Dialog : Form
    {

        public string MailCode
        {
            get { return mailcodeBox.Text; }
            set { guardBox.Text = value; }
        }
        public string GuardDesc
        {
            get { return guardBox.Text; }
            set { guardBox.Text = value; }
        }

        public string capchaText
        {
            get { return capchaBox.Text; }
            set { capchaBox.Text = value; }
        }

        public bool codgroupEnab
        {
            get { return codgroupBox.Enabled; }
            set { codgroupBox.Enabled = value; }
        }

        public bool capchgroupEnab
        {
            get { return capchgroupBox.Enabled; }
            set { capchgroupBox.Enabled = value; }
        }

         public PictureBox capchImg
        {
            get { return capchapicBox; }
            set { capchapicBox = value; }
        }

       

        public Dialog()
        {
            InitializeComponent();
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            return;
        }

    }
}
