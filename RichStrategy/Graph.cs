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
            DoubleBuffered = true;
            Settings = new GraphSettings(Color.Black, Color.Green, Color.DarkGray, Color.LightBlue, Color.DarkMagenta, Color.Red, Color.LimeGreen);
            DataIntervalSeconds = 30;
            DataFrames = 500;
            CandleCompoundCount = 10; // 30 second -> 5 minutes
            Redraw();
        }
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
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
            using (Graphics g = Graphics.FromImage(Image))
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
                    DrawLabels(g);
                }
                Refresh();
            }
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
        private void DrawCandles(Graphics g)
        {
            Color bullColor = Color.FromArgb(128, Settings.gCandleBullColor);
            Color bearColor = Color.FromArgb(128, Settings.gCandleBearColor);
            double candleMin = double.PositiveInfinity;
            double candleMax = double.NegativeInfinity;
            double candleOpen = double.PositiveInfinity;
            double candleClose = double.PositiveInfinity;
            int startPosition = dataStartIndex;
            for (int i = dataStartIndex; i < Data.Length; i++)
            {
                int mod = i % CandleCompoundCount;
                if (mod == 0)
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
                    }
                }
            }
        }
        private void DrawIndicators(Graphics g)
        {
            Color bullColor = Color.FromArgb(128, Settings.gCandleBullColor);
            Color bearColor = Color.FromArgb(128, Settings.gCandleBearColor);

        }
        private void DrawLabels(Graphics g)
        {
            g.ScaleTransform(1, -1);
            SolidBrush labelBrush = new SolidBrush(Settings.gLabelColor);
            g.DrawString(shiftV.ToString(), DefaultFont, labelBrush, dataStartIndex, -(float)(shiftV));
            g.DrawString((shiftV - diff).ToString(), DefaultFont, labelBrush, dataStartIndex, -(float)(shiftV - diff));
            float strHeight = g.MeasureString(shiftV.ToString(), DefaultFont).Height;
            g.DrawString((shiftV - 2 * diff).ToString(), DefaultFont, labelBrush, dataStartIndex, -(float)(shiftV - 2 * diff + strHeight));
        }
    }
}
