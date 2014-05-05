using System;
using System.Windows.Forms;

namespace SCMBot
{
    public partial class CookieFrm : Form
    {
        public const string sesid = "sessionid";
        public const string webtrade = "webTradeEligibility";
        public const string stlog = "steamLogin";

        public CookieFrm()
        {
            InitializeComponent();

            label1.Text = sesid;
            label2.Text = webtrade;
            label3.Text = stlog;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            return;
        }
    }
}
