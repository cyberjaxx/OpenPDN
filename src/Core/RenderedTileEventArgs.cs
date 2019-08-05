/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Drawing;

namespace PaintDotNet
{
    public sealed class RenderedTileEventArgs
        : EventArgs
    {
        public PdnRegion RenderedRegion { get; }
        public int TileNumber { get; }
        public int TileCount { get; }

        public RenderedTileEventArgs(PdnRegion renderedRegion, int tileCount, int tileNumber)
        {
            this.RenderedRegion = renderedRegion;
            this.TileCount = tileCount;
            this.TileNumber = tileNumber;
        }
    }
}
