using PaintDotNet;
using System;
using System.Collections.Generic;
using System.Text;

namespace PdnBench
{
    public delegate void SetLayerInfoDelegate(int layerIndex, Layer layer);

    public class CompositionBenchmark
        : Benchmark
    {
        public const int Iterations = 30;

        private Document ComposeMe { get; }
        private Surface DstSurface { get; }
        private SetLayerInfoDelegate SliDelegate { get; }

        protected override void OnBeforeExecute()
        {
            for (int i = 0; i < ComposeMe.Layers.Count; ++i)
            {
                SliDelegate(i, (Layer)ComposeMe.Layers[i]);
            }

            base.OnBeforeExecute();
        }

        protected override void OnExecute()
        {
            for (int i = 0; i < Iterations; ++i)
            {
                ComposeMe.Invalidate();
                ComposeMe.Update(new RenderArgs(DstSurface));
            }
        }

        public CompositionBenchmark(string name, Document composeMe, 
            Surface dstSurface, SetLayerInfoDelegate sliDelegate)
            : base(name)
        {
            ComposeMe = composeMe;
            DstSurface = dstSurface;
            SliDelegate = sliDelegate;
        }
    }
}
