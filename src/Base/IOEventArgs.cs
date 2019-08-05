/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

namespace PaintDotNet
{
    public sealed class IOEventArgs
        : EventArgs
    {
        public IOOperationType IOOperationType { get; }
        public long Position { get; }
        public int Count { get; }

        public IOEventArgs(IOOperationType ioOperationType, long position, int count)
        {
            this.IOOperationType = ioOperationType;
            this.Position = position;
            this.Count = count;
        }
    }
}
