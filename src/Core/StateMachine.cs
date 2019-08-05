/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;

namespace PaintDotNet
{
    public class StateMachine
    {
        private ArrayList inputAlphabet;
        private State initialState;
        private bool processingInput = false;
        private Queue inputQueue = new Queue();

        public event EventHandler<EventArgs<State>> NewState;
        private void OnNewState(State newState)
        {
            NewState?.Invoke(this, new EventArgs<State>(newState));
        }

        public event ProgressEventHandler StateProgress;
        public void OnStateProgress(double percent)
        {
            StateProgress?.Invoke(this, new ProgressEventArgs(percent));
        }

        public State CurrentState { get; private set; }

        public bool IsInFinalState
        {
            get
            {
                return this.CurrentState.IsFinalState;
            }
        }

        private void SetCurrentState(State newState)
        {
            if (this.CurrentState != null && this.CurrentState.IsFinalState)
            {
                throw new InvalidOperationException("state machine is already in a final state");
            }

            this.CurrentState = newState;
            this.CurrentState.StateMachine = this;
            OnNewState(this.CurrentState);
            this.CurrentState.OnEnteredState();

            if (!this.CurrentState.IsFinalState)
            {
                ProcessQueuedInput();
            }
        }

        public void QueueInput(object input)
        {
            this.inputQueue.Enqueue(input);
        }

        public void ProcessInput(object input)
        {
            if (this.processingInput)
            {
                throw new InvalidOperationException("already processing input");
            }

            if (this.CurrentState.IsFinalState)
            {
                throw new InvalidOperationException("state machine is already in a final state");
            }

            if (!this.inputAlphabet.Contains(input))
            {
                throw new ArgumentOutOfRangeException("must be contained in the input alphabet set", "input");
            }

            this.inputQueue.Enqueue(input);
            ProcessQueuedInput();
        }

        private void ProcessQueuedInput()
        {
            while (this.inputQueue.Count > 0)
            {
                object processMe = this.inputQueue.Dequeue();

                this.CurrentState.ProcessInput(processMe, out State newState);

                if (newState == CurrentState)
                {
                    throw new InvalidOperationException("must provide a clean, newly constructed state");
                }

                SetCurrentState(newState);
            }
        }

        public void Start()
        {
            if (this.CurrentState != null)
            {
                throw new InvalidOperationException("may only call Start() once after construction");
            }

            SetCurrentState(this.initialState);
        }

        public StateMachine(State initialState, IEnumerable inputAlphabet)
        {
            this.initialState = initialState;

            this.inputAlphabet = new ArrayList();

            foreach (object o in inputAlphabet)
            {
                this.inputAlphabet.Add(o);
            }
        }
    }
}
