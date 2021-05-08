using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RichStrategy
{
    public class GateIO : PlatformDataInterface
    {
        public bool Connect()
        {
            throw new NotImplementedException();
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
