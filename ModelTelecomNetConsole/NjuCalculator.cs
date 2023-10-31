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

        // TODO: Передать логгер
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
        /// Главная функция по расчету чего-то в матрицы nju*
        /// </summary>
        /// <param name="k">первый индекс импульсной реакции</param>
        /// <param name="q">второй индекс импульсной реакции</param>
        /// <param name="currentSample">текущий отсчет, раньше назывался <i>lt</i></param>
        /// <returns></returns>
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
        /// Взвешенное произведение двух импульсных реакций.
        /// </summary>
        /// <param name="i">индекс первой имп. реакции</param>
        /// <param name="j">индекс второй имп. реакции</param>
        /// <param name="sampleDifference">разница отсчетов</param>
        /// <returns>взвешенное произведение</returns>
        private double WeightedImpulseReactionProduct(int i, int j, int sampleDifference)
        {
            return impulseReactions[i] * impulseReactions[j] * SignalCorrelation(sampleDifference);
        }

        /// <summary>
        /// Нахождение корреляции сигнала.
        /// </summary>
        /// <param name="sampleDifference">разница между отсчетами</param>
        /// <returns>корреляция сигнала</returns>
        /// <remarks>Старое название метода - B</remarks>
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