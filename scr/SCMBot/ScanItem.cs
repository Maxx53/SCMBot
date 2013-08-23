using System;
using System.Windows.Forms;


namespace SCMBot
{
    public partial class ScanItem : UserControl
    {
        public event EventHandler ButtonClick;

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

        public string ButtonText
        {
            get { return button4.Text;  }
            set { button4.Text = value; }
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

    }
}
