/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using PaintDotNet;
using PaintDotNet.Effects;

namespace Cyberjax
{
    public class ControlPoints : ICloneable
    {
        public int MaxKey { get; set; }
        public int MaxValue { get; set; }
        public ControlPointList[] List { get; set; }

        public ControlPoints() { }

        public object Clone()
        {
            return new ControlPoints(this);
        }

        protected ControlPoints(ControlPoints copyMe)
        {
            MaxKey = copyMe.MaxKey;
            MaxValue = copyMe.MaxValue;
            List = (ControlPointList[])copyMe.List.Clone();
        }
    }

    public class DistortionEffectConfigToken
        : EffectConfigToken
    {
        public static int CPMaxKey { get; set; } = 500;
        public static int CPMaxValue { get; set; }  = 500;

        public double ValueX { get; set; }
        public double ValueY { get; set; }
        public double ValueR { get; set; }
        public ControlPoints ControlPoints { get; set; }

        public double[][] GetTransferCurves(int[] lengths, float[] maxValues)
        {
            Debug.Assert(lengths.Length == maxValues.Length);

            int channels = lengths.Length;
            double[][] xferCurves = new double[channels][];
 
            for (int channel = 0; channel < channels; ++channel)
            {
                BezierInterpolator interpolator = new BezierInterpolator(ControlPoints.List[channel]);

                int length = lengths[channel];
                float maxValue = maxValues[channel];
                xferCurves[channel] = new double[length];
                for (int i = 0; i < length; ++i)
                {
                    xferCurves[channel][i] = interpolator.Interpolate((float)i * ControlPoints.MaxKey / length) * maxValue / ControlPoints.MaxValue;
                }
            }
            return xferCurves;
        }

        public override object Clone()
        {
            return new DistortionEffectConfigToken(this);
        }

        public DistortionEffectConfigToken()
        {
            ValueX = 0;
            ValueY = 0;
            ValueR = 1;

            ControlPoints = new ControlPoints()
            {
                MaxKey = CPMaxKey,
                MaxValue = CPMaxValue,
                List = new ControlPointList[]
                {
                    new ControlPointList
                    {
                        new Point(0, 0),
                        new Point(CPMaxKey / 4, CPMaxValue / 4),
                        new Point(CPMaxKey * 3 / 4, CPMaxValue * 3/ 4),
                        new Point(CPMaxKey, CPMaxValue)
                    },
                    new ControlPointList
                    {
                        new Point(0, 0),
                        new Point(CPMaxKey / 4, CPMaxValue / 4),
                        new Point(CPMaxKey * 3 / 4, CPMaxValue * 3/ 4),
                        new Point(CPMaxKey, CPMaxValue)
                    }
                }
            };
        }

        protected DistortionEffectConfigToken(DistortionEffectConfigToken copyMe)
            : base(copyMe)
        {
            ValueX = copyMe.ValueX;
            ValueY = copyMe.ValueY;
            ValueR = copyMe.ValueR;
            ControlPoints = (ControlPoints)copyMe.ControlPoints.Clone();
        }
    }
}

