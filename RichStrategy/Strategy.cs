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
        public int Trend { get; set; }
        public double ATR14 { get; set; }
        public double EMA8 { get; set; }
        public double EMA20 { get; set; }
        public double EMA50 { get; set; }

        #endregion
        public Strategy() { }
        public void UpdateAction()
        { 

        }
    }
}
