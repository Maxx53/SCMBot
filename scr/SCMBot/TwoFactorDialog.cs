using System;
using System.Windows.Forms;

namespace SCMBot
{

    public partial class TwoFactorDialog : Form
    {
        public string TwoFactorCode
        {
            get { return twoFactorCode.Text; }
            set { twoFactorCode.Text = value; }
        }

        public TwoFactorDialog()
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
