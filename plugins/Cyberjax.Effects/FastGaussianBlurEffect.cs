/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using Cyberjax.Geometry;
using PaintDotNet;
using PaintDotNet.Effects;
using PaintDotNet.IndirectUI;
using PaintDotNet.PropertySystem;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Cyberjax
{
    [EffectCategory(EffectCategory.Adjustment)]
    public sealed class FastGaussianBlurEffect
        : PropertyBasedEffect
    {
        public enum PropertyNames
        {
            Radius = 0
        }

        public static string StaticName
        {
            get =>Properties.Resources.FastGaussianBlurEffectName;
        }

        public static Image StaticImage
        {
            get => Properties.Resources.HueSaturation;
        }


        public FastGaussianBlurEffect()
            : base(StaticName,
                   StaticImage,
                   null,
                   EffectFlags.Configurable | EffectFlags.SingleThreaded)
        {
        }

        protected override void OnCustomizeConfigUIWindowProperties(PropertyCollection props)
        {
            props[ControlInfoPropertyNames.WindowIsSizable].Value = false;
            base.OnCustomizeConfigUIWindowProperties(props);
        }

        protected override PropertyCollection OnCreatePropertyCollection()
        {
            List<Property> props = new List<Property>
            {
                new Int32Property(PropertyNames.Radius, 2, 0, 200)
            };

            return new PropertyCollection(props);
        }

        protected override ControlInfo OnCreateConfigUI(PropertyCollection props)
        {
            ControlInfo configUI = CreateDefaultConfigUI(props);

            // TODO: add units text property to slider?
            configUI.SetPropertyControlValue(PropertyNames.Radius, ControlInfoPropertyNames.DisplayName, "Radius");
            //aecg.SliderUnitsName = PdnResources.GetString("pixels");

            return configUI;
        }

        private int Radius { get; set; }

        protected override void OnSetRenderInfo(PropertyBasedEffectConfigToken newToken, RenderArgs dstArgs, RenderArgs srcArgs)
        {
            Radius = newToken.GetProperty<Int32Property>(PropertyNames.Radius).Value;
            base.OnSetRenderInfo(newToken, dstArgs, srcArgs);
        }

        protected unsafe override void OnRender(Rectangle[] rois, int startIndex, int length)
        {
            if (Radius == 0)
            {
                for (int ri = startIndex; ri < startIndex + length; ++ri)
                {
                    DstArgs.Surface.CopySurface(SrcArgs.Surface, rois[ri].Location, rois[ri]);
                }
                return;
            }

            for (int r = startIndex; r < startIndex + length; ++r)
            {
                Rectangle rect = rois[r];

                if (rect.Height >= 1 && rect.Width >= 1)
                {
                    Surface srcSurface = SrcArgs.Surface.Clone();
                    //GaussBlur1(SrcArgs.Surface, DstArgs.Surface, rect, Radius);
                    GaussBlur2(srcSurface, DstArgs.Surface, rect, Radius);
                    //GaussBlur3(srcSurface, DstArgs.Surface, rect, Radius);
                }
            }
        }

        private int[] BoxesForGauss(double sigma, int n)  // standard deviation, number of boxes
        {
            double wIdeal = (float)Math.Sqrt((12 * sigma * sigma / n) + 1);  // Ideal averaging filter width 
            int wl = (int)Math.Floor(wIdeal);
            if (wl % 2 == 0)
            {
                wl--;
            }
            int wu = wl + 2;

            double mIdeal = (12 * sigma * sigma - n * wl * wl - 4 * n * wl - 3 * n) / (-4 * wl - 4);
            int m = (int)Math.Round(mIdeal);
            // var sigmaActual = Math.sqrt( (m*wl*wl + (n-m)*wu*wu - n)/12 );

            int[] sizes = new int[n];
            for (int i = 0; i < n; i++)
            {
                sizes[i] = (i < m ? wl : wu);
            }
            return sizes;
        }

        private unsafe void GaussBlur1(Surface source, Surface dest, Rectangle rect, int radius)
        {
            int rs = (int)Math.Ceiling(radius * 2.57);     // significant radius
            float rsqr2 = 2 * radius * radius;
            float rsqr2PI = (float)(rsqr2 * Math.PI);

            for (int i = 0; i < rect.Height; i++)
            {
                ColorBgra* destPtr = dest.GetPointAddressUnchecked(rect.Left, rect.Top + i);
                for (int j = 0; j < rect.Width; j++)
                {
                    float sumR = 0;
                    float sumG = 0;
                    float sumB = 0;
                    float sumWght = 0;

                    for (int iy = i - rs; iy < i + rs + 1; iy++)
                    {
                        int y = Math.Min(source.Height - 1, Math.Max(0, rect.Top + iy));
                        for (int ix = j - rs; ix < j + rs + 1; ix++)
                        {
                            float dsq = (ix - j) * (ix - j) + (iy - i) * (iy - i);
                            float wght = (float)(Math.Exp(-dsq / rsqr2) / rsqr2PI);
                            int x = Math.Min(source.Width - 1, Math.Max(0, rect.Left + ix));
                            ColorBgra* srcPtr = source.GetPointAddressUnchecked(x, y);
                            sumR += srcPtr->R * wght;
                            sumG += srcPtr->G * wght;
                            sumB += srcPtr->B * wght;
                            sumWght += wght;
                        }
                    }
                    float sumWghtDiv2 = sumWght / 2;
                    byte red = Utility.ClampToByte((sumR + sumWghtDiv2) / sumWght);
                    byte green = Utility.ClampToByte((sumG + sumWghtDiv2) / sumWght);
                    byte blue = Utility.ClampToByte((sumB + sumWghtDiv2) / sumWght);
                    *destPtr++ = ColorBgra.FromBgr(blue, green, red);
                }
            }
        }

        private void GaussBlur2(Surface source, Surface dest, Rectangle rect, int radius)
        {
            int[] boxes = BoxesForGauss(radius, 3);
            BoxBlur2(source, dest, rect, (boxes[0] - 1) / 2);
            BoxBlur2(dest, source, rect, (boxes[1] - 1) / 2);
            BoxBlur2(source, dest, rect, (boxes[2] - 1) / 2);
        }

        private unsafe void BoxBlur2(Surface source, Surface dest, Rectangle rect, int radius)
        {
            float denominator = (radius + radius + 1) * (radius + radius + 1);

            for (int i = 0; i < rect.Height; i++)
            {
                ColorBgra* destPtr = dest.GetPointAddressUnchecked(rect.Left, rect.Top + i);
                for (int j = 0; j < rect.Width; j++)
                {
                    int sumR = 0;
                    int sumG = 0;
                    int sumB = 0;
                    for (int iy = i - radius; iy < i + radius + 1; iy++)
                    {
                        int y = Math.Min(source.Height - 1, Math.Max(0, rect.Top+ iy));
                        for (int ix = j - radius; ix < j + radius + 1; ix++)
                        {
                            int x = Math.Min(source.Width - 1, Math.Max(0, rect.Left + ix));
                            ColorBgra* srcPtr = source.GetPointAddressUnchecked(x, y);
                            sumR += srcPtr->R;
                            sumG += srcPtr->G;
                            sumB += srcPtr->B;
                        }
                    }
                    byte red = Utility.ClampToByte(sumR / denominator);
                    byte green = Utility.ClampToByte(sumG / denominator);
                    byte blue = Utility.ClampToByte(sumB / denominator);
                    *destPtr++ = ColorBgra.FromBgr(blue, green, red);
                }
            }
        }

        private void GaussBlur3(Surface source, Surface dest, Rectangle rect, int radius)
        {
            int[] boxes = BoxesForGauss(radius, 3);
            BoxBlur3(source, dest, rect, (boxes[0] - 1) / 2);
            BoxBlur3(dest, source, rect, (boxes[1] - 1) / 2);
            BoxBlur3(source, dest, rect, (boxes[2] - 1) / 2);
        }

        private void BoxBlur3(Surface source, Surface dest, Rectangle rect, int radius)
        {
            dest.CopySurface(source);
            BoxBlurH3(dest, source, rect, radius);
            BoxBlurT3(source, dest, rect, radius);
        }

        private unsafe void BoxBlurH3(Surface source, Surface dest, Rectangle rect, int radius)
        {
            float denominator = (radius + radius + 1);

            for (int i = 0; i < rect.Height; i++)
            {
                ColorBgra* destPtr = dest.GetPointAddressUnchecked(rect.Left, rect.Top + i);
                for (int j = 0; j < rect.Width; j++)
                {
                    int sumR = 0;
                    int sumG = 0;
                    int sumB = 0;
                    for (int ix = j - radius; ix < j + radius + 1; ix++)
                    {
                        int x = Math.Min(source.Width - 1, Math.Max(0, rect.Left + ix));
                        ColorBgra* srcPtr = source.GetPointAddressUnchecked(x, rect.Top + i);
                        sumR += srcPtr->R;
                        sumG += srcPtr->G;
                        sumB += srcPtr->B;
                    }
                    byte red = Utility.ClampToByte(sumR / denominator);
                    byte green = Utility.ClampToByte(sumG / denominator);
                    byte blue = Utility.ClampToByte(sumB / denominator);
                    *destPtr++ = ColorBgra.FromBgr(blue, green, red);
                }
            }
        }

        private unsafe void BoxBlurT3(Surface source, Surface dest, Rectangle rect, int radius)
        {
            float denominator = (radius + radius + 1);

            for (int i = 0; i < rect.Height; i++)
            {
                ColorBgra* destPtr = dest.GetPointAddressUnchecked(rect.Left, rect.Top + i);
                for (int j = 0; j < rect.Width; j++)
                {
                    int sumR = 0;
                    int sumG = 0;
                    int sumB = 0;
                    for (var iy = i - radius; iy < i + radius + 1; iy++)
                    {
                        int y = Math.Min(source.Height - 1, Math.Max(0, rect.Top + iy));
                        ColorBgra* srcPtr = source.GetPointAddressUnchecked(rect.Left + j, y);
                        sumR += srcPtr->R;
                        sumG += srcPtr->G;
                        sumB += srcPtr->B;
                    }
                    byte red = Utility.ClampToByte(sumR / denominator);
                    byte green = Utility.ClampToByte(sumG / denominator);
                    byte blue = Utility.ClampToByte(sumB / denominator);
                    *destPtr++ = ColorBgra.FromBgr(blue, green, red);
                }
            }
        }
    }
}