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
    public sealed class AlphaEffect
        : PropertyBasedEffect
    {
        public enum PropertyNames
        {
            Invert = 0,
            AlphaOp
        }

        public static string StaticName
        {
            get => Properties.Resources.ExtractAlphaEffectName;
        }

        public static Image StaticImage
        {
            get => Properties.Resources.AlphaEffect;
        }

        private Type[] AlphaOps { get; }

        public ColorBgra ForeColor => EnvironmentParameters.PrimaryColor;
        public ColorBgra BackColor => EnvironmentParameters.SecondaryColor;

        public AlphaEffect()
            : base(StaticName,
                   StaticImage,
                   "Cyberjax",
                   EffectFlags.Configurable | EffectFlags.SingleThreaded)
        {
            AlphaOps = GetAlphaOps();
        }

        public static Type[] GetAlphaOps()
        {
            Type[] allTypes = typeof(AlphaEffect).GetNestedTypes();
            List<Type> types = new List<Type>(allTypes.Length);

            foreach (Type type in allTypes)
            {
                if (type.IsSubclassOf(typeof(AlphaMaskOp)) && !type.IsAbstract)
                {
                    types.Add(type);
                }
            }

            return types.ToArray();
        }

        private static string GetAlphaOpFriendlyName(Type opType)
        {
            return Utility.GetStaticName(opType);
        }

        private static AlphaMaskOp CreateMaskOp(Type opType)
        {
            ConstructorInfo ci = opType.GetConstructor(Type.EmptyTypes);
            AlphaMaskOp op = (AlphaMaskOp)ci.Invoke(null);
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
                new StaticListChoiceProperty(PropertyNames.AlphaOp, AlphaOps, 0, false),
                new BooleanProperty(PropertyNames.Invert, false)
            };

            return new PropertyCollection(props);
        }

        protected override ControlInfo OnCreateConfigUI(PropertyCollection props)
        {
            ControlInfo configUI = CreateDefaultConfigUI(props);

            configUI.SetPropertyControlType(PropertyNames.AlphaOp, PropertyControlType.DropDown);
            PropertyControlInfo alphaOpControl = configUI.FindControlForPropertyName(PropertyNames.AlphaOp);
            alphaOpControl.ControlProperties[ControlInfoPropertyNames.DisplayName].Value = "Color Mask";
            foreach (Type maskOp in AlphaOps)
            {
                alphaOpControl.SetValueDisplayName(maskOp, GetAlphaOpFriendlyName(maskOp));
            }

            configUI.SetPropertyControlValue(PropertyNames.Invert, ControlInfoPropertyNames.Description, "Invert");


            return configUI;
        }

        private AlphaMaskOp MaskOp { get; set; }

        protected override void OnSetRenderInfo(PropertyBasedEffectConfigToken newToken, RenderArgs dstArgs, RenderArgs srcArgs)
        {
            bool invert = newToken.GetProperty<BooleanProperty>(PropertyNames.Invert).Value;
            Type maskOpType = (Type)newToken.GetProperty<StaticListChoiceProperty>(PropertyNames.AlphaOp).Value;
            MaskOp = CreateMaskOp(maskOpType);
            MaskOp.Invert = invert;
            MaskOp.ForeColor = ForeColor;
            MaskOp.BackColor = BackColor;
            MaskOp.OutputColor = ForeColor;
            MaskOp.OnValuesChanged();

            base.OnSetRenderInfo(newToken, dstArgs, srcArgs);
        }

        protected override void OnRender(Rectangle[] rois, int startIndex, int length)
        {
            MaskOp.Apply(DstArgs.Surface, SrcArgs.Surface, rois, startIndex, length);
        }

        [Serializable]
        public abstract class AlphaMaskOp
            : UnaryPixelOp
        {
            public bool Invert { get; set; }
            public ColorBgra ForeColor { get; set; }
            public ColorBgra BackColor { get; set; }
            public ColorBgra OutputColor { get; set; }

            public abstract void OnValuesChanged();
        }

        [Serializable]
        public class AlphaAverageOp
        : AlphaMaskOp
        {
            public static string StaticName
            {
                get => "Average Deviance";
            }

            private float ForeAverage { get; set; }
            private float BackAverage { get; set; }

            public override void OnValuesChanged()
            {
                ForeAverage = ForeColor.GetAverageColorChannelValueF();
                BackAverage = BackColor.GetAverageColorChannelValueF();
            }

            public override ColorBgra Apply(ColorBgra color)
            {
                float colorAverage = color.GetAverageColorChannelValueF();
                float scale = (colorAverage - BackAverage) / (ForeAverage - BackAverage);
                byte alpha = Utility.ClampToByte(scale * 255);
                return OutputColor.NewAlpha(alpha);
            }
        }

        [Serializable]
        public class AlphaChannelOp
            : AlphaMaskOp
        {
            public static string StaticName
            {
                get => "Channel Deviance";
            }

            private int Channel { get; set; }
            private int Deviance { get; set; }

            public override void OnValuesChanged()
            {
                Channel = 0;
                Deviance = 0;
                for (int channel = 0; channel <= 3; ++channel)
                {
                    int deviance = ForeColor[channel] - BackColor[channel];
                    if (Math.Abs(deviance) > Math.Abs(Deviance))
                    {
                        Channel = channel;
                        Deviance = deviance;
                    }
                }
            }

            public override ColorBgra Apply(ColorBgra color)
            {
                int deviance = color[Channel] - BackColor[Channel];
                float scale = (float)deviance / Deviance;
                byte alpha = Utility.ClampToByte(scale * 255);
                return OutputColor.NewAlpha(alpha);
            }
        }

        [Serializable]
        public class AlphaDistanceOp
    : AlphaMaskOp
        {
            public static string StaticName
            {
                get => "Distance";
            }

            private int Distance { get; set; }

            public override void OnValuesChanged()
            {

                Distance = Utility.ColorDifference(BackColor, ForeColor);
            }

            public override ColorBgra Apply(ColorBgra color)
            {
                int distance = Utility.ColorDifference(BackColor, color);
                float scale = (float)distance / Distance;
                byte alpha = Utility.ClampToByte(scale * 255);
                return OutputColor.NewAlpha(alpha);
            }
        }

        [Serializable]
        public class AlphaVectorOp
            : AlphaMaskOp
        {
            public static string StaticName
            {
                get => "Vector Projection";
            }

            public Point3D Origin3D { get; set; }
            public Vector3D AlphaVector { get; set; }
            public double AlphaVectorLength { get; set; }

            public override void OnValuesChanged()
            {
                Origin3D = BackColor.ToPoint3D();
                Vector3D alphaVector = BackColor.ToVector3D(ForeColor);
                AlphaVectorLength = alphaVector.Length;
                alphaVector.Unitize();
                AlphaVector = alphaVector;
            }

            public override ColorBgra Apply(ColorBgra color)
            {
                Vector3D vector2 = color.ToPoint3D() - Origin3D;
                double alpha = (vector2 * AlphaVector) / AlphaVectorLength;
                return OutputColor.NewAlpha(Utility.ClampToByte(alpha * 255));
            }
        }
    }
}
