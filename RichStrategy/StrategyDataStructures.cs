using System;
using System.ComponentModel;
using System.Reflection;

namespace RichStrategy.Strategy
{
    public enum MARKET_SHAPE
    {
        TREND_UP,
        TREND_DOWN,
        TREND_SIDEWAYS,
        TREND_CONFLICTING
    }

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
                throw new ArgumentException("EnumerationValue must be of Enum type", "enumerationValue");
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

	public struct MonetaryStatus
    {
        public double MyTotalAssets;
        public double MyBuyPosition;
        public double MySellPosition;
        public double MyAssetsReserve;
        public double MyLeverage;
    }
    public struct Position
    {
        public bool IsWaiting;
        public bool IsBuy;
        public int NumTokens;
        public int TimeoutSeconds;
        public int LifeTimeSeconds;
        public double InPrice;
        public double TargetPrice;
        public double CurrentGain;
    }

}