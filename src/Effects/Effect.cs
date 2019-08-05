/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace PaintDotNet.Effects
{
    public abstract class Effect
    {
        internal protected bool SetRenderInfoCalled { get; private set; } = false;

        /// <summary>
        /// Returns the category of the effect. If there is no EffectCategoryAttribute
        /// applied to the runtime type, then the default category, EffectCategory.Effect,
        /// will be returned.
        /// </summary>
        /// <remarks>
        /// This controls which menu in the user interface the effect is placed in to.
        /// </remarks>
        public EffectCategory Category
        {
            get
            {
                object[] attributes = GetType().GetCustomAttributes(true);

                foreach (Attribute attribute in attributes)
                {
                    if (attribute is EffectCategoryAttribute)
                    {
                        return ((EffectCategoryAttribute)attribute).Category;
                    }
                }

                return EffectCategory.Effect;
            }
        }

        public EffectEnvironmentParameters EnvironmentParameters { get; set; }

        public EffectFlags EffectFlags { get; }

        public bool CheckForEffectFlags(EffectFlags flags)
        {
            return (EffectFlags & flags) == flags;
        }

        public string SubMenuName { get; }

        public string Name { get; }

        public Image Image { get; }

        public CancellationToken CancelToken { get; set; } = CancellationToken.None;

        public void SetRenderInfo(EffectConfigToken parameters, RenderArgs dstArgs, RenderArgs srcArgs)
        {
            SetRenderInfoCalled = true;
            OnSetRenderInfo(parameters, dstArgs, srcArgs);
        }

        protected virtual void OnSetRenderInfo(EffectConfigToken parameters, RenderArgs dstArgs, RenderArgs srcArgs)
        {
        }

        /// <summary>
        /// Performs the effect's rendering. The source is to be treated as read-only,
        /// and only the destination pixels within the given rectangle-of-interest are
        /// to be written to. However, in order to compute the destination pixels,
        /// any pixels from the source may be utilized.
        /// </summary>
        /// <param name="parameters">The parameters to the effect. If IsConfigurable is true, then this must not be null.</param>
        /// <param name="dstArgs">Describes the destination surface.</param>
        /// <param name="srcArgs">Describes the source surface.</param>
        /// <param name="rois">The list of rectangles that describes the region of interest.</param>
        /// <param name="startIndex">The index within roi to start enumerating from.</param>
        /// <param name="length">The number of rectangles to enumerate from roi.</param>
        public abstract void Render(EffectConfigToken parameters, RenderArgs dstArgs, RenderArgs srcArgs, Rectangle[] rois, int startIndex, int length);

        public void Render(EffectConfigToken parameters, RenderArgs dstArgs, RenderArgs srcArgs, Rectangle[] rois)
        {
            Render(parameters, dstArgs, srcArgs, rois, 0, rois.Length);
        }

        public void Render(EffectConfigToken parameters, RenderArgs dstArgs, RenderArgs srcArgs, PdnRegion roi)
        {
            Rectangle[] scans = roi.GetRegionScansReadOnlyInt();
            Render(parameters, dstArgs, srcArgs, scans, 0, scans.Length);
        }

        public virtual EffectConfigDialog CreateConfigDialog()
        {
            if (CheckForEffectFlags(EffectFlags.Configurable))
            {
                throw new NotImplementedException("If IsConfigurable is true, then CreateConfigDialog() must be implemented");
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// This is a helper function. It allows you to render an effect "in place."
        /// That is, you don't need both a destination and a source Surface.
        /// </summary>
        public void RenderInPlace(RenderArgs srcAndDstArgs, PdnRegion roi)
        {
            using (Surface renderSurface = new Surface(srcAndDstArgs.Surface.Size))
            {
                using (RenderArgs renderArgs = new RenderArgs(renderSurface))
                {
                    Rectangle[] scans = roi.GetRegionScansReadOnlyInt();
                    Render(null, renderArgs, srcAndDstArgs, scans);
                    srcAndDstArgs.Surface.CopySurface(renderSurface, roi);
                }
            }
        }

        public void RenderInPlace(RenderArgs srcAndDstArgs, Rectangle roi)
        {
            using (PdnRegion region = new PdnRegion(roi))
            {
                RenderInPlace(srcAndDstArgs, region);
            }
        }

        public Effect(string name, Image image)
            : this(name, image, EffectFlags.None)
        {
        }

        public Effect(string name, Image image, EffectFlags flags)
            : this(name, image, null, flags)
        {
        }

        public Effect(string name, Image image, string subMenuName)
            : this(name, image, subMenuName, EffectFlags.None)
        {
        }

        /// <summary>
        /// Base constructor for the Effect class.
        /// </summary>
        /// <param name="name">A unique name for the effect.</param>
        /// <param name="image">A 16x16 icon for the effect that will show up in the menu.</param>
        /// <param name="subMenuName">The name of a sub-menu to place the effect into. Pass null for no sub-menu.</param>
        /// <param name="effectFlags">A set of flags indicating important information about the effect.</param>
        /// <remarks>
        /// Do not include the word 'effect' in the name parameter.
        /// The shortcut key is only honored for effects with the [EffectCategory(EffectCategory.Adjustment)] attribute.
        /// The sub-menu parameter can be used to group effects. The name parameter must still be unique.
        /// </remarks>

        public Effect(string name, Image image, string subMenuName, EffectFlags effectFlags)
        {
            Name = name;
            Image = image;
            SubMenuName = subMenuName;
            EffectFlags = effectFlags;
            EnvironmentParameters = EffectEnvironmentParameters.DefaultParameters;
        }
    }
}
