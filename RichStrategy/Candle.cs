using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RichStrategy
{
    class Candle
    {
        public double TimestampUnix { get; set; }
        public double Volume { get; set; }
        public double Close { get; set; }
        public double Open { get; set; }
        public double High { get; set; }
        public double Low { get; set; }

        private string _Json;

        public static Candle FromJson(string json)
        {
            return new Candle(json);
        }

        public static List<Candle> FromJsons(List<string> jsons)
        {
            List<Candle> candles = new();
            foreach (string j in jsons)
            {
                candles.Add(new Candle(j));
            }
            return candles;
        }

        public Candle(string json = "")
        {
            if (json is null || json == "")
            {
                TimestampUnix = -1;
                Volume = -1;
                Open = -1;
                Close = -1;
                High = -1;
                Low = -1;
                _Json = "";
            }
            else
            {
                _Json = json;
                string j = json.Replace("\"", "").Replace(",", "").Replace(
                    ":", "").Replace(" ", "");
                string[] sep_json = j.Split("\r\n")[1..^1];
                TimestampUnix = Convert.ToDouble(sep_json[0][1..]);
                Volume = Convert.ToDouble(sep_json[1][1..]);
                Close = Convert.ToDouble(sep_json[2][1..]);
                High = Convert.ToDouble(sep_json[3][1..]);
                Low = Convert.ToDouble(sep_json[4][1..]);
                Open = Convert.ToDouble(sep_json[5][1..]);
            }
        }

        public string ToJson()
        {
            return _Json;
        }

        public override string ToString()
        {
            return string.Format("Candle[t:{0}, v:{1}, c:{2}, h:{3}, l:{4}, o:{5}]",
                TimestampUnix, Volume, Close, High, Low, Open);
        }
    }
}
