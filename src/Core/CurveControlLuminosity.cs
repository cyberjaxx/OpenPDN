/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace PaintDotNet
{
    /// <summary>
    /// Curve control specialized for luminosity
    /// </summary>
    public sealed class CurveControlLuminosity
        : CurveControlColor
    {
        public CurveControlLuminosity()
            : base(1, 256)
        {
            Mask = new bool[1] {true};
            VisualColors = new ColorBgra[]
            {     
                ColorBgra.Black
            };
            ChannelNames = new string[]
            {
                PdnResources.GetString("CurveControlLuminosity.Luminosity")
            };
            ResetControlPoints();
        }

        public override ColorTransferMode ColorTransferMode
        {
            get => ColorTransferMode.Luminosity;
        }
    }
}
