/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;

namespace PaintDotNet
{
    /// <summary>
    /// A very simple linked-list class, done functional style. Use null for
    /// the tail to indicate the end of a list.
    /// </summary>
    public sealed class List
    {
        public object Head { get; }
        public List Tail { get; }

        public List(object head, List tail)
        {
            this.Head = head;
            this.Tail = tail;
        }
    }
}
