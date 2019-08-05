/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Specialized;
using System.Drawing;
using System.Globalization;
using System.Text;

namespace PaintDotNet
{
    public sealed class SnapDescription
    {
        public SnapObstacle SnappedTo { get; }

        public HorizontalSnapEdge HorizontalEdge { get; set; }

        public VerticalSnapEdge VerticalEdge { get; set; }

        public int XOffset { get; set; }

        public int YOffset { get; set; }

        public SnapDescription(
            SnapObstacle snappedTo,
            HorizontalSnapEdge horizontalEdge,
            VerticalSnapEdge verticalEdge,
            int xOffset,
            int yOffset)
        {
            if (snappedTo == null)
            {
                throw new ArgumentNullException("snappedTo");
            }

            this.SnappedTo = snappedTo;
            this.HorizontalEdge = horizontalEdge;
            this.VerticalEdge = verticalEdge;
            this.XOffset = xOffset;
            this.YOffset = yOffset;
        }
    }
}
