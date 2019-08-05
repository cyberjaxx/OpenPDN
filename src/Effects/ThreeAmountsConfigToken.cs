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
    public class ThreeAmountsConfigToken
        : TwoAmountsConfigToken
    {
        public int Amount3 { get; set; }

        public override object Clone()
        {
            return new ThreeAmountsConfigToken(this);
        }

        public ThreeAmountsConfigToken(int amount1, int amount2, int amount3)
            : base(amount1, amount2)
        {
            this.Amount3 = amount3;
        }

        private ThreeAmountsConfigToken(ThreeAmountsConfigToken copyMe)
            : base(copyMe)
        {
            this.Amount3 = copyMe.Amount3;
        }
    }
}
