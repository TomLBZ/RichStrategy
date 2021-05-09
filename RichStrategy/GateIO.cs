using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Io.Gate.GateApi.Api;
using Io.Gate.GateApi.Client;
using Io.Gate.GateApi.Model;

namespace RichStrategy
{
    public class GateIO : PlatformDataInterface
    {
        public bool Connect()
        {
            Configuration config = new Configuration();
            config.BasePath = "https://api.gateio.ws/api/v4";
            config.ApiV4Key = "your_key";
            config.ApiV4Secret = "your_secret";
            FuturesApi fa = new FuturesApi(config);
            const string settle = "btc";  // string | Settle currency
            const string contract = "BTC_USD";
            const string leverage = "10";
            fa.UpdatePositionLeverage(settle, contract, leverage);
            long positionSize = 0L;
            try
            {
                Position position = fa.GetPosition(settle, contract);
                positionSize = position.Size;
            }
            catch (GateApiException e)
            {
                // ignore no position error 
                if (!"POSITION_NOT_FOUND".Equals(e.ErrorLabel))
                {
                    return false;
                }
            }
            return true;
        }

        public void Disconnect()
        {
            throw new NotImplementedException();
        }

        public double FetchCurrentPrice()
        {
            throw new NotImplementedException();
        }

        public double FetchCurrentVolume()
        {
            throw new NotImplementedException();
        }

        public double[] FetchHistoricalPrice(int seconds)
        {
            throw new NotImplementedException();
        }

        public double[] FetchHistoricalVolume(int seconds)
        {
            throw new NotImplementedException();
        }
    }
}
