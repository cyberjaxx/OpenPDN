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
    internal class ExecutedHistoryMementoEventArgs
        : EventArgs
    {
        public HistoryMemento NewHistoryMemento { get; }

        public ExecutedHistoryMementoEventArgs(HistoryMemento newHistoryMemento)
        {
            this.NewHistoryMemento = newHistoryMemento;
        }
    }
}
