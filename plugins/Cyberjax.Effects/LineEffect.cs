using Cyberjax.Extensions;
using PaintDotNet;
using PaintDotNet.Effects;
using PaintDotNet.IndirectUI;
using PaintDotNet.PropertySystem;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;

namespace Cyberjax
{
    // plot the pixel at(x, y) with brightness c(where 0 ≤ c ≤ 1)
    public delegate void PlotFunction(int x, int y, float c);

    public sealed class LineEffect
        : PropertyBasedEffect
    {
        public static string StaticName
        {
            get => Properties.Resources.LineEffectName;
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
            Offset0 = 0,
            Offset1,
            LinePlotter
        }

        public ColorBgra LineColor => EnvironmentParameters.PrimaryColor;

        private Type[] LinePlotters { get; }

        public LineEffect()
            : base(StaticName, StaticImage, StaticSubMenuName,
                  EffectFlags.Configurable | EffectFlags.SingleThreaded | EffectFlags.SingleTile)
        {
            LinePlotters = GetLinePlotters();
        }

        public static Type[] GetLinePlotters()
        {
            Type[] allTypes = typeof(LineEffect).GetNestedTypes();
            List<Type> types = new List<Type>(allTypes.Length);

            foreach (Type type in allTypes)
            {
                if (type.IsSubclassOf(typeof(LinePlotter)) && !type.IsAbstract)
                {
                    types.Add(type);
                }
            }

            return types.ToArray();
        }

        private static string GetLinePlotterFriendlyName(Type opType)
        {
            return Utility.GetStaticName(opType);
        }

        private static LinePlotter CreateLinePlotter(Type plotterType)
        {
            ConstructorInfo ci = plotterType.GetConstructor(Type.EmptyTypes);
            LinePlotter plotter = (LinePlotter)ci.Invoke(null);
            return plotter;
        }

        protected override PropertyCollection OnCreatePropertyCollection()
        {

            List<Property> props = new List<Property>
            {
                new StaticListChoiceProperty(
                    PropertyNames.LinePlotter,
                    LinePlotters, 0, false),

                new DoubleVectorProperty(
                    PropertyNames.Offset0,
                    Pair.Create(0.0, 0.0),
                    Pair.Create(-1.0, -1.0),
                    Pair.Create(1.0, 1.0)),

               new DoubleVectorProperty(
                    PropertyNames.Offset1,
                    Pair.Create(0.0, 0.0),
                    Pair.Create(-1.0, -1.0),
                    Pair.Create(1.0, 1.0))
            };

            return new PropertyCollection(props);
        }

        private void SetOffsetPropertyControlValues(ControlInfo configUI, PropertyNames propertyName, string displayName)
        {
            configUI.SetPropertyControlValue(propertyName, ControlInfoPropertyNames.DisplayName, displayName);
            configUI.SetPropertyControlValue(propertyName, ControlInfoPropertyNames.SliderSmallChangeX, 0.05);
            configUI.SetPropertyControlValue(propertyName, ControlInfoPropertyNames.SliderLargeChangeX, 0.25);
            configUI.SetPropertyControlValue(propertyName, ControlInfoPropertyNames.UpDownIncrementX, 0.01);
            configUI.SetPropertyControlValue(propertyName, ControlInfoPropertyNames.SliderSmallChangeY, 0.05);
            configUI.SetPropertyControlValue(propertyName, ControlInfoPropertyNames.SliderLargeChangeY, 0.25);
            configUI.SetPropertyControlValue(propertyName, ControlInfoPropertyNames.UpDownIncrementY, 0.01);

            Surface sourceSurface = this.EnvironmentParameters.SourceSurface;
            Bitmap bitmap = sourceSurface.CreateAliasedBitmap();
            ImageResource imageResource = ImageResource.FromImage(bitmap);
            configUI.SetPropertyControlValue(propertyName, ControlInfoPropertyNames.StaticImageUnderlay, imageResource);
        }

        protected override ControlInfo OnCreateConfigUI(PropertyCollection props)
        {
            ControlInfo configUI = CreateDefaultConfigUI(props);

            SetOffsetPropertyControlValues(configUI, PropertyNames.Offset0, Properties.Resources.LineEffectOffset0Text);
            SetOffsetPropertyControlValues(configUI, PropertyNames.Offset1, Properties.Resources.LineEffectOffset1Text);

            configUI.SetPropertyControlType(PropertyNames.LinePlotter, PropertyControlType.DropDown);
            PropertyControlInfo linePlotterControl = configUI.FindControlForPropertyName(PropertyNames.LinePlotter);
            linePlotterControl.ControlProperties[ControlInfoPropertyNames.DisplayName].Value = "Color Mask";
            foreach (Type linePlotter in LinePlotters)
            {
                linePlotterControl.SetValueDisplayName(linePlotter, GetLinePlotterFriendlyName(linePlotter));
            }

            return configUI;
        }

        private Surface DestSurface { get; set; } = null;
        private PointF Point0 { get; set; }
        private PointF Point1 { get; set; }
        private LinePlotter Plotter { get; set; }

        private PointF GetPointProperty(PropertyBasedEffectConfigToken token, PropertyNames propertyName, SizeF size)
        {
            float offsetX = (float)token.GetProperty<DoubleVectorProperty>(propertyName).ValueX;
            float offsetY = (float)token.GetProperty<DoubleVectorProperty>(propertyName).ValueY;
            return new PointF(size.Width + offsetX * (size.Width - 1), size.Height + offsetY * (size.Height - 1));
        }

        protected override void OnSetRenderInfo(PropertyBasedEffectConfigToken newToken, RenderArgs dstArgs, RenderArgs srcArgs)
        {
            SizeF sizeDiv2 = new SizeF(dstArgs.Width / 2.0f, dstArgs.Height / 2.0f);
            Point0 = GetPointProperty(newToken, PropertyNames.Offset0, sizeDiv2);
            Point1 = GetPointProperty(newToken, PropertyNames.Offset1, sizeDiv2);
            Type plotterType = (Type)newToken.GetProperty<StaticListChoiceProperty>(PropertyNames.LinePlotter).Value;
            Plotter = CreateLinePlotter(plotterType);

            base.OnSetRenderInfo(newToken, dstArgs, srcArgs);

            // at this point we will perform the filter on the src and
            // store the result in DestSurface to be referenced in
            // subsequent calls to OnRender

            DestSurface = srcArgs.Surface.Clone();

            DrawLine(DestSurface, srcArgs.Surface);
        }

        private unsafe void DrawLine(Surface dstSurface, Surface srcSurface)
        {
            void Plot(int x, int y, float c)
            {
                ColorBgra source = srcSurface.GetPointUnchecked(x, y);
                ColorBgra* dstPtr = dstSurface.GetPointAddressUnchecked(x, y);
                *dstPtr = ColorBgra.Blend(source, LineColor, (byte)(c * 255));
            }

            Plotter.DrawLine(Point0, Point1, Plot);
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

        [Serializable]
        public abstract class LinePlotter
        {
            public abstract void DrawLine(PointF point0, PointF point1, PlotFunction plot);
        }

        [Serializable]
        public class BresenhamLinePlotter :
            LinePlotter
        {
            public static string StaticName
            {
                get => "Bresenham's Line Algorithm";
            }

            public override void DrawLine(PointF point0, PointF point1, PlotFunction plot)
            {
                BresenhamLine.Plot(Point.Truncate(point0), Point.Truncate(point1), (x, y) => plot(x, y, 1.0f));
            }
        }

        [Serializable]
        public class XiaolinWuLinePlotter :
            LinePlotter
        {
            public static string StaticName
            {
                get => "Xiaolin Wu's Line Algorithm";
            }

            public override void DrawLine(PointF point0, PointF point1, PlotFunction plot)
            {
                XiaolinWuLine.DrawLine(point0.X, point0.Y, point1.X, point1.Y, false, plot);
            }
        }


        [Serializable]
        public class DDALinePlotter :
            LinePlotter
        {
            public static string StaticName
            {
                get => "DDA Line Algorithm";
            }

            public override void DrawLine(PointF point0, PointF point1, PlotFunction plot)
            {
                float x, y, step;
                float dx = point1.X - point0.X;
                float dy = point1.Y - point0.Y;
                if (Math.Abs(dx) >= Math.Abs(dy))
                {
                    step = Math.Abs(dx);
                }
                else
                {
                    step = Math.Abs(dy);
                }

                dx = dx / step;
                dy = dy / step;

                x = point0.X;
                y = point0.Y;
                int i = 1;
                while (i <= step)
                {
                    plot((int)x, (int)y, 1.0f);
                    x += dx;
                    y += dy;
                    i += 1;
                }
            }
        }
    }
}
