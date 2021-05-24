using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Io.Gate.GateApi.Api;
using Io.Gate.GateApi.Client;
using Io.Gate.GateApi.Model;

namespace RichStrategy.API
{
    public static class GateIO
    {
        public static List<string> GetCandlesFromGateIO(
            string key, string secret)
        {
            Configuration config = new Configuration();
            config.BasePath = "https://api.gateio.ws/api/v4";
            config.ApiV4Key = key;
            config.ApiV4Secret = secret;
            FuturesApi fa = new FuturesApi(config);
            const string settle = "btc";  // string | Settle currency
            const string contract = "BTC_USD";
            const string leverage = "10";
            List<FuturesCandlestick> candles = fa.ListFuturesCandlesticks(settle, contract);
            List<string> candleJsons = new();
            foreach (FuturesCandlestick fcs in candles)
            {
                candleJsons.Add(fcs.ToJson());
            }
            return candleJsons;
        }

    }
}
