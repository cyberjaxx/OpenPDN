using PaintDotNet;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;

namespace PdnBench
{
    class TransformBenchmark
        : Benchmark
    {
        public const int Iterations = 30;
        private Surface Dest { get; }
        private MaskedSurface Source { get; }
        private Matrix Transform { get; }
        private bool HighQuality { get; }

        protected override void OnExecute()
        {
            for (int i = 0; i < Iterations; ++i)
            {
                Source.Draw(Dest, Transform, HighQuality ? ResamplingAlgorithm.Bilinear : ResamplingAlgorithm.NearestNeighbor);
            }
        }

        public TransformBenchmark(string name, Surface dst, MaskedSurface src, Matrix transform, bool highQuality)
            : base(name)
        {
            Dest = dst;
            Source = src;
            Transform = transform.Clone();
            HighQuality = highQuality;
        }
    }
}
