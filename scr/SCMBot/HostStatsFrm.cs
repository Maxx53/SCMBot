using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;

namespace SCMBot
{
    public partial class HostStatsFrm : Form
    {
        public HostStatsFrm()
        {
            InitializeComponent();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            listView1.Items.Clear();

            for (int i = 0; i < Main.hostList.Count; i++)
            {
                string[] row = { Main.hostList[i].Host, Main.hostList[i].InUsing.ToString(), Main.hostList[i].WorkLoad.ToString(), Main.hostList[i].Pingas };
                var lstItem = new ListViewItem(row);
                if (Main.hostList[i].InUsing)
                    lstItem.BackColor = Color.OrangeRed;
                else
                    lstItem.BackColor = Color.LightGreen;

                listView1.Items.Add(lstItem);
            }
        }

        private void ProxyStatsFrm_Load(object sender, EventArgs e)
        {
            ListViewHelper.EnableDoubleBuffer(listView1);
            this.Icon = Icon.FromHandle(Properties.Resources.host.GetHicon());
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
