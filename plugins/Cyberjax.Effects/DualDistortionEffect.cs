using System;
using System.Drawing;
using PaintDotNet;
using PaintDotNet.Effects;

namespace Cyberjax
{
    [EffectCategory(EffectCategory.Effect)]
    public sealed class DualDistortionEffect
        : Effect<DistortionEffectConfigTokens>
    {
        private DistortionEffectProperties[] Props { get; }

        public DualDistortionEffect()
            : base(Properties.Resources.DualDistortionEffectName,
                   Properties.Resources.BulgeEffect,
                   "Cyberjax",
                   EffectFlags.Configurable)
        {
            Props = new DistortionEffectProperties[]
            {
                new DistortionEffectProperties(),
                new DistortionEffectProperties()
            };
        }

        protected override void OnSetRenderInfo(DistortionEffectConfigTokens newTokens, RenderArgs dstArgs, RenderArgs srcArgs)
        {
            Surface src = srcArgs.Surface;
            int wdiv2 = src.Width / 2;
            int hdiv2 = src.Height / 2;
            int maxRadius = Math.Min(wdiv2, hdiv2);

            for (int i = 0; i < newTokens.SubTokens.Length; ++i)
            {
                DistortionEffectConfigToken token = newTokens.SubTokens[i];
                DistortionEffectProperties props = Props[i];
                props.MaxRadius = (int)(maxRadius * token.ValueR);
                props.TransferCurves = token.GetTransferCurves(new int[] { props.MaxRadius, props.MaxRadius }, new float[] { props.MaxRadius, props.MaxRadius });
                props.Center = new PointF(wdiv2 + (float)token.ValueX * wdiv2, hdiv2 + (float)token.ValueY * hdiv2);
            }
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

        public static ColorBgra GetColor(DistortionEffectProperties[] props, int x, int y, Surface src, ColorBgra bgra)
        {
            float offsetX = 0;
            float offsetY = 0;
            int radius = int.MaxValue;
            int selected = 0;

            for (int i = 0; i < props.Length; ++i)
            {
                float u = x - props[i].Center.X;
                float v = y - props[i].Center.Y;
                int r = (int)Math.Sqrt(u * u + v * v);
                if (r < radius)
                {
                    offsetX = u;
                    offsetY = v;
                    radius = r;
                    selected = i;
                }
            }

            DistortionEffectProperties selectedProps = props[selected];
            if (radius > 0 && radius < selectedProps.MaxRadius)
            {
                float scaleX = (float)selectedProps.TransferCurves[0][radius] / radius;
                float scaleY = (float)selectedProps.TransferCurves[1][radius] / radius;
                float xp = offsetX * scaleX;
                float yp = offsetY * scaleY;
                bgra = src.GetBilinearSampleClamped(selectedProps.Center.X + xp, selectedProps.Center.Y + yp);
            }
            return bgra;
        }

        public override EffectConfigDialog CreateConfigDialog()
        {
            Surface sourceSurface = EnvironmentParameters.SourceSurface;
            Bitmap bitmap = sourceSurface.CreateAliasedBitmap();
            ImageResource imageResource = ImageResource.FromImage(bitmap);

            return new DualDistortionEffectConfigDialog()
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

