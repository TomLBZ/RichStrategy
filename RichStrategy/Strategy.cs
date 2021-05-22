using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RichStrategy.Strategy
{
    public static class Strategy
    {
        public static double TOKEN_FRACTION = 0.25;
        public static double PROFIT_RISK_RATIO = 1.4142;
        public static TIMEFRAME TimeFrame = TIMEFRAME.TF_30S;
        public static int Leverage = 5;
        public static int MaxDataFrameInDays = 1;
        public static int TimeOutSeconds = 60;
        public static double TotalCurrentAssets = 1000;
        public static List<double> AllPrice = new();
        static Position CurrentPosition;
        static List<Position> PlacedPositions;
        static MARKET_SHAPE MarketShape;
        static double CurrentPrice;
        static double CurrentATR;
        static double CurrentMA8;
        static double CurrentMA20;
        static double CurrentMA50;
        static double CurrentMA200;
        static MARKET_SHAPE GetMARKET_SHAPE()
        {
            // not implemented yet
            return MARKET_SHAPE.TREND_SIDEWAYS;
        }
        static void GetLatestData()
        {
            // not implemented
        }
        static void UpdateData()
        {
            // -- 
            GetLatestData();
            // update variables
        }
        static bool CheckAgainstMA(bool IsUp)
        {
            if (!IsUp) return false;
            // not implemented
            return true;
        }
        static void Trade(bool isBuy)
        {

        }
        static void TestTradeSignal(bool isBuy)
        {
            // not implemented
            bool signal = true;
            CurrentPosition.TimeoutSeconds--;
            if (CurrentPosition.TimeoutSeconds == 0)
            {
                CurrentPosition = GetNewPosition(isBuy);
            }
            if (!signal) return;
            if (CurrentPosition.IsWaiting)
            {
                Trade(isBuy);
                CurrentPosition.TimeoutSeconds = 0;
                CurrentPosition.IsWaiting = false;
                PlacedPositions.Add(CurrentPosition);
                CurrentPosition = GetNewPosition(isBuy);
            }
        }
        static double GetTargetPrice(bool isBuy)
        {
            return isBuy ? CurrentPrice + PROFIT_RISK_RATIO * CurrentATR
                : CurrentPrice - PROFIT_RISK_RATIO * CurrentATR;
        }
        static Position GetNewPosition(bool isBuy)
        {
            Position p = new();
            p.InPrice = CurrentPrice;
            p.IsBuy = isBuy;
            p.IsWaiting = true;
            p.NumTokens = (int)(TotalCurrentAssets * TOKEN_FRACTION);
            p.TargetPrice = GetTargetPrice(isBuy);
            p.TimeoutSeconds = TimeOutSeconds;
            return p;
        }
        static void UpTrendStrategy() // strong up
        {
            if (CheckAgainstMA(true))
            {
                if (CurrentPosition.IsBuy)
                {
                    TestTradeSignal(true);
                }
                else
                {
                    CurrentPosition = GetNewPosition(true);
                }
            }
        }
        static void DownTrendStrategy() // strong down
        {
            if (CheckAgainstMA(false))
            {
                if (!CurrentPosition.IsBuy)
                {
                    TestTradeSignal(false);
                }
                else
                {
                    CurrentPosition = GetNewPosition(false);
                }
            }
        }
        static void SidewaysStrategy() // weak conflict
        {
            // wang ge jiao yi

        }
        static void ConflictingStrategy() // strong conflict
        {
            // test reversal
        }
        static void UpdatePlacedPositions()
        {
            // not implemented
        }
        public static MonetaryStatus GetMonetaryStatusFromGateIO()
        {
            return new();
        }
        public static MonetaryStatus Operate()
        {
            UpdateData();
            switch (MarketShape)
            {
                case MARKET_SHAPE.TREND_UP:
                    UpTrendStrategy();
                    break;
                case MARKET_SHAPE.TREND_DOWN:
                    DownTrendStrategy();
                    break;
                case MARKET_SHAPE.TREND_SIDEWAYS:
                    SidewaysStrategy();
                    break;
                case MARKET_SHAPE.TREND_CONFLICTING:
                    ConflictingStrategy();
                    break;
                default:
                    ConflictingStrategy();
                    break;
            }
            UpdatePlacedPositions();
            // not implemented yet
            return GetMonetaryStatusFromGateIO();
        }
    }
}
