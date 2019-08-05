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
    public abstract class GradientRenderer
    {
        private BinaryPixelOp normalBlendOp;
        private ColorBgra startColor;
        private ColorBgra endColor;
        private bool lerpCacheIsValid = false;
        private byte[] lerpAlphas;
        private ColorBgra[] lerpColors;
        
        public ColorBgra StartColor
        {
            get
            {
                return this.startColor;
            }

            set
            {
                if (this.startColor != value)
                {
                    this.startColor = value;
                    this.lerpCacheIsValid = false;
                }
            }
        }

        public ColorBgra EndColor
        {
            get
            {
                return this.endColor;
            }

            set
            {
                if (this.endColor != value)
                {
                    this.endColor = value;
                    this.lerpCacheIsValid = false;
                }
            }
        }

        public PointF StartPoint { get; set; }

        public PointF EndPoint { get; set; }

        public bool AlphaBlending { get; set; }

        public bool AlphaOnly { get; set; }

        public virtual void BeforeRender()
        {
            if (!this.lerpCacheIsValid)
            {
                byte startAlpha;
                byte endAlpha;

                if (this.AlphaOnly)
                {
                    ComputeAlphaOnlyValuesFromColors(this.startColor, this.endColor, out startAlpha, out endAlpha);
                }
                else
                {
                    startAlpha = this.startColor.A;
                    endAlpha = this.endColor.A;
                } 
                
                this.lerpAlphas = new byte[256];
                this.lerpColors = new ColorBgra[256];

                for (int i = 0; i < 256; ++i)
                {
                    byte a = (byte)i;
                    this.lerpColors[a] = ColorBgra.Blend(this.startColor, this.endColor, a);
                    this.lerpAlphas[a] = (byte)(startAlpha + ((endAlpha - startAlpha) * a) / 255);
                }

                this.lerpCacheIsValid = true;
            }
        }

        public abstract float ComputeUnboundedLerp(int x, int y);
        public abstract float BoundLerp(float t);

        public virtual void AfterRender()
        {
        }

        private static void ComputeAlphaOnlyValuesFromColors(ColorBgra startColor, ColorBgra endColor, out byte startAlpha, out byte endAlpha)
        {
            startAlpha = startColor.A;
            endAlpha = (byte)(255 - endColor.A);
        }

        public unsafe void Render(Surface surface, Rectangle[] rois, int startIndex, int length)
        {
            byte startAlpha;
            byte endAlpha;

            if (this.AlphaOnly)
            {
                ComputeAlphaOnlyValuesFromColors(this.startColor, this.endColor, out startAlpha, out endAlpha);
            }
            else
            {
                startAlpha = this.startColor.A;
                endAlpha = this.endColor.A;
            }

            for (int ri = startIndex; ri < startIndex + length; ++ri)
            {
                Rectangle rect = rois[ri];

                if (this.StartPoint == this.EndPoint)
                {
                    // Start and End point are the same ... fill with solid color.
                    for (int y = rect.Top; y < rect.Bottom; ++y)
                    {
                        ColorBgra* pixelPtr = surface.GetPointAddress(rect.Left, y);

                        for (int x = rect.Left; x < rect.Right; ++x)
                        {
                            ColorBgra result;

                            if (this.AlphaOnly && this.AlphaBlending)
                            {
                                byte resultAlpha = (byte)Utility.FastDivideShortByByte((ushort)(pixelPtr->A * endAlpha), 255);
                                result = *pixelPtr;
                                result.A = resultAlpha;
                            }
                            else if (this.AlphaOnly && !this.AlphaBlending)
                            {
                                result = *pixelPtr;
                                result.A = endAlpha;
                            }
                            else if (!this.AlphaOnly && this.AlphaBlending)
                            {
                                result = this.normalBlendOp.Apply(*pixelPtr, this.endColor);
                            }
                            else //if (!this.alphaOnly && !this.alphaBlending)
                            {
                                result = this.endColor;
                            }

                            *pixelPtr = result;
                            ++pixelPtr;
                        }
                    }
                }
                else
                {
                    for (int y = rect.Top; y < rect.Bottom; ++y)
                    {
                        ColorBgra* pixelPtr = surface.GetPointAddress(rect.Left, y);

                        if (this.AlphaOnly && this.AlphaBlending)
                        {
                            for (int x = rect.Left; x < rect.Right; ++x)
                            {
                                float lerpUnbounded = ComputeUnboundedLerp(x, y);
                                float lerpBounded = BoundLerp(lerpUnbounded);
                                byte lerpByte = (byte)(lerpBounded * 255.0f);
                                byte lerpAlpha = this.lerpAlphas[lerpByte];
                                byte resultAlpha = Utility.FastScaleByteByByte(pixelPtr->A, lerpAlpha);
                                pixelPtr->A = resultAlpha;
                                ++pixelPtr;
                            }
                        }
                        else if (this.AlphaOnly && !this.AlphaBlending)
                        {
                            for (int x = rect.Left; x < rect.Right; ++x)
                            {
                                float lerpUnbounded = ComputeUnboundedLerp(x, y);
                                float lerpBounded = BoundLerp(lerpUnbounded);
                                byte lerpByte = (byte)(lerpBounded * 255.0f);
                                byte lerpAlpha = this.lerpAlphas[lerpByte];
                                pixelPtr->A = lerpAlpha;
                                ++pixelPtr;
                            }
                        }
                        else if (!this.AlphaOnly && (this.AlphaBlending && (startAlpha != 255 || endAlpha != 255)))
                        {
                            // If we're doing all color channels, and we're doing alpha blending, and if alpha blending is necessary
                            for (int x = rect.Left; x < rect.Right; ++x)
                            {
                                float lerpUnbounded = ComputeUnboundedLerp(x, y);
                                float lerpBounded = BoundLerp(lerpUnbounded);
                                byte lerpByte = (byte)(lerpBounded * 255.0f);
                                ColorBgra lerpColor = this.lerpColors[lerpByte];
                                ColorBgra result = this.normalBlendOp.Apply(*pixelPtr, lerpColor);
                                *pixelPtr = result;
                                ++pixelPtr;
                            }
                        }
                        else //if (!this.alphaOnly && !this.alphaBlending) // or sC.A == 255 && eC.A == 255
                        {
                            for (int x = rect.Left; x < rect.Right; ++x)
                            {
                                float lerpUnbounded = ComputeUnboundedLerp(x, y);
                                float lerpBounded = BoundLerp(lerpUnbounded);
                                byte lerpByte = (byte)(lerpBounded * 255.0f);
                                ColorBgra lerpColor = this.lerpColors[lerpByte];
                                *pixelPtr = lerpColor;
                                ++pixelPtr;
                            }
                        }
                    }
                }
            }

            AfterRender();
        }

        protected internal GradientRenderer(bool alphaOnly, BinaryPixelOp normalBlendOp)
        {
            this.normalBlendOp = normalBlendOp;
            this.AlphaOnly = alphaOnly;
        }
    }
}
