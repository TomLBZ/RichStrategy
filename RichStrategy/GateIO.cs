using System.Collections.Generic;
using Io.Gate.GateApi.Api;
using Io.Gate.GateApi.Client;
using Io.Gate.GateApi.Model;
using RichStrategy.Strategy;
using System.Drawing;

namespace RichStrategy.API
{
    public static class GateIO
    {
        // modify here to test with your own API keys.
        public static readonly string Key = "";

        public static readonly string Secret = "";
        public static readonly string BasePath = "";
        private static readonly Configuration Config = new() { ApiV4Key = Key, ApiV4Secret = Secret, BasePath = BasePath };
        private static readonly FuturesApi Futures = new(Config);
        public static List<string> GetCandleStringsFromGateIO(
            TIMEFRAME timeFrame, string settle = "btc", string contract = "BTC_USD", int count = 100)
        {
            List<FuturesCandlestick> candles = Futures.ListFuturesCandlesticks(
                settle, contract, null, null, count, timeFrame.GetDescription());
            List<string> candleJsons = new();
            foreach (FuturesCandlestick fcs in candles)
            {
                candleJsons.Add(fcs.ToJson());
            }
            return candleJsons;
        }

        public static List<Candle> GetCandlesFromGateIO(
            TIMEFRAME timeFrame, string settle = "btc", string contract = "BTC_USD", int count = 100)
        {
            return Candle.FromJsons(GetCandleStringsFromGateIO(timeFrame, settle, contract, count));
        }

        public static List<PointF> GetOrderBookFromGateIO(string settle = "btc", string contract = "BTC_USD", 
            int aggregationInterval = 0, int limit = 10, bool withID = false)
        {
            FuturesOrderBook result = Futures.ListFuturesOrderBook(settle, contract, aggregationInterval.ToString(), limit, withID);
            List<PointF> rtn = new();
            foreach (FuturesOrderBookItem item in result.Asks)
            {
                float price = float.Parse(item.P);
                float size = -item.S;   // because is selling
                rtn.Add(new PointF(price, size));
            }
            foreach (FuturesOrderBookItem item in result.Bids)
            {
                float price = float.Parse(item.P);
                float size = item.S;   // because is buying
                rtn.Add(new PointF(price, size));
            }
            return rtn;
        }

        public static double GetCurrentFundingRate(string settle = "btc", string contract = "BTC_USD", int limit = 1)
        {
            FundingRateRecord record = Futures.ListFuturesFundingRateHistory(settle, contract, limit)[^1];
            return double.Parse(record.R);
        }

        public static double GetCurrentFuturesInsurance(string settle = "btc", int limit = 1)
        {
            InsuranceRecord record = Futures.ListFuturesInsuranceLedger(settle, limit)[^1];
            return double.Parse(record.B);
        }

        public static FuturesAccount GetFuturesAccountInfoFromGateIO(string settle = "btc")
        {
            string json = Futures.ListFuturesAccounts(settle).ToJson();
            return new FuturesAccount(json);
        }

        public static Position GetPositionFromGateIO(string settle = "btc", string contract = "BTC_USD")
        {
            string json = Futures.GetPosition(settle, contract).ToJson();
            return new Position(json);
        }

        public static Position UpdatePositionMargin(double deltaMargin, string settle = "btc", string contract = "BTC_USD")
        {
            string json = Futures.UpdatePositionMargin(settle, contract, deltaMargin.ToString()).ToJson();
            return new Position(json);
        }

        public static Position UpdatePositionLeverage(int newLeverage, string settle = "btc", string contract = "BTC_USD")
        {
            string json = Futures.UpdatePositionLeverage(settle, contract, newLeverage.ToString()).ToJson();
            return new Position(json);
        }

        public static Position UpdatePositionRiskLimit(int newRiskLimit, string settle = "btc", string contract = "BTC_USD")
        {
            string json = Futures.UpdatePositionRiskLimit(settle, contract, newRiskLimit.ToString()).ToJson();
            return new Position(json);
        }

        public static FuturesOrder PlaceFuturesOrder(FuturesOrder order, string settle = "btc")
        {
            return Futures.CreateFuturesOrder(settle, order);
        }

        public static FuturesOrder GetFuturesOrder(FuturesOrder order, string settle = "btc")
        {
            return Futures.GetFuturesOrder(settle, order.Id.ToString());
        }

        public static FuturesOrder CancelFuturesOrder(FuturesOrder order, string settle = "btc")
        {
            return Futures.CancelFuturesOrder(settle, order.Id.ToString());
        }

    }
}
