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
        public frmMain()
        {
            InitializeComponent();
        }

        private void btnTestAPI_Click(object sender, EventArgs e)
        {
            List<string> candleJsons = API.GateIO.GetCandleStringsFromGateIO(
                API.GateIO.Key, API.GateIO.Secret, Strategy.TIMEFRAME.TF_10S);
            List<Candle> candles = Candle.FromJsons(candleJsons);
            foreach (Candle candle in candles)
            {
                txtTestOutput.Text += candle.ToString() + "\r\n";
            }
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            candleGraph10S.TimeFrame = Strategy.TIMEFRAME.TF_10S;
            candleGraph10S.UpdatePeriodSeconds = 1;
            candleGraph10S.AutoUpdateEnabled = true;
            candleGraph10S.UpdateData();
            candleGraph1M.TimeFrame = Strategy.TIMEFRAME.TF_1M;
            candleGraph1M.UpdatePeriodSeconds = 6;
            candleGraph1M.AutoUpdateEnabled = true;
            candleGraph1M.UpdateData();
            candleGraph5M.TimeFrame = Strategy.TIMEFRAME.TF_5M;
            candleGraph5M.UpdatePeriodSeconds = 30;
            candleGraph5M.AutoUpdateEnabled = true;
            candleGraph5M.UpdateData();
            candleGraph30M.TimeFrame = Strategy.TIMEFRAME.TF_30M;
            candleGraph30M.UpdatePeriodSeconds = 90;
            candleGraph30M.AutoUpdateEnabled = true;
            candleGraph30M.UpdateData();
        }
    }
}
