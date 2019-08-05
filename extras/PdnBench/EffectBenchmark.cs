using PaintDotNet;
using PaintDotNet.Effects;
using PaintDotNet.SystemLayer;

namespace PdnBench
{
	/// <summary>
	/// Summary description for EffectBenchmark.
	/// </summary>
	public class EffectBenchmark
        : Benchmark
	{
        private Effect Effect { get; }
        private EffectConfigToken Token { get; }
        private Surface Image { get; }
        private Surface Dest { get; set; }
        private PdnRegion Region { get; set; }
        public int Iterations { get; }

        protected override void OnBeforeExecute()
        {
            Dest = Image.Clone();
            Region = new PdnRegion(Dest.Bounds);
        }

        protected sealed override void OnExecute()
        {
            for (int i = 0; i < this.Iterations; ++i)
            {
                EffectConfigToken localToken = Token == null ? null : (EffectConfigToken)Token.Clone();
                RenderArgs srcArgs = new RenderArgs(Image);
                RenderArgs dstArgs = new RenderArgs(Dest);

                using (BackgroundEffectRenderer ber = new BackgroundEffectRenderer(
                    Effect, localToken, dstArgs, srcArgs, Region,
                    25 * Processor.LogicalCpuCount, Processor.LogicalCpuCount))
                {
                    ber.Start();
                    ber.Join();
                }
            }
        }

        protected override void OnAfterExecute()
        {
            Region.Dispose();
            Dest.Dispose();
        }

        public EffectBenchmark(string name, int iterations, Effect effect, EffectConfigToken token, Surface image)
            : base(name + " (" + iterations + "x)")
		{
            Effect = effect;
            Token = token;
            Image = image;
            Iterations = iterations;
		}
	}
}
