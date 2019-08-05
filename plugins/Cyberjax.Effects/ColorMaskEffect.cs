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

namespace Cyberjax
{
    [EffectCategory(EffectCategory.Effect)]
    public sealed class ColorMaskEffect
        : PropertyBasedEffect
    {
        public enum PropertyNames
        {
            Tolerance = 0,
            Grayscale,
            Invert,
            MaskOp
        }

        public static string StaticName
        {
            get => Properties.Resources.ColorMaskEffectName;
        }

        public static Image StaticImage
        {
            get => Properties.Resources.MaskEffect;
        }

        private Type[] MaskOps { get; }

        public ColorBgra FilterColor => EnvironmentParameters.SecondaryColor;
        public ColorBgra OpaqueColor { get; } = ColorBgra.Black;
        public ColorBgra TransparentColor { get; } = ColorBgra.White;

        public ColorMaskEffect()
            : base(StaticName,
                   StaticImage,
                   "Cyberjax",
                   EffectFlags.Configurable)
        {
            MaskOps = GetMaskOps();
        }

        public static Type[] GetMaskOps()
        {
            Type[] allTypes = typeof(ColorMaskEffect).GetNestedTypes();
            List<Type> types = new List<Type>(allTypes.Length);

            foreach (Type type in allTypes)
            {
                if (type.IsSubclassOf(typeof(ColorToleranceMaskOp)) && !type.IsAbstract)
                {
                    types.Add(type);
                }
            }

            return types.ToArray();
        }

        private static string GetMaskOpFriendlyName(Type opType)
        {
            return Utility.GetStaticName(opType);
        }

        private static ColorToleranceMaskOp CreateMaskOp(Type opType)
        {
            ConstructorInfo ci = opType.GetConstructor(Type.EmptyTypes);
            ColorToleranceMaskOp op = (ColorToleranceMaskOp)ci.Invoke(null);
            return op;
        }


        protected override void OnCustomizeConfigUIWindowProperties(PropertyCollection props)
        {
            props[ControlInfoPropertyNames.WindowIsSizable].Value = false;
            base.OnCustomizeConfigUIWindowProperties(props);
        }

        protected override PropertyCollection OnCreatePropertyCollection()
        {
            List <Property> props = new List<Property>
            {
                new Int32Property(PropertyNames.Tolerance, 128, 0, 256),
                new BooleanProperty(PropertyNames.Grayscale, false),
                new BooleanProperty(PropertyNames.Invert, false),
                new StaticListChoiceProperty(PropertyNames.MaskOp, MaskOps, 0, false)
            };

            return new PropertyCollection(props);
        }

        protected override ControlInfo OnCreateConfigUI(PropertyCollection props)
        {
            ControlInfo configUI = CreateDefaultConfigUI(props);

            configUI.SetPropertyControlValue(PropertyNames.Tolerance, ControlInfoPropertyNames.DisplayName, "Tolerance");
            configUI.SetPropertyControlValue(PropertyNames.Grayscale, ControlInfoPropertyNames.Description, "Grayscale");
            configUI.SetPropertyControlValue(PropertyNames.Invert, ControlInfoPropertyNames.Description, "Invert");
            PropertyControlInfo maskOpControl = configUI.FindControlForPropertyName(PropertyNames.MaskOp);
            maskOpControl.ControlProperties[ControlInfoPropertyNames.DisplayName].Value = "Color Mask";

            foreach (Type maskOp in MaskOps)
            {
                maskOpControl.SetValueDisplayName(maskOp, GetMaskOpFriendlyName(maskOp));
            }

            return configUI;
        }

        private ColorToleranceMaskOp MaskOp { get; set; }

        protected override void OnSetRenderInfo(PropertyBasedEffectConfigToken newToken, RenderArgs dstArgs, RenderArgs srcArgs)
        {
            int Tolerance = newToken.GetProperty<Int32Property>(PropertyNames.Tolerance).Value;
            bool Grayscale = newToken.GetProperty<BooleanProperty>(PropertyNames.Grayscale).Value;
            bool Invert = newToken.GetProperty<BooleanProperty>(PropertyNames.Invert).Value;
            Type maskOpType = (Type)newToken.GetProperty<StaticListChoiceProperty>(PropertyNames.MaskOp).Value;
            MaskOp = CreateMaskOp(maskOpType);
            MaskOp.FilterColor = FilterColor;
            MaskOp.Tolerance = Tolerance;
            MaskOp.Grayscale = Grayscale;
            MaskOp.Invert = Invert;
            MaskOp.OpaqueColor = Invert ? TransparentColor : OpaqueColor;
            MaskOp.TransparentColor = Invert ? OpaqueColor : TransparentColor;

            base.OnSetRenderInfo(newToken, dstArgs, srcArgs);
        }

        protected override void OnRender(Rectangle[] rois, int startIndex, int length)
        {
            MaskOp.Apply(DstArgs.Surface, SrcArgs.Surface, rois, startIndex, length);
        }

        [Serializable]
        public abstract class ColorToleranceMaskOp
            : UnaryPixelOp
        {
            public ColorBgra FilterColor { get; set; }
            public int Tolerance { get; set; }
            public bool Grayscale { get; set; }
            public bool Invert { get; set; }
            public ColorBgra OpaqueColor { get; set; }
            public ColorBgra TransparentColor { get; set; }

        }

        [Serializable]
        public class RgbDeltaOp
        : ColorToleranceMaskOp
        {
            public static string StaticName
            {
                get => "RGB Delta";
            }

            public override ColorBgra Apply(ColorBgra color)
            {
                byte value = color.MaxDeviance(FilterColor);

                if (Grayscale)
                {
                    if (!Invert)
                    {
                        value = (byte)(255 - value);
                    }
                    return ColorBgra.FromBgr(value, value, value);
                }
                return value < Tolerance ? TransparentColor : OpaqueColor;
            }
        }

        [Serializable]
        public class RgbDistanceOp
        : ColorToleranceMaskOp
        {
            public static string StaticName
            {
                get => "RGB Distance";
            }

            public override ColorBgra Apply(ColorBgra color)
            {
                byte value = (byte)Utility.ColorDifference(color, FilterColor);

                if (Grayscale)
                {
                    if (!Invert)
                    {
                        value = (byte)(255 - value);
                    }
                    return ColorBgra.FromBgr(value, value, value);
                }
                return value < Tolerance ? TransparentColor : OpaqueColor;
            }
        }

        [Serializable]
        public class RgbAngleOp
            : ColorToleranceMaskOp
        {
            public static string StaticName
            {
                get => "RGB Angle";
            }

            public override ColorBgra Apply(ColorBgra color)
            {
                byte value = ColorAngle(color, FilterColor);
                if (Grayscale)
                {
                    if (!Invert)
                    {
                        value = (byte)(255 - value);
                    }
                    return ColorBgra.FromBgr(value, value, value);
                }
                return value < Tolerance ? TransparentColor : OpaqueColor;
            }

            private static byte ColorAngle(ColorBgra color, ColorBgra filter)
            {
                if (color == filter)
                {
                    return 0;
                }
                Vector3D filterVector = filter.ToVector3D();
                Vector3D colorVector = color.ToVector3D();
                double scale = 2 * Vector3D.VectorAngle(colorVector, filterVector) / Math.PI;
                return Utility.ClampToByte(scale * 255);
            }
        }

        [Serializable]
        public class IntensityOp
            : ColorToleranceMaskOp
        {
            public static string StaticName
            {
                get => "Intensity Delta";
            }

            public override ColorBgra Apply(ColorBgra color)
            {
                byte value = (byte)(Math.Abs(color.GetIntensityByte() - FilterColor.GetIntensityByte()));
                if (Grayscale)
                {
                    if (!Invert)
                    {
                        value = (byte)(255 - value);
                    }
                    return ColorBgra.FromBgr(value, value, value);
                }
                return value < Tolerance ? TransparentColor : OpaqueColor;
            }
        }


        [Serializable]
        public class HueDeltaOp
            : ColorToleranceMaskOp
        {
            public static string StaticName
            {
                get => "Hue Delta";
            }

            public override ColorBgra Apply(ColorBgra color)
            {
                byte value = (byte)(Math.Abs(color.GetHue() - FilterColor.GetHue()) * 255 / 360);
                if (Grayscale)
                {
                    if (!Invert)
                    {
                        value = (byte)(255 - value);
                    }
                    return ColorBgra.FromBgr(value, value, value);
                }
                return value < Tolerance ? TransparentColor : OpaqueColor;
            }
        }

        [Serializable]
        public class SaturationDeltaOp
            : ColorToleranceMaskOp
        {
            public static string StaticName
            {
                get => "Saturation Delta";
            }

            public override ColorBgra Apply(ColorBgra color)
            {
                byte value = (byte)(Math.Abs(color.GetSaturation() - FilterColor.GetSaturation()) * 255);
                if (Grayscale)
                {
                    if (!Invert)
                    {
                        value = (byte)(255 - value);
                    }
                    return ColorBgra.FromBgr(value, value, value);
                }
                return value < Tolerance ? TransparentColor : OpaqueColor;
            }
        }

        [Serializable]
        public class LightnessOp
            : ColorToleranceMaskOp
        {
            public static string StaticName
            {
                get => "Lightness Delta";
            }

            public override ColorBgra Apply(ColorBgra color)
            {
                byte value = (byte)(Math.Abs(color.GetBrightness() - FilterColor.GetBrightness()) * 255);
                if (Grayscale)
                {
                    if (!Invert)
                    {
                        value = (byte)(255 - value);
                    }
                    return ColorBgra.FromBgr(value, value, value);
                }
                return value < Tolerance ? TransparentColor : OpaqueColor;
            }
        }

        [Serializable]
        public class HslDistanceMaskOp
            : ColorToleranceMaskOp
        {
            public static string StaticName
            {
                get => "HSL Distance Mask";
            }

            public override ColorBgra Apply(ColorBgra color)
            {
                byte value = (byte)(HslDifference(color, FilterColor) * 255 / 240);
                if (Grayscale)
                {
                    if (!Invert)
                    {
                        value = (byte)(255 - value);
                    }
                    return ColorBgra.FromBgr(value, value, value);
                }
                return value < Tolerance ? TransparentColor : OpaqueColor;
            }

            public static int HslDifference(ColorBgra bgra1, ColorBgra bgra2)
            {
                return (int)Math.Ceiling(Math.Sqrt(HslDifferenceSquared(bgra1, bgra2)));
            }

            public static int HslDifferenceSquared(ColorBgra bgra1, ColorBgra bgra2)
            {
                ColorHSL hsl1 = bgra1.ToColor();
                ColorHSL hsl2 = bgra2.ToColor();

                int diffSq = 0, tmp;

                tmp = hsl1.H - hsl2.H;
                diffSq += tmp * tmp;
                tmp = hsl1.S - hsl2.S;
                diffSq += tmp * tmp;
                tmp = hsl1.L - hsl2.L;
                diffSq += tmp * tmp;

                return diffSq / 3;
            }
        }
    }
}
