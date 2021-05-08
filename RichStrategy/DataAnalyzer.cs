using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RichStrategy
{
    #region Enumerations
    public enum EvaluationResult
    {
        ALLOWBUY,
        ALLOWSELL,
        DISALLOW
    }
    public enum Timeframes
    {
        TF_10S,
        TF_30S,
        TF_1M,
        TF_5M,
        TF_10M,
        TF_15M,
        TF_20M,
        TF_25M,
        TF_30M,
        TF_45M,
        TF_1H,
        TF_2H,
        TF_4H,
        TF_8H,
        TF_12H,
        TF_1D,
        TF_2D,
        TF_7D,
        TF_14D,
        TF_30D
    }
    #endregion
    public class DataAnalyzer
    {
        #region Properties
        public Timeframes TimeFrame { get; set; }
        public double[] RawPriceData { get; set; }
        public double[] RawVolumeData { get; set; }
        #endregion
        #region Data
        private double[] MA9;
        private double[] MA20;
        private double[] MA50;
        private double[] MA200;
        private double[] EMA50;
        private double[] ATR14;
        #endregion
        public DataAnalyzer() { }

        public void Analyze() { }

        public EvaluationResult EvaluateStep(double currentPrice, double currentVolume) { return EvaluationResult.DISALLOW; }
    }
}
