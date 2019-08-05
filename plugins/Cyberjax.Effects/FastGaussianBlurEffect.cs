/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet;
using PaintDotNet.Effects;
using PaintDotNet.IndirectUI;
using PaintDotNet.PropertySystem;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;

namespace Cyberjax
{
    [EffectCategory(EffectCategory.Effect)]
    public sealed class FastGaussianBlurEffect
        : PropertyBasedEffect
    {
        public enum PropertyNames
        {
            Radius = 0
        }

        public static string StaticName
        {
            get => Properties.Resources.FastGaussianBlurEffectName;
        }

        public static Image StaticImage
        {
            get => Properties.Resources.HueSaturation;
        }

        public FastGaussianBlurEffect()
            : base(StaticName,
                   StaticImage,
                   "Cyberjax",
                   EffectFlags.Configurable | EffectFlags.Cancellable)
        {
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
                new Int32Property(PropertyNames.Radius, 2, 0, 200)
            };

            return new PropertyCollection(props);
        }

        protected override ControlInfo OnCreateConfigUI(PropertyCollection props)
        {
            ControlInfo configUI = CreateDefaultConfigUI(props);

            configUI.SetPropertyControlValue(PropertyNames.Radius, ControlInfoPropertyNames.DisplayName, "Radius");

            return configUI;
        }

        private int Radius { get; set; }
        private Surface DestSurface { get; set; } = null;

        protected override void OnSetRenderInfo(PropertyBasedEffectConfigToken newToken, RenderArgs dstArgs, RenderArgs srcArgs)
        {
            Radius = newToken.GetProperty<Int32Property>(PropertyNames.Radius).Value;
            base.OnSetRenderInfo(newToken, dstArgs, srcArgs);

            // at this point we will perform the filter on the src and
            // store the result in DestSurface to be referenced in
            // subsequent calls to OnRender

            DestSurface = new Surface(dstArgs.Size);
            try
            {
                GaussianBlur blur = new GaussianBlur(srcArgs.Surface, CancelToken);
                blur.Process(DestSurface, Radius);
                CancelToken = CancellationToken.None;
            }

            catch(OperationCanceledException)
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
