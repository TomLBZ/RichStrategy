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
            graph1.DataFrames = 1000;
        }

        double starting_price = 100;
        double volatility = 0.03;
        double daily_drift = 0.0008;
        private void btnTestData_Click(object sender, EventArgs e)
        {
            List<double> data = new List<double>();
            for (int i = 0; i < 1000; i++)
            {
                double norm_inversed = NormalDistribution.NormInv(rnd.NextGaussian(), 0, volatility);
                while (double.IsInfinity(norm_inversed)) norm_inversed = NormalDistribution.NormInv(rnd.NextGaussian(), 0, volatility);
                double new_price = starting_price * (1 + norm_inversed + daily_drift);
                data.Add(new_price);
                starting_price = new_price;
            }
            graph1.Data = data.ToArray();
            graph1.Redraw();
        }
    }
}
