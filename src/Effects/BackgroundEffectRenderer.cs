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
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Threading;

namespace PaintDotNet.Effects
{
    /// <summary>
    /// This class can be used to apply an effect using background worker threads
    /// which raise an event when a certain amount of the effect has been processed.
    /// You can use that event to update a status bar, display a preview of the
    /// rendering so far, or whatever.
    /// 
    /// Since two threads are used for rendering, this will improve performance on
    /// dual processor systems, and possibly on systems that have HyperThreading.
    /// 
    /// This class is NOT SAFE for multithreaded access. Note that the events will 
    /// be raised from arbitrary threads. The only method that is safe to call from
    /// a thread that is not managing Start(), Abort(), and Join() is AbortAsync().
    /// You may then query whether the rendering actually aborted by using the Abort
    /// property. If it returns false, then AbortAsync() was not called in time to
    /// abort anything, which means the rendering completed fully.
    /// </summary>
    public sealed class BackgroundEffectRenderer
        : IDisposable
    {
        private Effect Effect { get; }
        // EffectToken this references the original token that is passed in to the constructor
        private EffectConfigToken EffectToken { get; }
        // EffectTokenCopy: a copy of the token is updated every time you call Start()
        // to make sure it is up to date. This is then passed to the threads,
        // not the original one.
        private EffectConfigToken EffectTokenCopy { get; set; }                                                
        private PdnRegion RenderRegion { get; }
        private Rectangle[][] TileRegions { get; }
        private PdnRegion[] TilePdnRegions { get; }
        private int TileCount { get; }
        private Threading.ThreadPool ThreadPool { get; }
        private RenderArgs DstArgs { get; set; }
        private RenderArgs SrcArgs { get; set; }
        private int WorkerThreads { get; }
        private ArrayList Exceptions { get; } = ArrayList.Synchronized(new ArrayList());
        public volatile bool aborted = false;
        public bool Aborted
        {
            get => aborted;
            private set => aborted = value;
        }

        private CancellationTokenSource TokenSource { get; set; } = null;

        public event RenderedTileEventHandler RenderedTile;
        private void OnRenderedTile(RenderedTileEventArgs e)
        {
            RenderedTile?.Invoke(this, e);
        }

        public event EventHandler FinishedRendering;
        private void OnFinishedRendering()
        {
            FinishedRendering?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler StartingRendering;
        private void OnStartingRendering()
        {
            StartingRendering?.Invoke(this, EventArgs.Empty);
        }

        private sealed class RendererContext
        {
            private BackgroundEffectRenderer BackRenderer { get; }
            private EffectConfigToken Token { get; }
            private int ThreadNumber { get; }
            private int StartOffset { get; }

            public RendererContext(BackgroundEffectRenderer ber, EffectConfigToken token, int threadNumber)
                : this(ber, token, threadNumber, 0)
            {
            }

            public RendererContext(BackgroundEffectRenderer ber, EffectConfigToken token, int threadNumber, int startOffset)
            {
                BackRenderer = ber;
                Token = token;
                ThreadNumber = threadNumber;
                StartOffset = startOffset;
            }

            public void Renderer2(object ignored)
            {
                Renderer();
            }

            public void Renderer()
            {
                //using (new ThreadBackground(ThreadBackgroundFlags.Cpu))
                {
                    RenderImpl();
                }
            }

            private void RenderImpl()
            {
                int threadCount = BackRenderer.WorkerThreads;
                int start = ThreadNumber + (StartOffset * threadCount);
                int max = BackRenderer.TileCount;

                //try
                {
                    for (int tile = start; tile < max; tile += threadCount)
                    {
                        if (BackRenderer.ThreadShouldStop)
                        {
                            BackRenderer.Aborted = true;
                            break;
                        }

                        Rectangle[] subRegion = BackRenderer.TileRegions[tile];
                        BackRenderer.Effect.Render(Token, BackRenderer.DstArgs, BackRenderer.SrcArgs, subRegion);
                        PdnRegion subPdnRegion = BackRenderer.TilePdnRegions[tile];
                        BackRenderer.OnRenderedTile(new RenderedTileEventArgs(subPdnRegion, BackRenderer.TileCount, tile));
                    }
                }

                //catch (Exception ex)
                {
                    //BackRenderer.Exceptions.Add(ex);
                }
            }
        }

        public void ThreadFunction()
        {
            if (SrcArgs.Surface.Scan0.MaySetAllowWrites)
            {
                SrcArgs.Surface.Scan0.AllowWrites = false;
            }

            //try
            {
                if (Effect.CheckForEffectFlags(EffectFlags.Cancellable))
                {
                    TokenSource = new CancellationTokenSource();
                    Effect.CancelToken = TokenSource.Token;
                }

                Effect.SetRenderInfo(EffectTokenCopy, DstArgs, SrcArgs);

                if (TileCount > 0)
                {
                    Rectangle[] subRegion = TileRegions[0];

                    Effect.Render(EffectTokenCopy, DstArgs, SrcArgs, subRegion);

                    PdnRegion subPdnRegion = TilePdnRegions[0];
                    OnRenderedTile(new RenderedTileEventArgs(subPdnRegion, TileCount, 0));
                }

                bool stopping = false;
                for (int i = 0; i < WorkerThreads; ++i)
                {
                    if (ThreadShouldStop)
                    {
                        stopping = true;
                        break;
                    }

                    EffectConfigToken token = (EffectTokenCopy == null) ? null :
                        (EffectConfigToken)EffectTokenCopy.Clone();

                    RendererContext rc = new RendererContext(this, token, i, (i == 0) ? 1 : 0);
                    ThreadPool.QueueUserWorkItem(new WaitCallback(rc.Renderer2));
                }

                if (!stopping)
                {
                    ThreadPool.Drain();
                    OnFinishedRendering();
                }
            }

            //catch (Exception ex)
            //{
            //    Exceptions.Add(ex);
            //}

            //finally
            {
                ThreadPool.Drain();

                Exception[] newExceptions = ThreadPool.Exceptions;

                foreach (Exception exception in newExceptions)
                {
                    Exceptions.Add(exception);
                }

                if (SrcArgs.Surface.Scan0.MaySetAllowWrites)
                {
                    SrcArgs.Surface.Scan0.AllowWrites = true;
                }
            }
        }

        private volatile bool ThreadShouldStop = false;

        private Thread ThisThread { get; set; } = null;

        public void Start()
        {
            Abort();
            Aborted = false;

            if (EffectToken != null)
            {
                EffectTokenCopy = (EffectConfigToken)EffectToken.Clone();
            }

            ThreadShouldStop = false;
            OnStartingRendering();
            ThisThread = new Thread(new ThreadStart(ThreadFunction));
            ThisThread.Start();
        }

        public void Abort()
        {
            if (ThisThread != null)
            {
                ThreadShouldStop = true;

                TokenSource?.Cancel();

                Join();
                ThreadPool.Drain();

                TokenSource?.Dispose();
                TokenSource = null;
            }
        }

        // This is the only method that is safe to call from another thread
        // If the abort was successful, then get_Aborted will return true
        // after a Join().
        public void AbortAsync()
        {
            ThreadShouldStop = true;
        }

        /// <summary>
        /// Used to determine whether the rendering fully completed or not, and was not
        /// aborted in any way. You can use this method to sleep until the rendering
        /// finishes. Once this is set to the signaled state you should check the IsDone
        /// property to make sure that the rendering was actually finished, and not
        /// aborted.
        /// </summary>
        public void Join()
        {
            ThisThread.Join();

            if (Exceptions.Count > 0)
            {
                Exception throwMe = (Exception)Exceptions[0];
                Exceptions.Clear();
                if (!(throwMe.InnerException is OperationCanceledException))
                {
                    throw new WorkerThreadException("Worker thread threw an exception", throwMe);
                }
            }
        }

        private Rectangle[] ConsolidateRects(Rectangle[] scanRectangles)
        {
            if (scanRectangles.Length == 0)
            {
                return scanRectangles;
            }

            List<Rectangle> consolidatedRects = new List<Rectangle>();
            int currentIndex = 0;
            consolidatedRects.Add(scanRectangles[0]);

            for (int i = 1; i < scanRectangles.Length; ++i)
            {
                if (scanRectangles[i].Left == consolidatedRects[currentIndex].Left &&
                    scanRectangles[i].Right == consolidatedRects[currentIndex].Right &&
                    scanRectangles[i].Top == consolidatedRects[currentIndex].Bottom)
                {
                    Rectangle currentConsolidatedRect = consolidatedRects[currentIndex];
                    currentConsolidatedRect.Height = scanRectangles[i].Bottom - consolidatedRects[currentIndex].Top;
                    consolidatedRects[currentIndex] = currentConsolidatedRect;
                }
                else
                {
                    consolidatedRects.Add(scanRectangles[i]);
                    currentIndex = consolidatedRects.Count - 1; 
                }
            }

            return consolidatedRects.ToArray();
        }

        private Rectangle[][] SliceUpRegion(PdnRegion region, int sliceCount, Rectangle layerBounds)
        {
            Rectangle[][] slices = new Rectangle[sliceCount][];
            Rectangle[] regionRects = region.GetRegionScansReadOnlyInt();
            Scanline[] regionScans = Utility.GetRegionScans(regionRects);

            for (int i = 0; i < sliceCount; ++i)
            {
                int beginScan = (regionScans.Length * i) / sliceCount;
                int endScan = Math.Min(regionScans.Length, (regionScans.Length * (i + 1)) / sliceCount);

                // Try to arrange it such that the maximum size of the first region
                // is 1-pixel tall
                if (i == 0 && sliceCount > 1)
                {
                    endScan = Math.Min(endScan, beginScan + 1);
                }
                else if (i == 1)
                {
                    beginScan = Math.Min(beginScan, 1);
                }

                Rectangle[] newRects = Utility.ScanlinesToRectangles(regionScans, beginScan, endScan - beginScan);
                         
                for (int j = 0; j < newRects.Length; ++j)
                {
                    newRects[j].Intersect(layerBounds);
                }

                slices[i] = ConsolidateRects(newRects);
            }

            return slices;
        }

        public BackgroundEffectRenderer(Effect effect,
                                        EffectConfigToken effectToken,
                                        RenderArgs dstArgs,
                                        RenderArgs srcArgs,
                                        PdnRegion renderRegion,
                                        int tileCount,
                                        int workerThreads)
        {
            Effect = effect;
            EffectToken = effectToken;
            DstArgs = dstArgs;
            SrcArgs = srcArgs;
            RenderRegion = renderRegion;
            RenderRegion.Intersect(dstArgs.Bounds);

            TileRegions = SliceUpRegion(renderRegion, tileCount, dstArgs.Bounds);

            TilePdnRegions = new PdnRegion[TileRegions.Length];
            for (int i = 0; i < TileRegions.Length; ++i)
            {
                PdnRegion pdnRegion = Utility.RectanglesToRegion(TileRegions[i]);
                TilePdnRegions[i] = pdnRegion;
            }

            TileCount = tileCount;
            WorkerThreads = workerThreads;

            if (effect.CheckForEffectFlags(EffectFlags.SingleThreaded))
            {
                WorkerThreads = 1;
            }

            ThreadPool = new Threading.ThreadPool(WorkerThreads, false);
        }

        ~BackgroundEffectRenderer()
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
                SrcArgs?.Dispose();
                SrcArgs = null;

                DstArgs?.Dispose();
                DstArgs = null;

                TokenSource?.Dispose();
                TokenSource = null;
            }
        }
    }
}
