using System;
using System.Windows.Forms;

namespace RichStrategy
{
    public partial class frmMain : Form
    {
        private readonly Strategy.Strategy _Strategy;
        private Strategy.CandleGraphData _10SData;
        private Strategy.CandleGraphData _1MData;
        private bool IsStrategyEnabled = false;
        private readonly int _TradeLeverage = 10;
        private readonly int _TradeTimeout = 10;
        public frmMain()
        {
            InitializeComponent();
            _Strategy = new(_TradeLeverage, _TradeTimeout);
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            candleGraph10S.TimeFrame = Strategy.TIMEFRAME.TF_10S;
            candleGraph10S.UpdatePeriodSeconds = 1;
            candleGraph10S.AutoUpdateEnabled = true;
            candleGraph10S.BindData(ref _10SData);
            candleGraph10S.SetPeriodicTrigger(PeriodicTrigger);
            candleGraph10S.UpdateData(true);
            candleGraph1M.TimeFrame = Strategy.TIMEFRAME.TF_1M;
            candleGraph1M.UpdatePeriodSeconds = 6;
            candleGraph1M.AutoUpdateEnabled = true;
            candleGraph1M.BindData(ref _1MData);
            candleGraph1M.UpdateData(true);
        }

        private void PeriodicTrigger()
        {
            _Strategy.UpdateData(_1MData, _10SData);
            if (IsStrategyEnabled)
            {
                _Strategy.UpdateAction();
                txtStrategyResult.Text = _Strategy.GetStatus();
            }
            txtTestOutput.Text = _10SData.ToString() + _1MData.ToString();
        }

        private void ButtonTestStrategy_Click(object sender, EventArgs e)
        {
            if (!IsStrategyEnabled)
            {
                btnTestStrategy.Text = "Strategy Enabled";
                IsStrategyEnabled = true;
                _Strategy.InitialBalanceLatch = false;
            }
            else
            {
                btnTestStrategy.Text = "Test Strategy";
                IsStrategyEnabled = false;
            }
        }
    }
}
