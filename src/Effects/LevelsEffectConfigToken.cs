/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;

namespace PaintDotNet.Effects
{
    public class LevelsEffectConfigToken 
        : EffectConfigToken
    {
        public UnaryPixelOps.Level Levels { get; set; } = null;

        public LevelsEffectConfigToken()
        {
            Levels = new UnaryPixelOps.Level();
        }

        public override object Clone()
        {
            LevelsEffectConfigToken cpy = new LevelsEffectConfigToken();
            cpy.Levels = (UnaryPixelOps.Level)this.Levels.Clone();
            return cpy;
        }
    }
}
