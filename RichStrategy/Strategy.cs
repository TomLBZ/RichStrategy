using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Io.Gate.GateApi.Model;

namespace RichStrategy.Strategy
{
    public class Strategy
    {
        #region Data Properties
        private CandleGraphData _TradeFrameData;
        private CandleGraphData _LowerFrameData;
        private CandleGraphData _HigherFrameData;
        private CandleGraphData _OverarchingFrameData;
        private FuturesAccount _FuturesAccount;
        private Position _Position;
        private int _Leverage;
        private double _FundAvailable;
        private double _CurrentTokensPosition;
        private double _Gain;
        private double _MarketPrice;
        private List<CancellableFuturesOrder> MyFuturesOrders = new();
        #endregion
        public Strategy(int leverage) 
        {
            _Leverage = leverage;
        }
        public void UpdateData(CandleGraphData tradeFrameData, CandleGraphData lowerFrameData, CandleGraphData higherFrameData, CandleGraphData overarchingFrameData)
        {
            _TradeFrameData = tradeFrameData;
            _LowerFrameData = lowerFrameData;
            _HigherFrameData = higherFrameData;
            _OverarchingFrameData = overarchingFrameData;
            _FuturesAccount = API.GateIO.GetFuturesAccountInfoFromGateIO();
            _Position = API.GateIO.GetPositionFromGateIO();
            _FundAvailable = _FuturesAccount.AvailableBalance;
            _MarketPrice = _Position.MarketPrice; // NO. The trading price is NOT the market price. NEED FIXING URGENTLY
        }
        public void UpdateAction()
        {
            double sumOrders = 0;
            if (MyFuturesOrders.Count > 0)
            {
                foreach (CancellableFuturesOrder order in MyFuturesOrders)
                {
                    order.Tick(_MarketPrice);
                    if (order.IsPendingGain) _Gain += order.Gain;
                    else sumOrders += order.FullfilledTokensAmount;
                }
                MyFuturesOrders.RemoveAll(o => o.IsPendingGain);
            }
            if (_TradeFrameData.IsUpTrend() && _LowerFrameData.IsUpTrend())
            {
                if (sumOrders > 1000) return;
                FuturesOrder order = new("BTC_USD", 100, 0, _MarketPrice.ToString(), false, false);
                CancellableFuturesOrder cfo = new();
                cfo.PlaceOrder(order, _LowerFrameData.ATR14);
                MyFuturesOrders.Add(cfo);
            }
            else if (_TradeFrameData.IsDownTrend() && _LowerFrameData.IsDownTrend())
            {
                if (sumOrders > 1000 || -sumOrders > 1000) return;
                FuturesOrder order = new("BTC_USD", -100, 0, _MarketPrice.ToString(), false, false);
                CancellableFuturesOrder cfo = new();
                cfo.PlaceOrder(order, _LowerFrameData.ATR14);
                MyFuturesOrders.Add(cfo);
            }
        }
        public string GetStatus()
        {
            string str = "";
            foreach (CancellableFuturesOrder order in MyFuturesOrders)
            {
                str += string.Format("[Price:{0}, ATR:{1}, Amnt:{2}, Gain:{3}]\r\n", order.FuturesOrder.Price, order.ReferenceATR,
                    order.FullfilledTokensAmount, order.Gain);
            }
            return str + string.Format("\r\nTotal Gain:{0}\r\n",_Gain);
        }
    }
}
