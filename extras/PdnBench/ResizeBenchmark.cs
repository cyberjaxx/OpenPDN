using PaintDotNet;
using PaintDotNet.SystemLayer;
using PaintDotNet.Threading;
using System;
using System.Drawing;
using System.Threading;


namespace PdnBench
{
	/// <summary>
	/// Summary description for ResizeBenchmark.
	/// </summary>
	public class ResizeBenchmark
		: Benchmark
	{
        private Surface Source { get; }
        private Surface Dest { get; }
        private PaintDotNet.Threading.ThreadPool ThreadPool { get; set; }
        private Rectangle[] Rects { get; set; }

        private sealed class FitSurfaceContext
        {
            public Surface DstSurface { get; }
            public Surface SrcSurface { get; }
            public Rectangle[] DstRois { get; }
            public ResamplingAlgorithm Algorithm { get; }

            public event Procedure RenderedRect;

            private void OnRenderedRect()
            {
                RenderedRect?.Invoke();
            }

            public void FitSurface(object context)
            {
                int index = (int)context;
                DstSurface.FitSurface(Algorithm, SrcSurface, DstRois[index]);
            }

            public FitSurfaceContext(Surface dstSurface, Surface srcSurface, Rectangle[] dstRois, ResamplingAlgorithm algorithm)
            {
                DstSurface = dstSurface;
                SrcSurface = srcSurface;
                DstRois = dstRois;
                Algorithm = algorithm;
            }
        }

		protected override void OnBeforeExecute()
		{
            ThreadPool = new PaintDotNet.Threading.ThreadPool();
            Rects = new Rectangle[Processor.LogicalCpuCount];
            Utility.SplitRectangle(Dest.Bounds, Rects);
            base.OnBeforeExecute();
		}

		protected override void OnExecute()
		{
            FitSurfaceContext fsc = new FitSurfaceContext(Dest, Source, Rects, ResamplingAlgorithm.Bicubic);
            for (int i = 0; i < Rects.Length; ++i)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(fsc.FitSurface), i);
            }

            ThreadPool.Drain();
		}

		protected override void OnAfterExecute()
		{
            ThreadPool = null;
			base.OnAfterExecute ();
		}

		public ResizeBenchmark(string name, Surface src, Surface dst)
			: base(name)
		{
            Source = src;
            Dest = dst;
		}
	}
}
