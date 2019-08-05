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

namespace PaintDotNet
{
    /// <summary>
    /// This class is for manipulation of transfer functions.
    /// It is intended for curve adjustment
    /// </summary>
    public abstract class CurveControl 
        : UserControl
    {
        private System.ComponentModel.Container components = null;

        private readonly int[] curvesInvalidRange = new int[] { int.MaxValue, int.MinValue };
        private Point LastMouseXY { get; set; } = new Point(int.MinValue, int.MinValue);
        private int LastKey { get; set; } = -1;
        private int LastValue { get; set; } = -1;
        private bool Tracking { get; set; } = false;
        private Point[] PtSave { get; set; }
        private int[] PointsNearMousePerChannel { get; set; }
        private bool[] EffectChannel { get; set; }

        protected SortedList<int, int>[] controlPoints;
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public SortedList<int, int>[] ControlPoints
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

        protected internal CurveControl(int channels, int entries)
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
            SortedList<int, int>[] ctrlPoints = new SortedList<int, int>[Channels];

            for (int i = 0; i < Channels; ++i)
            {
                SortedList<int, int> newList = new SortedList<int, int>
                {
                    { 0, 0 },
                    { Entries - 1, Entries - 1 }
                };
                ctrlPoints[i] = newList;
            }

            ControlPoints = ctrlPoints;
            OnValueChanged();
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
                SortedList<int, int> channelControlPoints = ControlPoints[c];
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

                SplineInterpolator interpolator = new SplineInterpolator();
                IList<int> xa = channelControlPoints.Keys;
                IList<int> ya = channelControlPoints.Values;
                PointF[] line = new PointF[Entries];

                for (int i = 0; i < points; ++i)
                {
                    interpolator.Add(xa[i], ya[i]);
                }
                
                for (int i = 0; i < line.Length; ++i)
                {
                    line[i].X = (float)i * (width - 1) / (Entries - 1);
                    line[i].Y = (float)(Utility.Clamp(Entries - 1 - interpolator.Interpolate(i), 0, Entries - 1)) * 
                        (height - 1) / (Entries - 1);
                }

                pen.LineJoin = LineJoin.Round;
                g.DrawLines(pen, line);

                for (int i = 0; i < points; ++i)
                {
                    int k = channelControlPoints.Keys[i];
                    float x = k * (width - 1) / (Entries - 1);
                    float y = (Entries - 1 - channelControlPoints.Values[i]) * (height - 1) / (Entries - 1);

                    const float radiusSelected = 4;
                    const float radiusNotSelected = 3;
                    const float radiusUnMasked = 2;

                    bool selected = (Mask[c] && PointsNearMousePerChannel[c] == i);
                    float size = selected ? radiusSelected : (Mask[c] ? radiusNotSelected : radiusUnMasked);
                    RectangleF rect = Utility.RectangleFromCenter(new PointF(x, y), size);

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

        /* This is not used now, but may be used later
        /// <summary>
        /// Reduces the number of control points by at least given factor.
        /// </summary>
        /// <param name="factor"></param>
        public void Simplify(float factor)
        {
            for (int c = 0; c < channels; ++c)
            {
                SortedList<int, int> channelControlPoints = controlPoints[c];
                int targetPoints = (int)Math.Ceiling(channelControlPoints.Count / factor);

                float minPointWorth = float.MaxValue;

                //remove points until the target point count is reached, but always remove unnecessary
                while (channelControlPoints.Count > 2)
                {
                    minPointWorth = float.MaxValue;
                    int minPointWorthIndex = -1;

                    for (int i = 1; i < channelControlPoints.Count - 1; ++i)
                    {
                        Point left = new Point(
                            channelControlPoints.Keys[i - 1],
                            channelControlPoints.Values[i - 1]);
                        Point right = new Point(
                            channelControlPoints.Keys[i + 1],
                            channelControlPoints.Values[i + 1]);
                        Point actual = new Point(
                            channelControlPoints.Keys[i],
                            channelControlPoints.Values[i]);

                        float targetY = left.Y + (actual.X - left.X) * (right.Y - left.Y) / (float)(right.X - left.X);
                        float error = targetY - actual.Y;
                        float pointWorth = error * error * (right.X - left.X);

                        if (pointWorth < minPointWorth)
                        {
                            minPointWorth = pointWorth;
                            minPointWorthIndex = i;
                        }
                    }


                    if (channelControlPoints.Count > targetPoints || minPointWorth == 0)
                    {
                        //if we found a point and it's not the first point
                        if (minPointWorthIndex > 0)
                        {
                            channelControlPoints.RemoveAt(minPointWorthIndex);
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }

            Invalidate();
            OnValueChanged();
        }
        */

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            float width = this.ClientRectangle.Width;
            float height = this.ClientRectangle.Height;
            int mx = (int)Utility.Clamp(0.5f + e.X * (Entries - 1) / (width - 1), 0, Entries - 1);
            int my = (int)Utility.Clamp(0.5f + Entries - 1 - e.Y * (Entries - 1) / (height - 1), 0, Entries - 1);

            PtSave = new Point[Channels];
            for (int i = 0; i < Channels; ++i)
            {
                PtSave[i].X = -1;
            }

            if (0 != e.Button)
            {
                Tracking = (e.Button == MouseButtons.Left);
                LastKey = mx;

                bool anyNearMouse = false;

                EffectChannel = new bool[Channels];
                for (int c = 0; c < Channels; ++c)
                {
                    SortedList<int, int> channelControlPoints = ControlPoints[c];
                    int index = PointsNearMousePerChannel[c];
                    bool hasPoint = (index >= 0);
                    int key = hasPoint ? channelControlPoints.Keys[index] : index;

                    anyNearMouse = (anyNearMouse || hasPoint);

                    EffectChannel[c] = hasPoint;

                    if (Mask[c] && hasPoint && 
                        key > 0 && key < Entries - 1)
                    {
                        channelControlPoints.RemoveAt(index);
                        OnValueChanged();
                    }
                }

                if (!anyNearMouse)
                {
                    for (int c = 0; c < Channels; ++c)
                    {
                        EffectChannel[c] = true;
                    }
                }
            }

            OnMouseMove(e);
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
                    SortedList<int, int> channelControlPoints = ControlPoints[c];

                    PointsNearMousePerChannel[c] = -1;
                    if (Mask[c] && EffectChannel[c])
                    {
                        int lastIndex = channelControlPoints.IndexOfKey(LastKey);

                        if (PtSave[c].X >= 0 && PtSave[c].X != mx)
                        {
                            channelControlPoints[PtSave[c].X] = PtSave[c].Y;
                            PtSave[c].X = -1;

                            changed = true;
                        }
                        else if (LastKey > 0 && LastKey < Entries - 1 && lastIndex >= 0 && mx != LastKey)
                        {
                            channelControlPoints.RemoveAt(lastIndex);
                        }

                        if (mx >= 0 && mx < Entries)
                        {
                            int newValue = Utility.Clamp(my, 0, Entries - 1);
                            int oldIndex = channelControlPoints.IndexOfKey(mx);
                            int oldValue = (oldIndex >= 0) ? channelControlPoints.Values[oldIndex] : -1;

                            if (oldIndex >= 0 && mx != LastKey) 
                            {
                                // if we drag onto an existing point, delete it, but save it in case we drag away
                                PtSave[c].X = mx;
                                PtSave[c].Y = channelControlPoints.Values[oldIndex];
                            }

                            if (oldIndex < 0 ||
                                channelControlPoints[mx] != newValue)
                            {
                                channelControlPoints[mx] = newValue;
                                changed = true;
                            }

                            PointsNearMousePerChannel[c] = channelControlPoints.IndexOfKey(mx);
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
                    SortedList<int, int> channelControlPoints = ControlPoints[c];
                    int minRadiusSq = 30;
                    int bestIndex = -1;

                    if (Mask[c])
                    {
                        for (int i = 0; i < channelControlPoints.Count; ++i)
                        {
                            int sumsq = 0;
                            int diff = 0;

                            diff = channelControlPoints.Keys[i] - mx;
                            sumsq += diff * diff;

                            diff = channelControlPoints.Values[i] - my;
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
}
