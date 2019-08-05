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
using System.Text;

namespace PaintDotNet.Effects
{
    [Flags]
    public enum EffectFlags
        : ulong
    {
        None = 0,
        Configurable = 1,
        SingleTile = 0x2000000000000000,
        Cancellable = 0x4000000000000000,
        SingleThreaded = 0x8000000000000000
    }
}
