﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelTelecomNetConsole.Interference
{
    public class TraditionalInterferenceStrategy: AccurateMathematic, IInterferenceStrategy
    {
        private GivenData given;
        private List<List<double>> njus;

        public TraditionalInterferenceStrategy(GivenData given, List<List<double>> njus)
        {
            this.given = given;
            this.njus = njus;
        }

        /// <summary>
        /// The power of iterference noise
        /// </summary>
        /// <param name="p">signal power</param>
        /// <returns>noise power</returns>
        /// <remarks>Canonical name - <i>Interf</i></remarks>
        public double InterferationNoisePower(int p)
        {
            double sum = 0;
            for (int i = 0; i < given.FourierTransformBase; i++)
            {
                for (int j = 0; j < given.FourierTransformBase; j++)
                {
                    sum += njus[i][j] * Math.Cos(2 * PI * (p + given.FirstChannelNumber - 1) * (i - j) / given.FourierTransformBase);
                }
            }
            return sum;
        }
    }
}
