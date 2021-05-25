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
        //GateIO gateIO;
        public frmMain()
        {
            InitializeComponent();
            rnd = new Random();
        }

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
            candleGraph15M.TimeFrame = Strategy.TIMEFRAME.TF_15M;
            candleGraph15M.UpdatePeriodSeconds = 90;
            candleGraph15M.AutoUpdateEnabled = true;
            candleGraph15M.UpdateData();
        }
    }
}
