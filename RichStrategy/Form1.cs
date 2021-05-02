using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RichStrategy
{
    public partial class frmMain : Form
    {
        Random rnd;
        public frmMain()
        {
            InitializeComponent();
            rnd = new Random();
            graph1.DataFrames = 5000;
        }

        double starting_price = 57600;
        double volatility = 0.0001020320726; // 30 days 3.00%, 1 day 0.547722575%, 1 hour 0.1118033989%, 5 minute 0.03227486122%, 30 seconds 0.01020320736%
        double daily_drift = 0;
        private void btnTestData_Click(object sender, EventArgs e)
        {
            List<double> data = new List<double>();
            for (int i = 0; i < 5000; i++)
            {
                double dRtn = rnd.NextGaussian(0, volatility);
                //double norm_inversed = NormalDistribution.NormInv(rnd.NextGaussian(), 0, volatility);
                //while (double.IsInfinity(norm_inversed)) norm_inversed = NormalDistribution.NormInv(rnd.NextGaussian(), 0, volatility);
                double new_price = starting_price * (1 + dRtn + daily_drift);
                data.Add(new_price);
                starting_price = new_price;
            }
            graph1.Data = data.ToArray();
            graph1.Redraw();
        }
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (e.X >= graph1.Location.X && e.X <= graph1.Location.X + graph1.Width
            &&
            e.Y >= graph1.Location.Y && e.Y <= graph1.Location.Y + graph1.Height)
            {
                graph1.OnMouseWheel(e);
            }
            base.OnMouseWheel(e);
        }
    }
}
