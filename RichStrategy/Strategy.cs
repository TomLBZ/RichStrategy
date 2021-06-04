using System.Collections.Generic;
using System.Drawing;
using Io.Gate.GateApi.Model;
using System.Threading.Tasks;

namespace RichStrategy.Strategy
{
    public class Strategy
    {
        #region Data Properties
        private CandleGraphData _TradeFrameData;
        private CandleGraphData _LowerFrameData;
        private FuturesAccount _FuturesAccount;
        private List<PointF> _OrderBook;
        private readonly int _Leverage;
        private readonly int _Timeout;
        private double _TotalAsset;
        private double _TokenizedGain;
        private double _MarketPrice;
        private long _TradeAmount;
        private readonly double _TradeFundFactor = 40000;
        private readonly double _TradeCount = 1;
        private readonly double _PriceOrderOffset = 0.1;
        private readonly double _RewardRiskRatio = 1.5;
        private readonly string _Settle;
        private readonly string _Contract;
        public bool InitialBalanceLatch { get; set; }
        private double _LatchedTotalAssets = 0;
        private readonly List<TimedFuturesOrder> MyFuturesOrders = new();
        #endregion
        public Strategy(int leverage, int timeout, string settle = "btc", string contract = "BTC_USD") 
        {
            _Leverage = leverage;
            _Timeout = timeout;
            _Settle = settle;
            _Contract = contract;
            InitialBalanceLatch = false;
        }
        public async void UpdateData(CandleGraphData tradeFrameData, CandleGraphData lowerFrameData)
        {
            _TradeFrameData = tradeFrameData;
            _LowerFrameData = lowerFrameData;
            await Task.Factory.StartNew(() =>
            {
                _ = API.GateIO.UpdatePositionLeverage(_Leverage);
                _FuturesAccount = API.GateIO.GetFuturesAccountInfoFromGateIO();
                _OrderBook = API.GateIO.GetOrderBookFromGateIO();
            });
            _TotalAsset = _FuturesAccount.TotalAssets;
            if (!InitialBalanceLatch)
            {
                _LatchedTotalAssets = _TotalAsset;
                InitialBalanceLatch = true;
            }
            double tradeAmountMax = _TotalAsset * _TradeFundFactor;
            _TradeAmount = (long)(tradeAmountMax / _TradeCount);
            _MarketPrice = _OrderBook[10].X;
        }
        public void UpdateAction()
        {
            int ordersCount = MyFuturesOrders.Count;
            if (ordersCount >= _TradeCount)
            {
                foreach (TimedFuturesOrder order in MyFuturesOrders)
                {
                    order.Tick(_MarketPrice, _LowerFrameData.ATR14);
                    if (order.IsResolved) _TokenizedGain += order.TokenizedGain;
                }
                MyFuturesOrders.RemoveAll(o => o.IsResolved);
                ordersCount = MyFuturesOrders.Count;
            }
            else
            {
                if ((_TradeFrameData.IsUpTrend() && _LowerFrameData.Trend == 1) || (_LowerFrameData.IsUpTrend() && _TradeFrameData.Trend == 1))
                {
                    TimedFuturesOrder order = new(_TradeAmount, _PriceOrderOffset, _MarketPrice + _PriceOrderOffset, _Timeout, _Settle, _Contract);
                    order.PlaceOrder(_LowerFrameData.ATR14, _RewardRiskRatio, _LowerFrameData.LastCandle);
                    MyFuturesOrders.Add(order);
                }
                else if ((_TradeFrameData.IsDownTrend() && _LowerFrameData.Trend == -1) || (_LowerFrameData.IsDownTrend() && _TradeFrameData.Trend == -1))
                {
                    TimedFuturesOrder order = new(-_TradeAmount, _PriceOrderOffset, _MarketPrice - _PriceOrderOffset, _Timeout, _Settle, _Contract);
                    order.PlaceOrder(_LowerFrameData.ATR14, _RewardRiskRatio, _LowerFrameData.LastCandle);
                    MyFuturesOrders.Add(order);
                }
            }
        }
        public string GetStatus()
        {
            string str = "";
            foreach (TimedFuturesOrder order in MyFuturesOrders)
            {
                str += string.Format("Order {{\r\n  FullfilledAmount: {0},\r\n  StartPrice: {1},\r\n  ATR: {2},\r\n  Target: {3},\r\n  " +
                    "Stoploss: {4},\r\n  MarketPrice: {5},\r\n  RefCandle: {6},\r\n  Mode: {7}\r\n}}\r\n",
                    order.FullfilledAmount, order.StartPrice.ToString("C"), order.ReferenceATR.ToString("C"), order.TargetPrice.ToString("C"),
                    order.StopLossPrice.ToString("C"), _MarketPrice.ToString("C"), order.GetCandleString(), order.GetMode());
            }
            return str + string.Format("\r\nToken Gain: {0},\r\nBTC Gain: {1:0.########}\r\n", _TokenizedGain, _TotalAsset - _LatchedTotalAssets);
        }
    }
}
