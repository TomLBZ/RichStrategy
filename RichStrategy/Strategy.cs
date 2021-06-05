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
        private readonly int _Leverage;
        private readonly int _Timeout;
        private double _TotalAsset;
        private double _TokenizedGain;
        private double _MarketPrice;
        private long _TradeAmount;
        private int _TotalProfittingOrders;
        private int _TotalLosingOrders;
        private readonly double _TradeFundFactor = 40000;
        private readonly double _TradeCount = 1;
        private readonly double _PriceOrderOffset = 0.1;
        private readonly double _RewardRiskRatio = 1.5;
        private readonly string _Settle;
        private readonly string _Contract;
        private bool InitialBalanceLatch;
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
        public void FreshStart()
        {
            _TotalProfittingOrders = 0;
            _TotalLosingOrders = 0;
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
            });
            _TotalAsset = _FuturesAccount.TotalAssets;
            if (!InitialBalanceLatch)
            {
                _LatchedTotalAssets = _TotalAsset;
                InitialBalanceLatch = true;
            }
            double tradeAmountMax = _TotalAsset * _TradeFundFactor;
            _TradeAmount = (long)(tradeAmountMax / _TradeCount);
            _MarketPrice = (_LowerFrameData.MarketBuyPrice + _LowerFrameData.MarketSellPrice) / 2;
        }
        public void UpdateAction()
        {
            if(MyFuturesOrders.Count < _TradeCount)
            {
                if ((_TradeFrameData.IsUpTrend() && _LowerFrameData.Trend == 1))// || (_LowerFrameData.IsUpTrend() && _TradeFrameData.Trend == 1))
                {
                    TimedFuturesOrder order = new(_TradeAmount, _PriceOrderOffset, _MarketPrice + _PriceOrderOffset, _Timeout,
                        _RewardRiskRatio, _LowerFrameData.LastCandle, _Settle, _Contract);
                    MyFuturesOrders.Add(order);
                }
                else if ((_TradeFrameData.IsDownTrend() && _LowerFrameData.Trend == -1))// || (_LowerFrameData.IsDownTrend() && _TradeFrameData.Trend == -1))
                {
                    TimedFuturesOrder order = new(-_TradeAmount, _PriceOrderOffset, _MarketPrice - _PriceOrderOffset, _Timeout,
                        _RewardRiskRatio, _LowerFrameData.LastCandle, _Settle, _Contract);
                    MyFuturesOrders.Add(order);
                }
            }
            foreach (TimedFuturesOrder order in MyFuturesOrders)
            {
                order.DebugTick(_MarketPrice, _LowerFrameData.ATR14);
                if (order.IsResolved())
                {
                    _TokenizedGain += order.TokenizedGain;
                    if (order.IsProfitting) _TotalProfittingOrders++;
                    else _TotalLosingOrders++;
                }
            }
            MyFuturesOrders.RemoveAll(o => o.IsResolved());
        }
        public string GetStatus()
        {
            string str = "";
            foreach (TimedFuturesOrder order in MyFuturesOrders)
            {
                str += string.Format("{0} Order {{\r\n  FullfilledAmount: {1},\r\n  StartPrice: {2},\r\n  ATR: {3},\r\n  Target: {4},\r\n  " +
                    "Stoploss: {5},\r\n  MarketPrice: {6},\r\n  RefCandle: {7},\r\n  Mode: {8}\r\n}}\r\n", order.GetDirectionString(),
                    order.FullfilledAmount, order.StartPrice.ToString("C"), order.ReferenceATR.ToString("C"), order.TargetPrice.ToString("C"),
                    order.StopLossPrice.ToString("C"), _MarketPrice.ToString("C"), order.GetCandleString(), order.GetMode());
            }
            return str + string.Format("-- Profitting {0} : {1} Losing --\r\nToken Gain: {2},\r\nBTC Gain: {3:0.########}\r\n",
                _TotalProfittingOrders, _TotalLosingOrders, (int)_TokenizedGain, _TotalAsset - _LatchedTotalAssets);
        }
    }
}
