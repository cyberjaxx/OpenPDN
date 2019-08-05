// Bezier algorithms adapted from BEZMATH.PS (1993)
// by Don Lancaster, SYNERGETICS Inc. 
// http://www.tinaja.com/text/bezmath.html

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;

namespace Cyberjax
{
    public sealed class BezierInterpolator
    {
        public PointF[] Points { get; set; }

        public BezierInterpolator(List<Point> points)
        {
            int count = points.Count;
            Points = new PointF[count];
            for (int i = 0; i < count; ++i)
            {
                Points[i] = points[i];
            }
        }

        public double Interpolate(double x)
        {
            int points = Points.Length;

            Debug.Assert(points >= 4);
            Debug.Assert(points % 3 ==  1);
            Debug.Assert(Points[0].IsEmpty);
            Debug.Assert(!Points[points - 1].IsEmpty);


            int index = 0;

            for (int i = 2; i < points; i += 3)
            {
                if (Points[i + 1].X >= x)    // check X against anchor
                {
                    index = i - 2;
                    break;
                }
            }

            if (Points[index] == Points[index + 1] ||
                Points[index + 2] == Points[index + 3] ||
                Points[index].X == Points[index + 1].X ||
                Points[index + 2].X == Points[index + 3].X)
            {
                return x;
            }

            float width = Points[index + 3].X - Points[index].X;
            float height = Points[index + 3].Y - Points[index].Y;

            return CubicBezier((float)((x - Points[index].X) / width),
                (Points[index + 1].X - Points[index].X) / width, (Points[index + 1].Y - Points[index].Y) / height,
                (Points[index + 2].X - Points[index].X) / width, (Points[index + 2].Y - Points[index].Y) / height) *
                height + Points[index].Y;
        }

        public PointF GetPointOnBezierCurve(float t)
        {
            float u = 1f - t;
            float t2 = t * t;
            float u2 = u * u;
            float u3 = u2 * u;
            float t3 = t2 * t;

            float x =
                (u3) * Points[0].X +
                (3f * u2 * t) * Points[1].X +
                (3f * u * t2) * Points[2].X +
                (t3) * Points[3].X;

            float y =
                (u3) * Points[0].Y +
                (3f * u2 * t) * Points[1].Y +
                (3f * u * t2) * Points[2].Y +
                (t3) * Points[3].Y;

            return new PointF(x, y);
        }

        public static float QuadraticBezier(float x, float a, float b)
        {
            // adapted from BEZMATH.PS (1993)
            // by Don Lancaster, SYNERGETICS Inc. 
            // http://www.tinaja.com/text/bezmath.html

            float epsilon = 0.00001f;
            a = Math.Max(0, Math.Min(1, a));
            b = Math.Max(0, Math.Min(1, b));
            if (a == 0.5)
            {
                a += epsilon;
            }

            // solve t from x (an inverse operation)
            float om2a = 1 - 2 * a;
            float t = (float)((Math.Sqrt(a * a + om2a * x) - a) / om2a);
            float y = (1 - 2 * b) * (t * t) + (2 * b) * t;
            return y;
        }

        public static float CubicBezier(float x, float a, float b, float c, float d)
        {
            // adapted from BEZMATH.PS (1993)
            // by Don Lancaster, SYNERGETICS Inc. 
            // http://www.tinaja.com/text/bezmath.html

            float y0a = 0.0f; // initial y
            float x0a = 0.0f; // initial x 
            float y1a = b;    // 1st influence y   
            float x1a = a;    // 1st influence x 
            float y2a = d;    // 2nd influence y
            float x2a = c;    // 2nd influence x
            float y3a = 1.0f; // final y 
            float x3a = 1.0f; // final x 

            float A = x3a - 3 * x2a + 3 * x1a - x0a;
            float B = 3 * x2a - 6 * x1a + 3 * x0a;
            float C = 3 * x1a - 3 * x0a;
            float D = x0a;

            float E = y3a - 3 * y2a + 3 * y1a - y0a;
            float F = 3 * y2a - 6 * y1a + 3 * y0a;
            float G = 3 * y1a - 3 * y0a;
            float H = y0a;

            // Solve for t given x (using Newton-Raphelson), then solve for y given t.
            // Assume for the first guess that t = x.
            float currentt = x;
            int nRefinementIterations = 5;
            for (int i = 0; i < nRefinementIterations; i++)
            {
                float currentx = XFromT(currentt, A, B, C, D);
                float currentslope = SlopeFromT(currentt, A, B, C);
                currentt -= (currentx - x) * (currentslope);
                currentt = Math.Max(0, Math.Min(1, currentt));
            }

            float y = YFromT(currentt, E, F, G, H);
            return y;
        }

        // Helper functions:
        private static float SlopeFromT(float t, float A, float B, float C)
        {
            float dtdx = 1.0f / (3.0f * A * t * t + 2.0f * B * t + C);
            return dtdx;
        }

        private static float XFromT(float t, float A, float B, float C, float D)
        {
            float x = A * (t * t * t) + B * (t * t) + C * t + D;
            return x;
        }

        private static float YFromT(float t, float E, float F, float G, float H)
        {
            float y = E * (t * t * t) + F * (t * t) + G * t + H;
            return y;
        }

        public static float CubicBezierNearlyThroughTwoPoints(float x, float a, float b, float c, float d)
        {
            float y = 0;
            float epsilon = 0.00001f;
            float min_param_a = 0.0f + epsilon;
            float max_param_a = 1.0f - epsilon;
            float min_param_b = 0.0f + epsilon;
            float max_param_b = 1.0f - epsilon;
            a = Math.Max(min_param_a, Math.Min(max_param_a, a));
            b = Math.Max(min_param_b, Math.Min(max_param_b, b));

            float x0 = 0;
            float y0 = 0;
            float x4 = a;
            float y4 = b;
            float x5 = c;
            float y5 = d;
            float x3 = 1;
            float y3 = 1;
            float x1, y1, x2, y2; // to be solved.

            // arbitrary but reasonable 
            // t-values for interior control points
            float t1 = 0.3f;
            float t2 = 0.7f;

            float B0t1 = B0(t1);
            float B1t1 = B1(t1);
            float B2t1 = B2(t1);
            float B3t1 = B3(t1);
            float B0t2 = B0(t2);
            float B1t2 = B1(t2);
            float B2t2 = B2(t2);
            float B3t2 = B3(t2);

            float ccx = x4 - x0 * B0t1 - x3 * B3t1;
            float ccy = y4 - y0 * B0t1 - y3 * B3t1;
            float ffx = x5 - x0 * B0t2 - x3 * B3t2;
            float ffy = y5 - y0 * B0t2 - y3 * B3t2;

            x2 = (ccx - (ffx * B1t1) / B1t2) / (B2t1 - (B1t1 * B2t2) / B1t2);
            y2 = (ccy - (ffy * B1t1) / B1t2) / (B2t1 - (B1t1 * B2t2) / B1t2);
            x1 = (ccx - x2 * B2t1) / B1t1;
            y1 = (ccy - y2 * B2t1) / B1t1;

            x1 = Math.Max(0 + epsilon, Math.Min(1 - epsilon, x1));
            x2 = Math.Max(0 + epsilon, Math.Min(1 - epsilon, x2));

            // Note that this function also requires cubicBezier()!
            y = CubicBezier(x, x1, y1, x2, y2);
            y = Math.Max(0, Math.Min(1, y));
            return y;
        }

        // Helper functions. 
        private static float B0(float t)
        {
            return (1 - t) * (1 - t) * (1 - t);
        }

        private static float B1(float t)
        {
            return 3 * t * (1 - t) * (1 - t);
        }

        private static float B2(float t)
        {
            return 3 * t * t * (1 - t);
        }

        private static float B3(float t)
        {
            return t * t * t;
        }

        private static float FindX(float t, float x0, float x1, float x2, float x3)
        {
            return x0 * B0(t) + x1 * B1(t) + x2 * B2(t) + x3 * B3(t);
        }

        private static float FindY(float t, float y0, float y1, float y2, float y3)
        {
            return y0 * B0(t) + y1 * B1(t) + y2 * B2(t) + y3 * B3(t);
        }
    }
}
