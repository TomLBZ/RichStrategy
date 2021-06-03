using System;
using System.ComponentModel;
using System.Reflection;
using Io.Gate.GateApi.Model;

namespace RichStrategy.Strategy
{
    public enum TIMEFRAME
    {
        [Description("10s")]
        TF_10S,
        [Description("1m")]
        TF_1M,
        [Description("5m")]
        TF_5M,
        [Description("15m")]
        TF_15M,
        [Description("30m")]
        TF_30M,
        [Description("1h")]
        TF_1H,
        [Description("4h")]
        TF_4H,
        [Description("8h")]
        TF_8H,
        [Description("1d")]
        TF_1D,
        [Description("7d")]
        TF_7D
    }
	public static class EnumExtensions
	{
        public static string GetDescription<T>(this T enumerationValue)
            where T : struct
        {
            Type type = enumerationValue.GetType();
            if (!type.IsEnum)
            {
                throw new ArgumentException("EnumerationValue must be of Enum type", nameof(enumerationValue));
            }
            //Tries to find a DescriptionAttribute for a potential friendly name
            //for the enum
            MemberInfo[] memberInfo = type.GetMember(enumerationValue.ToString());
            if (memberInfo != null && memberInfo.Length > 0)
            {
                object[] attrs = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (attrs != null && attrs.Length > 0)
                {
                    //Pull out the description value
                    return ((DescriptionAttribute)attrs[0]).Description;
                }
            }
            //If we have no description attribute, just return the ToString of the enum
            return enumerationValue.ToString();
        }
    }
    public class CandleGraphData
    {
        public TIMEFRAME TimeFrame { get; set; }
        public int Trend { get; set; }
        public double Value { get; set; }
        public double Estimate { get; set; }
        public double EMA8 { get; set; }
        public double EMA20 { get; set; }
        public double EMA50 { get; set; }
        public double ATR14 { get; set; }
        public double Volume { get; set; }
        public double RefVolume { get; set; }
        public CandleGraphData()
        {
            TimeFrame = 0;
            Trend = 0;
            Value = 0;
            Estimate = 0;
            EMA8 = 0;
            EMA20 = 0;
            EMA50 = 0;
            ATR14 = 0;
            Volume = 0;
            RefVolume = 0;
        }
        public override string ToString()
        {
            string formatStr = "CandlegraphData: {{\r\n    TimeFrame: {0},\r\n    Trend: {1},\r\n    Value: {2},\r\n    " +
                "Estimate: {3},\r\n    EMA8: {4},\r\n    EMA20: {5},\r\n    EMA50: {6},\r\n    ATR14: {7},\r\n    " +
                "Volume: {8:0.##}k,\r\n    RefVolume: {9:0.##}k\r\n}}\r\n";
            return string.Format(formatStr, TimeFrame.GetDescription(), Trend == 1 ? "Up" : Trend == -1 ? "Down" : "Unknown",
                Value.ToString("C"), Estimate.ToString("C"), EMA8.ToString("C"), EMA20.ToString("C"), EMA50.ToString("C"),
                ATR14.ToString("C"), Volume, RefVolume);
        }
        public bool IsStrongUpTrend()
        {
            return Trend == 1 && EMA8 > EMA20 && EMA20 > EMA50 && Estimate > Value;
        }
        public bool IsStrongDownTrend()
        {
            return Trend == -1 && EMA8 < EMA20 && EMA20 < EMA50 && Estimate < Value;
        }
        public bool IsUpTrend()
        {
            return Trend == 1;
        }
        public bool IsDownTrend()
        {
            return Trend == -1;
        }
    }
    public class CancellableFuturesOrder
    {
        public FuturesOrder FuturesOrder { get; set; }
        public int TimeoutTicks { get; set; }
        public long OrderID { get; set; }
        public double ReferenceATR { get; set; }
        public double RewardRiskRatio { get; set; }
        public long FullfilledTokensAmount { get; set; }
        public bool IsPendingGain { get; set; }
        public double Gain { get; set; }
        private double StartPrice;
        public CancellableFuturesOrder()
        {
            RewardRiskRatio = 1.5;
            TimeoutTicks = 30;
            IsPendingGain = false;
        }
        public void PlaceOrder(FuturesOrder order, double refATR)
        {
            FuturesOrder = order;
            // place order here.
            // temp code for testing the overall logic:
            StartPrice = double.Parse(FuturesOrder.Price);
            FullfilledTokensAmount = order.Size;
            ReferenceATR = refATR;
        }
        public void Tick(double marketPrice)
        {
            if (FullfilledTokensAmount == FuturesOrder.Size)
            {
                TimeoutTicks = -1;
                CheckAgainstATR(marketPrice);
            }
            else
            {
                if (TimeoutTicks > -1) TimeoutTicks--;
                if (TimeoutTicks == 0)
                {
                    CancelRemainingOrder();
                    CheckAgainstATR(marketPrice);
                }
            }
            IsPendingGain = TimeoutTicks == -1 && FullfilledTokensAmount == 0;
        }
        private void CancelRemainingOrder()
        {
            // cancel
            FuturesOrder.Close = true;
        }
        private void CheckAgainstATR(double marketPrice)
        {
            double pricediff = marketPrice - StartPrice;
            if (FullfilledTokensAmount == 0) return;
            else if (FullfilledTokensAmount < 0) // sell
            {
                if (-pricediff > ReferenceATR * RewardRiskRatio)
                {
                    // buy back the amount to earn the profit
                    Gain = ReferenceATR * RewardRiskRatio;
                    // for debug
                    FullfilledTokensAmount = 0;
                }
                else if (pricediff > ReferenceATR)
                {
                    // buy back the amount to stop the loss
                    Gain = -ReferenceATR;
                    // for debug
                    FullfilledTokensAmount = 0;
                }
            }
            else // buy
            {
                if (pricediff > ReferenceATR * RewardRiskRatio)
                {
                    // sell back the amount to earn the profit
                    Gain = ReferenceATR * RewardRiskRatio;
                    // for debug
                    FullfilledTokensAmount = 0;
                }
                else if (-pricediff > ReferenceATR)
                {
                    // sell back the amount to stop the loss
                    Gain = -ReferenceATR;
                    // for debug
                    FullfilledTokensAmount = 0;
                }
            }
        }
    }

}