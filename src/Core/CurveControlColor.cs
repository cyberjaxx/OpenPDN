/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

namespace PaintDotNet
{
    /// <summary>
    /// Curve control for color transfer curves
    /// </summary>
    public abstract class CurveControlColor
        : CurveControl
    {
        public CurveControlColor(int channels, int entries)
            : base(channels, entries)
        {
        }

        public abstract ColorTransferMode ColorTransferMode
        {
            get;
        }
    }
}
