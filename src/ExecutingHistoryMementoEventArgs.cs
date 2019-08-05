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
    internal class ExecutingHistoryMementoEventArgs
        : EventArgs
    {

        public HistoryMemento HistoryMemento { get; }

        public bool MayAlterSuspendTool { get; }

        private bool suspendTool;
        public bool SuspendTool
        {
            get => suspendTool;

            set
            {
                if (!this.MayAlterSuspendTool)
                {
                    throw new InvalidOperationException("May not alter the SuspendTool property when MayAlterSuspendToolProperty is false");
                }

                this.suspendTool = value;
            }
        }

        public ExecutingHistoryMementoEventArgs(HistoryMemento historyMemento, bool mayAlterSuspendToolProperty, bool suspendTool)
        {
            this.HistoryMemento = historyMemento;
            this.MayAlterSuspendTool = mayAlterSuspendToolProperty;
            this.suspendTool = suspendTool;
        }
    }
}
