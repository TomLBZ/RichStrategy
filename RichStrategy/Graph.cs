using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Collections;

namespace RichStrategy
{


    public partial class Graph : PictureBox
    {
        #region structs
        public struct GraphSettings
        {
            public Color gBackColor;
            public Color gAxisColor;
            public Color gGridColor;
            public Color gLabelColor;
            public Color gRawDataColor;
            public Color gCandleBullColor;
            public Color gCandleBearColor;
            public Color gTrendTopTurningColor;
            public Color gTrendBottomTurningColor;
            public GraphSettings(Color backColor, Color axisColor, Color gridColor, Color labelColor, Color rawDataColor, Color candleBullColor, Color candleBearColor,
                Color trendTopTurningColor, Color trendBottomTurningColor)
            {
                gBackColor = backColor;
                gAxisColor = axisColor;
                gGridColor = gridColor;
                gLabelColor = labelColor;
                gRawDataColor = rawDataColor;
                gCandleBullColor = candleBullColor;
                gCandleBearColor = candleBearColor;
                gTrendTopTurningColor = trendTopTurningColor;
                gTrendBottomTurningColor = trendBottomTurningColor;
            }
        }

        private struct Candle
        {
            public double OpenPrice;
            public double ClosePrice;
            public double MaxPrice;
            public double MinPrice;
            public Candle(double open, double close, double max, double min)
            {
                OpenPrice = open;
                ClosePrice = close;
                MinPrice = min;
                MaxPrice = max;
            }
        }

        #endregion

        #region Properties
        public GraphSettings Settings { get; set; }
        public double[] Data { get; set; }
        public int DataIntervalSeconds { get; set; }
        public int DataFrames { get; set; }
        public int CandleCompoundCount { get; set; }
        #endregion
        public Graph()
        {
            MouseMove += new MouseEventHandler(Graph_MouseMove);
            MouseLeave += new EventHandler(Graph_MouseLeave);
            MouseDown += new MouseEventHandler(Graph_MouseDown);
            MouseUp += new MouseEventHandler(Graph_MouseUp);
            DoubleBuffered = true;
            Settings = new GraphSettings(Color.Black, Color.Green, Color.DarkGray, Color.LightBlue, Color.DarkMagenta, Color.Red, Color.LimeGreen, Color.HotPink, Color.LightSeaGreen);
            DataIntervalSeconds = 30;
            DataFrames = 500;
            CandleCompoundCount = 10; // 30 second -> 5 minutes
            Redraw();
        }
        bool RIGHT_BTN_DOWN = false;
        private void Graph_MouseDown(object sender, MouseEventArgs e)
        {
            RIGHT_BTN_DOWN = e.Button == MouseButtons.Right;
        }
        private void Graph_MouseUp(object sender, MouseEventArgs e)
        {
            RIGHT_BTN_DOWN = e.Button != MouseButtons.Right;
        }
        public new void OnMouseWheel(MouseEventArgs e)
        {
            if (!RIGHT_BTN_DOWN)
            {
                DataFrames += e.Delta;
                int lowerLimit = Math.Min(120, CandleCompoundCount * 10);
                if (Data is not null && DataFrames > Data.Length) DataFrames = Data.Length;
                if (DataFrames < lowerLimit) DataFrames = lowerLimit;
            }
            else
            {
                CandleCompoundCount += e.Delta / 60;
                if (Data is not null && CandleCompoundCount > Data.Length / 10) CandleCompoundCount = Data.Length / 10;
                if (CandleCompoundCount < 10) CandleCompoundCount = 10;
            }
            Redraw();
            base.OnMouseWheel(e);
        }
        private void Graph_MouseMove(object sender, MouseEventArgs e)
        {
            if (Image is null) Image = new Bitmap(Width, Height);
            using(Graphics g = Graphics.FromImage(Image))
            {
                g.Clear(Color.Transparent);
                DrawLabels(g);
                Pen gridPen = new Pen(Settings.gGridColor, 0.001f);
                g.DrawLine(gridPen, 0, e.Y, Width, e.Y);
                g.DrawLine(gridPen, e.X, 0, e.X, Height);
                float fracDown = (float)e.Y / Height;
                float valueY = (float)(shiftV - 2 * diff * fracDown);
                SolidBrush labelBrush = new SolidBrush(Settings.gLabelColor);
                g.DrawString(valueY.ToString(), DefaultFont, labelBrush, 0, e.Y);
            }
            Refresh();
        }
        private void Graph_MouseLeave(object sender, EventArgs e)
        {
            if (Image is null) Image = new Bitmap(Width, Height);
            using (Graphics g = Graphics.FromImage(Image))
            {
                g.Clear(Color.Transparent);
                DrawLabels(g);
            }
            Refresh();
        }
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            if (BackgroundImage.Width != Width || BackgroundImage.Height != Height)
            {
                Image img = BackgroundImage;
                BackgroundImage = null;
                img.Dispose();
            }
            if (Image.Width != Width || Image.Height != Height)
            {
                Image img = Image;
                Image = null;
                img.Dispose();
            }
            Redraw();
        }
        double dataMax = -1;
        double dataMin = -1;
        int frameH = -1;
        double diff = -1;
        double frameV = -1;
        double shiftV = -1;
        int dataStartIndex = -1;
        private void UpdateData()
        {
            if (Data is null)
            {
                dataMax = -1;
                dataMin = -1;
                frameH = -1;
                diff = -1;
                frameV = -1;
                shiftV = -1;
                dataStartIndex = -1;
                return;
            }
            double min = double.PositiveInfinity;
            double max = double.NegativeInfinity;
            int len = Data.Length;
            frameH = Math.Min(len, DataFrames);
            for(int i = Math.Max(len - frameH, 0); i < len; i++)
            {
                min = Math.Min(min, Data[i]);
                max = Math.Max(max, Data[i]);
            }
            dataMin = min;
            dataMax = max;
            diff = dataMax - dataMin;
            frameV = diff * 2;
            shiftV = (dataMax + dataMin) / 2 + diff;
            dataStartIndex = Data.Length - frameH;
        }
        public void Redraw()
        {
            UpdateData();
            if (Image is null) Image = new Bitmap(Width, Height);
            if (BackgroundImage is null) BackgroundImage = new Bitmap(Width, Height);
            using (Graphics g = Graphics.FromImage(BackgroundImage))
            {
                g.Clear(Settings.gBackColor);
                if (dataMax > dataMin && dataMin >= 0 && frameH > 0)
                {
                    g.ScaleTransform(Width / (float)frameH, -Height / (float)frameV);
                    g.TranslateTransform(-dataStartIndex, -(float)shiftV);
                    DrawCanvas(g);
                    DrawData(g);
                    DrawCandles(g);
                    DrawTrends(g);
                    DrawIndicators(g);
                }
            }
            using (Graphics g = Graphics.FromImage(Image))
            {
                g.Clear(Color.Transparent);
                DrawLabels(g);
            }
            Refresh();
        }
        private void DrawCanvas(Graphics g)
        {
            Pen axisPen = new Pen(Settings.gAxisColor, 0.001f);
            g.DrawLine(axisPen, dataStartIndex, (float)shiftV, dataStartIndex, (float)(shiftV - 2 * diff));
            g.DrawLine(axisPen, dataStartIndex, (float)(shiftV - diff), dataStartIndex + frameH - 1, (float)(shiftV - diff));
        }

        private void DrawData(Graphics g)
        {
            List<PointF> framedData = new List<PointF>();
            for (int i = dataStartIndex; i < Data.Length; i++)
            {
                framedData.Add(new PointF(i, (float)Data[i]));
            }
            Color transparentRawDataColor = Color.FromArgb(128, Settings.gRawDataColor);
            g.DrawLines(new Pen(transparentRawDataColor, 0.001f), framedData.ToArray());
        }
        private Candle[] candles;
        private void DrawCandles(Graphics g)
        {
            Color bullColor = Color.FromArgb(128, Settings.gCandleBullColor);
            Color bearColor = Color.FromArgb(128, Settings.gCandleBearColor);
            double candleMin = double.PositiveInfinity;
            double candleMax = double.NegativeInfinity;
            double candleOpen = double.PositiveInfinity;
            double candleClose = double.PositiveInfinity;
            int startPosition = dataStartIndex;
            List<Candle> candles_data = new List<Candle>();
            int initialMod = dataStartIndex % CandleCompoundCount;
            for (int i = dataStartIndex; i < Data.Length; i++)
            {
                int mod = i % CandleCompoundCount;
                if (mod == initialMod)
                {
                    candleOpen = Data[i];
                    candleMin = candleOpen;
                    candleMax = candleOpen;
                    candleClose = candleOpen;
                } else
                {
                    candleMin = Math.Min(candleMin, Data[i]);
                    candleMax = Math.Max(candleMax, Data[i]);
                    candleClose = Data[i];
                    if (mod == CandleCompoundCount - 1)
                    {
                        Color color = candleClose > candleOpen ? bullColor : bearColor;
                        float cBottom = (float)Math.Min(candleClose, candleOpen);
                        float tailLinePos = startPosition + (float)CandleCompoundCount / 2;
                        g.FillRectangle(new SolidBrush(color), startPosition, cBottom, CandleCompoundCount, (float)Math.Abs(candleOpen - candleClose));
                        g.DrawLine(new Pen(color), tailLinePos, (float)candleMin, tailLinePos, (float)candleMax);
                        startPosition += CandleCompoundCount;
                        candles_data.Add(new Candle(candleOpen, candleClose, candleMax, candleMin));
                    }
                }
            }
            candles = candles_data.ToArray();
        }
        private void DrawTrends(Graphics g)
        {
            int data_trigger = Math.Max(5, DataFrames / CandleCompoundCount / 100); // 50 sections or 10 candles
            double tmin = double.PositiveInfinity, tmax = double.NegativeInfinity;
            int accumax = 0, accumin = 0;
            int index = dataStartIndex, maxindex = dataStartIndex, minindex = dataStartIndex;
            List<PointF> turns = new List<PointF>();
            List<PointF> topturns = new List<PointF>();
            List<PointF> bottomturns = new List<PointF>();
            foreach (Candle c in candles)
            {
                if (c.MaxPrice < tmax) accumax++;
                else { tmax = c.MaxPrice; maxindex = index; accumax = 0; }
                if(accumax > data_trigger)
                {
                    accumax = 0;
                    PointF ptfTop = new PointF(maxindex, (float)tmax);
                    turns.Add(ptfTop);
                    topturns.Add(ptfTop);
                    tmax = double.NegativeInfinity;
                }
                if (c.MinPrice > tmin) accumin++;
                else { tmin = c.MinPrice; minindex = index; accumin = 0; }
                if (accumin > data_trigger)
                {
                    accumin = 0;
                    PointF ptfBtm = new PointF(minindex, (float)tmin);
                    turns.Add(ptfBtm);
                    bottomturns.Add(ptfBtm);
                    tmin = double.PositiveInfinity;
                }
                index += CandleCompoundCount;
            }
            if (tmax != double.NegativeInfinity)
            {
                PointF ptf = new PointF(maxindex, (float)tmax);
                if (!turns.Contains(ptf)) { turns.Add(ptf); topturns.Add(ptf); }
            }
            if (tmin != double.PositiveInfinity)
            {
                PointF ptf = new PointF(minindex, (float)tmin);
                if (!turns.Contains(ptf)) { turns.Add(ptf); bottomturns.Add(ptf); }
            }
            Color topTurningColor = Color.FromArgb(192, Settings.gTrendTopTurningColor);
            Color bottomTurningColor = Color.FromArgb(192, Settings.gTrendBottomTurningColor);
            foreach (PointF ptF in topturns) g.FillEllipse(new SolidBrush(topTurningColor), ptF.X, ptF.Y, CandleCompoundCount, CandleCompoundCount);
            foreach (PointF ptF in bottomturns) g.FillEllipse(new SolidBrush(bottomTurningColor), ptF.X, ptF.Y, CandleCompoundCount, CandleCompoundCount);
            PointF ptfMax = turns[0];
            PointF ptfMin = turns[0];
            PointF ptfUpMinBuffer = turns[0];
            PointF ptfDownMaxBuffer = turns[0];
            int trendState = 0; // -1: down; 0: unclear; 1: up
            List<RectangleF> upTrends = new List<RectangleF>();
            List<RectangleF> downTrends = new List<RectangleF>();
            foreach (PointF ptf in turns)
            {
                if (ptf.Y >= ptfMax.Y) // up trend initiate
                {
                    ptfMax = ptf;
                    trendState = 1;
                }
                else if (ptf.Y < ptfMax.Y && ptf.Y >= ptfUpMinBuffer.Y) // up trend correction
                {
                    ptfUpMinBuffer = ptf;
                }
                else if (ptf.Y < ptfUpMinBuffer.Y && ptf.Y > ptfDownMaxBuffer.Y) // trend reversal
                {
                    ptfUpMinBuffer = ptf;
                    ptfDownMaxBuffer = ptf;
                    if (trendState == 1) upTrends.Add(new RectangleF(ptfMin.X, ptfMin.Y, Math.Abs(ptf.X - ptfMin.X), Math.Abs(ptfMax.Y - ptfMin.Y)));
                    else if (trendState == -1) downTrends.Add(new RectangleF(ptfMax.X, ptfMin.Y, Math.Abs(ptfMax.X - ptf.X), Math.Abs(ptfMax.Y - ptfMin.Y)));
                    ptfMin = ptfMax = ptf;
                    trendState = 0;
                }
                else if (ptf.Y <= ptfDownMaxBuffer.Y && ptf.Y > ptfMin.Y) // down trend correction
                {
                    ptfDownMaxBuffer = ptf;
                }
                else if (ptf.Y <= ptfMin.Y) // down trend initiate
                {
                    ptfMin = ptf;
                    trendState = -1;
                }
            }
            if (trendState != 0)
            {
                if (trendState == 1) upTrends.Add(new RectangleF(ptfMin.X, ptfMin.Y, Math.Abs(turns.Last().X - ptfMin.X), Math.Abs(ptfMax.Y - ptfMin.Y)));
                else if (trendState == -1) downTrends.Add(new RectangleF(ptfMax.X, ptfMin.Y, Math.Abs(ptfMax.X - turns.Last().X), Math.Abs(ptfMax.Y - ptfMin.Y)));
            }
            Color bullColor = Color.FromArgb(48, Settings.gCandleBullColor);
            Color bearColor = Color.FromArgb(48, Settings.gCandleBearColor);
            foreach (RectangleF rectf in upTrends) g.FillRectangle(new SolidBrush(bullColor), rectf);
            foreach (RectangleF rectf in downTrends) g.FillRectangle(new SolidBrush(bearColor), rectf);
        }
        private void DrawIndicators(Graphics g)
        {
            if (candles is null) return;
            Color bullColor = Color.FromArgb(128, Settings.gCandleBullColor);
            Color bearColor = Color.FromArgb(128, Settings.gCandleBearColor);

        }
        private void DrawLabels(Graphics g)
        {
            SolidBrush labelBrush = new SolidBrush(Settings.gLabelColor);
            g.DrawString(shiftV.ToString("C"), DefaultFont, labelBrush, 0, 0);
            g.DrawString((shiftV - diff).ToString("C"), DefaultFont, labelBrush, 0, (float)(Height / 2f));
            float strHeight = g.MeasureString(shiftV.ToString(), DefaultFont).Height;
            g.DrawString((shiftV - 2 * diff).ToString("C"), DefaultFont, labelBrush, 0, (float)(Height - strHeight));
            int candleSeconds = CandleCompoundCount * DataIntervalSeconds;
            string str = "Data Interval: " + DataIntervalSeconds.ToString() + "s; Data Frame: " + DataFrames.ToString() + " samples = " + (DataFrames / 2).ToString()
                + "min; Candle Interval: " + candleSeconds.ToString() + "s = " + (candleSeconds / 60).ToString() + " min = " + (candleSeconds / 3600).ToString() + " hrs.";
            float strWidth = g.MeasureString(str, DefaultFont).Width;
            g.DrawString(str, DefaultFont, labelBrush, (Width - strWidth) / 2f, 0);
        }
    }
}
