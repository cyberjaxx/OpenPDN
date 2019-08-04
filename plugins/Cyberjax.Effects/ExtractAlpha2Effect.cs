using System;
using PaintDotNet;
using PaintDotNet.Effects;
using PaintDotNet.PropertySystem;
using System.Drawing;
using Cyberjax.Geometry;

namespace Cyberjax
{
    [EffectCategory(EffectCategory.Adjustment)]
    public sealed class ExtractAlpha2Effect
        : PropertyBasedEffect
    {
        private ExtractAlpha2Op PixelOp { get; set; }

        public ColorBgra ForeColor => EnvironmentParameters.PrimaryColor;
        public ColorBgra BackColor => EnvironmentParameters.SecondaryColor;

        protected override PropertyCollection OnCreatePropertyCollection()
        {
            return PropertyCollection.CreateEmpty();
        }

        protected override void OnRender(Rectangle[] rois, int startIndex, int length)
        {
            PixelOp.Origin3D = BackColor.ToPoint3D();
            Vector3D AlphaVector= BackColor.ToVector3D(ForeColor);
            PixelOp.AlphaVectorLength = AlphaVector.Length;
            AlphaVector.Unitize();
            PixelOp.AlphaVector = AlphaVector;
            PixelOp.Apply(DstArgs.Surface, SrcArgs.Surface, rois, startIndex, length);
            PixelOp.OutputColor = ForeColor;
        }

        public ExtractAlpha2Effect()
            : base(Properties.Resources.ExtractAlpha2EffectName,
                   Properties.Resources.AlphaEffect,
                   null,
                   EffectFlags.None)
        {
            PixelOp = new ExtractAlpha2Op();
        }

        [Serializable]
        public class ExtractAlpha2Op
            : UnaryPixelOp
        {
            public Point3D Origin3D { get; set; }
            public Vector3D AlphaVector { get; set; }
            public double AlphaVectorLength { get; set; }
            public ColorBgra OutputColor { get; set; }

            public override ColorBgra Apply(ColorBgra color)
            {
                Vector3D vector2 = color.ToPoint3D() - Origin3D;
                double alpha = (vector2 * AlphaVector) / AlphaVectorLength;
                return OutputColor.NewAlpha(Utility.ClampToByte(alpha * 255));
            }
        }
    }
}
