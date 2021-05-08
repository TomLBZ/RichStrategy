using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RichStrategy
{
    interface PlatformDataInterface
    {
        public bool Connect();
        public void Disconnect();
        public double FetchCurrentPrice();
        public double[] FetchHistoricalPrice(int seconds);
        public double FetchCurrentVolume();
        public double FetchHistoricalVolume();
    }
}
