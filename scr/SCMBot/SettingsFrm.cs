using System;
using System.Drawing;
using System.Windows.Forms;
using System.Net;

namespace SCMBot
{
    public partial class SettingsFrm : Form
    {
        CookieFrm cookieForm = new CookieFrm();

        public bool isLangChg = false;

        public SettingsFrm()
        {
            InitializeComponent();
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            isLangChg = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            return;
        }


        private void tabControl1_DrawItem(object sender, DrawItemEventArgs e)
        {
            string tabName = tabControl1.TabPages[e.Index].Text;
            StringFormat stringFormat = new StringFormat();
            stringFormat.Alignment = StringAlignment.Center;
            stringFormat.LineAlignment = StringAlignment.Center;
            //Find if it is selected, this one will be hightlighted...
            if (e.Index == tabControl1.SelectedIndex)
                e.Graphics.FillRectangle(Brushes.LightBlue, e.Bounds);
            e.Graphics.DrawString(tabName, this.Font, Brushes.Black, tabControl1.GetTabRect(e.Index), stringFormat);

        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (cookieForm.ShowDialog() == DialogResult.OK)
            {
                var site = new Uri(SteamSite._mainsite);
                var hugeCock = new CookieContainer();
                hugeCock.Add(site, new Cookie(CookieFrm.sesid, cookieForm.textBox1.Text));
                hugeCock.Add(site, new Cookie(CookieFrm.webtrade, cookieForm.textBox2.Text));
                hugeCock.Add(site, new Cookie(CookieFrm.stlog, cookieForm.textBox3.Text));

                Main.steam_srch.cookieCont = hugeCock;
                Main.SaveBinary(Main.cockPath, Main.steam_srch.cookieCont);
            }

        }

        private void SettingsFrm_Load(object sender, EventArgs e)
        {
            this.Icon = Icon.FromHandle(Properties.Resources.settings.GetHicon());
        }




    }
}
