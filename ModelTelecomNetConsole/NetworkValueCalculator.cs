using System.Numerics;

namespace TelecomNetModelling
{
    public class NetworkValueCalculator
    {
        /// Нужно использовать именно такой PI (20 знаков, после запятой).
       public  const double PI = 3.14159265358979323846;
       
       /// <summary>
       ///  <para>Основа преобразования Фурье. Количество отсчетов на интервале
       ///  ортогональности.</para>
       ///  <para>Используется как размерность входяшей матрицы.</para>
       /// </summary>
       /// <remarks>Старое название - N</remarks>
       public static int fourierTransformBase;

       /// <summary>
       /// Номер максимальной несущей частоты.
       /// </summary>
       /// <remarks>Старое название - n</remarks>
       public static int carrierFrequencyMaxNumber;

       /// <summary>
       /// Количество отсчетов на защитном интервале.
       /// </summary>
       /// <remarks>Старое название - L</remarks>
       public static int protectionIntervalSamplesNumber;
       
       /// <summary>
       /// Номер первого канала.
       /// </summary>
       /// <remarks>Старое название - m</remarks>
       public static int firstChannelNumber;

       /// <summary>
       /// Начальная точка отсчета.
       /// </summary>
       /// <remarks>Старое название - from_lt</remarks>
       public static int firstSample;

       /// <summary>
       /// Конечная точка отсчета.
       /// </summary>
       /// <remarks>Старое название - until_lt</remarks>
       public static int lastSample;

       /// <summary>
       /// Длительность импульсной реакции. Зависит от размера файла.
       /// </summary>
       /// <remarks>Старое название - R</remarks>
       public static  int impulseReactionLength;
       
       /// <summary>
       /// Вектор импульсных реакций.
       /// </summary>
       /// <remarks>Старое название - g</remarks>
       public static  double[] impulseReactions = new double[fourierTransformBase];
       
       /// <summary>
       /// Вектор мощностей сигналов.
       /// </summary>
       /// <remarks>Старое название - power</remarks>
       public static  double[] signalPowers = new double[carrierFrequencyMaxNumber];
       
       /// <summary>
       /// Маска сигнала.
       /// </summary>
       /// <remarks>Старое название - PSD</remarks>
       public static  double[] signalMask = new double[carrierFrequencyMaxNumber];
       
       /// <summary>
       /// Выходной массив чего-то.
       /// </summary>
       public static  double[,] njus = new double[fourierTransformBase,fourierTransformBase];
       
       /// <summary>
       /// Промежуточный выходной массив.
       /// </summary>
       public static  double[,] currrentNjus = new double[fourierTransformBase,fourierTransformBase];


        /// <summary>
        /// Нахождение корреляции сигнала.
        /// </summary>
        /// <param name="sampleDifference">разница между отсчетами</param>
        /// <returns>корреляция сигнала</returns>
        /// <remarks>Старое название метода - B</remarks>
        double SignalCorrelation(int sampleDifference)
        {
            double sum = 0;
            for (int p = 1; p <= carrierFrequencyMaxNumber; p++)
            {
                sum += signalPowers[p + firstChannelNumber - 1] * Math.Cos(PI * sampleDifference * (p + firstChannelNumber - 1) / carrierFrequencyMaxNumber);
            }
            return sum;
        }


        /// <summary>
        /// Главная функция по расчету чего-то в матрицы nju*
        /// </summary>
        /// <param name="k">какой-то индекс</param>
        /// <param name="q">еще один какой-то индекс</param>
        /// <param name="sampleCount">количество отсчетов, раньше называлось <i>lt</i></param>
        /// <returns></returns>
        double Nju(int k, int q, int sampleCount)
        {
            if (sampleCount == firstSample)
            {
                double element = 0;

                if (k <= impulseReactionLength - 2 - sampleCount && q <= impulseReactionLength - 2 - sampleCount)
                {
                    for (int i = k + sampleCount + 1; i <= impulseReactionLength - 1; i++)
                    {
                        for (int j = q + sampleCount + 1; j <= impulseReactionLength - 1; j++)
                        {
                            element += impulseReactions[i] * impulseReactions[j] * SignalCorrelation(k + j - q - i);
                        }
                    }
                    return 2 * element;
                }
                else if (k >= fourierTransformBase + protectionIntervalSamplesNumber - sampleCount && q >= fourierTransformBase + protectionIntervalSamplesNumber - sampleCount)
                {
                    for (int i = 0; i <= k + sampleCount - fourierTransformBase - protectionIntervalSamplesNumber; i++)
                    {
                        for (int j = 0; j <= q + sampleCount - fourierTransformBase - protectionIntervalSamplesNumber; j++)
                        {
                            element += impulseReactions[i] * impulseReactions[j] * SignalCorrelation(k + j - q - i);
                        }
                    }
                    return 2 * element;
                }
                else if (k <= impulseReactionLength - 2 - sampleCount && q >= fourierTransformBase + protectionIntervalSamplesNumber - sampleCount)
                {
                    for (int i = k + sampleCount + 1; i <= impulseReactionLength - 1; i++)
                    {
                        for (int j = 0; j <= q + sampleCount - fourierTransformBase - protectionIntervalSamplesNumber; j++)
                        {
                            element += impulseReactions[i] * impulseReactions[j] * SignalCorrelation(2 * fourierTransformBase + protectionIntervalSamplesNumber + k + j - q - i);
                        }
                    }
                    return element;
                }
                else if (k >= fourierTransformBase + protectionIntervalSamplesNumber - sampleCount && q <= impulseReactionLength - 2 - sampleCount)
                {
                    for (int i = 0; i <= k + sampleCount - fourierTransformBase - protectionIntervalSamplesNumber; i++)
                    {
                        for (int j = q + sampleCount + 1; j <= impulseReactionLength - 1; j++)
                        {
                            element += impulseReactions[i] * impulseReactions[j] * SignalCorrelation(2 * fourierTransformBase + protectionIntervalSamplesNumber + q + i - k - j);
                        }
                    }
                    return element;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                if (k == fourierTransformBase - 1 || q == fourierTransformBase - 1)
                {
                    double element = 0;

                    if (k <= impulseReactionLength - 2 - sampleCount && q <= impulseReactionLength - 2 - sampleCount)
                    {
                        for (int i = k + sampleCount + 1; i <= impulseReactionLength - 1; i++)
                        {
                            for (int j = q + sampleCount + 1; j <= impulseReactionLength - 1; j++)
                            {
                                element += impulseReactions[i] * impulseReactions[j] * SignalCorrelation(k + j - q - i);
                            }
                        }
                        return 2 * element;
                    }
                    else if (k >= fourierTransformBase + protectionIntervalSamplesNumber - sampleCount && q >= fourierTransformBase + protectionIntervalSamplesNumber - sampleCount)
                    {
                        for (int i = 0; i <= k + sampleCount - fourierTransformBase - protectionIntervalSamplesNumber; i++)
                        {
                            for (int j = 0; j <= q + sampleCount - fourierTransformBase - protectionIntervalSamplesNumber; j++)
                            {
                                element += impulseReactions[i] * impulseReactions[j] * SignalCorrelation(k + j - q - i);
                            }
                        }
                        return 2 * element;
                    }
                    else if (k <= impulseReactionLength - 2 - sampleCount && q >= fourierTransformBase + protectionIntervalSamplesNumber - sampleCount)
                    {
                        for (int i = k + sampleCount + 1; i <= impulseReactionLength - 1; i++)
                        {
                            for (int j = 0; j <= q + sampleCount - fourierTransformBase - protectionIntervalSamplesNumber; j++)
                            {
                                element += impulseReactions[i] * impulseReactions[j] * SignalCorrelation(2 * fourierTransformBase + protectionIntervalSamplesNumber + k + j - q - i);
                            }
                        }
                        return element;
                    }
                    else if (k >= fourierTransformBase + protectionIntervalSamplesNumber - sampleCount && q <= impulseReactionLength - 2 - sampleCount)
                    {
                        for (int i = 0; i <= k + sampleCount - fourierTransformBase - protectionIntervalSamplesNumber; i++)
                        {
                            for (int j = q + sampleCount + 1; j <= impulseReactionLength - 1; j++)
                            {
                                element += impulseReactions[i] * impulseReactions[j] * SignalCorrelation(2 * fourierTransformBase + protectionIntervalSamplesNumber + q + i - k - j);
                            }
                        }
                        return element;
                    }
                    else
                    {
                        return 0;
                    }
                }
                else
                {
                    return njus[k + 1,q + 1];
                }
            }
        }

        /// <summary>
        /// Мощность интерференционной помехи.
        /// </summary>
        /// <param name="p">индекс?</param>
        /// <returns>мощность помехи</returns>
        /// <remarks>Старое название - Interf</remarks>
        double InterferationNoisePower(int p)
        {
            double sum = 0;
            for (int k = 0; k <= fourierTransformBase - 1; k++)
            {
                for (int q = 0; q <= fourierTransformBase - 1; q++)
                {
                    sum += njus[k,q] * Math.Cos(2 * PI * (p + firstChannelNumber - 1) * (k - q) / fourierTransformBase);
                }
            }
            return sum;
        }

        /// <summary>
        /// Расчет мощности полезного сигнала.
        /// </summary>
        /// <param name="p">мощность?</param>
        /// <returns>мощность полезного сигнала</returns>
        /// <remarks>Старое название - Signal</remarks>
        double SignalPower(int p)
        {
            Complex sum = new Complex(); // TODO Возможно, стоит объявить как double.
            Complex  J = new Complex(0, 1);
            for (int i = 0; i <= fourierTransformBase - 1; i++)
            {
                // TODO Нужно вернуть реальную часть либо модуль вектора от комплексного числа.
                // sum += impulseReactions[i] * Math.Exp((-J) * 2.0 * PI * (double)(p + firstChannelNumber - 1) * (double)i / (double)fourierTransformBase);
            }
            // return Math.Pow(Math.Abs(sum), 2) * fourierTransformBase * fourierTransformBase / 2.0 * signalPowers[p + firstChannelNumber - 1];
            throw new NotImplementedException("Не готово преобразование комплексного числа в double.");
        }

        /// <summary>
        /// Соотношение сигнал/шум.
        /// </summary>
        /// <param name="power">мощность сигнала?</param>
        /// <returns>значение SNR</returns>
        /// <remarks>Старое название - Ratio</remarks>>
        double SNR(int power)
        {
            double ratio;
            ratio = Math.Sqrt(InterferationNoisePower(power) / SignalPower(power));
            return ratio * 100.0;
        }
    }
}