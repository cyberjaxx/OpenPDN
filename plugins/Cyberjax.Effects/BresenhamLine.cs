using System;
using System.Drawing;

namespace Cyberjax
{
    public class BresenhamLine
    {
        public delegate void PlotFunction(int x, int y);

        // algorithm for positive or negative low slope lines
        private static void PlotLowSlope(Point pt1, Point pt2, PlotFunction plotFunction)
        {
            int dx = pt2.X - pt1.X;
            int dy = pt2.Y - pt1.Y;
            int yi = 1;
            int xi = 1;
            if (dx < 0)
            {
                xi = -1;
                dx = -dx;
            }
            if (dy < 0)
            {
                yi = -1;
                dy = -dy;
            }

            int D = 2 * dy - dx;
            int y = pt1.Y;

            for (int x = pt1.X; xi < 0 ? x >= pt2.X : x <= pt2.X; x += xi)
            {
                plotFunction(x, y);

                if (D > 0)
                {
                    y += yi;
                    D -= 2 * dx;
                }
                D += 2 * dy;
            }
        }

        // algorithm for positive or negative steep slope lines (switched the x and y axis)
        private static void PlotHighSlope(Point pt1, Point pt2, PlotFunction plotFunction)
        {
            int dx = pt2.X - pt1.X;
            int dy = pt2.Y - pt1.Y;
            int xi = 1;
            int yi = 1;
            if (dx < 0)
            {
                xi = -1;
                dx = -dx;
            }
            if (dy < 0)
            {
                yi = -1;
                dy = -dy;
            }
            int D = 2 * dx - dy;
            int x = pt1.X;

            for (int y = pt1.Y; yi < 0 ? y >= pt2.Y : y <= pt2.Y; y += yi)
            {
                plotFunction(x, y);

                if (D > 0)
                {
                    x += xi;
                    D -= 2 * dy;
                }
                D += 2 * dx;
            }
        }

        // Detect whether x1 > x0 or y1 > y0 and reverse the input coordinates, if needed, before drawing
        public static void Plot(Point pt1, Point pt2, PlotFunction plotFunction)
        {
            if (Math.Abs(pt2.Y - pt1.Y) < Math.Abs(pt2.X - pt1.X))
            {
                PlotLowSlope(pt1, pt2, plotFunction);
            }
            else
            {
                PlotHighSlope(pt1, pt2, plotFunction);
            }
        }
    }
}
