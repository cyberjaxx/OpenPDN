using PaintDotNet;
using System.Drawing;

namespace PdnBench
{
    public class ZoomOutBlitBenchmark
        : Benchmark
    {
        public const int IterationCount = 1000;

        private Surface Source { get; }
        private Surface Dest { get; }
        private Size BlitSize { get; }
        private Rectangle[] BlitRects { get; set; }
        private Surface[] BlitWindows { get; set; }
        private PaintDotNet.Threading.ThreadPool ThreadPool { get; set; }

        protected override void OnBeforeExecute()
        {
            Rectangle blitRect = new Rectangle(0, 0, BlitSize.Width, BlitSize.Height);

            BlitRects = new Rectangle[PaintDotNet.SystemLayer.Processor.LogicalCpuCount];
            Utility.SplitRectangle(blitRect, BlitRects);

            BlitWindows = new Surface[BlitRects.Length];
            for (int i = 0; i < BlitRects.Length; ++i)
            {
                BlitWindows[i] = Dest.CreateWindow(BlitRects[i]);
            }

            ThreadPool = new PaintDotNet.Threading.ThreadPool();

            base.OnBeforeExecute();
        }

        private void Render(object indexObj)
        {
            int index = (int)indexObj;
            SurfaceBoxBaseRenderer.RenderZoomOutRotatedGridMultisampling(BlitWindows[index], Source,
                BlitRects[index].Location, BlitSize);
        }

        protected override void OnExecute()
        {
            System.Threading.WaitCallback renderDelegate = new System.Threading.WaitCallback(Render);

            for (int i = 0; i < IterationCount; ++i)
            {
                for (int j = 0; j < BlitRects.Length; ++j)
                {
                    object jObj = BoxedConstants.GetInt32(j);
                    ThreadPool.QueueUserWorkItem(renderDelegate, jObj);
                }

                ThreadPool.Drain();
            }
        }

        protected override void OnAfterExecute()
        {
            for (int i = 0; i < BlitWindows.Length; ++i)
            {
                BlitWindows[i].Dispose();
                BlitWindows[i] = null;
            }

            ThreadPool = null;
            base.OnAfterExecute();
        }

        public ZoomOutBlitBenchmark(string name, Surface source, Surface dst, Size blitSize)
            : base(name)
        {
            Source = source;
            Dest = dst;
            BlitSize = blitSize;
        }
    }
}
