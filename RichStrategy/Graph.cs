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
        #endregion
        public Graph()
        {
            DoubleBuffered = true;
            Settings = new GraphSettings(Color.Black, Color.Green, Color.DarkGray, Color.LightBlue, Color.DarkMagenta, Color.Red, Color.LimeGreen);
            DataIntervalSeconds = 30;
            DataFrames = 500;
            Redraw();
        }
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            Redraw();
        }
        public void Redraw()
        {
            UpdateData();
            DrawCanvas();
            DrawData();
            DrawIndicators();
        }
        double dataMax = -1;
        double dataMin = -1;
        int frameH = -1;
        private void UpdateData()
        {
            if (Data is null)
            {
                dataMax = -1;
                dataMin = -1;
                frameH = -1;
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
        }
        private void DrawCanvas()
        {
            if (Image is null)
            {
                Image = new Bitmap(Width, Height);
            }
            using (Graphics g = Graphics.FromImage(Image))
            {
                g.Clear(Settings.gBackColor);
                if (dataMax > dataMin && dataMin >= 0 && frameH > 0)
                {
                    double diff = dataMax - dataMin;
                    double frameV = diff * 2;
                    double shiftV = (dataMax + dataMin) / 2 + diff;
                    int dataStartIndex = Data.Length - frameH;
                    g.ScaleTransform(Width / (float)frameH, -Height / (float)frameV);
                    g.TranslateTransform(-dataStartIndex, -(float)shiftV);
                    List<PointF> framedData = new List<PointF>();
                    for (int i = dataStartIndex; i < Data.Length; i++)
                    {
                        framedData.Add(new PointF(i, (float)Data[i]));
                    }
                    g.DrawLines(new Pen(Settings.gRawDataColor, 0.001f), framedData.ToArray());
                }
                Refresh();
            }
        }

        private void DrawData()
        {
            if (Data is null) return;
        }
        private void DrawIndicators()
        {

        }
    }
}
