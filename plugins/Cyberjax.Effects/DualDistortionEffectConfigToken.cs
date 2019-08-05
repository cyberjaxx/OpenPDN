/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet.Effects;
using System;

namespace Cyberjax
{
    public class DistortionEffectConfigTokens
        : EffectConfigToken
    {
        public DistortionEffectConfigToken[] SubTokens { get; set; }

        public override object Clone()
        {
            return new DistortionEffectConfigTokens(this);
        }

        public DistortionEffectConfigTokens(int count)
        {
            SubTokens = new DistortionEffectConfigToken[count];
            for (int i = 0; i < count; ++i)
            {
                SubTokens[i] = new DistortionEffectConfigToken();
            }
        }

        protected DistortionEffectConfigTokens(DistortionEffectConfigTokens copyMe)
            : base(copyMe)
        {
            SubTokens = new DistortionEffectConfigToken[copyMe.SubTokens.Length];
            for (int i = 0; i < copyMe.SubTokens.Length; ++i)
            {
                SubTokens[i] = (DistortionEffectConfigToken)copyMe.SubTokens[i].Clone();
            }
        }

    }
}

