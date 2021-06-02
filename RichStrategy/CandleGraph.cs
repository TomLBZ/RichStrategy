using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Drawing;
using RichStrategy.Strategy;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.Statistics;

namespace RichStrategy
{
    public partial class CandleGraph : PictureBox, INotifyPropertyChanged
    {
        #region Private Members
        private Color _ColorBack = Color.Black;
        private Color _ColorCross = Color.DarkGray;
        private Color _ColorLabel = Color.LightBlue;
        private Color _ColorBull = Color.FromArgb(127, Color.Red);
        private Color _ColorBear = Color.FromArgb(127, Color.LimeGreen);
        private Color _ColorEMA8 = Color.FromArgb(127, Color.LightPink);
        private Color _ColorEMA20 = Color.FromArgb(127, Color.HotPink);
        private Color _ColorEMA50 = Color.FromArgb(127, Color.DeepPink);
        private Color _ColorDistributionEstimator = Color.FromArgb(127, Color.Navy);
        private Color _ColorTrendline = Color.FromArgb(127, Color.DarkKhaki);
        private TIMEFRAME _TimeFrame = TIMEFRAME.TF_1M;
        private string _Contract = "BTC_USD";
        private string _Settle = "btc";
        private int _DataFrame = 200;
        private int _CandleWidth = 10;
        private int _UpdatePeriodSeconds = 10;
        private double _MaxPrice = double.NegativeInfinity;
        private double _MinPrice = double.PositiveInfinity;
        private double _MaxY = double.NegativeInfinity;
        private double _MinY = double.PositiveInfinity;
        private double _CenterY = double.PositiveInfinity;
        private double _FrameWidth = 0;
        private double _FrameHeight = 0;
        private bool _IsEmptyData = true;
        private bool _DrawingMutex = false;
        private bool _AutoUpdateEnabled = false;
        private List<Candle> _CandleList;
        private Timer _Timer = new();
        private Random _Random = new();
        #endregion

        #region Properties
        public TIMEFRAME TimeFrame
        {
            get { return _TimeFrame; }
            set { _TimeFrame = value; }
        }
        public int UpdatePeriodSeconds 
        {
            get { return _UpdatePeriodSeconds; }
            set { 
                if (value != _UpdatePeriodSeconds)
                {
                    _UpdatePeriodSeconds = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public bool AutoUpdateEnabled 
        {
            get { return _AutoUpdateEnabled; }
            set
            {
                if (value != _AutoUpdateEnabled)
                {
                    _AutoUpdateEnabled = value;
                    NotifyPropertyChanged();
                }
            }
        }
        
        public event PropertyChangedEventHandler PropertyChanged; 
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        #region Auto Ranging
        private void RangeGraph()
        {
            if (_CandleList is null || _CandleList.Count == 0) _IsEmptyData = true;
            else _IsEmptyData = false;
            foreach (Candle candle in _CandleList)
            {
                _MaxPrice = Math.Max(_MaxPrice, candle.High);
                _MinPrice = Math.Min(_MinPrice, candle.Low);
            }
            double offset = (_MaxPrice - _MinPrice) / 8;
            _MaxY = _MaxPrice + offset;
            _MinY = _MinPrice - offset;
            _CenterY = _MinPrice + offset * 4;
            _FrameWidth = _DataFrame * _CandleWidth;
            _FrameHeight = _MaxY - _MinY;
        }
        #endregion

        #region Drawing
        private void DrawCandles(Graphics g)
        {
            for (int i = 0; i < _CandleList.Count; i++)
            {
                Candle candle = _CandleList[i];
                Color color = Color.Transparent;
                float rectBLX = i * _CandleWidth, rectBLY = 0, candleHeight = 0;
                if (candle.Open > candle.Close)
                {
                    color = _ColorBear;
                    rectBLY = (float)candle.Close;
                    candleHeight = (float)(candle.Open - candle.Close);
                }
                else
                {
                    color = _ColorBull;
                    rectBLY = (float)candle.Open;
                    candleHeight = (float)(candle.Close - candle.Open);
                }
                float lineX = rectBLX + (float)(_CandleWidth / 2);
                g.FillRectangle(new SolidBrush(color), rectBLX, rectBLY, _CandleWidth, candleHeight);
                g.DrawLine(new Pen(color), lineX, (float)candle.High, lineX, (float)candle.Low);
            }
        }
        private void DrawTrends(Graphics g)
        {
            List<PointF> closePoints = GetClosePricePointsFromCandles();
            List<PointF> turningClosePoints = GetTurningPointsFromPricePoints(closePoints);
            List<PointF> turningCloseZigZag = GetTrendZigLinesFromTurningPoints(turningClosePoints);
            g.DrawLines(new Pen(_ColorTrendline), turningCloseZigZag.ToArray());
        }
        private void DrawIndicators(Graphics g)
        {
            double[] samples = GetNumericClosePricesFromCandles();
            double estimateY = GetLocalDistributionEstimator(samples[^50..], ref _Random);
            g.DrawLine(new Pen(_ColorDistributionEstimator), 0, (float)estimateY, (float)_FrameWidth, (float)estimateY);
            List<PointF> closePoints = GetClosePricePointsFromCandles();
            List<PointF> EMA8 = GetEMAFromPricePoints(closePoints, 8);
            List<PointF> EMA20 = GetEMAFromPricePoints(closePoints, 20);
            List<PointF> EMA50 = GetEMAFromPricePoints(closePoints, 50);
            g.DrawLines(new Pen(_ColorEMA8), EMA8.ToArray());
            g.DrawLines(new Pen(_ColorEMA20), EMA20.ToArray());
            g.DrawLines(new Pen(_ColorEMA50), EMA50.ToArray());
        }
        private void DrawEmpty()
        {
            using (Graphics g = Graphics.FromImage(BackgroundImage))
            {
                g.Clear(_ColorBack);
                Pen pEmpty = new Pen(_ColorCross);
                g.DrawLine(pEmpty, 0, 0, Width, Height);
                g.DrawLine(pEmpty, 0, Height, Width, 0);
            }
            using (Graphics g = Graphics.FromImage(Image))
            {
                g.Clear(Color.Transparent);
                DrawLabels(g);
            }
        }
        private void DrawLabels(Graphics g)
        {
            SolidBrush labelBrush = new SolidBrush(_ColorLabel);
            g.DrawString(_MaxY.ToString("C"), DefaultFont, labelBrush, 0, 0);
            g.DrawString(_CenterY.ToString("C"), DefaultFont, labelBrush, 0, (float)(Height / 2f));
            float strHeight = g.MeasureString(_MaxY.ToString(), DefaultFont).Height;
            g.DrawString(_MinY.ToString("C"), DefaultFont, labelBrush, 0, (float)(Height - strHeight));
            string str = "TimeFrame: " + _TimeFrame.GetDescription() + "; DataFrame: " + _DataFrame.ToString() + " samples";
            float strWidth = g.MeasureString(str, DefaultFont).Width;
            g.DrawString(str, DefaultFont, labelBrush, (Width - strWidth) / 2f, 0);
        }
        private void DrawCross(Graphics g, MouseEventArgs e)
        {
            Pen gridPen = new Pen(_ColorCross, 0.001f);
            g.DrawLine(gridPen, 0, e.Y, Width, e.Y);
            g.DrawLine(gridPen, e.X, 0, e.X, Height);
            float fracDown = (float)e.Y / Height;
            float valueY = (float)(_MaxY - _FrameHeight * fracDown);
            float fracRight = (float)e.X / Width;
            float valueX = (float)(_FrameWidth * fracRight);
            string strX = valueX.ToString();
            SizeF strSize = g.MeasureString(strX, DefaultFont);
            float strxX = e.X + strSize.Width > Width ? e.X - strSize.Width : e.X;
            SolidBrush labelBrush = new SolidBrush(_ColorLabel);
            g.DrawString(valueY.ToString(), DefaultFont, labelBrush, 0, e.Y);
            g.DrawString(strX, DefaultFont, labelBrush,  strxX, Height - strSize.Height);
        }
        private void Redraw()
        {
            if (Width == 0 || Height == 0) return;
            if (_DrawingMutex) return;
            _DrawingMutex = true;
            if (null == Image) Image = new Bitmap(Width, Height);
            if (BackgroundImage is null) BackgroundImage = new Bitmap(Width, Height);
            if (_IsEmptyData) DrawEmpty();
            else
            {
                using (Graphics g = Graphics.FromImage(BackgroundImage))
                {
                    g.Clear(_ColorBack);
                    g.ScaleTransform(Width / (float)_FrameWidth, -Height / (float)_FrameHeight);
                    g.TranslateTransform(0, -(float)_MaxY);
                    DrawCandles(g);
                    DrawTrends(g);
                    DrawIndicators(g);
                }
                using (Graphics g = Graphics.FromImage(Image))
                {
                    g.Clear(Color.Transparent);
                    DrawLabels(g);
                }
            }
            Refresh();
            _DrawingMutex = false;
        }
        #endregion

        #region Analysis
        private List<PointF>[] ReduceTrendZigZagLines(List<PointF> higherTrendPoints, List<PointF> lowerTrendPoints)
        {
            if (higherTrendPoints.Count != lowerTrendPoints.Count) return null;
            List<PointF> highList = new();
            List<PointF> lowList = new();
            float oldHigher = -1, oldLower = -1, oldLineX = -1;
            float lastHigher = -1, lastLower = -1, lastLineX = -1;
            int trendState = 0; // -1 down, 0 unknown, 1 up
            for (int i = 0; i < higherTrendPoints.Count; i += 2)
            {
                float lineX = higherTrendPoints[i].X;
                float higher = higherTrendPoints[i].Y;
                float lower = lowerTrendPoints[i].Y;
                if (oldHigher == -1 && oldLower == -1)
                {
                    oldHigher = higher;
                    oldLower = lower;
                    oldLineX = lineX;
                    lastHigher = higher;
                    lastLower = lower;
                    trendState = 0;
                    continue;
                }
                if (higher >= lastHigher && lower >= lastLower)
                {
                    if (trendState != 1)
                    {
                        highList.Add(new PointF(oldLineX, oldHigher));
                        lowList.Add(new PointF(oldLineX, oldLower));
                        trendState = 1;
                        oldHigher = lastHigher;
                        oldLower = lastLower;
                        oldLineX = lastLineX;
                    }
                }
                else if (higher <= lastHigher && lower <= lastLower)
                {
                    if (trendState != -1)
                    {
                        highList.Add(new PointF(oldLineX, oldHigher));
                        lowList.Add(new PointF(oldLineX, oldLower));
                        trendState = -1;
                        oldHigher = lastHigher;
                        oldLower = lastLower;
                        oldLineX = lastLineX;
                    }
                }
                else
                {
                    if (trendState != 0)
                    {
                        highList.Add(new PointF(oldLineX, oldHigher));
                        lowList.Add(new PointF(oldLineX, oldLower));
                        trendState = 0;
                        oldHigher = lastHigher;
                        oldLower = lastLower;
                        oldLineX = lastLineX;
                    }
                }
                lastHigher = higher;
                lastLower = lower;
                lastLineX = lineX;
            }
            //Candle candleLast = _CandleList[^1];
            float lineXLast = higherTrendPoints[^1].X;
            float higherLast = higherTrendPoints[^1].Y;
            float lowerLast = lowerTrendPoints[^1].Y;
            highList.Add(new PointF(lineXLast, higherLast));
            lowList.Add(new PointF(lineXLast, lowerLast));
            List<PointF>[] rtn = { highList, lowList };
            return rtn;
        }
        private List<PointF>[] GetTrendZiglinesFromCandles()
        {
            List<PointF> highList = new();
            List<PointF> lowList = new();
            float oldHigher = -1, oldLower = -1, oldLineX = -1;
            float lastHigher = -1, lastLower = -1, lastLineX = -1;
            int trendState = 0; // -1 down, 0 unknown, 1 up
            for (int i = 0; i < _CandleList.Count; i++)
            {
                Candle candle = _CandleList[i];
                float lineX = (float)i * _CandleWidth + _CandleWidth / 2f;
                float higher = (float)Math.Max(candle.Open, candle.Close);
                float lower = (float)Math.Min(candle.Open, candle.Close);
                if (oldHigher == -1 && oldLower == -1)
                {
                    oldHigher = higher;
                    oldLower = lower;
                    oldLineX = lineX;
                    lastHigher = higher;
                    lastLower = lower;
                    trendState = 0;
                    continue;
                }
                if (higher >= lastHigher && lower >= lastLower)
                {
                    if (trendState != 1)
                    {
                        highList.Add(new PointF(oldLineX, oldHigher));
                        lowList.Add(new PointF(oldLineX, oldLower));
                        trendState = 1;
                        oldHigher = lastHigher;
                        oldLower = lastLower;
                        oldLineX = lastLineX;
                    }
                }
                else if (higher <= lastHigher && lower <= lastLower)
                {
                    if (trendState != -1)
                    {
                        highList.Add(new PointF(oldLineX, oldHigher));
                        lowList.Add(new PointF(oldLineX, oldLower));
                        trendState = -1;
                        oldHigher = lastHigher;
                        oldLower = lastLower;
                        oldLineX = lastLineX;
                    }
                }
                else
                {
                    if (trendState != 0)
                    {
                        highList.Add(new PointF(oldLineX, oldHigher));
                        lowList.Add(new PointF(oldLineX, oldLower));
                        trendState = 0;
                        oldHigher = lastHigher;
                        oldLower = lastLower;
                        oldLineX = lastLineX;
                    }
                }
                lastHigher = higher;
                lastLower = lower;
                lastLineX = lineX;
            }
            Candle candleLast = _CandleList[^1];
            float lineXLast = (float)(_FrameWidth - _CandleWidth / 2);
            float higherLast = (float)Math.Max(candleLast.Open, candleLast.Close);
            float lowerLast = (float)Math.Min(candleLast.Open, candleLast.Close);
            highList.Add(new PointF(lineXLast, higherLast));
            lowList.Add(new PointF(lineXLast, lowerLast));
            List<PointF>[] rtn = { highList, lowList };
            return rtn;
        }
        private List<PointF> GetTrendZigLinesFromPricePoints(List<PointF> pricePoints)
        {
            List<PointF> rtn = new();
            float startVal = pricePoints[0].Y, lastHigherVal = startVal, lastLowerVal = startVal;
            float startX = pricePoints[0].X, lastHigherX = startX, lastLowerX = startX;
            float lastX = startX, lastVal = startVal;
            int trendState = 0; // -1 down, 0 unknown, 1 up
            int innerTrend = 0;
            for (int i = 1; i < pricePoints.Count; i++)
            {
                float lineX = pricePoints[i].X;
                float lineVal = pricePoints[i].Y;
                if (lineVal > lastHigherVal)
                {
                    lastHigherVal = lineVal;
                    lastHigherX = lineX;
                    if (trendState != 1)
                    {
                        rtn.Add(new PointF(startX, startVal));
                        trendState = 1;
                    }
                    if (innerTrend == -1)
                    {
                        lastLowerVal = lastVal;
                        lastLowerX = lastX;
                    }
                    innerTrend = 0;
                }
                else if (lineVal >= lastLowerVal)
                {
                    int currentInnerTrend = lineVal > lastVal ? 1 : lineVal < lastVal ? -1 : 0;
                    if (innerTrend != currentInnerTrend)
                    {
                        if (innerTrend != 0 && currentInnerTrend != 0)
                        {
                            if (innerTrend == -1)
                            {
                                lastLowerX = lastX;
                                lastLowerVal = lastVal;
                            }
                            else if (innerTrend == 1)
                            {
                                lastHigherVal = lastVal;
                                lastHigherX = lastX;
                            }
                        }
                        innerTrend = currentInnerTrend;
                    }
                    if (trendState == 1)
                    {
                        startVal = lastHigherVal;
                        startX = lastHigherX;
                    } 
                    else if (trendState == -1)
                    {
                        startX = lastLowerX;
                        startVal = lastLowerVal;
                    }
                    else
                    {
                        startX = lineX;
                        startVal = lineVal;
                    }
                }
                else
                {
                    lastLowerVal = lineVal;
                    lastLowerX = lineX;
                    if (trendState != -1)
                    {
                        rtn.Add(new PointF(startX, startVal));
                        trendState = -1;
                    }
                    if (innerTrend == 1)
                    {
                        lastHigherX = lastX;
                        lastHigherVal = lastVal;
                    }
                    innerTrend = 0;
                }
                lastX = lineX;
                lastVal = lineVal;
            }
            rtn.Add(new PointF(lastX, lastVal));
            return rtn;
        }
        private List<PointF> GetTrendZigLinesFromTurningPoints(List<PointF> turningPoints)
        {
            List<PointF> rtn = new();
            PointF startPt = turningPoints[0], lastHighPt = startPt, lastLowPt = startPt, lastPt = startPt;
            int lastTrend = 0, innerTrend = 0;
            for (int i = 1; i < turningPoints.Count; i++)
            {
                PointF ptf = turningPoints[i];
                if (ptf.Y > lastHighPt.Y)
                {
                    lastHighPt = ptf;
                    if (lastTrend != 1)
                    {
                        rtn.Add(startPt);
                        lastTrend = 1;
                    }
                    startPt = ptf;
                    if (innerTrend == -1) lastLowPt = lastPt;
                    innerTrend = 0;
                }
                else if (ptf.Y >= lastLowPt.Y)
                {
                    int currentInnerTrend = ptf.Y > lastPt.Y ? 1 : ptf.Y < lastPt.Y ? -1 : 0;
                    if (innerTrend != 0)
                    {
                        if (currentInnerTrend == -1 && innerTrend == 1) lastHighPt = lastPt;
                        else if (currentInnerTrend == 1 && innerTrend == -1) lastLowPt = lastPt;
                    }
                    innerTrend = currentInnerTrend;
                    if (lastTrend == 1) startPt = lastHighPt;
                    else if (lastTrend == -1) startPt = lastLowPt;
                    else startPt = lastPt;
                }
                else
                {
                    lastLowPt = ptf;
                    if (lastTrend != -1)
                    {
                        rtn.Add(startPt);
                        lastTrend = -1;
                    }
                    startPt = ptf;
                    if (innerTrend == 1) lastHighPt = lastPt;
                    innerTrend = 0;
                }
                lastPt = ptf;
            }
            rtn.Add(turningPoints[^1]);
            return rtn;
        }
        private List<PointF> GetClosePricePointsFromCandles()
        {
            List<PointF> rtn = new();
            for (int i = 0; i < _CandleList.Count; i++)
            {
                float lineX = (float)i * _CandleWidth + _CandleWidth / 2f;
                rtn.Add(new PointF(lineX, (float)_CandleList[i].Close));
            }
            return rtn;
        }
        private List<PointF> GetTurningPointsFromPricePoints(List<PointF> pricePoints)
        {
            List<PointF> rtn = new();
            int lastTrend = 2;
            PointF lastPt = pricePoints[0];
            for (int i = 1; i < pricePoints.Count; i++)
            {
                PointF ptf = pricePoints[i];
                int trend = ptf.Y > lastPt.Y ? 1 : ptf.Y < lastPt.Y ? -1 : 0;
                if (trend != lastTrend)
                {
                    lastTrend = trend;
                    rtn.Add(lastPt);
                }
                lastPt = ptf;
            }
            rtn.Add(pricePoints[^1]);
            return rtn;
        }
        private double[] GetNumericClosePricesFromCandles()
        {
            double[] rtn = new double[_CandleList.Count];
            for (int i = 0; i < _CandleList.Count; i++)
            {
                rtn[i] = _CandleList[i].Close;
            }
            return rtn;
        }
        private double GetLocalDistributionEstimator(double[] data, ref Random rnd)
        {
            double[] diff = new double[data.Length - 1];
            for (int i = 1; i < data.Length; i++)
            {
                diff[i - 1] = (data[i] - data[i - 1]) / data[i - 1];
            }
            int[] partitionCounts = new int[22];
            foreach (double d in diff)
            {
                if (d > 0.001) partitionCounts[0]++;
                else if (d > 0.0009) partitionCounts[1]++;
                else if (d > 0.0008) partitionCounts[2]++;
                else if (d > 0.0007) partitionCounts[3]++;
                else if (d > 0.0006) partitionCounts[4]++;
                else if (d > 0.0005) partitionCounts[5]++;
                else if (d > 0.0004) partitionCounts[6]++;
                else if (d > 0.0003) partitionCounts[7]++;
                else if (d > 0.0002) partitionCounts[8]++;
                else if (d > 0.0001) partitionCounts[9]++;
                else if (d > 0) partitionCounts[10]++;
                else if (d > -0.0001) partitionCounts[11]++;
                else if (d > -0.0002) partitionCounts[12]++;
                else if (d > -0.0003) partitionCounts[13]++;
                else if (d > -0.0004) partitionCounts[14]++;
                else if (d > -0.0005) partitionCounts[15]++;
                else if (d > -0.0006) partitionCounts[16]++;
                else if (d > -0.0007) partitionCounts[17]++;
                else if (d > -0.0008) partitionCounts[18]++;
                else if (d > -0.0009) partitionCounts[19]++;
                else if (d > -0.001) partitionCounts[20]++;
                else partitionCounts[21]++;
            }
            int max = int.MinValue;
            int index = -1;
            for (int i = 0; i < partitionCounts.Length; i++)
            {
                if (partitionCounts[i] > max)
                {
                    max = partitionCounts[i];
                    index = i;
                }
            }
            double rtn = rnd.NextGaussian(0.001 - 0.0001 * index, 0.0001);
            return (rtn + 1) * data[^1];
        }
        private List<PointF> GetEMAFromPricePoints(List<PointF> data, int MA_Window)
        {
            double ema = data[0].Y;
            double multiplier = 2.0 / (MA_Window + 1.0);
            for (int i = 0; i < MA_Window; i++)
            {
                ema += (data[i].Y - ema) * multiplier;
            }
            List<PointF> rtn = new();
            for (int i = MA_Window; i < data.Count; i++)
            {
                ema += (data[i].Y - ema) * multiplier;
                rtn.Add(new PointF(data[i].X, (float)ema));
            }
            return rtn;
        }
        #endregion

        #region Events
        public async void UpdateData()
        {
            await Task.Factory.StartNew(() =>
            {
                _CandleList = API.GateIO.GetCandlesFromGateIO(API.GateIO.Key, API.GateIO.Secret, _TimeFrame, _Settle, _Contract, _DataFrame);
            });
            RangeGraph();
            Redraw();
        }
        private void CG_MouseWheel(object sender, MouseEventArgs e)
        {
            _DataFrame += e.Delta / 12; // wheel once => increase by 10
            if (_DataFrame < 100) _DataFrame = 100;
            UpdateData();
        }
        private void CG_MouseMove(object sender, MouseEventArgs e)
        {
            if (Image is null) Image = new Bitmap(Width, Height);
            using (Graphics g = Graphics.FromImage(Image))
            {
                g.Clear(Color.Transparent);
                DrawLabels(g);
                DrawCross(g, e);
            }
            Refresh();
        }
        private void CG_MouseLeave(object sender, EventArgs e)
        {
            if (Image is null) Image = new Bitmap(Width, Height);
            using (Graphics g = Graphics.FromImage(Image))
            {
                g.Clear(Color.Transparent);
                DrawLabels(g);
            }
            Refresh();
        }
        private void CG_SizeChanged(object sender, EventArgs e)
        {
            if (Width == 0 || Height == 0) return;
            if (null != BackgroundImage && BackgroundImage.Width != Width || BackgroundImage.Height != Height)
            {
                Image img = BackgroundImage;
                BackgroundImage = null;
                img.Dispose();
            }
            if (null != Image && Image.Width != Width || Image.Height != Height)
            {
                Image img = Image;
                Image = null;
                img.Dispose();
            }
            Redraw();
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            UpdateData();
        }
        private void CG_PropertyChanged(object sender, EventArgs e)
        {
            _Timer.Interval = UpdatePeriodSeconds * 1000;
            _Timer.Enabled = AutoUpdateEnabled;
        }
        #endregion

        public CandleGraph()
        {
            MouseWheel += new MouseEventHandler(CG_MouseWheel);
            MouseMove += new MouseEventHandler(CG_MouseMove);
            MouseLeave += new EventHandler(CG_MouseLeave);
            SizeChanged += new EventHandler(CG_SizeChanged);
            PropertyChanged += new PropertyChangedEventHandler(CG_PropertyChanged);
            _Timer.Tick += new EventHandler(Timer_Tick);
            TimeFrame = TIMEFRAME.TF_1M;
            DoubleBuffered = true;
            UpdatePeriodSeconds = 10;
            AutoUpdateEnabled = false;
            _Timer.Interval = UpdatePeriodSeconds * 1000;
            _Timer.Enabled = AutoUpdateEnabled;
            _Random = new();
            Redraw();
        }

    }

}
