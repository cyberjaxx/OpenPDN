using System;
using PaintDotNet;
using PaintDotNet.Effects;
using PaintDotNet.PropertySystem;
using System.Drawing;
using Cyberjax.Geometry;

namespace Cyberjax
{
    [EffectCategory(EffectCategory.Adjustment)]
    public sealed class ColorDistanceGraphEffect
        : PropertyBasedEffect
    {
        private static double Sqrt2Over2 {get; } = Math.Sqrt(2.0)/2.0;

        private ColorDistanceOp PixelOp { get; set; }

        public ColorBgra SelectedColor => EnvironmentParameters.SecondaryColor;

        protected override PropertyCollection OnCreatePropertyCollection()
        {
            return PropertyCollection.CreateEmpty();
        }

        protected override void OnRender(Rectangle[] rois, int startIndex, int length)
        {
            PixelOp.SelectedPoint = SelectedColor.ToPoint3D();
            PixelOp.Apply(DstArgs.Surface, SrcArgs.Surface, rois, startIndex, length);
        }

        public ColorDistanceGraphEffect()
            : base(Properties.Resources.ColorDistanceGraphEffectName,
                   Properties.Resources.MaskEffect,
                   null,
                   EffectFlags.None)
        {
            PixelOp = new ColorDistanceOp();
        }

        [Serializable]
        public class ColorDistanceOp
            : UnaryPixelOp
        {
            public Point3D SelectedPoint { get; set; }

            public override ColorBgra Apply(ColorBgra color)
            {
                double distance = SelectedPoint.DistanceTo(color.ToPoint3D());
                byte channel = Utility.ClampToByte(Math.Ceiling(distance * Sqrt2Over2));
                return ColorBgra.FromBgr(channel, channel, channel);
            }
        }
    }
}
