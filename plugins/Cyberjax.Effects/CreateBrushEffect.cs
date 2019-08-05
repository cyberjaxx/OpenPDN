using Cyberjax.Extensions;
using PaintDotNet;
using PaintDotNet.Effects;
using PaintDotNet.IndirectUI;
using PaintDotNet.PropertySystem;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Cyberjax
{
    public sealed class CreateBrushEffect
        : PropertyBasedEffect
    {
        public static string StaticName
        {
            get => Properties.Resources.BrushEffectName;
        }

        public static Image StaticImage
        {
            get => Properties.Resources.LineEffect;
        }

        public static string StaticSubMenuName
        {
            get => Properties.Resources.EffectSubMenuName;
        }

        public enum PropertyNames
        {
            Radius = 0,
            Hardness,
            AutoSigma,
            Sigma,
        }

        public ColorBgra BrushColor => EnvironmentParameters.PrimaryColor;

        public CreateBrushEffect()
            : base(StaticName, StaticImage, StaticSubMenuName,
                  EffectFlags.Configurable | EffectFlags.SingleThreaded | EffectFlags.SingleTile)
        {
        }

        protected override PropertyCollection OnCreatePropertyCollection()
        {
            List<Property> props = new List<Property>
            {
                new Int32Property(PropertyNames.Radius, 50, 0, 200),
                new Int32Property(PropertyNames.Hardness, 50, 0, 100),
                new BooleanProperty(PropertyNames.AutoSigma, false),
                new DoubleProperty(PropertyNames.Sigma, 1.0, 0.001, 2.0),
            };

            return new PropertyCollection(props);
        }

        protected override ControlInfo OnCreateConfigUI(PropertyCollection props)
        {
            ControlInfo configUI = CreateDefaultConfigUI(props);

            configUI.SetPropertyControlValue(PropertyNames.Radius, ControlInfoPropertyNames.DisplayName, "Radius");
            configUI.SetPropertyControlValue(PropertyNames.Hardness, ControlInfoPropertyNames.DisplayName, "Hardness");
            configUI.SetPropertyControlValue(PropertyNames.AutoSigma, ControlInfoPropertyNames.Description, "Auto Sigma");
            configUI.SetPropertyControlValue(PropertyNames.Sigma, ControlInfoPropertyNames.DisplayName, "Sigma");
            configUI.SetPropertyControlValue(PropertyNames.Sigma, ControlInfoPropertyNames.SliderSmallChange, 0.05);
            configUI.SetPropertyControlValue(PropertyNames.Sigma, ControlInfoPropertyNames.SliderLargeChange, 0.25);
            configUI.SetPropertyControlValue(PropertyNames.Sigma, ControlInfoPropertyNames.UpDownIncrement, 0.01);

            return configUI;
        }

        private int Radius { get; set; }
        private float Hardness { get; set; }
        private float Sigma { get; set; }

        private Surface DestSurface { get; set; } = null;

        protected override void OnSetRenderInfo(PropertyBasedEffectConfigToken newToken, RenderArgs dstArgs, RenderArgs srcArgs)
        {
            Radius = newToken.GetProperty<Int32Property>(PropertyNames.Radius).Value;
            Hardness = (float)newToken.GetProperty<Int32Property>(PropertyNames.Hardness).Value * 0.01f;
            bool autoSigma = newToken.GetProperty<BooleanProperty>(PropertyNames.AutoSigma).Value;
            if (autoSigma)
            {
                Sigma = Math.Max(1.0f - Hardness, 0.01f);
            }
            else
            {
                Sigma = (float)newToken.GetProperty<DoubleProperty>(PropertyNames.Sigma).Value;
            }

            base.OnSetRenderInfo(newToken, dstArgs, srcArgs);

            // at this point we will perform the filter on the src and
            // store the result in DestSurface to be referenced in
            // subsequent calls to OnRender

            DestSurface = srcArgs.Surface.Clone();
            DestSurface.Clear(ColorBgra.Transparent);

            DrawBrush(DestSurface);
        }

        private unsafe void DrawBrush(Surface dstSurface)
        {
            Point center = new Point(dstSurface.Bounds.Width / 2, dstSurface.Bounds.Height / 2);

            double k1 = 1 / (Sigma * Math.Sqrt(2 * Math.PI));
            double k2 = -.5 / (Sigma * Sigma);
            double scaleAlpha = 255 / k1;
            int max = Math.Min(Radius * 2, Math.Min(center.X, center.Y) - 1);
            float opaqueRadius = Hardness * Radius;

            ColorBgra GaussianColor(float distance)
            {
                if (distance < opaqueRadius)
                {
                    return BrushColor;
                }
                distance -= opaqueRadius;
                float x = distance * 2.0f / Radius;
                float guassian = (float)(k1 * Math.Exp(k2 * x * x));
                byte alpha = Utility.ClampToByte(guassian * scaleAlpha);
                return BrushColor.NewAlpha(alpha);
            }

            ColorBgra* dstPtrCenter = dstSurface.GetPointAddressUnchecked(center.X, center.Y);
            *dstPtrCenter = GaussianColor(0);

            ColorBgra* dstPtrPosX = dstPtrCenter + 1;
            ColorBgra* dstPtrNegX = dstPtrCenter - 1;
            for (int x = 1; x < max; ++x)
            {
                ColorBgra color = GaussianColor(x);
                *dstPtrPosX++ = color;
                *dstPtrNegX-- = color;
            }

            int posY = center.Y + 1;
            int negY = center.Y - 1;
            for (int y = 1; y < max; ++y)
            {
                ColorBgra color = GaussianColor(y);
                ColorBgra* dstPtrPosY = dstSurface.GetPointAddressUnchecked(center.X, posY++);
                ColorBgra* dstPtrNegY = dstSurface.GetPointAddressUnchecked(center.X, negY--);
                *dstPtrPosY = color;
                *dstPtrNegY = color;

                ColorBgra* dstPtrPosYPosX = dstPtrPosY + 1;
                ColorBgra* dstPtrPosYNegX = dstPtrPosY - 1;
                ColorBgra* dstPtrNegYPosX = dstPtrNegY + 1;
                ColorBgra* dstPtrNegYNegX = dstPtrNegY - 1;

                for (int x = 1; x < max; ++x)
                {
                    float distance = (float)Math.Sqrt(x * x + y * y);

                    color = GaussianColor(distance);
                    *dstPtrPosYPosX++ = color;
                    *dstPtrPosYNegX-- = color;
                    *dstPtrNegYPosX++ = color;
                    *dstPtrNegYNegX-- = color;
                }
            }
        }

        protected unsafe override void OnRender(Rectangle[] rois, int startIndex, int length)
        {
            if (DestSurface != null)
            {
                for (int ri = startIndex; ri < startIndex + length; ++ri)
                {
                    DstArgs.Surface.CopySurface(DestSurface, rois[ri].Location, rois[ri]);
                }
            }
        }
    }
}

