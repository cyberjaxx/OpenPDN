/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace PaintDotNet
{
    [Serializable]
    internal sealed class GradientInfo
        : ICloneable
    {
        public GradientType GradientType { get; }

        public bool AlphaOnly { get; }

        public GradientRenderer CreateGradientRenderer()
        {
            UserBlendOps.NormalBlendOp normalBlendOp = new UserBlendOps.NormalBlendOp();

            switch (GradientType)
            {
                case GradientType.LinearClamped:
                    return new GradientRenderers.LinearClamped(AlphaOnly, normalBlendOp);

                case GradientType.LinearReflected:
                    return new GradientRenderers.LinearReflected(AlphaOnly, normalBlendOp);

                case GradientType.LinearDiamond:
                    return new GradientRenderers.LinearDiamond(AlphaOnly, normalBlendOp);
                    
                case GradientType.Radial:
                    return new GradientRenderers.Radial(AlphaOnly, normalBlendOp);

                case GradientType.Conical:
                    return new GradientRenderers.Conical(AlphaOnly, normalBlendOp);

                default:
                    throw new InvalidEnumArgumentException();
            }
        }

        public override bool Equals(object obj)
        {
            return (obj is GradientInfo gi && 
                gi.GradientType == GradientType &&
                gi.AlphaOnly == AlphaOnly);
        }

        public override int GetHashCode()
        {
            return unchecked(GradientType.GetHashCode() + AlphaOnly.GetHashCode());
        }

        public GradientInfo(GradientType gradientType, bool alphaOnly)
        {
            GradientType = gradientType;
            AlphaOnly = alphaOnly;
        }

        public GradientInfo Clone()
        {
            return new GradientInfo(GradientType, AlphaOnly);
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
    }
}
