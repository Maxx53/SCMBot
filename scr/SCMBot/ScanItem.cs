using System;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;
using System.Threading;


namespace SCMBot
{
    public partial class ScanItem : UserControl
    {
        public event EventHandler ButtonClick;
        List<byte> lboxCols = new List<byte>();
        string imgLinkVal = string.Empty;

        public string ItemName { set; get; }

        public string linkValue
        {
            get { return textBox5.Text;  }
            set { textBox5.Text = value; }
        }

        public string wishedValue
        {
            get { return wishpriceBox.Text;  }
            set { wishpriceBox.Text = value; }
        }

        public string delayValue
        {
            get { return textBox1.Text;  }
            set { textBox1.Text = value; }
        }

        public bool tobuyValue
        {
            get { return checkBox1.Checked;  }
            set { checkBox1.Checked = value; }
        }

        public int tobuyQuant
        {
            get { return Convert.ToInt32(numericUpDown1.Value); }
            set { numericUpDown1.Value = value; }
        }


        public string ButtonText
        {
            get { return button4.Text;  }
            set { button4.Text = value; }
        }

        public string ImgLink
        {
            get { return imgLinkVal; }
            set {
            
                imgLinkVal = value;
                if (value != string.Empty)
                {
                        ThreadStart threadStart = delegate() { Main.loadImg(string.Format(SteamSite.fndImgUrl, value), pictureBox1, false, true); };
                        Thread pTh = new Thread(threadStart);
                        pTh.IsBackground = true;
                        pTh.Start();
                }
            }
        }

        public ScanItem()
        {
            InitializeComponent();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (this.ButtonClick != null)
                this.ButtonClick(this, e);  
        }


        public void lboxAdd(string rowtxt, byte colbyte, int limit)
        {
            if (listBox1.Items.Count > limit)
            {
                listBox1.Items.Clear();
                lboxCols.Clear();
            }

            lboxCols.Add(colbyte);
            listBox1.Items.Add(rowtxt);
            listBox1.SelectedIndex = listBox1.Items.Count - 1;
            listBox1.SelectedIndex = -1;
        }

        private void listBox1_DrawItem(object sender, DrawItemEventArgs e)
        {
                 e.DrawBackground();

            Brush myBrush = Brushes.Black;

            if (((ListBox)sender).Items.Count != 0)
            {
                switch (lboxCols[e.Index])
                {
                    case 0:
                        myBrush = Brushes.Black;
                        break;
                    case 1:
                        myBrush = Brushes.Red;
                        break;
                    case 2:
                        myBrush = Brushes.Green;
                        break;

                }

                e.Graphics.DrawString(((ListBox)sender).Items[e.Index].ToString(), e.Font, myBrush, e.Bounds,  StringFormat.GenericDefault);
                e.DrawFocusRectangle();
            }

    
        }

    }
}
