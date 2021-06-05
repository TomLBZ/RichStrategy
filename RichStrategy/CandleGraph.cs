using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Drawing;
using RichStrategy.Strategy;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RichStrategy
{
    public partial class CandleGraph : PictureBox, INotifyPropertyChanged
    {
        #region Private Members
        private readonly Color _ColorBack = Color.Black;
        private readonly Color _ColorCross = Color.DarkGray;
        private readonly Color _ColorLabel = Color.LightBlue;
        private readonly Color _ColorBull = Color.FromArgb(127, Color.Red);
        private readonly Color _ColorBear = Color.FromArgb(127, Color.LimeGreen);
        private readonly Color _ColorBullLabel = Color.FromArgb(192, Color.Red);
        private readonly Color _ColorBearLabel = Color.FromArgb(192, Color.LimeGreen);
        private readonly Color _ColorEMA8 = Color.FromArgb(127, Color.LightPink);
        private readonly Color _ColorEMA20 = Color.FromArgb(127, Color.HotPink);
        private readonly Color _ColorEMA50 = Color.FromArgb(127, Color.DeepPink);
        private readonly Color _ColorDistributionEstimator = Color.FromArgb(127, Color.Blue);
        private readonly Color _ColorTrendline = Color.FromArgb(127, Color.DarkKhaki);
        private readonly Color _ColorATR14 = Color.FromArgb(127, Color.Cyan);
        private readonly Color _ColorATR14Bound = Color.FromArgb(63, Color.Cyan);
        private readonly Color _ColorVolume = Color.FromArgb(127, Color.Orange);
        private readonly Color _ColorStrategyBuy = Color.FromArgb(225, Color.Red);
        private readonly Color _ColorStrategySell = Color.FromArgb(225, Color.LimeGreen);
        private TIMEFRAME _TimeFrame = TIMEFRAME.TF_1M;
        private string _Contract = "BTC_USD";
        private string _Settle = "btc";
        private int _DataFrame = 200;
        private int _UpdatePeriodSeconds = 10;
        private int _TrendLevels = 2;
        private readonly int _CandleWidth = 10;
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
        private bool _IsExportData = false;
        private bool _IsPeriodiclyTriggered = false;
        private List<Candle> _CandleList;
        private readonly Timer _Timer = new();
        private Random _Random = new();
        private readonly CandleGraphData _ExportData = new();
        private Action _PeriodicTrigger = null;
        private int _ExportableTrend = 0;
        private double _ExportableEstimateY = 0.0;
        private double _ExportableEMA8 = 0.0;
        private double _ExportableEMA20 = 0.0;
        private double _ExportableEMA50 = 0.0;
        private double _ExportableATR14 = 0.0;
        private double _ExportableVolume = 0.0;
        private double _ExportableRefVolume = 0.0;
        private double _ExportableValue = 0.0;
        private double _RewardRiskRatio = 1.5;
        private static double _ExportableMarketBuyPrice = 0.0;
        private static double _ExportableMarketSellPrice = 0.0;
        private readonly int _InstanceID = 0;
        private static int _InstanceCount = 0;
        private static int _LowestTimeFrame = (int)TIMEFRAME.TF_10S;
        private static readonly List<int> _TimeFrameIDList = new();
        #endregion

        #region Properties
        public TIMEFRAME TimeFrame
        {
            get { return _TimeFrame; }
            set { _TimeFrame = value; NotifyPropertyChanged(); }
        }
        public string Contract {
            get { return _Contract; }
            set { _Contract = value; }
        }
        public string Settle
        {
            get { return _Settle; }
            set { _Settle = value; }
        }
        public int DataFrame
        {
            get { return _DataFrame; }
            set { 
                if (value >= 100 && value <= 500) _DataFrame = value; 
            }
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
        public int TrendLevels
        {
            get { return _TrendLevels; }
            set
            {
                if (value >= 1 && value <= 3) _TrendLevels = value;
            }
        }
        public double RewardRiskRatio
        {
            get { return _RewardRiskRatio; }
            set
            {
                if (value > 1 && value < 5) _RewardRiskRatio = value;
            }
        }
        public event PropertyChangedEventHandler PropertyChanged; 
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region Auto Ranging
        private void RangeGraph()
        {
            if (_CandleList is null || _CandleList.Count == 0) _IsEmptyData = true;
            else _IsEmptyData = false;
            _MaxPrice = double.NegativeInfinity;
            _MinPrice = double.PositiveInfinity;
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
                Color color;
                float rectBLX = i * _CandleWidth, rectBLY, candleHeight;
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
                float lineX = rectBLX + _CandleWidth / 2f;
                if (candleHeight == 0)
                {
                    color = Color.FromArgb(_ColorBull.A,(_ColorBull.R+_ColorBear.R)/2, (_ColorBull.G + _ColorBear.G) / 2, (_ColorBull.B + _ColorBear.B) / 2);
                    g.DrawLine(new Pen(color), rectBLX, rectBLY, rectBLX + _CandleWidth, rectBLY);
                }
                else g.FillRectangle(new SolidBrush(color), rectBLX, rectBLY, _CandleWidth, candleHeight);
                g.DrawLine(new Pen(color), lineX, (float)candle.High, lineX, (float)candle.Low);
            }
        }
        private void DrawTrends(Graphics g)
        {
            List<PointF> closePoints = GetClosePricePointsFromCandles();
            _ExportableValue = closePoints[^1].Y;
            List<PointF> turningCloseZigZag = GetTurningPointsFromPricePoints(closePoints);
            if (_TrendLevels == 2) turningCloseZigZag = GetTrendZigLinesFromTurningPoints(turningCloseZigZag);
            if (_TrendLevels == 3) turningCloseZigZag = GetTrendZigLinesFromTurningPoints(turningCloseZigZag);
            g.DrawLines(new Pen(_ColorTrendline), turningCloseZigZag.ToArray());
            PointF last = turningCloseZigZag[^1], last2 = turningCloseZigZag[^2];
            _ExportableTrend = last.Y > last2.Y ? 1 : last.Y < last2.Y ? -1 : 0;
        }
        private void DrawIndicators(Graphics g)
        {
            double[] samples = GetNumericClosePricesFromCandles();
            _ExportableEstimateY = GetLocalDistributionEstimator(samples[^50..], ref _Random);
            g.DrawLine(new Pen(_ColorDistributionEstimator), 0, (float)_ExportableEstimateY, (float)_FrameWidth, (float)_ExportableEstimateY);
            List<PointF> closePoints = GetClosePricePointsFromCandles();
            List<PointF> EMA8 = GetEMAFromPricePoints(closePoints, 8);
            List<PointF> EMA20 = GetEMAFromPricePoints(closePoints, 20);
            List<PointF> EMA50 = GetEMAFromPricePoints(closePoints, 50);
            _ExportableEMA8 = EMA8[^1].Y;
            _ExportableEMA20 = EMA20[^1].Y;
            _ExportableEMA50 = EMA50[^1].Y;
            g.DrawLines(new Pen(_ColorEMA8), EMA8.ToArray());
            g.DrawLines(new Pen(_ColorEMA20), EMA20.ToArray());
            g.DrawLines(new Pen(_ColorEMA50), EMA50.ToArray());
            List<PointF> ATR14 = GetATR14FromCandles();
            _ExportableATR14 = ATR14[^1].Y - _MinY;
            g.DrawLines(new Pen(_ColorATR14), ATR14.ToArray());
            List<PointF> Volumes = GetVolumeFromCandles();
            _ExportableVolume = (Volumes[^1].Y - _MinY) * (_CenterY - _MinY) / 1000.0;
            g.DrawLines(new Pen(_ColorVolume), Volumes.ToArray());
            double refVolume = 0;
            foreach (PointF ptf in Volumes) refVolume += ptf.Y;
            refVolume /= Volumes.Count;
            g.DrawLine(new Pen(_ColorVolume), 0, (float)refVolume, (float)_FrameWidth, (float)refVolume);
            _ExportableRefVolume = (refVolume - _MinY) * (_CenterY - _MinY) / 1000.0;
            if ((int)TimeFrame == _LowestTimeFrame)
            {
                int lowestID = _InstanceID;
                foreach (int id in _TimeFrameIDList)
                {
                    if (id < lowestID) lowestID = id;
                }
                if (lowestID == _InstanceID)
                {
                    List<PointF> _OrderBook = API.GateIO.GetOrderBookFromGateIO();
                    _ExportableMarketSellPrice = _OrderBook[0].X;
                    _ExportableMarketBuyPrice = _OrderBook[10].X;
                }
            }
            g.FillRectangle(new SolidBrush(_ColorATR14Bound), 0, (float)(_ExportableMarketBuyPrice - _ExportableATR14),
                (float)_FrameWidth, (float)(2 * _ExportableATR14 + _ExportableMarketSellPrice - _ExportableMarketBuyPrice));
            g.DrawLine(new Pen(_ColorBull), 0, (float)_ExportableMarketBuyPrice, (float)_FrameWidth, (float)_ExportableMarketBuyPrice);
            g.DrawLine(new Pen(_ColorBear), 0, (float)_ExportableMarketSellPrice, (float)_FrameWidth, (float)_ExportableMarketSellPrice);
            Candle lastCandle = _CandleList[^2];
            double buyPrice = _ExportableMarketSellPrice, sellPrice = _ExportableMarketBuyPrice;
            double stoplossBuy = Math.Min(lastCandle.Low - _ExportableATR14, buyPrice - _ExportableATR14 * _RewardRiskRatio);
            double stoplossSell = Math.Max(lastCandle.High + _ExportableATR14, sellPrice + _ExportableATR14 * _RewardRiskRatio);
            double targetBuy = buyPrice + _RewardRiskRatio * (buyPrice - stoplossBuy);
            double targetSell = sellPrice - _RewardRiskRatio * (stoplossSell - sellPrice);
            Pen penStrategyBuy = new(_ColorStrategyBuy) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dot };
            Pen penStrategySell = new(_ColorStrategySell) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dot };
            g.DrawRectangle(penStrategyBuy, _CandleWidth * _CandleList.Count - _CandleWidth - _CandleWidth - 1,
                (float)stoplossBuy, _CandleWidth + _CandleWidth, (float)(targetBuy - stoplossBuy));
            g.DrawRectangle(penStrategySell, _CandleWidth * _CandleList.Count - _CandleWidth - _CandleWidth - 1,
                (float)targetSell, _CandleWidth + _CandleWidth, (float)(stoplossSell - targetSell));
        }
        private void DrawEmpty()
        {
            using (Graphics g = Graphics.FromImage(BackgroundImage))
            {
                g.Clear(_ColorBack);
                Pen pEmpty = new(_ColorCross);
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
            SolidBrush labelBrush = new(_ColorLabel);
            g.DrawString(_MaxY.ToString("C"), DefaultFont, labelBrush, 0, 0);
            g.DrawString(_CenterY.ToString("C"), DefaultFont, labelBrush, 0, (float)(Height / 2f));
            float strHeight = g.MeasureString(_MaxY.ToString(), DefaultFont).Height;
            g.DrawString(_MinY.ToString("C"), DefaultFont, labelBrush, 0, (float)(Height - strHeight));
            string str = "TimeFrame: " + _TimeFrame.GetDescription() + "; DataFrame: " + _DataFrame.ToString() + " samples";
            float strWidth = g.MeasureString(str, DefaultFont).Width;
            g.DrawString(str, DefaultFont, labelBrush, (Width - strWidth) / 2f, 0);
            float estimatorY = (float)((_MaxY - _ExportableEstimateY) / (_MaxY - _MinY) * Height);
            string strEstimator ="Estimated Next: " + _ExportableEstimateY.ToString("C");
            SizeF estimatorStrSize = g.MeasureString(strEstimator, DefaultFont);
            g.DrawString(strEstimator, DefaultFont, labelBrush, (Width - estimatorStrSize.Width) / 2f, estimatorY - estimatorStrSize.Height / 2f);
            float marketBuyY = (float)((_MaxY - _ExportableMarketBuyPrice) / (_MaxY - _MinY) * Height);
            string strMarketBuy = "Market Buy Price: " + _ExportableMarketBuyPrice.ToString("C");
            g.DrawString(strMarketBuy, DefaultFont, new SolidBrush(_ColorBullLabel), 0, marketBuyY);
            float marketSellY = (float)((_MaxY - _ExportableMarketSellPrice) / (_MaxY - _MinY) * Height);
            string strMarketSell = "Market Sell Price: " + _ExportableMarketSellPrice.ToString("C");
            float strMarketSellHeight = g.MeasureString(strMarketSell, DefaultFont).Height;
            g.DrawString(strMarketSell, DefaultFont, new SolidBrush(_ColorBearLabel), 0, marketSellY - strMarketSellHeight);
        }
        private void DrawCross(Graphics g, MouseEventArgs e)
        {
            Pen gridPen = new(_ColorCross, 0.001f);
            g.DrawLine(gridPen, 0, e.Y, Width, e.Y);
            g.DrawLine(gridPen, e.X, 0, e.X, Height);
            float fracDown = (float)e.Y / Height;
            float valueY = (float)(_MaxY - _FrameHeight * fracDown);
            float fracRight = (float)e.X / Width;
            float valueX = (float)(_FrameWidth * fracRight);
            string strX = valueX.ToString();
            SizeF strSize = g.MeasureString(strX, DefaultFont);
            float strxX = e.X + strSize.Width > Width ? e.X - strSize.Width : e.X;
            float stryY = e.Y + strSize.Height > Height ? e.Y - strSize.Height : e.Y;
            SolidBrush labelBrush = new(_ColorLabel);
            g.DrawString(valueY.ToString(), DefaultFont, labelBrush, 0, stryY);
            g.DrawString(strX, DefaultFont, labelBrush,  strxX, Height - strSize.Height);
        }
        private async void Redraw()
        {
            if (Width == 0 || Height == 0) return;
            if (_DrawingMutex) return;
            _DrawingMutex = true;
            if (null == Image) Image = new Bitmap(Width, Height);
            if (null == BackgroundImage) BackgroundImage = new Bitmap(Width, Height);
            await Task.Factory.StartNew(() =>
            {
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
                _DrawingMutex = false;
            });
            Refresh();
        }
        #endregion

        #region Analysis
        private static List<PointF> GetTrendZigLinesFromTurningPoints(List<PointF> turningPoints)
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
        private static List<PointF> GetTurningPointsFromPricePoints(List<PointF> pricePoints)
        {
            List<PointF> rtn = new();
            int lastTrend = 2;
            PointF lastPt = pricePoints[0];
            for (int i = 1; i < pricePoints.Count; i++)
            {
                PointF ptf = pricePoints[i];
                int trend = ptf.Y > lastPt.Y ? 1 : ptf.Y < lastPt.Y ? -1 : lastTrend;// 0;
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
        private static double GetLocalDistributionEstimator(double[] data, ref Random rnd)
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
        private static List<PointF> GetEMAFromPricePoints(List<PointF> data, int MA_Window)
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
        private List<PointF> GetATR14FromCandles()
        {
            Queue<double> TR14 = new(14);
            Candle currentCandle, lastCandle;
            double hcp, lcp, sumTR;
            float lineX;
            List<PointF> rtn = new();
            for (int i = 1; i < 15; i++)
            {
                currentCandle = _CandleList[i];
                lastCandle = _CandleList[i - 1];
                hcp = Math.Abs(currentCandle.High - lastCandle.Close);
                lcp = Math.Abs(currentCandle.Low - lastCandle.Close);
                TR14.Enqueue(Math.Max(currentCandle.High - currentCandle.Low, Math.Max(hcp, lcp)));
            }
            sumTR = 0;
            foreach (double d in TR14) sumTR += d;
            lineX = 14f * _CandleWidth + _CandleWidth / 2f;
            rtn.Add(new PointF(lineX, (float)(_MinY + sumTR / 14.0)));
            for (int i = 15; i < _CandleList.Count; i++)
            {
                lineX = (float)i * _CandleWidth + _CandleWidth / 2f;
                currentCandle = _CandleList[i];
                lastCandle = _CandleList[i - 1];
                hcp = Math.Abs(currentCandle.High - lastCandle.Close);
                lcp = Math.Abs(currentCandle.Low - lastCandle.Close);
                _ = TR14.Dequeue();
                TR14.Enqueue(Math.Max(currentCandle.High - currentCandle.Low, Math.Max(hcp, lcp)));
                sumTR = 0;
                foreach (double d in TR14) sumTR += d;
                rtn.Add(new PointF(lineX, (float)(_MinY + sumTR / 14.0)));
            }
            return rtn;
        }
        private List<PointF> GetVolumeFromCandles()
        {
            List<PointF> rtn = new();
            for (int i = 0; i < _CandleList.Count; i++)
            {
                float lineX = (float)i * _CandleWidth + _CandleWidth / 2f;
                rtn.Add(new PointF(lineX, (float)(_CandleList[i].Volume / (_CenterY - _MinY) + _MinY)));
            }
            return rtn;
        }
        #endregion

        #region Events
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
            UpdateData(true);
        }
        private void CG_PropertyChanged(object sender, EventArgs e)
        {
            if (e is PropertyChangedEventArgs args)
            {
                string pName = args.PropertyName;
                if(pName == nameof(TimeFrame))
                {
                    _TimeFrameIDList.RemoveAll(i => i == _InstanceID);
                    if ((int)TimeFrame <= _LowestTimeFrame)
                    {
                        _LowestTimeFrame = (int)TimeFrame;
                        _TimeFrameIDList.Add(_InstanceID);
                    }
                }
                else if (pName == nameof(UpdatePeriodSeconds))
                {
                    _Timer.Interval = UpdatePeriodSeconds * 1000;
                }
                else if (pName == nameof(AutoUpdateEnabled))
                {
                    _Timer.Enabled = AutoUpdateEnabled;
                }
            }
        }
        #endregion

        #region Public Methods
        public async void UpdateData(bool isTicked = false)
        {
            await Task.Factory.StartNew(() =>
            {
                _CandleList = API.GateIO.GetCandlesFromGateIO(_TimeFrame, _Settle, _Contract, _DataFrame);
            });
            RangeGraph();
            Redraw();
            if (isTicked && _IsExportData) ExportData();
            if (isTicked && _IsPeriodiclyTriggered) _PeriodicTrigger();
        }
        public void BindData(ref CandleGraphData data)
        {
            _IsExportData = true;
            data = _ExportData;
        }
        public void SetPeriodicTrigger(Action trigger)
        {
            _PeriodicTrigger = trigger;
            _IsPeriodiclyTriggered = true;
        }
        #endregion

        #region Data Exports
        private void ExportData()
        {
            if (!_IsExportData) return;
            _ExportData.TimeFrame = _TimeFrame;
            _ExportData.Trend = _ExportableTrend;
            _ExportData.Value = _ExportableValue;
            _ExportData.Estimate = _ExportableEstimateY;
            _ExportData.EMA8 = _ExportableEMA8;
            _ExportData.EMA20 = _ExportableEMA20;
            _ExportData.EMA50 = _ExportableEMA50;
            _ExportData.ATR14 = _ExportableATR14;
            _ExportData.Volume = _ExportableVolume;
            _ExportData.RefVolume = _ExportableRefVolume;
            _ExportData.LastCandle = _CandleList[^2];
            _ExportData.MarketBuyPrice = _ExportableMarketBuyPrice;
            _ExportData.MarketSellPrice = _ExportableMarketSellPrice;
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
            _InstanceID = _InstanceCount;
            _InstanceCount++;
            if ((int)TimeFrame <= _LowestTimeFrame)
            {
                _LowestTimeFrame = (int)TimeFrame;
                _TimeFrameIDList.Add(_InstanceID);
            }
            Redraw();
        }

    }

}
