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
    #endregion
    public class DataAnalyzer
    {
        #region Properties
        public Strategy.TIMEFRAME TimeFrame { get; set; }
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
        /// <summary>
        /// 1、趋势判断从交易时间单位向上一级、两级、三级均相符时（例如，交易时间单位是5分钟，则需要查看15分钟、1小时、8小时），
        /// 2、且在交易的时间单位上与均线满足一定关系（例如，涨势超过均线，跌时低于均线）时，
        /// 3、且在交易的时间单位上存在可辨识的方向性的特征（例如旗帜、三角、双底、山形等等）时，
        /// 4、且在更低一或两级的时间单位（例如交易时间单位时5分钟，低级时间单位是1分钟或30秒）存在局部的入场时机（例如38.2%蜡烛、紧随的蜡烛等等）时，
        /// 5、且在局部的时机上有成交量支持时，
        /// 6、根据1中的趋势进行买入/卖出的判断。
        /// 7、如果从趋势市进入震荡市，则用网格交易策略。
        /// 具体的买卖数额不在这里计算。
        /// 第一个任务：判断当前是趋势市还是震荡市。
        /// </summary>
        /// <param name="currentPrice">当前价格</param>
        /// <param name="currentVolume">当前成交量</param>
        /// <returns></returns>
        public EvaluationResult EvaluateStep(double currentPrice, double currentVolume) { return EvaluationResult.DISALLOW; }
    }
}
