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
            public GraphSettings(Color backColor, Color axisColor, Color gridColor, Color labelColor, Color rawDataColor, Color candleBullColor, Color candleBearColor)
            {
                gBackColor = backColor;
                gAxisColor = axisColor;
                gGridColor = gridColor;
                gLabelColor = labelColor;
                gRawDataColor = rawDataColor;
                gCandleBullColor = candleBullColor;
                gCandleBearColor = candleBearColor;
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
            Settings = new GraphSettings(Color.Black, Color.Green, Color.DarkGray, Color.LightBlue, Color.DarkMagenta, Color.Red, Color.LimeGreen);
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
            string str = "Data Interval: " + DataIntervalSeconds.ToString() + "s; Data Frame: " + DataFrames.ToString() + " samples; Candle Interval: "
                + candleSeconds.ToString() + "s = " + (candleSeconds / 60).ToString() + " min = " + (candleSeconds / 3600).ToString() + " hrs.";
            float strWidth = g.MeasureString(str, DefaultFont).Width;
            g.DrawString(str, DefaultFont, labelBrush, (Width - strWidth) / 2f, 0);
        }
    }
}
