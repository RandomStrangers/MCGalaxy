//
// Copyright (c) 2013 Jason Bell
//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included
// in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
// OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
//
using System;
namespace MCGalaxy
{
    public sealed class RidgedMultifractal : IModule
    {
        public int OctaveCount;
        double mLacunarity;
        readonly double[] SpectralWeights = new double[30];
        public RidgedMultifractal()
        {
            Lacunarity = 2.0;
            OctaveCount = 6;
        }
        public override double GetValue(double x, double y, double z)
        {
            x *= Frequency;
            y *= Frequency;
            z *= Frequency;
            double value = 0.0, weight = 1.0,
                offset = 1.0, gain = 2.0;
            for (int octave = 0; octave < OctaveCount; octave++)
            {
                double signal = GradientNoise.GradientCoherentNoise(x, y, z, (Seed + octave) & 0x7fffffff);
                signal = Math.Abs(signal);
                signal = offset - signal;
                signal *= signal;
                signal *= weight;
                weight = signal * gain;
                if (weight > 1.0)
                    weight = 1.0;
                if (weight < 0.0)
                    weight = 0.0;
                value += signal * SpectralWeights[octave];
                x *= Lacunarity;
                y *= Lacunarity;
                z *= Lacunarity;
            }
            return (value * 1.25) - 1.0;
        }
        public double Lacunarity
        {
            get { return mLacunarity; }
            set
            {
                mLacunarity = value;
                CalculateSpectralWeights();
            }
        }
        void CalculateSpectralWeights()
        {
            double h = 1.0, frequency = 1.0;
            for (int i = 0; i < 30; i++)
            {
                SpectralWeights[i] = Math.Pow(frequency, -h);
                frequency *= mLacunarity;
            }
        }
    }
}
