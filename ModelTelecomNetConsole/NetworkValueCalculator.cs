using System.Numerics;
using Microsoft.Extensions.Configuration;

namespace TelecomNetModelling
{
    public class NetworkValueCalculator
    {
        /// Нужно использовать именно такой PI (20 знаков, после запятой).
       public const double PI = 3.14159265358979323846;
       
       /// <summary>
       ///  <para>Основа преобразования Фурье. Количество отсчетов на интервале
       ///  ортогональности.</para>
       ///  <para>Используется как размерность входяшей матрицы.</para>
       /// </summary>
       /// <remarks>Старое название - N</remarks>
       private static int fourierTransformBase;

       /// <summary>
       /// Номер максимальной несущей частоты.
       /// </summary>
       /// <remarks>Старое название - n</remarks>
       private static int carrierFrequencyMaxNumber;

       /// <summary>
       /// Количество отсчетов на защитном интервале.
       /// </summary>
       /// <remarks>Старое название - L</remarks>
       private int protectionIntervalSamplesNumber;
       
       /// <summary>
       /// Номер первого канала.
       /// </summary>
       /// <remarks>Старое название - m</remarks>
       private int firstChannelNumber;

       /// <summary>
       /// Начальная точка отсчета.
       /// </summary>
       /// <remarks>Старое название - from_lt</remarks>
       private int firstSample;

       /// <summary>
       /// Конечная точка отсчета.
       /// </summary>
       /// <remarks>Старое название - until_lt</remarks>
       private int lastSample;

       /// <summary>
       /// Длительность импульсной реакции. Зависит от размера файла.
       /// </summary>
       /// <remarks>Старое название - R</remarks>
       private int impulseReactionLength;
       
       /// <summary>
       /// Вектор импульсных реакций.
       /// </summary>
       /// <remarks>Старое название - g</remarks>
       private double[] impulseReactions;
       
       /// <summary>
       /// Вектор мощностей сигналов.
       /// </summary>
       /// <remarks>Старое название - power</remarks>
       private  double[] signalPowers;
       
       /// <summary>
       /// Маска сигнала.
       /// </summary>
       /// <remarks>Старое название - PSD</remarks>
       private  double[] signalMask;
       
       /// <summary>
       /// Выходной массив чего-то.
       /// </summary>
       private  double[,] njus = new double[fourierTransformBase,fourierTransformBase];
       
       /// <summary>
       /// Промежуточный выходной массив.
       /// </summary>
       private  double[,] currrentNjus = new double[fourierTransformBase,fourierTransformBase];

       public NetworkValueCalculator(IConfiguration configuration, double[] impulseReactions, double[] signalPowers, double[] signalMask)
       {
           fourierTransformBase = int.Parse(configuration["AppSettings:fourierTransformBase"]);
           carrierFrequencyMaxNumber = int.Parse(configuration["AppSettings:carrierFrequencyMaxNumber"]);
           protectionIntervalSamplesNumber = int.Parse(configuration["AppSettings:protectionIntervalSamplesNumber"]);
           firstChannelNumber = int.Parse(configuration["AppSettings:firstChannelNumber"]);
           firstSample = int.Parse(configuration["AppSettings:firstSample"]);
           lastSample = int.Parse(configuration["AppSettings:lastSample"]);
           impulseReactionLength = int.Parse(configuration["AppSettings:impulseReactionLength"]);

           this.impulseReactions = impulseReactions;
           this.signalPowers = signalPowers;
           this.signalMask = signalMask;
       }

       // TODO Нужно ещё выяснить какие данные возвращает функция.
       /// <summary>
       /// Фасадный метод, который запускает всю бизнес-логику приложения.
       /// </summary>
       /// <exception cref="NotImplementedException">потому что мы еще не
       /// доделали расчет</exception>
       public void Execute()
       {
           for (int currentSample = firstSample; currentSample <= lastSample; currentSample++)
           {
               for (int i = 0; i < fourierTransformBase; i++)
               {
                   for (int j = 0; j < fourierTransformBase; j++)
                   {
                       if (i >= j)
                       {
                           currrentNjus[i, j] = Nju(i, j, currentSample);
                       }
                   }
               }

               // TODO Заполнение нижнего треугольника матрицы можно сделать и
               // в верхнем цикле.
               for (int i = 0; i < fourierTransformBase; i++)
               {
                   for (int j = 0; j < fourierTransformBase; j++)
                   {
                       if (i >= j)
                       {
                           njus[i, j] = currrentNjus[i, j];
                       }
                       else
                       {
                           njus[i, j] = currrentNjus[j, i];
                       }
                   }
               }

               for (int i = 0; i <= carrierFrequencyMaxNumber; i++)
               {
                   // TODO Это нужно вывести
                   double ratio = SNR(i);
               }
           }
           throw new NotImplementedException("Главная функция расчета не готова.");
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
                sum += signalPowers[p + firstChannelNumber - 1] * Math.Cos(PI * sampleDifference * (p + firstChannelNumber - 1) / carrierFrequencyMaxNumber);
            }
            return sum;
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
            if (currentSample == firstSample)
            {
                double element = 0;

                if (k <= impulseReactionLength - 2 - currentSample && q <= impulseReactionLength - 2 - currentSample)
                {
                    for (int i = k + currentSample + 1; i <= impulseReactionLength - 1; i++)
                    {
                        for (int j = q + currentSample + 1; j <= impulseReactionLength - 1; j++)
                        {
                            element += impulseReactions[i] * impulseReactions[j] * SignalCorrelation(k + j - q - i);
                        }
                    }
                    return 2 * element;
                }
                else if (k >= fourierTransformBase + protectionIntervalSamplesNumber - currentSample && q >= fourierTransformBase + protectionIntervalSamplesNumber - currentSample)
                {
                    for (int i = 0; i <= k + currentSample - fourierTransformBase - protectionIntervalSamplesNumber; i++)
                    {
                        for (int j = 0; j <= q + currentSample - fourierTransformBase - protectionIntervalSamplesNumber; j++)
                        {
                            element += impulseReactions[i] * impulseReactions[j] * SignalCorrelation(k + j - q - i);
                        }
                    }
                    return 2 * element;
                }
                else if (k <= impulseReactionLength - 2 - currentSample && q >= fourierTransformBase + protectionIntervalSamplesNumber - currentSample)
                {
                    for (int i = k + currentSample + 1; i <= impulseReactionLength - 1; i++)
                    {
                        for (int j = 0; j <= q + currentSample - fourierTransformBase - protectionIntervalSamplesNumber; j++)
                        {
                            element += impulseReactions[i] * impulseReactions[j] * SignalCorrelation(2 * fourierTransformBase + protectionIntervalSamplesNumber + k + j - q - i);
                        }
                    }
                    return element;
                }
                else if (k >= fourierTransformBase + protectionIntervalSamplesNumber - currentSample && q <= impulseReactionLength - 2 - currentSample)
                {
                    for (int i = 0; i <= k + currentSample - fourierTransformBase - protectionIntervalSamplesNumber; i++)
                    {
                        for (int j = q + currentSample + 1; j <= impulseReactionLength - 1; j++)
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

                    if (k <= impulseReactionLength - 2 - currentSample && q <= impulseReactionLength - 2 - currentSample)
                    {
                        for (int i = k + currentSample + 1; i <= impulseReactionLength - 1; i++)
                        {
                            for (int j = q + currentSample + 1; j <= impulseReactionLength - 1; j++)
                            {
                                element += impulseReactions[i] * impulseReactions[j] * SignalCorrelation(k + j - q - i);
                            }
                        }
                        return 2 * element;
                    }
                    else if (k >= fourierTransformBase + protectionIntervalSamplesNumber - currentSample && q >= fourierTransformBase + protectionIntervalSamplesNumber - currentSample)
                    {
                        for (int i = 0; i <= k + currentSample - fourierTransformBase - protectionIntervalSamplesNumber; i++)
                        {
                            for (int j = 0; j <= q + currentSample - fourierTransformBase - protectionIntervalSamplesNumber; j++)
                            {
                                element += impulseReactions[i] * impulseReactions[j] * SignalCorrelation(k + j - q - i);
                            }
                        }
                        return 2 * element;
                    }
                    else if (k <= impulseReactionLength - 2 - currentSample && q >= fourierTransformBase + protectionIntervalSamplesNumber - currentSample)
                    {
                        for (int i = k + currentSample + 1; i <= impulseReactionLength - 1; i++)
                        {
                            for (int j = 0; j <= q + currentSample - fourierTransformBase - protectionIntervalSamplesNumber; j++)
                            {
                                element += impulseReactions[i] * impulseReactions[j] * SignalCorrelation(2 * fourierTransformBase + protectionIntervalSamplesNumber + k + j - q - i);
                            }
                        }
                        return element;
                    }
                    else if (k >= fourierTransformBase + protectionIntervalSamplesNumber - currentSample && q <= impulseReactionLength - 2 - currentSample)
                    {
                        for (int i = 0; i <= k + currentSample - fourierTransformBase - protectionIntervalSamplesNumber; i++)
                        {
                            for (int j = q + currentSample + 1; j <= impulseReactionLength - 1; j++)
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
        public double InterferationNoisePower(int p)
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
        public double SignalPower(int p)
        {
            Complex sum = new Complex();
            Complex J = new Complex(0, 1);
            for (int i = 0; i <= fourierTransformBase - 1; i++)
            {
                sum += Complex.Multiply(impulseReactions[i],
                    Complex.Exp(
                        Complex.Multiply(-J,
                            PI * (double) (p + firstChannelNumber - 1) * (double) i / (double) fourierTransformBase
                        )
                    )
                );
            }
            return Math.Pow(Complex.Abs(sum), 2) * fourierTransformBase * fourierTransformBase / 2.0 * signalPowers[p + firstChannelNumber - 1];
        }

        /// <summary>
        /// Соотношение сигнал/шум.
        /// </summary>
        /// <param name="power">мощность сигнала?</param>
        /// <returns>значение SNR</returns>
        /// <remarks>Старое название - Ratio</remarks>>
        public double SNR(int power)
        {
            double ratio;
            ratio = Math.Sqrt(InterferationNoisePower(power) / SignalPower(power));
            return ratio * 100.0;
        }
    }
}
