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
        GateIO gateIO;
        public frmMain()
        {
            InitializeComponent();
            rnd = new Random();
            gateIO = new GateIO();
            graph1.DataFrames = 5000;
        }

        double starting_price = 57600;
        double volatility = 0.0001020320726; // 30 days 3.00%, 1 day 0.547722575%, 1 hour 0.1118033989%, 5 minute 0.03227486122%, 30 seconds 0.01020320736%
        double daily_drift = 0;
        /// <summary>
        /// 计划的逻辑（未实现）：
        /// 1、如果计时未开启则开启计时，如果分析器未配置则配置分析器
        /// 2、如果分析器缺少数据则获取历史价格和历史成交量的数据
        /// 3、如果分析器缺少参数则分析一次，算出参数
        /// 4、所允许的最短时间单位（例如10秒）过后获取当前价格和当前成交数据
        /// 5、分析器更新数据，更新参数
        /// 6、如果当前没有仓位则评估一次，否则比较出仓条件，满足则出仓（1ATR止损，1.4ATR止盈）
        /// 7、如果评估结果不许可交易，重复4、5、6、7；如果许可交易，进行交易
        /// 8、可视化
        /// </summary>
        /// <param name="sender">源</param>
        /// <param name="e">参</param>
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

        private void btnTestAPI_Click(object sender, EventArgs e)
        {
            if (!gateIO.Connect()) throw new Exception();
        }
    }
}
