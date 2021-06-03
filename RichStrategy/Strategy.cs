using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RichStrategy.Strategy
{
    public class Strategy
    {
        #region Data Properties
        private CandleGraphData _TradeFrameData;
        private CandleGraphData _LowerFrameData;
        private CandleGraphData _HigherFrameData;
        private CandleGraphData _OverarchingFrameData;
        private int _Leverage;
        private double _FundAvailable;
        private double _CurrentTokensPosition;
        private double _Gain;
        #endregion
        public Strategy(int leverage) 
        {
            _Leverage = leverage;

        }
        public void UpdateData(CandleGraphData tradeFrameData, CandleGraphData lowerFrameData, CandleGraphData higherFrameData, CandleGraphData overarchingFrameData)
        {
            _TradeFrameData = tradeFrameData;
            _LowerFrameData = lowerFrameData;
            _HigherFrameData = higherFrameData;
            _OverarchingFrameData = overarchingFrameData;
        }
        public void UpdateAction()
        { 

        }
        public string GetStatus()
        {
            return API.GateIO.GetFuturesAccountInfoFromGateIO().ToString() + API.GateIO.GetPositionFromGateIO().ToString();
        }
    }
}
