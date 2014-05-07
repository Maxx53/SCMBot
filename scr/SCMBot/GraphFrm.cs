using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace SCMBot
{
    public partial class GraphFrm : Form
    {
        public string currency { get; set; }


        Point? prevPosition = null;
        ToolTip tooltip = new ToolTip();

        public GraphFrm()
        {
            InitializeComponent();
        }

        private void chart1_FormatNumber(object sender, System.Windows.Forms.DataVisualization.Charting.FormatNumberEventArgs e)
        {
            if (e.ElementType == System.Windows.Forms.DataVisualization.Charting.ChartElementType.AxisLabels)
            {
                if (e.Format == "customY")
                    e.LocalizedValue = MainScanItem.LogItem.DoFracture(e.Value.ToString()) + " " + currency;
            }
        }

        private void chart1_MouseMove(object sender, MouseEventArgs e)
        {
            var pos = e.Location;
            if (prevPosition.HasValue && pos == prevPosition.Value)
                return;
            tooltip.RemoveAll();
            prevPosition = pos;
            var results = chart1.HitTest(pos.X, pos.Y, false,
                                            ChartElementType.DataPoint);
            foreach (var result in results)
            {
                if (result.ChartElementType == ChartElementType.DataPoint)
                {
                    var prop = result.Object as DataPoint;
                    if (prop != null)
                    {
                        var pointXPixel = result.ChartArea.AxisX.ValueToPixelPosition(prop.XValue);
                        var pointYPixel = result.ChartArea.AxisY.ValueToPixelPosition(prop.YValues[0]);

                        if (Math.Abs(pos.X - pointXPixel) < 5 &&
                            Math.Abs(pos.Y - pointYPixel) < 5)
                        {
                            tooltip.Show(string.Format(Strings.GraphTip, DateTime.FromOADate(prop.XValue).ToString("HH:mm:ss"), MainScanItem.LogItem.DoFracture(prop.YValues[0].ToString()), currency), this.chart1, pos.X, pos.Y - 30);
                        }
                    }
                }
            }

        }

        private void GraphFrm_Load(object sender, EventArgs e)
        {
            this.Icon = Icon.FromHandle(Properties.Resources.graph.GetHicon());
        }

        private void GraphFrm_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
        }
    }
}
