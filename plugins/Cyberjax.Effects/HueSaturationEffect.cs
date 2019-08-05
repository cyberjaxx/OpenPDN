/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet;
using PaintDotNet.Effects;
using PaintDotNet.PropertySystem;
using System;
using System.Drawing;

namespace Cyberjax
{
    [EffectCategory(EffectCategory.Effect)]
    public sealed class HueSaturationEffect
        : PropertyBasedEffect
    {
        protected override PropertyCollection OnCreatePropertyCollection()
        {
            return PropertyCollection.CreateEmpty();
        }

        public HueSaturationEffect()
            : base(Properties.Resources.HueSaturationEffectName,
                   Properties.Resources.HueSaturation,
                   "Cyberjax",
                   EffectFlags.None)
        {
        }

        protected unsafe override void OnRender(Rectangle[] rois, int startIndex, int length)
        {
            Surface dst = DstArgs.Surface;
            int maxX = dst.Width - 1;
            int maxY = dst.Height - 1;

            for (int r = startIndex; r < startIndex + length; ++r)
            {
                Rectangle rect = rois[r];

                for (int y = rect.Top; y < rect.Bottom; ++y)
                {
                    ColorBgra* dstPtr = dst.GetPointAddressUnchecked(rect.Left, y);

                    float saturation = (float)y / maxY;

                    for (int x = rect.Left; x < rect.Right; ++x)
                    {
                        float hue = x * 360.0f / maxX;

                        *dstPtr++ = ColorBgraExt.FromAhsl(1.0f, hue, 1.0f - saturation, 0.5f);
                    }
                }
            }
        }
    }
}
