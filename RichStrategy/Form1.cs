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
        private Strategy.Strategy _Strategy;
        private Strategy.CandleGraphData _10SData;
        private Strategy.CandleGraphData _1MData;
        private Strategy.CandleGraphData _5MData;
        private Strategy.CandleGraphData _30MData;
        public frmMain()
        {
            InitializeComponent();
            _Strategy = new();
        }

        private void btnTestAPI_Click(object sender, EventArgs e)
        {

        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            candleGraph10S.TimeFrame = Strategy.TIMEFRAME.TF_10S;
            candleGraph10S.UpdatePeriodSeconds = 1;
            candleGraph10S.AutoUpdateEnabled = true;
            candleGraph10S.BindData(ref _10SData);
            candleGraph10S.SetPeriodicTrigger(PeriodicTrigger);
            candleGraph10S.UpdateData();
            candleGraph1M.TimeFrame = Strategy.TIMEFRAME.TF_1M;
            candleGraph1M.UpdatePeriodSeconds = 6;
            candleGraph1M.AutoUpdateEnabled = true;
            candleGraph1M.BindData(ref _1MData);
            candleGraph1M.UpdateData();
            candleGraph5M.TimeFrame = Strategy.TIMEFRAME.TF_5M;
            candleGraph5M.UpdatePeriodSeconds = 30;
            candleGraph5M.AutoUpdateEnabled = true;
            candleGraph5M.BindData(ref _5MData);
            candleGraph5M.UpdateData();
            candleGraph30M.TimeFrame = Strategy.TIMEFRAME.TF_30M;
            candleGraph30M.UpdatePeriodSeconds = 90;
            candleGraph30M.AutoUpdateEnabled = true;
            candleGraph30M.BindData(ref _30MData);
            candleGraph30M.UpdateData();
        }

        private void PeriodicTrigger()
        {
            txtTestOutput.Text = _10SData.ToString() + _1MData.ToString() + _5MData.ToString() + _30MData.ToString();
        }
    }
}
