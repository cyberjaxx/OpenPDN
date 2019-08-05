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
using System.Runtime.Serialization;

namespace PaintDotNet
{
    [Serializable]
    internal sealed class SelectionDrawModeInfo
        : ICloneable<SelectionDrawModeInfo>,
          IDeserializationCallback
    {
        public SelectionDrawMode DrawMode { get; }

        public double Width { get; }

        public double Height { get; }

        public MeasurementUnit Units { get; private set; }

        public override bool Equals(object obj)
        {
            if (!(obj is SelectionDrawModeInfo asSDMI))
            {
                return false;
            }

            return (asSDMI.DrawMode == DrawMode) && (asSDMI.Width == Width) && (asSDMI.Height == Height) && (asSDMI.Units == Units);
        }

        public override int GetHashCode()
        {
            return unchecked(DrawMode.GetHashCode() ^ Width.GetHashCode() ^ Height.GetHashCode() & Units.GetHashCode());
        }

        public SelectionDrawModeInfo(SelectionDrawMode drawMode, double width, double height, MeasurementUnit units)
        {
            DrawMode = drawMode;
            Width = width;
            Height = height;
            Units = units;
        }

        public static SelectionDrawModeInfo CreateDefault()
        {
            return new SelectionDrawModeInfo(SelectionDrawMode.Normal, 4.0, 3.0, MeasurementUnit.Inch);
        }

        public SelectionDrawModeInfo CloneWithNewDrawMode(SelectionDrawMode newDrawMode)
        {
            return new SelectionDrawModeInfo(newDrawMode, Width, Height, Units);
        }

        public SelectionDrawModeInfo CloneWithNewWidth(double newWidth)
        {
            return new SelectionDrawModeInfo(DrawMode, newWidth, Height, Units);
        }

        public SelectionDrawModeInfo CloneWithNewHeight(double newHeight)
        {
            return new SelectionDrawModeInfo(DrawMode, Width, newHeight, Units);
        }

        public SelectionDrawModeInfo CloneWithNewWidthAndHeight(double newWidth, double newHeight)
        {
            return new SelectionDrawModeInfo(DrawMode, newWidth, newHeight, Units);
        }

        public SelectionDrawModeInfo Clone()
        {
            return new SelectionDrawModeInfo(DrawMode, Width, Height, Units);
        }

        public SelectionDrawModeInfo CloneWithNewUnits(MeasurementUnit newUnits)
        {
            return new SelectionDrawModeInfo(DrawMode, Width, Height, newUnits);
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        void IDeserializationCallback.OnDeserialization(object sender)
        {
            switch (Units)
            {
                case MeasurementUnit.Centimeter:
                case MeasurementUnit.Inch:
                case MeasurementUnit.Pixel:
                    break;

                default:
                    Units = MeasurementUnit.Pixel;
                    break;
            }
        }
    }
}
