using System;
using System.ComponentModel;
using System.Reflection;

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
        public Candle LastCandle { get; set; }
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
            LastCandle = null;
        }
        public override string ToString()
        {
            string candleStr = null == LastCandle ? "null" : string.Format("[\r\n    Open:{0},\r\n    Close:{1},\r\n    High:{2},\r\n    Low{3}\r\n  ]",
                LastCandle.Open.ToString("C"), LastCandle.Close.ToString("C"), LastCandle.High.ToString("C"), LastCandle.Low.ToString("C"));
            string formatStr = "CandlegraphData: {{\r\n  TimeFrame: {0},\r\n  Trend: {1},\r\n  Value: {2},\r\n  Estimate: {3},\r\n  EMA8: {4},\r\n  " +
                "EMA20: {5},\r\n  EMA50: {6},\r\n  ATR14: {7},\r\n  Volume: {8:0.##}k,\r\n  RefVolume: {9:0.##}k\r\n  LastCandle: {10}\r\n}}\r\n";
            return string.Format(formatStr, TimeFrame.GetDescription(), Trend == 1 ? "Up" : Trend == -1 ? "Down" : "Unknown",
                Value.ToString("C"), Estimate.ToString("C"), EMA8.ToString("C"), EMA20.ToString("C"), EMA50.ToString("C"),
                ATR14.ToString("C"), Volume, RefVolume, candleStr);
        }
        public bool IsUpTrend()
        {
            return (Value > EMA8 && EMA8 > EMA20 && EMA20 > EMA50) && Trend == 1;
        }
        public bool IsDownTrend()
        {
            return (Value < EMA8 && EMA8 < EMA20 && EMA20 < EMA50) && Trend == -1;
        }
    }
}