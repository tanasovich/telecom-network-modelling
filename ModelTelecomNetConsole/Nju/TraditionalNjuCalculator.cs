using ModelTelecomNetConsole;

namespace ModelTelecomNetConsole.Nju
{
    public class TraditionalNjuCalculator : AccurateMathematic, INjuCalculator
    {
        private readonly GivenData given;

        // TODO: Set logger
        public TraditionalNjuCalculator(GivenData given)
        {
            this.given = given;
        }

        /// <inheritdoc cref="INjuCalculator.Nju(int, int, int)"/>
        public double Nju(int k, int q, int currentSample)
        {
            double element = 0;

            if (FirstCondition(k, q, currentSample))
            {
                for (int i = k + currentSample + 1; i <= given.ImpulseReactionLength - 1; i++)
                {
                    for (int j = q + currentSample + 1; j <= given.ImpulseReactionLength - 1; j++)
                    {
                        element += FirstEquation(k, q, i, j);
                    }
                }
                return 2 * element;
            }
            else if (SecondCondition(k, q, currentSample))
            {
                for (int i = 0; i <= k + currentSample - given.FourierTransformBase - given.ProtectionIntervalSamplesNumber; i++)
                {
                    for (int j = 0; j <= q + currentSample - given.FourierTransformBase - given.ProtectionIntervalSamplesNumber; j++)
                    {
                        element += SecondEquation(k, q, i, j);
                    }
                }
                return 2 * element;
            }
            else if (ThirdCondition(k, q, currentSample))
            {
                for (int i = k + currentSample + 1; i <= given.ImpulseReactionLength - 1; i++)
                {
                    for (int j = 0; j <= q + currentSample - given.FourierTransformBase - given.ProtectionIntervalSamplesNumber; j++)
                    {
                        element += ThirdEquation(k, q, i, j);
                    }
                }
                return element;
            }
            else if (FourthCondition(k, q, currentSample))
            {
                for (int i = 0; i <= k + currentSample - given.FourierTransformBase - given.ProtectionIntervalSamplesNumber; i++)
                {
                    for (int j = q + currentSample + 1; j <= given.ImpulseReactionLength - 1; j++)
                    {
                        element += FourthEquation(k, q, i, j);
                    }
                }
                return element;
            }

            return 0;
        }

        private bool FirstCondition(int k, int q, int currentSample)
        {
            return LessThanSampleReactionDiff(k, currentSample) && LessThanSampleReactionDiff(q, currentSample);
        }

        private bool SecondCondition(int k, int q, int currentSample)
        {
            return NotLessThanFourierWithSamplesDiff(k, currentSample) && NotLessThanFourierWithSamplesDiff(q, currentSample);
        }

        private bool ThirdCondition(int k, int q, int currentSample)
        {
            return LessThanSampleReactionDiff(k, currentSample) && NotLessThanFourierWithSamplesDiff(q, currentSample);
        }

        private bool FourthCondition(int k, int q, int currentSample)
        {
            return NotLessThanFourierWithSamplesDiff(k, currentSample) && LessThanSampleReactionDiff(q, currentSample);
        }

        private bool LessThanSampleReactionDiff(int x, int currentSample)
        {
            return x <= given.ImpulseReactionLength - 2 - currentSample;
        }

        private bool NotLessThanFourierWithSamplesDiff(int x, int currentSample)
        {
            return x >= given.FourierTransformBase + given.ProtectionIntervalSamplesNumber - currentSample;
        }

        private double FirstEquation(int k, int q, int i, int j)
        {
            return WeightedImpulseReactionProduct(i, j, k + j - q - i);
        }

        private double SecondEquation(int k, int q, int i, int j)
        {
            return WeightedImpulseReactionProduct(i, j, k + j - q - i);
        }

        private double ThirdEquation(int k, int q, int i, int j)
        {
            return WeightedImpulseReactionProduct(i, j, 2 * given.FourierTransformBase + given.ProtectionIntervalSamplesNumber + k + j - q - i);
        }

        private double FourthEquation(int k, int q, int i, int j)
        {
            return WeightedImpulseReactionProduct(i, j, 2 * given.FourierTransformBase + given.ProtectionIntervalSamplesNumber + q + i - k - j);
        }

        /// <summary>
        /// Weighted product of two impulse reactions
        /// </summary>
        /// <param name="i">first impulse reaction index</param>
        /// <param name="j">second impulse reaction index</param>
        /// <param name="sampleDifference">sample difference</param>
        /// <returns>weighted product</returns>
        private double WeightedImpulseReactionProduct(int i, int j, int sampleDifference)
        {
            return given.ImpulseReactions[i] * given.ImpulseReactions[j] * SignalCorrelation(sampleDifference);
        }

        /// <summary>
        /// Computes signal correlation
        /// </summary>
        /// <param name="sampleDifference">sample difference</param>
        /// <returns>signal correlation</returns>
        /// <remarks>Canonical name - <i>B</i></remarks>
        public double SignalCorrelation(int sampleDifference)
        {
            double sum = 0;
            for (int p = 1; p <= given.CarrierFrequencyMaxNumber; p++)
            {
                sum += given.SignalPowers[p + given.FirstChannelNumber - 1] * Math.Cos(PI * sampleDifference * (p + given.FirstChannelNumber - 1) / given.CarrierFrequencyMaxNumber);
            }
            return sum;
        }
    }
}
