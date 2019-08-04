using System;
using PaintDotNet;
using PaintDotNet.Effects;
using PaintDotNet.PropertySystem;
using System.Drawing;
using Cyberjax.Geometry;

namespace Cyberjax
{
    [EffectCategory(EffectCategory.Adjustment)]
    public sealed class AngleDevianceEffect
        : PropertyBasedEffect
    {
        private AngleDevianceOp PixelOp { get; set; }

        public ColorBgra ForeColor => EnvironmentParameters.PrimaryColor;
        public ColorBgra BackColor => EnvironmentParameters.SecondaryColor;

        protected override PropertyCollection OnCreatePropertyCollection()
        {
            return PropertyCollection.CreateEmpty();
        }

        protected override void OnRender(Rectangle[] rois, int startIndex, int length)
        {
            PixelOp.SelectedVector = BackColor.ToVector3D();
            PixelOp.Apply(DstArgs.Surface, SrcArgs.Surface, rois, startIndex, length);
            PixelOp.OutputColor = ColorBgra.White;
        }

        public AngleDevianceEffect()
            : base(Properties.Resources.AngleDevianceEffectName,
                   Properties.Resources.ColorAngleEffect,
                   null,
                   EffectFlags.None)
        {
            PixelOp = new AngleDevianceOp();
        }

        [Serializable]
        public class AngleDevianceOp
            : UnaryPixelOp
        {
            public Vector3D SelectedVector { get; set; }
            public ColorBgra OutputColor { get; set; }

            public override ColorBgra Apply(ColorBgra color)
            {

                Vector3D colorVector = color.ToVector3D();
                double scale = 2 * Vector3D.VectorAngle(colorVector, SelectedVector) / Math.PI;
                byte channel = Utility.ClampToByte(scale * 255);
                return ColorBgra.FromBgr(channel, channel, channel);
            }
        }
    }
}
