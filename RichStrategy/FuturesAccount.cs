using System;

namespace RichStrategy
{
    public class FuturesAccount
    {
        public double TotalAssets { get; set; }
        public double UnrealizedGain { get; set; }
        public double AvailableBalance { get; set; }
        public double OrderMargin { get; set; }
        public double PositionMargin { get; set; }
        public double Point { get; set; }
        public string Currency { get; set; }
        public bool IsDualMode { get; set; }
        private readonly string _Json;
        
        public static FuturesAccount FromJson(string json)
        {
            return new FuturesAccount(json);
        }

        public FuturesAccount(string json = "")
        {
            if (null == json || json == "")
            {
                TotalAssets = -1;
                UnrealizedGain = 0;
                AvailableBalance = -1;
                OrderMargin = -1;
                PositionMargin = -1;
                Point = -1;
                Currency = "unknown";
                IsDualMode = false;
                _Json = "";
            }
            else
            {
                _Json = json;
                IsDualMode = false;
                string j = json.Replace("\"", "").Replace(",", "").Replace(":", "").Replace(" ", "");
                string[] sep_json = j.Split("\r\n")[1..^1];
                foreach (string str in sep_json)
                {
                    if (str.Contains("total"))
                        TotalAssets = double.Parse(str.Remove(0, 5));
                    else if (str.Contains("unrealised_pnl"))
                        UnrealizedGain = double.Parse(str.Remove(0, 14));
                    else if (str.Contains("available"))
                        AvailableBalance = double.Parse(str.Remove(0, 9));
                    else if (str.Contains("order_margin"))
                        OrderMargin = double.Parse(str.Remove(0, 12));
                    else if (str.Contains("position_margin"))
                        PositionMargin = double.Parse(str.Remove(0, 15));
                    else if (str.Contains("point"))
                        Point = double.Parse(str.Remove(0, 5));
                    else if (str.Contains("currency"))
                        Currency = str.Remove(0, 8);
                    else if (str.Contains("in_dual_mode"))
                        IsDualMode = bool.Parse(str.Remove(0, 12));
                    else
                        throw new Exception("Unknown Json");
                }
            }
        }
    
        public string ToJson()
        {
            return _Json;
        }

        public override string ToString()
        {
            string str = "F_Account[\r\n  asset:{0:0.#########},\r\n  u-gain:{1:0.#########},\r\n  available:{2:0.#########},\r\n  " +
                "o-margin:{3:0.#########},\r\n  p-margin:{4:0.#########},\r\n  pt:{5:0.#},\r\n  currency:{6},\r\n  isdual:{7}\r\n]\r\n";
            return string.Format(str, TotalAssets, UnrealizedGain, AvailableBalance, OrderMargin, PositionMargin, Point, Currency, IsDualMode);
        }
    }
}
