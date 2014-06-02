using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;

namespace SCMBot
{
    public partial class ProxyStatsFrm : Form
    {
        public ProxyStatsFrm()
        {
            InitializeComponent();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            listView1.Items.Clear();

            for (int i = 0; i < Main.proxyList.Count; i++)
            {
                string[] row = { Main.proxyList[i].Proxy.Address.Host.ToString(), Main.proxyList[i].InUsing.ToString(), Main.proxyList[i].WorkLoad.ToString() };
                var lstItem = new ListViewItem(row);
                if (Main.proxyList[i].InUsing)
                    lstItem.BackColor = Color.OrangeRed;
                else
                    lstItem.BackColor = Color.LightGreen;

                listView1.Items.Add(lstItem);
            }
        }

        private void ProxyStatsFrm_Load(object sender, EventArgs e)
        {
            ListViewHelper.EnableDoubleBuffer(listView1);
        }

        private void ProxyStatsFrm_Shown(object sender, EventArgs e)
        {

        }

        private void ProxyStatsFrm_FormClosing(object sender, FormClosingEventArgs e)
        {
            timer1.Enabled = false;
            e.Cancel = true;
            this.Hide();
        }

        private void ProxyStatsFrm_Activated(object sender, EventArgs e)
        {
            timer1.Enabled = true;
        }

       
    }
}
