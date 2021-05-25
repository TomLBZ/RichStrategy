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
        private Color _ColorBack = Color.Black;
        private Color _ColorCross = Color.DarkGray;
        private Color _ColorLabel = Color.LightBlue;
        private Color _ColorBull = Color.Red;
        private Color _ColorBear = Color.LimeGreen;
        private Color _ColorMA8 = Color.LightPink;
        private Color _ColorMA20 = Color.Pink;
        private Color _ColorMA50 = Color.DeepPink;
        private TIMEFRAME _TimeFrame = TIMEFRAME.TF_1M;
        private string _Contract = "BTC_USD";
        private string _Settle = "btc";
        private int _DataFrame = 100;
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
            double offset = (_MaxPrice - _MinPrice) / 2;
            _MaxY = _MaxPrice + offset;
            _MinY = _MinPrice - offset;
            _CenterY = _MinPrice + offset;
            _FrameWidth = _DataFrame * _CandleWidth;
            _FrameHeight = _MaxY - _MinY;
        }
        #endregion

        #region Drawing
        private void DrawCandles(Graphics g)
        {
            Color bullColor = Color.FromArgb(128, _ColorBull);
            Color bearColor = Color.FromArgb(128, _ColorBear);
            for (int i = 0; i < _CandleList.Count; i++)
            {
                Candle candle = _CandleList[i];
                Color color = Color.Transparent;
                float rectBLX = i * _CandleWidth, rectBLY = 0, candleHeight = 0;
                if (candle.Open > candle.Close)
                {
                    color = bearColor;
                    rectBLY = (float)candle.Close;
                    candleHeight = (float)(candle.Open - candle.Close);
                }
                else
                {
                    color = bullColor;
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
            List<PointF>[] zigzag = GetTrendZiglinesFromCandles();
            Color bullColor = Color.FromArgb(128, _ColorBull);
            Color bearColor = Color.FromArgb(128, _ColorBear);
            g.DrawLines(new Pen(bullColor), zigzag[0].ToArray());
            g.DrawLines(new Pen(bearColor), zigzag[1].ToArray());
        }
        private void DrawIndicators(Graphics g)
        {

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
            string str = "TimeFrame: " + _TimeFrame.GetDescription() + "; DataFrame: " + _DataFrame.ToString() + " samples [" +
                0 + " ~ " + (_FrameWidth - 1).ToString() + "]";
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
                float lineX = i * _CandleWidth + (float)(_CandleWidth / 2);
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
            Redraw();
        }

    }

}
