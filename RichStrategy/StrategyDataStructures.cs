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
        TF_10S,
        TF_30S,
        TF_1M,
        TF_5M,
        TF_15M,
        TF_30M,
        TF_1H,
        TF_2H,
        TF_4H,
        TF_8H,
        TF_12H,
        TF_1D,
        TF_2D,
        TF_7D,
        TF_14D,
        TF_30D
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