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
    public partial class ItemInfoForm : Form
    {
        public ItemInfoForm()
        {
            InitializeComponent();
        }

        public decimal SelectedPrice
        {
            get {

                decimal result = (decimal)0.03;

                if (buyOrdersListView.SelectedItems.Count != 0)
                    result = Steam.ToDecimal(buyOrdersListView.SelectedItems[0].SubItems[0].Text);

                return result;
            }
        }

        private void ItemInfoForm_Load(object sender, EventArgs e)
        {
            this.Icon = Icon.FromHandle(Properties.Resources.info.GetHicon());
        }

        private void ItemInfoForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Hide();
        }

        private void toPriceButton_Click(object sender, EventArgs e)
        {
            if (buyOrdersListView.SelectedItems.Count != 0)
            {
                this.DialogResult = DialogResult.OK;
                return;
            }
            else
                MessageBox.Show("Check buy order price first!", Strings.Attention, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}
