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
    /// Curve control specialization for RGB curves
    /// </summary>
    public sealed class CurveControlRgb
        : CurveControlColor
    {
        public CurveControlRgb()
            : base(3, 256)
        {
            Mask = new bool[3] { true, true, true };
            VisualColors = new ColorBgra[]
            {     
                ColorBgra.Red,
                ColorBgra.Green,
                ColorBgra.Blue
            };
            ChannelNames = new string[]
            {
                PdnResources.GetString("CurveControlRgb.Red"),
                PdnResources.GetString("CurveControlRgb.Green"),
                PdnResources.GetString("CurveControlRgb.Blue")
            };
            ResetControlPoints();
        }

        public override ColorTransferMode ColorTransferMode
        {
            get => ColorTransferMode.Rgb;
        }
    }
}
