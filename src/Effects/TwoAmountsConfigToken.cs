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
    public class TwoAmountsConfigToken
        : EffectConfigToken
    {
        public int Amount1 { get; set; }
        public int Amount2 { get; set; }

        public override object Clone()
        {
            return new TwoAmountsConfigToken(this);
        }

        public TwoAmountsConfigToken(int amount1, int amount2)
        {
            this.Amount1 = amount1;
            this.Amount2 = amount2;
        }

        public TwoAmountsConfigToken(TwoAmountsConfigToken copyMe)
            : base(copyMe)
        {
            this.Amount1 = copyMe.Amount1;
            this.Amount2 = copyMe.Amount2;
        }
    }
}
