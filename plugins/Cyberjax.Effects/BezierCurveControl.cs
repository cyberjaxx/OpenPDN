/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using PaintDotNet;

namespace Cyberjax
{
    /// <summary>
    /// This class is for manipulation of transfer functions.
    /// It is intended for curve adjustment
    /// </summary>
    public abstract class BezierCurveControl 
        : UserControl
    {
        private System.ComponentModel.Container components = null;

        private readonly int[] curvesInvalidRange = new int[] { int.MaxValue, int.MinValue };
        private Point LastMouseXY { get; set; } = new Point(int.MinValue, int.MinValue);
        private int LastKey { get; set; } = -1;
        private int LastValue { get; set; } = -1;
        private bool Tracking { get; set; } = false;
        private int[] DragPointIndex { get; set; }
        private int[] PointsNearMousePerChannel { get; set; }
        private bool[] EffectChannel { get; set; }

        protected ControlPointList[] controlPoints;
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ControlPointList[] ControlPoints
        {
            get => this.controlPoints;

            set
            {
                if (value.Length != Channels)
                {
                    throw new ArgumentException("value must have a matching channel count", "value");
                }

                this.controlPoints = value;
                Invalidate();
            }
        }

        public int Channels { get; protected set; } = 0;

        public int Entries { get; protected set; } = 0;

        protected ColorBgra[] VisualColors { get; set; } = null;
        public ColorBgra GetVisualColor(int channel)
        {
            return VisualColors == null ? ColorBgra.Zero : VisualColors[channel];
        }

        protected string[] ChannelNames { get; set; }
        public string GetChannelName(int channel)
        {
            return ChannelNames == null ? string.Empty : ChannelNames[channel];
        }

        protected bool[] Mask { get; set; }

        public void SetSelected(int channel, bool val)
        {
            if (Mask != null)
            {
                Mask[channel] = val;
                Invalidate();
            }
        }

        public bool GetSelected(int channel)
        {
            return Mask == null ? false : Mask[channel];
        }

        protected internal BezierCurveControl(int channels, int entries)
        {
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);

            Channels = channels;
            Entries = entries;

            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            PointsNearMousePerChannel = new int[channels];
            for (int i = 0; i < channels; ++i)
            {
                PointsNearMousePerChannel[i] = -1;
            }
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                    components = null;
                }
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code
        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.TabStop = false;
        }

        #endregion

        public event EventHandler ValueChanged;
        protected virtual void OnValueChanged()
        {
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler<EventArgs<Point>> CoordinatesChanged;
        protected virtual void OnCoordinatesChanged()
        {
            CoordinatesChanged?.Invoke(this, new EventArgs<Point>(new Point(LastKey, LastValue)));
        }

        public void ResetControlPoints()
        {
            ControlPointList[] ctrlPoints = new ControlPointList[Channels];

            for (int i = 0; i < Channels; ++i)
            {
                ControlPointList newList = new ControlPointList
                {
                    new Point(0, 0),
                    new Point(Entries / 4, Entries / 4),
                    new Point(Entries * 3 / 4, Entries * 3/ 4),
                    new Point(Entries - 1, Entries - 1)
                };
                ctrlPoints[i] = newList;
            }

            ControlPoints = ctrlPoints;
            OnValueChanged();
        }

        private PointF[] ControlPointsToPointArray(ControlPointList channelControlPoints, float width, float height)
        {
            int points = channelControlPoints.Count;
            PointF[] pointArray = new PointF[points];

            for (int i = 0; i < points; ++i)
            {
                float x = channelControlPoints[i].X * (width - 1) / (Entries - 1);
                float y = (Entries - 1 - channelControlPoints[i].Y) * (height - 1) / (Entries - 1);
                pointArray[i] = new PointF(x, y);
            }
            return pointArray;
        }

        private int InsertPoint(int channel, Point point)
        {
            ControlPointList channelControlPoints = ControlPoints[channel];
            int points = channelControlPoints.Count;
            for (int i = 2; i < points; i += 3)
            {
                if (channelControlPoints[i + 1].X > point.X)    // check point against anchor
                {
                    channelControlPoints.Insert(i, new Point(point.X - 30, point.Y));
                    channelControlPoints.Insert(i + 1, point);
                    channelControlPoints.Insert(i + 2, new Point(point.X + 30, point.Y));
                    return i + 1;
                }
            }
            return -1;
        }

        private void DrawToGraphics(Graphics g)
        {
            ColorBgra colorSolid = ColorBgra.FromColor(this.ForeColor);
            ColorBgra colorGuide = ColorBgra.FromColor(this.ForeColor);
            ColorBgra colorGrid = ColorBgra.FromColor(this.ForeColor);

            colorGrid.A = 128;
            colorGuide.A = 96;

            Pen penSolid = new Pen(colorSolid.ToColor(), 1);
            Pen penGrid = new Pen(colorGrid.ToColor(), 1);
            Pen penGuide = new Pen(colorGuide.ToColor(), 1);

            penGrid.DashStyle = DashStyle.Dash;

            g.Clear(this.BackColor);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle ourRect = ClientRectangle;

            ourRect.Inflate(-1, -1);

            if (LastMouseXY.Y >= 0)
            {
                g.DrawLine(penGuide, 0, LastMouseXY.Y, Width, LastMouseXY.Y);
            }

            if (LastMouseXY.X >= 0)
            {
                g.DrawLine(penGuide, LastMouseXY.X, 0, LastMouseXY.X, Height);
            }

            for (float f = 0.25f; f <= 0.75f; f += 0.25f)
            {
                float x = Utility.Lerp(ourRect.Left, ourRect.Right, f);
                float y = Utility.Lerp(ourRect.Top, ourRect.Bottom, f);

                g.DrawLine(penGrid,
                    Point.Round(new PointF(x, ourRect.Top)),
                    Point.Round(new PointF(x, ourRect.Bottom)));

                g.DrawLine(penGrid,
                    Point.Round(new PointF(ourRect.Left, y)),
                    Point.Round(new PointF(ourRect.Right, y)));
            }

            g.DrawLine(penGrid, ourRect.Left, ourRect.Bottom, ourRect.Right, ourRect.Top);

            float width = this.ClientRectangle.Width;
            float height = this.ClientRectangle.Height;

            for (int c = 0; c < Channels; ++c)
            {
                ControlPointList channelControlPoints = ControlPoints[c];
                int points = channelControlPoints.Count;

                ColorBgra color = GetVisualColor(c);
                ColorBgra colorSelected = ColorBgra.Blend(color, ColorBgra.White, 128);

                const float penWidthNonSelected = 1;
                const float penWidthSelected = 2;
                float penWidth = Mask[c] ? penWidthSelected : penWidthNonSelected;
                Pen penSelected = new Pen(color.ToColor(), penWidth);

                color.A = 128;

                Pen pen = new Pen(color.ToColor(), penWidth);
                Brush brush = new SolidBrush(color.ToColor());
                SolidBrush brushSelected = new SolidBrush(Color.White);

                BezierInterpolator interpolator = new BezierInterpolator(channelControlPoints);
                PointF[] line = new PointF[Entries];
                for (int i = 0; i < line.Length; ++i)
                {
                    line[i].X = (float)i * (width - 1) / (Entries - 1);
                    line[i].Y = (float)(Utility.Clamp(Entries - 1 - interpolator.Interpolate(i), 0, Entries - 1)) *
                        (height - 1) / (Entries - 1);
                }

                pen.LineJoin = LineJoin.Round;
                g.DrawLines(pen, line);

                // Draw channel control points
                PointF[] pointArray = ControlPointsToPointArray(channelControlPoints, width, height);

                penGuide.DashStyle = DashStyle.DashDot;
                g.DrawLine(penGuide, pointArray[0], pointArray[1]);
                g.DrawLine(penGuide, pointArray[points - 2], pointArray[points - 1]);
                for (int i = 2; i < points - 2; i += 3)
                {
                    g.DrawLine(penGuide, pointArray[i], pointArray[i + 1]);
                    g.DrawLine(penGuide, pointArray[i + 1], pointArray[i + 2]);
                }

                for (int i = 0; i < points; ++i)
                {
                    const float radiusSelected = 4;
                    const float radiusNotSelected = 3;
                    const float radiusUnMasked = 2;

                    bool masked = Mask[c];
                    bool selected = (masked && PointsNearMousePerChannel[c] == i);
                    float size = selected ? radiusSelected : (masked ? radiusNotSelected : radiusUnMasked);
                    RectangleF rect = Utility.RectangleFromCenter(pointArray[i], size);

                    g.FillEllipse(selected ? brushSelected : brush, rect.X, rect.Y, rect.Width, rect.Height);
                    g.DrawEllipse(selected ? penSelected : pen, rect.X, rect.Y, rect.Width, rect.Height);
                }

                pen.Dispose();
            }

            penSolid.Dispose();
            penGrid.Dispose();
            penGuide.Dispose();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            DrawToGraphics(e.Graphics);
            base.OnPaint(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            float width = this.ClientRectangle.Width;
            float height = this.ClientRectangle.Height;
            int mx = (int)Utility.Clamp(0.5f + e.X * (Entries - 1) / (width - 1), 0, Entries - 1);
            int my = (int)Utility.Clamp(0.5f + Entries - 1 - e.Y * (Entries - 1) / (height - 1), 0, Entries - 1);

            DragPointIndex = new int[Channels];
            for (int i = 0; i < Channels; ++i)
            {
                DragPointIndex[i] = -1;
            }

            if (0 != e.Button)
            {

                if (e.Button == MouseButtons.Right)
                {
                    for (int c = 0; c < Channels; ++c)
                    {
                        if (Mask[c])
                        {
                            int index = PointsNearMousePerChannel[c];
                            ControlPointList channelControlPoints = ControlPoints[c];
                            if (index > 1 && index < channelControlPoints.Count - 2)
                            {
                                index = (index - 2) / 3 * 3 + 2;
                                channelControlPoints.RemoveAt(index);
                                channelControlPoints.RemoveAt(index);
                                channelControlPoints.RemoveAt(index);
                                OnValueChanged();
                            }
                        }
                    }
                }
                else if (e.Button == MouseButtons.Left)
                {
                    LastKey = mx;
                    Tracking = true;

                    bool anyNearMouse = false;

                    EffectChannel = new bool[Channels];

                    for (int c = 0; c < Channels; ++c)
                    {
                        int index = PointsNearMousePerChannel[c];
                        bool hasPoint = (index >= 0);

                        anyNearMouse = (anyNearMouse || hasPoint);

                        EffectChannel[c] = hasPoint;

                        if (Mask[c] && hasPoint)
                        {
                            DragPointIndex[c] = index;
                            OnValueChanged();
                        }
                    }

                    if (!anyNearMouse)
                    {
                        for (int c = 0; c < Channels; ++c)
                        {
                            if (Mask[c])
                            {
                                DragPointIndex[c] = InsertPoint(c, new Point(mx, my));
                                EffectChannel[c] = true;
                            }
                        }
                    }
                }

                OnMouseMove(e);
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (0 != (e.Button & MouseButtons.Left) && Tracking)
            {
                Tracking = false;
                LastKey = -1;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            LastMouseXY = new Point(e.X, e.Y);
            float width = this.ClientRectangle.Width;
            float height = this.ClientRectangle.Height;
            int mx = (int)Utility.Clamp(0.5f + e.X * (Entries - 1) / (width - 1), 0, Entries - 1);
            int my = (int)Utility.Clamp(0.5f + Entries - 1 - e.Y * (Entries - 1) / (height - 1), 0, Entries - 1);

            Invalidate();

            if (Tracking && e.Button == MouseButtons.None)
            {
                Tracking = false;
            }

            if (Tracking)
            {
                bool changed = false;
                for (int c = 0; c < Channels; ++c)
                {
                    ControlPointList channelControlPoints = ControlPoints[c];

                    PointsNearMousePerChannel[c] = -1;
                    if (Mask[c] && EffectChannel[c])
                    {
                        int index = DragPointIndex[c];

                        if (mx >= 0 && mx < Entries)
                        {
                            if (index >= 0 &&
                                (channelControlPoints[index].X != mx ||
                                channelControlPoints[index].Y != my))
                            {
                                channelControlPoints[index] = new Point(mx, my);
                                changed = true;
                            }

                            PointsNearMousePerChannel[c] = index;
                        }
                    }
                }

                if (changed)
                {
                    Update();
                    OnValueChanged();
                }
            }
            else
            {
                PointsNearMousePerChannel = new int[Channels];

                for (int c = 0; c < Channels; ++c)
                {
                    ControlPointList channelControlPoints = ControlPoints[c];
                    int minRadiusSq = 30;
                    int bestIndex = -1;

                    if (Mask[c])
                    {
                        // ignore first and last points
                        for (int i = 1; i < channelControlPoints.Count - 1; ++i)
                        {
                            int sumsq = 0;
                            int diff = 0;

                            diff = channelControlPoints[i].X - mx;
                            sumsq += diff * diff;

                            diff = channelControlPoints[i].Y - my;
                            sumsq += diff * diff;

                            if (sumsq < minRadiusSq)
                            {
                                minRadiusSq = sumsq;
                                bestIndex = i;
                            }
                        }
                    }

                    PointsNearMousePerChannel[c] = bestIndex;
                }

                Update();
            }

            LastKey = mx;
            LastValue = my;
            OnCoordinatesChanged();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            LastKey = -1;
            LastValue = -1;
            LastMouseXY = new Point(int.MinValue, int.MinValue);
            Invalidate();
            OnCoordinatesChanged();
            base.OnMouseLeave(e);
        }
    }

    public struct BezierPoint
    {
        public bool Smooth;
        public Point Anchor;
        public Point Handle1;
        public Point Handle2;

        public BezierPoint(Point anchor)
        {
            Anchor = anchor;
            Handle1 = new Point(anchor.X - 10, anchor.Y);
            Handle2 = new Point(anchor.X + 10, anchor.Y);
            Smooth = true;
        }

        public BezierPoint(Point anchor, Point handle1, Point handle2, bool smooth = false)
        {
            Anchor = anchor;
            Handle1 = handle1;
            Handle2 = handle2;
            Smooth = smooth;
        }

        public static implicit operator Point(BezierPoint bezier)
        {
            return bezier.Anchor;
        }

        public static implicit operator BezierPoint(Point point)
        {
            return new BezierPoint(point);
        }
    }

    public class ControlPointList : List<Point>
    {
    }
}
