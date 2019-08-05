/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using CJ.Geometry;
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
    public sealed class FastGaussianBlurEffectOld
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


        public FastGaussianBlurEffectOld()
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

            configUI.SetPropertyControlValue(PropertyNames.Radius, ControlInfoPropertyNames.DisplayName, "Radius");

            return configUI;
        }

        private int Radius { get; set; }
        private Surface DestSurface { get; set; } = null;

        protected override void OnSetRenderInfo(PropertyBasedEffectConfigToken newToken, RenderArgs dstArgs, RenderArgs srcArgs)
        {
            Radius = newToken.GetProperty<Int32Property>(PropertyNames.Radius).Value;
            base.OnSetRenderInfo(newToken, dstArgs, srcArgs);

            // at this point we will perform the filter on the src and
            // store the result in DestSurface to be referenced in
            // subsequent calls to OnRender

            DestSurface = srcArgs.Surface.Clone();
            if (Radius != 0)
            {
                // radius is limited to the lesser of the width or the height
                int radius = Math.Min(Radius, Math.Min(SrcArgs.Bounds.Width - 1, SrcArgs.Bounds.Height - 1));
                //GaussBlur1(SrcArgs.Surface, DestSurface, SrcArgs.Bounds, radius);
                //GaussBlur2(SrcArgs.Surface, DestSurface, SrcArgs.Bounds, radius);
                //GaussBlur3(SrcArgs.Surface, DestSurface, SrcArgs.Bounds, radius);
                GaussBlur4(SrcArgs.Surface, DestSurface, SrcArgs.Bounds, radius);
            }
        }

        protected unsafe override void OnRender(Rectangle[] rois, int startIndex, int length)
        {
            for (int ri = startIndex; ri < startIndex + length; ++ri)
            {
                DstArgs.Surface.CopySurface(DestSurface, rois[ri].Location, rois[ri]);
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
                    *destPtr++ = ColorBgraExt.FromBgrSumScaled(sumB, sumG, sumR, 1 / sumWght);
                }
            }
        }

        private void GaussBlur2(Surface source, Surface dest, Rectangle rect, int radius)
        {
            using (Surface scratch = SrcArgs.Surface.Clone())
            {
                int[] boxes = BoxesForGauss(radius, 3);
                BoxBlur2(scratch, dest, rect, (boxes[0] - 1) / 2);
                BoxBlur2(dest, scratch, rect, (boxes[1] - 1) / 2);
                BoxBlur2(scratch, dest, rect, (boxes[2] - 1) / 2);
            }
        }

        private unsafe void BoxBlur2(Surface source, Surface dest, Rectangle rect, int radius)
        {
            float iarr2 = 1f / ((radius + radius + 1) * (radius + radius + 1));

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
                    *destPtr++ = ColorBgraExt.FromBgrSumScaled(sumB, sumG, sumR, iarr2);
                }
            }
        }

        private void GaussBlur3(Surface source, Surface dest, Rectangle rect, int radius)
        {
            using (Surface scratch = source.Clone())
            {
                int[] boxes = BoxesForGauss(radius, 3);
                BoxBlur3(scratch, dest, rect, (boxes[0] - 1) / 2);
                BoxBlur3(dest, scratch, rect, (boxes[1] - 1) / 2);
                BoxBlur3(scratch, dest, rect, (boxes[2] - 1) / 2);
            }
        }

        private void BoxBlur3(Surface source, Surface dest, Rectangle rect, int radius)
        {
            dest.CopySurface(source);
            BoxBlurH3(dest, source, rect, radius);
            BoxBlurT3(source, dest, rect, radius);
        }

        private unsafe void BoxBlurH3(Surface source, Surface dest, Rectangle rect, int radius)
        {
            float iarr = 1f / (radius + radius + 1);

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
                    *destPtr++ = ColorBgraExt.FromBgrSumScaled(sumB, sumG, sumR, iarr);
                }
            }
        }

        private unsafe void BoxBlurT3(Surface source, Surface dest, Rectangle rect, int radius)
        {
            float iarr = 1f / (radius + radius + 1);

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
                    *destPtr++ = ColorBgraExt.FromBgrSumScaled(sumB, sumG, sumR, iarr);
                }
            }
        }


        private void GaussBlur4(Surface source, Surface dest, Rectangle rect, int radius)
        {
            using (Surface scratch = source.Clone())
            {
                int[] boxes = BoxesForGauss(radius, 4);
                BoxBlur4(scratch, dest, rect, (boxes[0] - 1) / 2);
                BoxBlur4(dest, scratch, rect, (boxes[1] - 1) / 2);
                BoxBlur4(scratch, dest, rect, (boxes[2] - 1) / 2);
            }
        }

        private void BoxBlur4(Surface source, Surface dest, Rectangle rect, int radius)
        {
            dest.CopySurface(source);
            BoxBlurH4(dest, source, rect, radius);
            BoxBlurT4(source, dest, rect, radius);
        }

        private unsafe void BoxBlurH4(Surface source, Surface dest, Rectangle rect, int radius)
        {
            int w = rect.Width;
            int h = rect.Height;

            float iarr = 1f / (radius + radius + 1);
            for (int i = 0; i < h; i++)
            {
                // ti = i * w + 0   => (0, i)
                // li = ti          => (0, i)
                // ri = ti + radius => (radius, i)

                ColorBgra* destPtr = dest.GetPointAddressUnchecked(0, i);
                ColorBgra* srcPtr = source.GetPointAddressUnchecked(0, i);

                // fv = scl[ti] => source(0, i)
                ColorBgra* fPtr = srcPtr;
                int fR = fPtr->R;
                int fG = fPtr->G;
                int fB = fPtr->B;

                // lv = scl[ti + w - 1] => source(w - 1, i)
                ColorBgra* lvPtr = srcPtr + w - 1;
                int lR = lvPtr->R;
                int lG = lvPtr->G;
                int lB = lvPtr->B;

                // val = (radius + 1) * fv => (radius + 1) * source(0, i)
                int sumR = (radius + 1) * fR;
                int sumG = (radius + 1) * fG;
                int sumB = (radius + 1) * fB;

                ColorBgra* jPtr = srcPtr;
                for (int j = 0; j < radius; j++)
                {
                    // val += scl[ti + j] => source(j, i)
                    sumR += jPtr->R;
                    sumG += jPtr->G;
                    sumB += jPtr->B;
                    jPtr++;
                }

                ColorBgra* rPtr = srcPtr + radius;
                for (int j = 0; j <= radius; j++)
                {
                    // val += scl[ri++] - fv; => source(ri++, i) - source(0, i)
                    sumR += rPtr->R - fR;
                    sumG += rPtr->G - fG;
                    sumB += rPtr->B - fB;
                    rPtr++;

                    // tcl[ti++] = Math.Round(val * iarr)
                    *destPtr++ = ColorBgraExt.FromBgrSumScaled(sumB, sumG, sumR, iarr);
                }

                ColorBgra* lPtr = srcPtr;
                for (int j = radius + 1; j < w - radius; j++)
                {
                    // val += scl[ri++] - scl[li++] => source(ri++, i) - source(li++, i)
                    sumR += rPtr->R - lPtr->R;
                    sumG += rPtr->G - lPtr->G;
                    sumB += rPtr->B - lPtr->B;
                    rPtr++;
                    lPtr++;

                    // tcl[ti++] = Math.Round(val * iarr)
                    *destPtr++ = ColorBgraExt.FromBgrSumScaled(sumB, sumG, sumR, iarr);
                }


                for (int j = w - radius; j < w; j++)
                {
                    // val += lv - scl[li++] => source(w - 1, i) - source(li++, i)
                    sumR += lR - lPtr->R;
                    sumG += lG - lPtr->G;
                    sumB += lB - lPtr->B;
                    lPtr++;

                    // tcl[ti++] = Math.Round(val * iarr)
                    *destPtr++ = ColorBgraExt.FromBgrSumScaled(sumB, sumG, sumR, iarr);
                }
            }
        }

        private unsafe void BoxBlurT4(Surface source, Surface dest, Rectangle rect, int radius)
        {
            int w = rect.Width;
            int h = rect.Height;

            float iarr = 1f / (radius + radius + 1);
            for (int i = 0; i < w; i++)
            {
                // ti = i               => (i, 0)
                // li = ti              => (i, 0)
                // ri = ti + radius * w => (i, radius)

                int ti = 0;
                int li = 0;
                int ri = radius;

                // fv = scl[ti] => source(i, 0)
                ColorBgra* fPtr = source.GetPointAddressUnchecked(i, 0);
                int fR = fPtr->R;
                int fG = fPtr->G;
                int fB = fPtr->B;

                // lv = scl[ti + w * (h - 1)] => source(i, h - 1)
                ColorBgra* lptr = source.GetPointAddressUnchecked(i, h - 1);
                int lR = lptr->R;
                int lG = lptr->G;
                int lB = lptr->B;

                // val = (radius + 1) * fv => (radius + 1) * source(i, 0)
                int sumR = (radius + 1) * fR;
                int sumG = (radius + 1) * fG;
                int sumB = (radius + 1) * fB;

                for (var j = 0; j < radius; j++)
                {
                    // val += scl[ti + j * w] => source(i, j)
                    ColorBgra* jPtr = source.GetPointAddressUnchecked(i, j);
                    sumR += jPtr->R;
                    sumG += jPtr->G;
                    sumB += jPtr->B;
                }

                for (var j = 0; j <= radius; j++)
                {
                    // val += scl[ri] - fv; => source(i, ri) - source(i, 0)
                    ColorBgra* rPtr = source.GetPointAddressUnchecked(i, ri++);
                    sumR += rPtr->R - fR;
                    sumG += rPtr->G - fG;
                    sumB += rPtr->B - fB;

                    // tcl[ti] = Math.Round(val * iarr);
                    ColorBgra* destPtr = dest.GetPointAddressUnchecked(i, ti++);
                    *destPtr = ColorBgraExt.FromBgrSumScaled(sumB, sumG, sumR, iarr);

                    // ri += w; ti += w;
                }

                for (var j = radius + 1; j < h - radius; j++)
                {
                    // val += scl[ri] - scl[li] => source(i, ri) - source(i, li)
                    ColorBgra* rPtr = source.GetPointAddressUnchecked(i, ri++);
                    ColorBgra* lPtr = source.GetPointAddressUnchecked(i, li++);
                    sumR += rPtr->R - lPtr->R;
                    sumG += rPtr->G - lPtr->G;
                    sumB += rPtr->B - lPtr->B;

                    // tcl[ti] = Math.Round(val * iarr);
                    ColorBgra* destPtr = dest.GetPointAddressUnchecked(i, ti++);
                    *destPtr = ColorBgraExt.FromBgrSumScaled(sumB, sumG, sumR, iarr);

                    // li += w; ri += w; ti += w;
                }

                for (var j = h - radius; j < h; j++)
                {
                    // val += lv - scl[li] => source(i, h - 1) - source(i, li)
                    ColorBgra* lPtr = source.GetPointAddressUnchecked(i, li++);
                    sumR += lR - lPtr->R;
                    sumG += lG - lPtr->G;
                    sumB += lB - lPtr->B;

                    // tcl[ti] = Math.Round(val * iarr);
                    ColorBgra* destPtr = dest.GetPointAddressUnchecked(i, ti++);
                    *destPtr = ColorBgraExt.FromBgrSumScaled(sumB, sumG, sumR, iarr);

                    // li += w; ti += w;
                }
            }
        }
    }
}