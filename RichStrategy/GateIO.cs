using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Io.Gate.GateApi.Api;
using Io.Gate.GateApi.Client;
using Io.Gate.GateApi.Model;
using RichStrategy.Strategy;

namespace RichStrategy.API
{
    public static class GateIO
    {
        public static readonly string Key = "82cac15cbe518008df484e9a5ad330b7";

        public static readonly string Secret = "7b92fc112e895d9f509ac8fec4bf392e3acc49065692cd4ba497cb70d71e0880";
        public static List<string> GetCandleStringsFromGateIO(string key, string secret, 
            TIMEFRAME timeFrame, string settle = "btc", string contract = "BTC_USD", int count = 100)
        {
            Configuration config = new();
            config.BasePath = "https://api.gateio.ws/api/v4";
            config.ApiV4Key = key;
            config.ApiV4Secret = secret;
            FuturesApi fa = new(config);
            List<FuturesCandlestick> candles = fa.ListFuturesCandlesticks(
                settle, contract, null, null, count, timeFrame.GetDescription());
            List<string> candleJsons = new();
            foreach (FuturesCandlestick fcs in candles)
            {
                candleJsons.Add(fcs.ToJson());
            }
            return candleJsons;
        }

        public static List<Candle> GetCandlesFromGateIO(string key, string secret,
            TIMEFRAME timeFrame, string settle = "btc", string contract = "BTC_USD", int count = 100)
        {
            return Candle.FromJsons(GetCandleStringsFromGateIO(
                key, secret, timeFrame, settle, contract, count));
        }

    }
}
