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
    public abstract class State
    {
        protected bool AbortRequested { get; private set; } = false;

        public StateMachine StateMachine { get; set; }

        public bool IsFinalState { get; }

        protected virtual void OnAbort()
        {
        }

        public virtual bool CanAbort
        {
            get
            {
                return false;
            }
        }

        public void Abort()
        {
            if (CanAbort)
            {
                this.AbortRequested = true;
                OnAbort();
            }
        }

        public virtual void OnEnteredState()
        {
        }

        public abstract void ProcessInput(object input, out State newState);

        protected void OnProgress(double percent)
        {
            if (this.StateMachine != null)
            {
                this.StateMachine.OnStateProgress(percent);
            }
        }

        protected State()
            : this(false)
        {
        }

        protected State(bool isFinalState)
        {
            this.IsFinalState = isFinalState;
        }
    }
}
