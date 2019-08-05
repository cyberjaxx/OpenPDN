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
using System.Collections;
using System.Threading;

namespace PaintDotNet.Threading
{
    /// <summary>
    /// Uses the .NET ThreadPool to do our own type of thread pool. The main difference
    /// here is that we limit our usage of the thread pool, and that we can also drain
    /// the threads we have ("fence"). The default maximum number of threads is
    /// Processor.LogicalCpuCount.
    /// </summary>
    public class ThreadPool
    {
        public static ThreadPool Global { get; } = new ThreadPool(2 * Processor.LogicalCpuCount);

        private ArrayList ExceptionList { get; } = ArrayList.Synchronized(new ArrayList());

        private bool UseFXThreadPool { get; }

        public static int MinimumCount => WaitableCounter.MinimumCount;

        public static int MaximumCount => WaitableCounter.MaximumCount;

        public Exception[] Exceptions => (Exception[])ExceptionList.ToArray(typeof(Exception));

        public void ClearExceptions()
        {
            ExceptionList.Clear();
        }

        public void DrainExceptions()
        {
            if (ExceptionList.Count > 0)
            {
                throw new WorkerThreadException("Worker thread threw an exception", (Exception)ExceptionList[0]);
            }

            ClearExceptions();
        }

        private WaitableCounter Counter { get; }

        public ThreadPool()
            : this(Processor.LogicalCpuCount)
        {
        }

        public ThreadPool(int maxThreads)
            : this(maxThreads, true)
        {
        }

        public ThreadPool(int maxThreads, bool useFXThreadPool)
        {
            if (maxThreads < MinimumCount || maxThreads > MaximumCount)
            {
                throw new ArgumentOutOfRangeException("maxThreads", "must be between " + MinimumCount.ToString() + " and " + MaximumCount.ToString() + " inclusive");
            }

            Counter = new WaitableCounter(maxThreads);
            UseFXThreadPool = useFXThreadPool;
        }

        /*
        private sealed class FunctionCallTrampoline
        {
            private Delegate theDelegate;
            private object[] parameters;

            public void WaitCallback(object ignored)
            {
                theDelegate.DynamicInvoke(this.parameters);
            }

            public FunctionCallTrampoline(Delegate theDelegate, object[] parameters)
            {
                this.theDelegate = theDelegate;
                this.parameters = parameters;
            }
        }

        public void QueueFunctionCall(Delegate theDelegate, params object[] parameters)
        {
            FunctionCallTrampoline fct = new FunctionCallTrampoline(theDelegate, parameters);
            QueueUserWorkItem(fct.WaitCallback, null);
        }           
        */

        public void QueueUserWorkItem(WaitCallback callback)
        {
            QueueUserWorkItem(callback, null);
        }

        public void QueueUserWorkItem(WaitCallback callback, object state)
        {
            IDisposable token = Counter.AcquireToken();
            ThreadWrapperContext twc = new ThreadWrapperContext(callback, state, token, ExceptionList);

            if (UseFXThreadPool)
            {
                System.Threading.ThreadPool.QueueUserWorkItem(new WaitCallback(twc.ThreadWrapper), twc);
            }
            else
            {
                Thread thread = new Thread(new ThreadStart(twc.ThreadWrapper));
                thread.IsBackground = true;
                thread.Start();
            }
        }

        public bool IsDrained(uint msTimeout)
        {
            bool result = Counter.IsEmpty(msTimeout);

            if (result)
            {
                Drain();
            }

            return result;
        }

        public bool IsDrained()
        {
            return IsDrained(0);
        }

        public void Drain()
        {
            Counter.WaitForEmpty();
            DrainExceptions();
        }

        private sealed class ThreadWrapperContext
        {
            private WaitCallback Callback { get; }
            private object Context { get; }
            private IDisposable CounterToken { get; }
            private ArrayList ExceptionsBucket { get; }

            public ThreadWrapperContext(WaitCallback callback, object context, 
                IDisposable counterToken, ArrayList exceptionsBucket)
            {
                Callback = callback;
                Context = context;
                CounterToken = counterToken;
                ExceptionsBucket = exceptionsBucket;
            }

            public void ThreadWrapper()
            {
                using (IDisposable token = CounterToken)
                {
                    //try
                    {
                        Callback(Context);
                    }

                    //catch (Exception ex)
                    {
                       // ExceptionsBucket.Add(ex);
                    }
                }
            }

            public void ThreadWrapper(object state)
            {
                ThreadWrapper();
            }
        }
    }
}
