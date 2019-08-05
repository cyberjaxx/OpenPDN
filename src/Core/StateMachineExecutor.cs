/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet.SystemLayer;
using System;
using System.ComponentModel;
using System.Threading;

namespace PaintDotNet
{
    public sealed class StateMachineExecutor
        : IDisposable
    {
        private bool Disposed = false;
        private Thread StateMachineThread;
        private Exception ThreadException;
        private StateMachine StateMachine { get; set; }
        private ManualResetEvent StateMachineInitialized = new ManualResetEvent(false);
        private ManualResetEvent StateMachineNotBusy = new ManualResetEvent(false); // non-signaled when busy, signaled when not busy
        private ManualResetEvent InputAvailable = new ManualResetEvent(false); // non-signaled when no input sent from main thread, signaled when there is input or an abort signal
        private volatile bool PleaseAbort = false;
        private object QueuedInput;

        public event EventHandler StateMachineBegin;
        private void OnStateMachineBegin()
        {
            if (SyncContext != null && SyncContext.InvokeRequired)
            {
                SyncContext.BeginInvoke(new Procedure(OnStateMachineBegin), null);
            }
            else
            {
                StateMachineBegin?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler<EventArgs<State>> StateBegin;
        private void OnStateBegin(State state)
        {
            if (SyncContext != null && SyncContext.InvokeRequired)
            {
                SyncContext.BeginInvoke(new Procedure<State>(OnStateBegin), new object[] { state });
            }
            else
            {
                StateBegin?.Invoke(this, new EventArgs<State>(state));
            }
        }

        public event ProgressEventHandler StateProgress;
        private void OnStateProgress(double percent)
        {
            if (SyncContext != null && SyncContext.InvokeRequired)
            {
                SyncContext.BeginInvoke(new Procedure<double>(OnStateProgress), new object[] { percent });
            }
            else
            {
                StateProgress?.Invoke(this, new ProgressEventArgs(percent));
            }
        }

        public event EventHandler<EventArgs<State>> StateWaitingForInput;
        private void OnStateWaitingForInput(State state)
        {
            if (SyncContext != null && SyncContext.InvokeRequired)
            {
                SyncContext.BeginInvoke(new Procedure<State>(OnStateWaitingForInput), new object[] { state });
            }
            else
            {
                StateWaitingForInput?.Invoke(this, new EventArgs<State>(state));
            }
        }

        public event EventHandler StateMachineFinished;
        private void OnStateMachineFinished()
        {
            if (SyncContext != null && SyncContext.InvokeRequired)
            {
                SyncContext.BeginInvoke(new Procedure(OnStateMachineFinished), null);
            }
            else
            {
                StateMachineFinished?.Invoke(this, EventArgs.Empty);
            }
        }

        public bool IsStarted { get; private set; } = false;

        private bool lowPriorityExecution = false;
        public bool LowPriorityExecution
        {
            get => lowPriorityExecution;

            set
            {
                if (IsStarted)
                {
                    throw new InvalidOperationException("Can only enable low priority execution before the state machine begins execution");
                }

                lowPriorityExecution = value;
            }
        }

        public ISynchronizeInvoke SyncContext { get; set; }

        public State CurrentState
        {
            get => StateMachine.CurrentState;
        }

        public bool IsInFinalState
        {
            get => StateMachine.IsInFinalState;
        }

        private void StateMachineThreadProc()
        {
            ThreadBackground tbm = null;

            try
            {
                if (LowPriorityExecution)
                {
                    tbm = new ThreadBackground(ThreadBackgroundFlags.Cpu);
                }

                StateMachineThreadImpl();
            }

            finally
            {
                tbm?.Dispose();
                tbm = null;
            }
        }

        private void StateMachineThreadImpl()
        {
            ThreadException = null;

            void NewStateHandler(object sender, EventArgs<State> e)
            {
                StateMachineInitialized.Set();
                OnStateBegin(e.Data);
            }

            void StateProgressHandler(object sender, ProgressEventArgs e)
            {
                OnStateProgress(e.Percent);
            }

            try
            {
                StateMachineNotBusy.Set();

                OnStateMachineBegin();

                StateMachineNotBusy.Reset();
                StateMachine.NewState += NewStateHandler;                
                StateMachine.StateProgress += StateProgressHandler;
                StateMachine.Start();

                while (true)
                {
                    StateMachineNotBusy.Set();
                    OnStateWaitingForInput(StateMachine.CurrentState);
                    InputAvailable.WaitOne();
                    InputAvailable.Reset();
                    // main thread should call Reset() on stateMachineNotBusy

                    if (PleaseAbort)
                    {
                        break;
                    }

                    StateMachine.ProcessInput(QueuedInput);

                    if (StateMachine.IsInFinalState)
                    {
                        break;
                    }
                }

                StateMachineNotBusy.Set();
            }

            catch (Exception ex)
            {
                ThreadException = ex;
            }

            finally
            {
                StateMachineNotBusy.Set();
                StateMachineInitialized.Set();
                StateMachine.NewState -= NewStateHandler;
                StateMachine.StateProgress -= StateProgressHandler;
                OnStateMachineFinished();
            }
        }

        public void Start()
        {
            if (IsStarted)
            {
                throw new InvalidOperationException("State machine thread is already executing");
            }

            IsStarted = true;

            StateMachineThread = new Thread(new ThreadStart(StateMachineThreadProc));
            StateMachineInitialized.Reset();
            StateMachineThread.Start();
            StateMachineInitialized.WaitOne();
        }

        public void ProcessInput(object input)
        {
            StateMachineNotBusy.WaitOne();
            StateMachineNotBusy.Reset();
            QueuedInput = input;
            InputAvailable.Set();
        }

        public void Abort()
        {
            if (Disposed)
            {
                return;
            }

            PleaseAbort = true;

            State currentState2 = StateMachine.CurrentState;
            if (currentState2 != null && currentState2.CanAbort)
            {
                StateMachine.CurrentState.Abort();
            }

            StateMachineNotBusy.WaitOne();
            InputAvailable.Set();
            StateMachineThread.Join();

            if (ThreadException != null)
            {
                throw new WorkerThreadException("State machine thread threw an exception", ThreadException);
            }
        }

        public StateMachineExecutor(StateMachine stateMachine)
        {
            StateMachine = stateMachine;
        }

        ~StateMachineExecutor()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                Abort();

                StateMachineInitialized?.Close();
                StateMachineInitialized = null;

                StateMachineNotBusy?.Close();
                StateMachineNotBusy = null;

                InputAvailable?.Close();
                InputAvailable = null;
            }

            Disposed = true;
        }
    }
}
