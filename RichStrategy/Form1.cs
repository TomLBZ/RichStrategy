using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RichStrategy
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
        }

        private enum PlotFakeDataType
        {
            UP, DOWN, U, N, UNKNOWN
        }

        const double _FAKE_TIME_RANGE = 5;           //range for time
        const double _FAKE_TIMESTAMP = 0.01;       //step for time
        const double _FAKE_PRICE_HALFRANGE = 125;    //delta price
        const double _FAKE_RANDOM_BASE_PERIOD_FACTOR = 0.5; //determines the frequency of the base sine
        const double _FAKE_RANDOM_BASE_PHASE_FACTOR = 1; //determines the phase of the base sine
        const double _FAKE_RANDOM_PATTERN_RANGE = 5;  //determines the frequency of sine patterns
        const double _FAKE_RANDOM_PATTERN_RANGE_RATIO = 5; //determines the percentage of randon patterns that falls normal
        const double _FAKE_RANDOM_SCALE_RANGE = 50;  //determines the magnitude of sine patterns
        const double _FAKE_RANDOM_PHASE_FACTOR = 1.5; //determines the phase of sine patterns
        const double _FAKE_RANDOM_NOISE_RANGE = 10;  //determines noise scale

        const double _STRATEGY_FEE_RATIO = 0.001; //fee for making a transaction
        const double _STRATEGY_PAINC_RATIO = 0.5;//force transaction when the loss destroys this fraction of gain
        const int _STRATEGY_MINIMUM_HOLD_TIME = 10; //minimum hold time (arbitrary unit)
        const int _STRATEGY_MAXIMUM_LOSS = 25; //maximum acceptable loss (arbitrary unit)
        const int _STRATEGY_COOLDOWN_TIME = 2; //cool down time before next transaction
        const int _STRATEGY_MINIMUM_PROFIT_TO_FEE_RATIO = 3;//minimum profit to fee ratio before a transaction
        const int _STRATEGY_FAST_TREND = 5; //K line, MA5
        const int _STRATEGY_SLOW_TREND = 20;//K line, MA20

        Random fakeDataRandomizer = new Random();
        Bitmap dataPlotBitmap;
        Point[] dataPoints;

        private void btnTestPlot_Click(object sender, EventArgs e)
        {
            PlotFakeDataType dataType;
            Button btnPressed = (Button)sender;
            switch (btnPressed.Text.Split('(')[1])
            {
                case "Up)":
                    dataType = PlotFakeDataType.UP;
                    break;
                case "Down)":
                    dataType = PlotFakeDataType.DOWN;
                    break;
                case "U)":
                    dataType = PlotFakeDataType.U;
                    break;
                case "N)":
                    dataType = PlotFakeDataType.N;
                    break;
                default:
                    dataType = PlotFakeDataType.UNKNOWN;
                    break;
            }
            dataPoints = processFakeData(picTestPlot.Width, picTestPlot.Height, getFakeData(dataType));
            dataPlotBitmap = plotData(picTestPlot.Width, picTestPlot.Height, dataPoints);
            picTestPlot.Image = dataPlotBitmap;
        }

        private double getPeriodicFunction(PlotFakeDataType dataType, double input)
        {
            switch (dataType)
            {
                case PlotFakeDataType.UP:
                    return -Math.Sin(input);
                case PlotFakeDataType.DOWN:
                    return Math.Sin(input);
                case PlotFakeDataType.U:
                    return Math.Cos(input);
                case PlotFakeDataType.N:
                    return -Math.Cos(input);
                default:
                    return 0;
            }
        }

        private double[] getFakeData(PlotFakeDataType dataType)
        {
            double[] fakeData;
            int patternMax = (int)Math.Ceiling(_FAKE_RANDOM_PATTERN_RANGE);
            double[] randomPatterns = new double[patternMax];
            double[] randomScales = new double[patternMax];
            double[] randomPhases = new double[patternMax];
            for (int patterns = 0; patterns < patternMax; patterns++)
            {
                randomPatterns[patterns] = fakeDataRandomizer.NextDouble() * _FAKE_RANDOM_PATTERN_RANGE;
                randomScales[patterns] = fakeDataRandomizer.NextDouble() * _FAKE_RANDOM_SCALE_RANGE;
                randomPhases[patterns] = fakeDataRandomizer.NextDouble() * _FAKE_RANDOM_PHASE_FACTOR;
            }
            fakeData = new double[(int)Math.Ceiling(_FAKE_TIME_RANGE / _FAKE_TIMESTAMP)];
            int index = 0;
            double previousData = 0;
            for (double i = 0; i <= _FAKE_TIME_RANGE - _FAKE_TIMESTAMP; i += _FAKE_TIMESTAMP)
            {
                double stableI = i % (2 * Math.PI);
                double fakeBasePeriodPreFactor = _FAKE_RANDOM_BASE_PERIOD_FACTOR * fakeDataRandomizer.NextDouble();
                double fakeBasePeriodFactor = 1 - _FAKE_RANDOM_BASE_PERIOD_FACTOR / 2 + fakeBasePeriodPreFactor;
                double fakeBasePhaseShift = _FAKE_RANDOM_BASE_PHASE_FACTOR * fakeDataRandomizer.NextDouble();
                double fakeBaseline = _FAKE_PRICE_HALFRANGE + _FAKE_PRICE_HALFRANGE * getPeriodicFunction(dataType, fakeBasePeriodFactor * stableI + fakeBasePhaseShift);
                double fakePatternOutput = 0;
                for (int j = 0; j < patternMax; j++)
                {
                    int jIndex = fakeDataRandomizer.Next(0, (int)(patternMax * _FAKE_RANDOM_PATTERN_RANGE_RATIO));
                    if (jIndex >= patternMax) jIndex = j;
                    fakePatternOutput += randomScales[jIndex] * getPeriodicFunction(dataType, randomPatterns[jIndex] * stableI + randomPhases[jIndex]);
                }
                double fakeNoiseValue = _FAKE_RANDOM_NOISE_RANGE * fakeDataRandomizer.NextDouble();
                if (i == 0) previousData = fakeBaseline + fakePatternOutput + fakeNoiseValue;
                else
                {
                    previousData = (previousData + fakeBaseline + fakePatternOutput + fakeNoiseValue) / 2;
                }
                fakeData[index] = previousData;
                index++;
            }
            return fakeData;
        }

        private Point[] processFakeData(int w, int h, double[] data)
        {
            double dataMax = double.MinValue;
            double dataMin = double.MaxValue;
            for (int i = 0; i < data.Length; i++)
            {
                dataMax = Math.Max(dataMax, data[i]);
                dataMin = Math.Min(dataMin, data[i]);
            }
            for (int i = 0; i < data.Length; i++)
            {
                data[i] -= dataMin;
            }
            dataMax -= dataMin;
            lblRandomRange.Text = "Max = " + (int)dataMax;
            Point[] points = new Point[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                points[i] = new Point((int)(i / (double)data.Length * w), (int)(data[i] / Math.Ceiling(dataMax) * h));
            }
            return points;
        }

        private Bitmap plotData(int w, int h, Point[] dataPoints)
        {
            Bitmap bmp = new Bitmap(w, h);
            Graphics g = Graphics.FromImage(bmp);
            g.ScaleTransform(1f, -1f);      //flips y axis
            g.TranslateTransform(0f, -h + 1);   //shifts down by image height
            g.FillRectangle(Brushes.Black, 0, 0, w - 1, h - 1);
            g.DrawLines(Pens.DarkRed, dataPoints);
            foreach (Point p in dataPoints)
            {
                g.FillEllipse(Brushes.Orange, p.X - 1, p.Y - 1, 3, 3);
            }
            g.DrawRectangle(new Pen(Color.Blue, 1), 0, 0, w - 1, h - 1);
            g.Dispose();
            return bmp;
        }

        private void btnDiffInvTime_Click(object sender, EventArgs e)
        {
            if (dataPoints == null || dataPoints.Length < 1)
            {
                btnDiffInvTime.Text = "Difference - Inverse Time: Data Invalid";
                return;
            }
            Graphics g = Graphics.FromImage(picTestPlot.Image);
            g.ScaleTransform(1f, -1f);      //flips y axis
            g.TranslateTransform(0f, -picTestPlot.Height + 1);   //shifts down by image height
            double initialValue = 0;
            double lastValue = 0;
            double gain = 0;
            int holdTimeCountDown = _STRATEGY_MINIMUM_HOLD_TIME;
            int cooldownCountDown = 0;
            Point initialPoint = new Point(0, 0);
            for (int i = 0; i < dataPoints.Length; i++)
            {
                if (cooldownCountDown >= 0) {
                    if (cooldownCountDown == 0)
                    {
                        initialValue = dataPoints[i].Y;
                        lastValue = dataPoints[i].Y;
                        g.DrawRectangle(Pens.DarkGreen, dataPoints[i].X - 1, dataPoints[i].Y - 1, 3, 3);
                        initialPoint = dataPoints[i];
                    }
                    cooldownCountDown--; 
                    continue; 
                }
                double diff = dataPoints[i].Y - lastValue;
                double profitToFeeRatio = (dataPoints[i].Y - initialValue) / (dataPoints[i].Y * _STRATEGY_FEE_RATIO);
                bool isProfitEnough = profitToFeeRatio > _STRATEGY_MINIMUM_PROFIT_TO_FEE_RATIO;
                if (diff < 0 || isProfitEnough)
                {
                    double panicLoss = (lastValue - initialValue) * _STRATEGY_PAINC_RATIO;
                    panicLoss = Math.Min(panicLoss, _STRATEGY_MAXIMUM_LOSS);
                    double timeDecreaseFactor = -diff / panicLoss;
                    bool isResetting = (dataPoints[i].Y <= initialValue) || (timeDecreaseFactor >= 1) || (holdTimeCountDown <= 0) || isProfitEnough;
                    if (isResetting)
                    {
                        double tmpGain = dataPoints[i].Y - initialValue - dataPoints[i].Y * _STRATEGY_FEE_RATIO;
                        gain += tmpGain;                 //update gain
                        holdTimeCountDown = _STRATEGY_MINIMUM_HOLD_TIME; //reset hold time
                        cooldownCountDown = isProfitEnough ? 0 : _STRATEGY_COOLDOWN_TIME;    //reset cooldown time
                        g.DrawRectangle(Pens.DarkViolet, dataPoints[i].X - 1, dataPoints[i].Y - 1, 3, 3);
                        g.DrawLine(Pens.LightBlue, initialPoint, dataPoints[i]);
                    }
                    else
                    {
                        holdTimeCountDown = (int)(holdTimeCountDown - holdTimeCountDown / (2 - timeDecreaseFactor));
                    }
                }
                lastValue = dataPoints[i].Y;
            }
            btnDiffInvTime.Text = "Difference - Inverse Time: Gain = " + (int)gain;
            picTestPlot.Refresh();
            g.Dispose();
        }

        private void btnDerivative_Click(object sender, EventArgs e)
        {
            if (dataPoints == null || dataPoints.Length < 1)
            {
                btnDerivative.Text = "Derivative: Data Invalid";
                return;
            }
            Graphics g = Graphics.FromImage(picTestPlot.Image);
            g.ScaleTransform(1f, -1f);      //flips y axis
            g.TranslateTransform(0f, -picTestPlot.Height + 1);   //shifts down by image height
            double initialValue = 0;
            double lastValue = 0;
            double gain = 0;
            int cooldownCountDown = 0;
            Point initialPoint = new Point(0, 0);
            for (int i = 0; i < dataPoints.Length; i++)
            {
                if (cooldownCountDown >= 0)
                {
                    if (cooldownCountDown == 0)
                    {
                        initialValue = dataPoints[i].Y;
                        lastValue = dataPoints[i].Y;
                        g.DrawRectangle(Pens.DarkGreen, dataPoints[i].X - 1, dataPoints[i].Y - 1, 3, 3);
                        initialPoint = dataPoints[i];
                    }
                    cooldownCountDown--;
                    continue;
                }
                double diff = dataPoints[i].Y - lastValue;
                bool isDecreasing = diff < 0;
                double profitToFeeRatio = (dataPoints[i].Y - initialValue) / (dataPoints[i].Y * _STRATEGY_FEE_RATIO);
                bool isProfitEnough = profitToFeeRatio > _STRATEGY_MINIMUM_PROFIT_TO_FEE_RATIO;
                if (isDecreasing || isProfitEnough)
                {
                    double tmpGain = dataPoints[i].Y - initialValue - dataPoints[i].Y * _STRATEGY_FEE_RATIO;
                    gain += tmpGain;                 //update gain
                    cooldownCountDown = isProfitEnough ? 0 : _STRATEGY_COOLDOWN_TIME;    //reset cooldown time
                    g.DrawRectangle(Pens.DarkViolet, dataPoints[i].X - 1, dataPoints[i].Y - 1, 3, 3);
                    g.DrawLine(Pens.LightBlue, initialPoint, dataPoints[i]);
                }
                lastValue = dataPoints[i].Y;
            }
            btnDerivative.Text = "Derivative: Gain = " + (int)gain;
            picTestPlot.Refresh();
            g.Dispose();
        }

        private Point[] getKLines(int ma, Point[] pts)
        {
            if (pts == null || pts.Length < 1) return null;
            Point[] ptsNew = new Point[pts.Length];
            for (int i = 0; i < pts.Length; i++)
            {
                int y = 0;
                int count = ma;
                for (int j = i + 1 - ma; j < i + 1; j++)
                {
                    if (j < 0) count--;
                    else y += pts[j].Y;
                }
                ptsNew[i] = new Point(pts[i].X, y / count);
            }
            return ptsNew;
        }

        private void btnDoubleTrendlines_Click(object sender, EventArgs e)
        {
            if (dataPoints == null || dataPoints.Length < 1)
            {
                btnDoubleTrendlines.Text = "Double Trendlines: Data Invalid";
                return;
            }
            Point[] k5 = getKLines(_STRATEGY_FAST_TREND, dataPoints);
            Point[] k20 = getKLines(_STRATEGY_SLOW_TREND, dataPoints);
            Graphics g = Graphics.FromImage(picTestPlot.Image);
            g.ScaleTransform(1f, -1f);      //flips y axis
            g.TranslateTransform(0f, -picTestPlot.Height + 1);   //shifts down by image height
            g.DrawLines(Pens.DarkViolet, k5);
            g.DrawLines(Pens.DarkCyan, k20);
            g.DrawRectangle(Pens.Blue, 0, 0, 100, 40);
            double initialValue = 0;
            double gain = 0;
            Point initialPoint = new Point(0, 0);
            bool lastEntering = false;
            for (int i = 0; i < dataPoints.Length; i++)
            {
                if (k5[i].Y > k20[i].Y)
                {
                    if (!lastEntering)
                    {
                        initialValue = dataPoints[i].Y;
                        g.DrawRectangle(Pens.DarkGreen, dataPoints[i].X - 1, dataPoints[i].Y - 1, 3, 3);
                        initialPoint = dataPoints[i];
                    }
                    lastEntering = true;
                }
                else 
                { 
                    if (lastEntering)
                    {
                        double tmpGain = dataPoints[i].Y - initialValue - dataPoints[i].Y * _STRATEGY_FEE_RATIO;
                        gain += tmpGain;                 //update gain
                        g.DrawRectangle(Pens.DarkViolet, dataPoints[i].X - 1, dataPoints[i].Y - 1, 3, 3);
                        g.DrawLine(Pens.LightBlue, initialPoint, dataPoints[i]);
                        lastEntering = false;
                    }
                }
            }
            btnDoubleTrendlines.Text = "Double Trendlines: Gain = " + (int)gain;
            picTestPlot.Refresh();
            g.Dispose();
        }
    }
}
