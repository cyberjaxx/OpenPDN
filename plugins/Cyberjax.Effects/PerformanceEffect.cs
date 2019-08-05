using System;
using PaintDotNet;
using PaintDotNet.Effects;
using PaintDotNet.PropertySystem;
using System.Drawing;
using System.Collections.Generic;
using PaintDotNet.IndirectUI;
using Cyberjax.Geometry;
using System.Reflection;
using Cyberjax.Extensions;
using System.Threading;
using PaintDotNet.SystemLayer;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace Cyberjax
{
    [EffectCategory(EffectCategory.Effect)]
    public sealed class PerformanceEffect
        : PropertyBasedEffect
    {
        public enum PropertyNames
        {
            Iterations = 0,
            PerformanceOp
        }

        public static string StaticName
        {
            get => Properties.Resources.PerformanceEffectName;
        }

        public static Image StaticImage
        {
            get => Properties.Resources.PerformanceEffect;
        }

        private Type[] PerformanceOps { get; }

        public ColorBgra ForeColor => EnvironmentParameters.PrimaryColor;
        public ColorBgra BackColor => EnvironmentParameters.SecondaryColor;

        UserBlendOp RenderOp = new UserBlendOps.NormalBlendOp();

        public PerformanceEffect()
            : base(StaticName,
                   StaticImage,
                   "Cyberjax",
                   EffectFlags.Configurable)
        {
            PerformanceOps = GetPerformanceOps();
        }

        public static Type[] GetPerformanceOps()
        {
            Type[] allTypes = typeof(PerformanceEffect).GetNestedTypes();
            List<Type> types = new List<Type>(allTypes.Length);

            foreach (Type type in allTypes)
            {
                if (type.IsSubclassOf(typeof(PerformanceOp)) && !type.IsAbstract)
                {
                    types.Add(type);
                }
            }

            return types.ToArray();
        }

        private static string GetPerformanceOpFriendlyName(Type opType)
        {
            return Utility.GetStaticName(opType);
        }

        private static PerformanceOp CreatePerformanceOp(Type opType)
        {
            ConstructorInfo ci = opType.GetConstructor(Type.EmptyTypes);
            PerformanceOp op = (PerformanceOp)ci.Invoke(null);
            return op;
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
                new StaticListChoiceProperty(PropertyNames.PerformanceOp, PerformanceOps, 0, false),
                new DoubleProperty(PropertyNames.Iterations, 0, 0, 100000),
            };

            return new PropertyCollection(props);
        }

        protected override ControlInfo OnCreateConfigUI(PropertyCollection props)
        {
            ControlInfo configUI = CreateDefaultConfigUI(props);

            configUI.SetPropertyControlType(PropertyNames.PerformanceOp, PropertyControlType.DropDown);
            PropertyControlInfo perfOpControl = configUI.FindControlForPropertyName(PropertyNames.PerformanceOp);
            perfOpControl.ControlProperties[ControlInfoPropertyNames.DisplayName].Value = "Performance Test";
            foreach (Type perfOp in PerformanceOps)
            {
                perfOpControl.SetValueDisplayName(perfOp, GetPerformanceOpFriendlyName(perfOp));
            }
            
            configUI.SetPropertyControlValue(PropertyNames.Iterations, ControlInfoPropertyNames.DisplayName, "Iterations");
            configUI.SetPropertyControlValue(PropertyNames.Iterations, ControlInfoPropertyNames.SliderSmallChange, 1000.0);
            configUI.SetPropertyControlValue(PropertyNames.Iterations, ControlInfoPropertyNames.SliderLargeChange, 10000.0);
            configUI.SetPropertyControlValue(PropertyNames.Iterations, ControlInfoPropertyNames.UpDownIncrement, 100.0);

            return configUI;
        }

        private Timing timing = new Timing();
        private Surface DestSurface { get; set; } = null;

        protected override void OnSetRenderInfo(PropertyBasedEffectConfigToken newToken, RenderArgs dstArgs, RenderArgs srcArgs)
        {
            int iterations = (int)newToken.GetProperty<DoubleProperty>(PropertyNames.Iterations).Value;
            Type perfOpType = (Type)newToken.GetProperty<StaticListChoiceProperty>(PropertyNames.PerformanceOp).Value;
            base.OnSetRenderInfo(newToken, dstArgs, srcArgs);

            // at this point we will perform the filter on the src and
            // store the result in DestSurface to be referenced in
            // subsequent calls to OnRender
            DestSurface = new Surface(dstArgs.Size);
            try
            {
                PerformanceOp PerfOp = CreatePerformanceOp(perfOpType);
                double startTick = timing.GetTickCountDouble();
                PerfOp.Run(iterations);
                double endTick = timing.GetTickCountDouble();

                CancelToken = CancellationToken.None;

                double elapsedTime = endTick - startTick;
                string text = string.Format("Ellapsed Time: {0:N3}", elapsedTime);

                RenderString(DestSurface, text, DestSurface.Width / 50);
            }

            catch (OperationCanceledException)
            {
                DestSurface.Dispose();
                DestSurface = null;
            }

            catch (AggregateException)
            {
                DestSurface.Dispose();
                DestSurface = null;
            }

        }

        protected override unsafe void OnRender(Rectangle[] rois, int startIndex, int length)
        {
            if (DestSurface != null)
            {
                for (int ri = startIndex; ri < startIndex + length; ++ri)
                {
                    Rectangle roi = rois[ri];

                    for (int y = roi.Top; y < roi.Bottom; ++y)
                    {
                        ColorBgra* dstPtr = DstArgs.Surface.GetPointAddressUnchecked(roi.Left, y);
                        ColorBgra* srcPtr = SrcArgs.Surface.GetPointAddressUnchecked(roi.Left, y);
                        ColorBgra* textPtr = DestSurface.GetPointAddressUnchecked(roi.Left, y);

                        RenderOp.Apply(dstPtr, srcPtr, textPtr, roi.Width);
                    }
                }
            }
        }

        private void RenderString(Surface surface, string text, float fontSize)
        {
            StringFormat format = new StringFormat();
            format.Alignment = StringAlignment.Center;
            format.LineAlignment = StringAlignment.Center;

            using (RenderArgs renderArgs = new RenderArgs(surface))
            {
                using (Font font = Utility.CreateFont("Tahoma", fontSize, FontStyle.Regular))
                {
                    Graphics g1 = renderArgs.Graphics;
                    g1.SmoothingMode = SmoothingMode.AntiAlias;
                    g1.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                    SizeF sizeF = g1.MeasureString(text, font, surface.Width, format);
                    Size size = Size.Ceiling(sizeF);
                    Point location = new Point((surface.Width - size.Width) / 2,
                        (surface.Height - size.Height) / 2);
                    Rectangle clippedRect = new Rectangle(location, size);

                    using (Surface clippedSurface = renderArgs.Surface.CreateWindow(clippedRect))
                    {
                        using (RenderArgs clippedRenderArgs = new RenderArgs(clippedSurface))
                        {
                            Graphics g2 = clippedRenderArgs.Graphics;
                            g2.SmoothingMode = SmoothingMode.AntiAlias;
                            g2.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                            g2.DrawString(text, font, Brushes.Black, new Point(0, 0));
                        }
                    }
                }
            }
        }

        [Serializable]
        public abstract class PerformanceOp
        {
            public abstract void Run(int iterations);
        }

        [Serializable]
        public class ByteFieldPerfOp
        : PerformanceOp
        {
            public static string StaticName
            {
                get => "Byte Field Test";
            }

            public override void Run(int iterations)
            {
                for (int i = 0; i < iterations; ++i)
                {
                    ColorBgra color = new ColorBgra()
                    {
                        B = (byte)i,
                        G = (byte)i,
                        R = (byte)i,
                        A = (byte)i
                    };
                }
            }
        }

        [Serializable]
        public class FuturePerfOp
            : PerformanceOp
        {
            public static string StaticName
            {
                get => "Uint Test";
            }

            public override void Run(int iterations)
            {
                for (int i = 0; i < iterations; ++i)
                {
                    ColorBgra color = new ColorBgra()
                    {
                        Bgra = (uint)i + ((uint)i << 8) + ((uint)i << 16) + ((uint)i << 24)
                    };
                }
            }
        }

        [Serializable]
        public class AlphaLimitPerfOp1
            : PerformanceOp
        {
            public static string StaticName
            {
                get => "Alpha Limit Test 1";
            }

            public override unsafe void Run(int iterations)
            {
                const byte _limit = 128;
                byte limit = _limit;
                uint addValue = (uint)limit << 24;

                uint bgra1 = 0xdeadbeef;
                uint bgra2 = 0xfeedbeef;
                ColorBgra* dst = (ColorBgra*)&bgra1;
                ColorBgra* src = (ColorBgra*)&bgra2;

                for (int i = 0; i < iterations; ++i)
                {
                    dst->Bgra = src->A <= limit ? src->Bgra : (src->Bgra & 0x00ffffff) + addValue;
                }
            }
        }

        [Serializable]
        public class AlphaLimitPerfOp2
            : PerformanceOp
        {
            public static string StaticName
            {
                get => "Alpha Limit Test 2";
            }

            public override unsafe void Run(int iterations)
            {
                const byte _limit = 128;
                byte limit = _limit;
                uint addValue = (uint)limit << 24;

                uint bgra1 = 0xdeadbeef;
                uint bgra2 = 0xfeedbeef;
                ColorBgra* dst = (ColorBgra*)&bgra1;
                ColorBgra* src = (ColorBgra*)&bgra2;

                for (int i = 0; i < iterations; ++i)
                {
                    dst->Bgra = src->Bgra;
                    if (dst->A > limit)
                    {
                        dst->A = limit;
                    }
                }
            }
        }
    }
}

