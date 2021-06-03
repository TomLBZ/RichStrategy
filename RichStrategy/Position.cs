using System;

namespace RichStrategy
{
    public class Position
    {
        #region Public Properties
        public long User { get; set; }
        public string Contract { get; set; }
        public long Size { get; set; }
        public int Leverage { get; set; }
        public int RiskLimit { get; set; }
        public int LeverageMax { get; set; }
        public double MaintenanceRate { get; set; }
        public double Value { get; set; }
        public double Margin { get; set; }
        public double EntryPrice { get; set; }
        public double LiquidationPrice { get; set; }
        public double MarketPrice { get; set; }
        public double UnrealizedGain { get; set; }
        public double RealizedGain { get; set; }
        public long CloseOrderID { get; set; }
        public double CloseOrderPrice { get; set; }
        public bool CloseIsLiquidated { get; set; }
        #endregion

        #region Private Members
        private readonly double HistoryRealizedGain;
        private readonly double LastCloseGain;
        private readonly double RealizedPointGain;
        private readonly double HistoryRealizedPointGain;
        private readonly int ADLRanking;
        private readonly int PendingOrders;
        private readonly string Mode;
        private readonly string _Json;
        #endregion

        public static Position FromJson(string json)
        {
            return new Position(json);
        }
        public Position(string json = "")
        {
            if (null == json || json == "")
            {
                User = -1;
                Contract = "unknown";
                Size = 0;
                Leverage = -1;
                RiskLimit = -1;
                LeverageMax = -1;
                MaintenanceRate = -1;
                Value = -1;
                Margin = 0;
                EntryPrice = -1;
                LiquidationPrice = -1;
                MarketPrice = -1;
                UnrealizedGain = 0;
                RealizedGain = 0;
                HistoryRealizedGain = 0;
                LastCloseGain = 0;
                RealizedPointGain = 0;
                HistoryRealizedPointGain = 0;
                ADLRanking = 0;
                PendingOrders = 0;
                CloseIsLiquidated = false;
                CloseOrderID = -1;
                CloseOrderPrice = 0;
                Mode = "unknown";
                _Json = "";
            }
            else
            {
                _Json = json;
                string j = json.Replace("\"", "").Replace(",", "").Replace(":", "").Replace(" ", "");
                string[] sep_json = j.Split("\r\n")[1..^1];
                foreach (string str in sep_json)
                {
                    if (str.Contains("user")) User = long.Parse(str.Remove(0, 4));
                    else if (str.Contains("contract")) Contract = str.Remove(0, 8);
                    else if (str.Contains("size")) Size = long.Parse(str.Remove(0, 4));
                    else if (str.Contains("leverage_max")) LeverageMax = int.Parse(str.Remove(0, 12));
                    else if (str.Contains("leverage")) Leverage = int.Parse(str.Remove(0, 8));
                    else if (str.Contains("risk_limit")) RiskLimit = int.Parse(str.Remove(0, 10));
                    else if (str.Contains("maintenance_rate")) MaintenanceRate = double.Parse(str.Remove(0, 16));
                    else if (str.Contains("value")) Value = double.Parse(str.Remove(0, 5));
                    else if (str.Contains("margin")) Margin = double.Parse(str.Remove(0, 6));
                    else if (str.Contains("entry_price")) EntryPrice = double.Parse(str.Remove(0, 11));
                    else if (str.Contains("liq_price")) LiquidationPrice = double.Parse(str.Remove(0, 9));
                    else if (str.Contains("mark_price")) MarketPrice = double.Parse(str.Remove(0, 10));
                    else if (str.Contains("unrealised_pnl")) UnrealizedGain = double.Parse(str.Remove(0, 14));
                    else if (str.Contains("realised_pnl")) RealizedGain = double.Parse(str.Remove(0, 12));
                    else if (str.Contains("history_pnl")) HistoryRealizedGain = double.Parse(str.Remove(0, 11));
                    else if (str.Contains("last_close_pnl")) LastCloseGain = double.Parse(str.Remove(0, 14));
                    else if (str.Contains("realised_point")) RealizedPointGain = double.Parse(str.Remove(0, 14));
                    else if (str.Contains("history_point")) HistoryRealizedPointGain = double.Parse(str.Remove(0, 13));
                    else if (str.Contains("adl_ranking")) ADLRanking = int.Parse(str.Remove(0, 11));
                    else if (str.Contains("pending_orders")) PendingOrders = int.Parse(str.Remove(0, 14));
                    else if (str.Contains("id")) CloseOrderID = long.Parse(str.Remove(0, 2));
                    else if (str.Contains("price")) CloseOrderPrice = double.Parse(str.Remove(0, 5));
                    else if (str.Contains("is_liq")) CloseIsLiquidated = bool.Parse(str.Remove(0, 6));
                    else if (str.Contains("mode")) Mode = str.Remove(0, 4);
                    else if (!str.Contains("{") && !str.Contains("}")) throw new Exception("Unknown Json");
                }
            }
        }

        public string ToJson()
        {
            return _Json;
        }

        public override string ToString()
        {
            string str = "Position[\r\n  User:{0},\r\n  Contract:{1},\r\n  Size:{2},\r\n  Leverage:{3},\r\n  " +
                "RiskLimit:{4},\r\n  LeverageMax:{5},\r\n  MaintenanceRate:{6},\r\n  Value:{7},\r\n  Margin:{8},\r\n  " +
                "EntryPrice:{9},\r\n  LiquidationPrice:{10},\r\n  MarketPrice:{11},\r\n  UnrealizedGain:{12},\r\n  " +
                "RealizedGain:{13},\r\n  HistoryRealizedGain:{14},\r\n  LastCloseGain:{15},\r\n  RealizedPointGain:{16},\r\n  " +
                "HistoryRealizedPointGain:{17},\r\n  ADLRanking:{18},\r\n  PendingOrders:{19},\r\n  CloseOrderID:{20}\r\n  " +
                "CloseOrderPrice:{21},\r\n  CloseIsLiquidated:{22},\r\n  Mode:{23}\r\n]\r\n";
            return string.Format(str, User, Contract, Size, Leverage, RiskLimit, LeverageMax, MaintenanceRate, Value, Margin, 
                EntryPrice, LiquidationPrice, MarketPrice, UnrealizedGain, RealizedGain, HistoryRealizedGain, LastCloseGain, 
                RealizedPointGain, HistoryRealizedPointGain, ADLRanking, PendingOrders, CloseOrderID, CloseOrderPrice, CloseIsLiquidated, Mode);
        }

    }
}
