using PaintDotNet;
using PaintDotNet.SystemLayer;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading;

namespace PdnBench
{
    public class GradientBenchmark
        : Benchmark
    {
        private Surface Surface { get; }
        private GradientRenderer Renderer { get; }
        private PaintDotNet.Threading.ThreadPool ThreadPool { get; set; }
        private Rectangle[] Rois { get; set; }

        public int Iterations { get; }

        private void RenderThreadProc(object indexObj)
        {
            int index = (int)indexObj;
            Renderer.Render(Surface, Rois, index, 1);
        }

        protected override void OnBeforeExecute()
        {
            Renderer.StartColor = ColorBgra.Black;
            Renderer.EndColor = ColorBgra.FromBgra(255, 128, 64, 64);
            Renderer.StartPoint = new PointF(Surface.Width / 2, Surface.Height / 2);
            Renderer.EndPoint = new PointF(0, 0);
            Renderer.AlphaBlending = true;
            Renderer.AlphaOnly = false;

            Renderer.BeforeRender();

            Rois = new Rectangle[Processor.LogicalCpuCount];
            Utility.SplitRectangle(Surface.Bounds, Rois);

            ThreadPool = new PaintDotNet.Threading.ThreadPool(Processor.LogicalCpuCount, false);

            base.OnBeforeExecute();
        }

        protected override void OnExecute()
        {
            WaitCallback wc = new WaitCallback(RenderThreadProc);

            for (int n = 0; n < Iterations; ++n)
            {
                for (int i = 0; i < Rois.Length; ++i)
                {
                    object iObj = BoxedConstants.GetInt32(i);
                    ThreadPool.QueueUserWorkItem(wc, iObj);
                }
            }

            ThreadPool.Drain();
        }

        protected override void OnAfterExecute()
        {
            Renderer.AfterRender();
            ThreadPool = null;
            base.OnAfterExecute();
        }

        public GradientBenchmark(string name, Surface surface, GradientRenderer renderer, int iterations)
            : base(name)
        {
            Surface = surface;
            Renderer = renderer;
            Iterations = iterations;
        }
    }
}
