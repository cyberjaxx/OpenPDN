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
using System.Windows.Forms;

namespace PaintDotNet.Effects
{
    public abstract class Effect<TToken>
        : Effect
          where TToken : EffectConfigToken
    {
        protected TToken Token { get; private set; }

        protected RenderArgs DstArgs { get; private set; }

        protected RenderArgs SrcArgs { get; private set; }

        protected abstract void OnRender(Rectangle[] renderRects, int startIndex, int length);

        public void Render(Rectangle[] renderRects, int startIndex, int length)
        {
            if (!SetRenderInfoCalled && !RenderInfoAvailable)
            {
                throw new InvalidOperationException("SetRenderInfo() was not called, nor was render info available implicitly");
            }

            OnRender(renderRects, startIndex, length);
        }

        protected virtual void OnSetRenderInfo(TToken newToken, RenderArgs dstArgs, RenderArgs srcArgs)
        {
        }

        protected override sealed void OnSetRenderInfo(EffectConfigToken parameters, RenderArgs dstArgs, RenderArgs srcArgs)
        {
            Token = (TToken)parameters;
            DstArgs = dstArgs;
            SrcArgs = srcArgs;

            OnSetRenderInfo((TToken)parameters, dstArgs, srcArgs);

            base.OnSetRenderInfo(parameters, dstArgs, srcArgs);
        }
        internal protected bool RenderInfoAvailable { get; private set; } = false;

        public override sealed void Render(EffectConfigToken parameters, RenderArgs dstArgs, RenderArgs srcArgs, Rectangle[] rois, int startIndex, int length)
        {
            if (!SetRenderInfoCalled)
            {
                lock (this)
                {
                    Token = (TToken)parameters;
                    DstArgs = dstArgs;
                    SrcArgs = srcArgs;

                    RenderInfoAvailable = true;

                    OnSetRenderInfo(Token, DstArgs, SrcArgs);
                }
            }

            Render(rois, startIndex, length);
        }

        public Effect(string name, Image image)
            : base(name, image)
        {
        }

        public Effect(string name, Image image, string subMenuName)
            : base(name, image, subMenuName)
        {
        }

        public Effect(string name, Image image, string subMenuName, EffectFlags flags)
            : base(name, image, subMenuName, flags)
        {
        }
    }
}