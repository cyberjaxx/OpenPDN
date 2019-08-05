/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.Serialization;

namespace PaintDotNet
{
    /// <summary>
    /// Carries information about the subset of Pen configuration details that we support.
    /// Does not carry color information.
    /// </summary>
    [Serializable]
    internal sealed class PenInfo
        : ICloneable,
          ISerializable
    {
        public const DashStyle DefaultDashStyle = DashStyle.Solid;
        public const LineCap2 DefaultLineCap = LineCap2.Flat;
        public const float DefaultCapScale = 1.0f;
        public const float MinCapScale = 1.0f;
        public const float MaxCapScale = 5.0f;
        public DashStyle DashStyle { get; set; }
        public float Width { get; set; }
        public LineCap2 StartCap { get; set; }
        public LineCap2 EndCap { get; set; }

        private float capScale;
        private float CapScale
        {
            get
            {
                return Utility.Clamp(this.capScale, MinCapScale, MaxCapScale);
            }

            set
            {
                this.capScale = value;
            }
        }

        public static bool operator==(PenInfo lhs, PenInfo rhs)
        {
            return (
                lhs.DashStyle == rhs.DashStyle && 
                lhs.Width == rhs.Width &&
                lhs.StartCap == rhs.StartCap &&
                lhs.EndCap == rhs.EndCap &&
                lhs.capScale == rhs.capScale);
        }

        public static bool operator!=(PenInfo lhs, PenInfo rhs)
        {
            return !(lhs == rhs);
        }

        public override bool Equals(object obj)
        {
            PenInfo rhs = obj as PenInfo;

            if (rhs == null)
            {
                return false;
            }

            return this == rhs;
        }

        public override int GetHashCode()
        {
            return 
                this.DashStyle.GetHashCode() ^ 
                this.Width.GetHashCode() ^ 
                this.StartCap.GetHashCode() ^ 
                this.EndCap.GetHashCode() ^
                this.capScale.GetHashCode();
        }

        private void LineCapToLineCap2(LineCap2 cap2, out LineCap capResult, out CustomLineCap customCapResult)
        {
            switch (cap2)
            {
                case LineCap2.Flat:
                    capResult = LineCap.Flat;
                    customCapResult = null;
                    break;

                case LineCap2.Arrow:
                    capResult = LineCap.ArrowAnchor;
                    customCapResult = new AdjustableArrowCap(5.0f * this.capScale, 5.0f * this.capScale, false);
                    break;

                case LineCap2.ArrowFilled:
                    capResult = LineCap.ArrowAnchor;
                    customCapResult = new AdjustableArrowCap(5.0f * this.capScale, 5.0f * this.capScale, true);
                    break;

                case LineCap2.Rounded:
                    capResult = LineCap.Round;
                    customCapResult = null;
                    break;

                default:
                    throw new InvalidEnumArgumentException();
            }
        }

        public Pen CreatePen(BrushInfo brushInfo, Color foreColor, Color backColor)
        {
            Pen pen;

            if (brushInfo.BrushType == BrushType.None)
            {
                pen = new Pen(foreColor, Width);
            }
            else
            {
                pen = new Pen(brushInfo.CreateBrush(foreColor, backColor), Width);
            }

            LineCapToLineCap2(this.StartCap, out LineCap startLineCap, out CustomLineCap startCustomLineCap);

            if (startCustomLineCap != null)
            {
                pen.CustomStartCap = startCustomLineCap;
            }
            else
            {
                pen.StartCap = startLineCap;
            }

            LineCapToLineCap2(this.EndCap, out LineCap endLineCap, out CustomLineCap endCustomLineCap);

            if (endCustomLineCap != null)
            {
                pen.CustomEndCap = endCustomLineCap;
            }
            else
            {
                pen.EndCap = endLineCap;
            }

            pen.DashStyle = this.DashStyle;

            return pen;
        }

        public PenInfo(DashStyle dashStyle, float width, LineCap2 startCap, LineCap2 endCap, float capScale)
        {
            this.DashStyle = dashStyle;
            this.Width = width;
            this.capScale = capScale;
            this.StartCap = startCap;
            this.EndCap = endCap;
        }

        private PenInfo(SerializationInfo info, StreamingContext context)
        {
            this.DashStyle = (DashStyle)info.GetValue("dashStyle", typeof(DashStyle));
            this.Width = info.GetSingle("width");

            // Save the caps as integers because we want to change the "LineCap2" name.
            // Just not feeling very creative right now I guess.
            try
            {
                this.StartCap = (LineCap2)info.GetInt32("startCap");
            }

            catch (SerializationException)
            {
                this.StartCap = DefaultLineCap;
            }

            try
            {
                this.EndCap = (LineCap2)info.GetInt32("endCap");
            }

            catch (SerializationException)
            {
                this.EndCap = DefaultLineCap;
            }

            try
            {
                float loadedCapScale = info.GetSingle("capScale");
                this.capScale = Utility.Clamp(loadedCapScale, MinCapScale, MaxCapScale);
            }

            catch (SerializationException)
            {
                this.capScale = DefaultCapScale;
            }
        }

        public PenInfo Clone()
        {
            return new PenInfo(this.DashStyle, this.Width, this.StartCap, this.EndCap, this.capScale);
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("dashStyle", this.DashStyle);
            info.AddValue("width", this.Width);
            info.AddValue("startCap", (int)this.StartCap);
            info.AddValue("endCap", (int)this.EndCap);
            info.AddValue("capScale", this.capScale);
        }
    }
}
