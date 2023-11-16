namespace TelecomNetModelling
{
    public class NjuCalculator
    {
        private readonly int fourierTransformBase;
        private readonly int impulseReactionLength;
        private readonly int protectionIntervalSamplesNumber;
        private readonly int carrierFrequencyMaxNumber;
        private readonly int firstChannelNumber;
        private readonly List<double> impulseReactions;
        private readonly List<double> signalPowers;

        // TODO: Set logger
        public NjuCalculator(
            int fourierTransformBase, int impulseReactionLength,
            int protectionIntervalSamplesNumber, int carrierFrequencyMaxNumber,
            int firstChannelNumber, List<double> impulseReactions,
            List<double> signalPowers)
        {
            this.fourierTransformBase = fourierTransformBase;
            this.impulseReactionLength = impulseReactionLength;
            this.protectionIntervalSamplesNumber = protectionIntervalSamplesNumber;
            this.carrierFrequencyMaxNumber = carrierFrequencyMaxNumber;
            this.firstChannelNumber = firstChannelNumber;
            this.impulseReactions = impulseReactions;
            this.signalPowers = signalPowers;
        }

        /// <summary>
        /// Compute impulse reactions' integral
        /// </summary>
        /// <param name="k">first impulse reaction index</param>
        /// <param name="q">second impulse reaction index</param>
        /// <param name="currentSample">current sample, canonical name - <i>lt</i></param>
        /// <returns>imulse reaction integral</returns>
        public double Nju(int k, int q, int currentSample)
        {
            double element = 0;

            if (FirstCondition(k, q, currentSample))
            {
                for (int i = k + currentSample + 1; i <= impulseReactionLength - 1; i++)
                {
                    for (int j = q + currentSample + 1; j <= impulseReactionLength - 1; j++)
                    {
                        element += WeightedImpulseReactionProduct(i, j, k + j - q - i);
                    }
                }
                return 2 * element;
            }
            else if (SecondCondition(k, q, currentSample))
            {
                for (int i = 0; i <= k + currentSample - fourierTransformBase - protectionIntervalSamplesNumber; i++)
                {
                    for (int j = 0; j <= q + currentSample - fourierTransformBase - protectionIntervalSamplesNumber; j++)
                    {
                        element += WeightedImpulseReactionProduct(i, j, k + j - q - i);
                    }
                }
                return 2 * element;
            }
            else if (ThirdCondition(k, q, currentSample))
            {
                for (int i = k + currentSample + 1; i <= impulseReactionLength - 1; i++)
                {
                    for (int j = 0; j <= q + currentSample - fourierTransformBase - protectionIntervalSamplesNumber; j++)
                    {
                        element += WeightedImpulseReactionProduct(i, j, 2 * fourierTransformBase + protectionIntervalSamplesNumber + k + j - q - i);
                    }
                }
                return element;
            }
            else if (FourthCondition(k, q, currentSample))
            {
                for (int i = 0; i <= k + currentSample - fourierTransformBase - protectionIntervalSamplesNumber; i++)
                {
                    for (int j = q + currentSample + 1; j <= impulseReactionLength - 1; j++)
                    {
                        element += WeightedImpulseReactionProduct(i, j, 2 * fourierTransformBase + protectionIntervalSamplesNumber + q + i - k - j);
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
            return x <= impulseReactionLength - 2 - currentSample;
        }

        private bool NotLessThanFourierWithSamplesDiff(int x, int currentSample)
        {
            return x >= fourierTransformBase + protectionIntervalSamplesNumber - currentSample;
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
            return impulseReactions[i] * impulseReactions[j] * SignalCorrelation(sampleDifference);
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
            for (int p = 1; p <= carrierFrequencyMaxNumber; p++)
            {
                sum += signalPowers[p + firstChannelNumber - 1] * Math.Cos(NetworkValueCalculator.PI * sampleDifference * (p + firstChannelNumber - 1) / carrierFrequencyMaxNumber);
            }
            return sum;
        }
    }
}
