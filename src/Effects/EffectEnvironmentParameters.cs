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
using System.Drawing.Drawing2D;

namespace PaintDotNet.Effects
{
    public class EffectEnvironmentParameters
        : IDisposable
    {
        public static EffectEnvironmentParameters DefaultParameters
        {
            get
            {
                return new EffectEnvironmentParameters(ColorBgra.FromBgra(255, 255, 255, 255),
                                                       ColorBgra.FromBgra(0, 0, 0, 255),
                                                       2.0f,
                                                       new PdnRegion(),
                                                       null);
            }
        }

        private PdnRegion selection;
        private bool haveIntersectedSelection = false;

        public ColorBgra PrimaryColor { get; } = ColorBgra.FromBgra(0, 0, 0, 0);

        public ColorBgra SecondaryColor { get; } = ColorBgra.FromBgra(0, 0, 0, 0);

        public float BrushWidth { get; } = 0.0f;

        public Surface SourceSurface { get; }

        /// <summary>
        /// Gets the user's currently selected area.
        /// </summary>
        /// <param name="boundingRect">
        /// The bounding rectangle of the surface you will be rendering to. 
        /// The region returned will be clipped to this bounding rectangle.
        /// </param>
        /// <remarks>
        /// Note that calls to Render() will already be clipped to this selection area. 
        /// This data is only useful when an effect wants to change its rendering based
        /// on what the user has selected. For instance, This is used by Auto-Levels to
        /// only calculate new levels based on what the user has selected
        /// </remarks>
        public PdnRegion GetSelection(Rectangle boundingRect)
        {
            if (!this.haveIntersectedSelection)
            {
                this.selection.Intersect(boundingRect);
                this.haveIntersectedSelection = true;
            }

            return this.selection;
        }

        public EffectEnvironmentParameters(ColorBgra primaryColor, ColorBgra secondaryColor, float brushWidth, PdnRegion selection, Surface sourceSurface)
        {
            this.PrimaryColor = primaryColor;
            this.SecondaryColor = secondaryColor;
            this.BrushWidth = brushWidth;
            this.selection = (PdnRegion)selection.Clone();
            this.SourceSurface = sourceSurface;
        }

        ~EffectEnvironmentParameters()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.selection != null)
                {
                    this.selection.Dispose();
                    this.selection = null;
                }
            }
        }
    }
}
