using System;

namespace Cyberjax
{
    public static class XiaolinWuLine
    {
        // integer part of x
        private static int IntegerOf(float x)
        {
            return (int)Math.Floor(x);
        }

        // integer part of x
        private static int Round(float x)
        {
            return IntegerOf(x + 0.5f);
        }

        // fractional part of x
        private static float FractionOf(float x)
        {
            return (float)(x - Math.Floor(x));
        }

        private static float RFractionOf(float x)
        {
            return 1 - FractionOf(x);
        }

        public static void DrawLine(float x0, float y0, float x1, float y1, bool aliasEndPoint, PlotFunction plot)
        {
            bool steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);

            if (steep)
            {
                // swap(x0, y0);
                float temp = x0;
                x0 = y0;
                y0 = temp;

                // swap(x1, y1);
                temp = x1;
                x1 = y1;
                y1 = temp;
            }
            if (x0 > x1)
            {
                // swap(x0, x1);
                float temp = x0;
                x0 = x1;
                x1 = temp;

                // swap(y0, y1);
                temp = y0;
                y0 = y1;
                y1 = temp;
            }

            float dx = x1 - x0;
            float dy = y1 - y0;
            float gradient = (dx == 0.0f) ? 1.0f : dy / dx;

            // handle first endpoint
            int xend = Round(x0);
            int yend = (int)(y0 + gradient * (xend - x0));
            float xgap = RFractionOf(x0 + 0.5f);
            int xpxl1 = xend;   // this will be used in the main loop
            int ypxl1 = IntegerOf(yend);

            if (steep)
            {
                plot(ypxl1, xpxl1, RFractionOf(yend) * xgap);
                plot(ypxl1 + 1, xpxl1, FractionOf(yend) * xgap);
            }
            else
            {
                plot(xpxl1, ypxl1, RFractionOf(yend) * xgap);
                plot(xpxl1, ypxl1 + 1, FractionOf(yend) * xgap);
            }
            float intery = yend + gradient; // first y-intersection for the main loop

            // handle second endpoint
            xend = Round(x1);
            yend = (int)(y1 + gradient * (xend - x1));
            xgap = FractionOf(x1 + 0.5f);
            int xpxl2 = xend;   //this will be used in the main loop
            int ypxl2 = IntegerOf(yend);
            if (steep)
            {
                plot(ypxl2, xpxl2, RFractionOf(yend) * xgap);
                if (aliasEndPoint)
                {
                    plot(ypxl2 + 1, xpxl2, FractionOf(yend) * xgap);
                }
            }
            else
            {
                plot(xpxl2, ypxl2, RFractionOf(yend) * xgap);
                if (aliasEndPoint)
                {
                    plot(xpxl2, ypxl2 + 1, FractionOf(yend) * xgap);
                }
            }

            // main loop
            if (steep)
            {
                for (int x = xpxl1 + 1; x < xpxl2; ++x)
                {
                    plot(IntegerOf(intery), x, RFractionOf(intery));
                    plot(IntegerOf(intery) + 1, x, FractionOf(intery));
                    intery = intery + gradient;
                }
            }
            else
            {
                for (int x = xpxl1 + 1; x < xpxl2; ++x)
                {
                    plot(x, IntegerOf(intery), RFractionOf(intery));
                    plot(x, IntegerOf(intery) + 1, FractionOf(intery));
                    intery = intery + gradient;
                }
            }
        }
    }
}
