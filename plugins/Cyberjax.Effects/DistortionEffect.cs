using System;
using System.Drawing;
using PaintDotNet;
using PaintDotNet.Effects;

namespace Cyberjax
{
    public class DistortionEffectProperties
    {
        public int MaxRadius;
        public PointF Center;
        public double[][] TransferCurves;
    }

    [EffectCategory(EffectCategory.Effect)]
    public sealed class DistortionEffect
        : Effect<DistortionEffectConfigToken>
    {
        public DistortionEffect()
            : base(Properties.Resources.DistortionEffectName,
                   Properties.Resources.BulgeEffect,
                   "Cyberjax",
                   EffectFlags.Configurable)
        {
        }

        private DistortionEffectProperties Props = new DistortionEffectProperties();

        protected override void OnSetRenderInfo(DistortionEffectConfigToken newToken, RenderArgs dstArgs, RenderArgs srcArgs)
        {
            Surface src = srcArgs.Surface;
            int wdiv2 = src.Width / 2;
            int hdiv2 = src.Height / 2;
            int maxRadius = Math.Min(wdiv2, hdiv2);

            Props.MaxRadius = (int)(maxRadius * Token.ValueR);
            Props.TransferCurves = Token.GetTransferCurves(new int[] { Props.MaxRadius, Props.MaxRadius }, new float[] { Props.MaxRadius, Props.MaxRadius });
            Props.Center = new PointF(wdiv2 + (float)Token.ValueX * wdiv2, hdiv2 + (float)Token.ValueY * hdiv2);;
        }

        protected unsafe override void OnRender(Rectangle[] rois, int startIndex, int length)
        {
            Surface dst = DstArgs.Surface;
            Surface src = SrcArgs.Surface;

            for (int n = startIndex; n < startIndex + length; ++n)
            {
                Rectangle rect = rois[n];

                for (int y = rect.Top; y < rect.Bottom; y++)
                {
                    ColorBgra* dstPtr = dst.GetPointAddressUnchecked(rect.Left, y);
                    ColorBgra* srcPtr = src.GetPointAddressUnchecked(rect.Left, y);

                    for (int x = rect.Left; x < rect.Right; x++)
                    {
                        *dstPtr = GetColor(Props, x, y, src, *srcPtr);

                        ++dstPtr;
                        ++srcPtr;
                    }
                }
            }
        }

        public static ColorBgra GetColor(DistortionEffectProperties props, int x, int y, Surface src, ColorBgra bgra)
        {
            float u = x - props.Center.X;
            float v = y - props.Center.Y;
            int radius = (int)Math.Sqrt(u * u + v * v);

            if (radius > 0 && radius < props.MaxRadius)
            {
                float scaleX = (float)props.TransferCurves[0][radius] / radius;
                float scaleY = (float)props.TransferCurves[1][radius] / radius;
                float xp = u * scaleX;
                float yp = v * scaleY;
                bgra = src.GetBilinearSampleClamped(props.Center.X + xp, props.Center.Y + yp);
            }
            return bgra;
        }

        public override EffectConfigDialog CreateConfigDialog()
        {
            Surface sourceSurface = EnvironmentParameters.SourceSurface;
            Bitmap bitmap = sourceSurface.CreateAliasedBitmap();
            ImageResource imageResource = ImageResource.FromImage(bitmap);

            return new DistortionEffectConfigDialog()
            {
                PanControlSettings = new PanControlSettings()
                {
                    MinValueX = -1.0,
                    MaxValueX = 1.0,
                    MinValueY = -1.0,
                    MaxValueY = 1.0,
                    MinValueR = 0.0,
                    MaxValueR = 1.0,
                    StaticImageUnderlay = imageResource
                }
            };
        }
    }
}
