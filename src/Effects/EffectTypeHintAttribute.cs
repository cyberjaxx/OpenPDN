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
    [Obsolete]
    public sealed class EffectTypeHintAttribute
        : Attribute
    {
        public EffectTypeHint EffectTypeHint { get; }

        public EffectTypeHintAttribute(EffectTypeHint effectTypeHint)
        {
            this.EffectTypeHint = effectTypeHint;
        }
    }
}
