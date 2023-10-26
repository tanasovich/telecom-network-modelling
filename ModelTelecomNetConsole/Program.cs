using System;
using System.IO;
using System.Numerics;


// N. B. Пока что просто берем константы из cpp-файла, описания сообщат позже.
namespace TelecomNetModelling
{
    class Program
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
        double SignalCorrelation(int sampleDifference)
        {
            double sum = 0;
            for (int p = 1; p <= carrierFrequencyMaxNumber; p++)
            {
                sum += signalPowers[p + firstChannelNumber - 1] * Math.Cos(PI * sampleDifference * (p + firstChannelNumber - 1) / carrierFrequencyMaxNumber);
            }
            return sum;
        }


        /// Главный виновник торжества - расчитывает каждый элемент в матрицах nju*
        double Nju(int k, int q, int lt)
        {
            if (lt == firstSample)
            {
                double element = 0;

                if (k <= impulseReactionLength - 2 - lt && q <= impulseReactionLength - 2 - lt)
                {
                    for (int i = k + lt + 1; i <= impulseReactionLength - 1; i++)
                    {
                        for (int j = q + lt + 1; j <= impulseReactionLength - 1; j++)
                        {
                            element += impulseReactions[i] * impulseReactions[j] * SignalCorrelation(k + j - q - i);
                        }
                    }
                    return 2 * element;
                }

                else if (k >= fourierTransformBase + protectionIntervalSamplesNumber - lt && q >= fourierTransformBase + protectionIntervalSamplesNumber - lt)
                {
                    for (int i = 0; i <= k + lt - fourierTransformBase - protectionIntervalSamplesNumber; i++)
                    {
                        for (int j = 0; j <= q + lt - fourierTransformBase - protectionIntervalSamplesNumber; j++)
                        {
                            element += impulseReactions[i] * impulseReactions[j] * SignalCorrelation(k + j - q - i);
                        }
                    }
                    return 2 * element;
                }

                else if (k <= impulseReactionLength - 2 - lt && q >= fourierTransformBase + protectionIntervalSamplesNumber - lt)
                {
                    for (int i = k + lt + 1; i <= impulseReactionLength - 1; i++)
                    {
                        for (int j = 0; j <= q + lt - fourierTransformBase - protectionIntervalSamplesNumber; j++)
                        {
                            element += impulseReactions[i] * impulseReactions[j] * SignalCorrelation(2 * fourierTransformBase + protectionIntervalSamplesNumber + k + j - q - i);
                        }
                    }
                    return element;
                }

                else if (k >= fourierTransformBase + protectionIntervalSamplesNumber - lt && q <= impulseReactionLength - 2 - lt)
                {
                    for (int i = 0; i <= k + lt - fourierTransformBase - protectionIntervalSamplesNumber; i++)
                    {
                        for (int j = q + lt + 1; j <= impulseReactionLength - 1; j++)
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

                    if (k <= impulseReactionLength - 2 - lt && q <= impulseReactionLength - 2 - lt)
                    {
                        for (int i = k + lt + 1; i <= impulseReactionLength - 1; i++)
                        {
                            for (int j = q + lt + 1; j <= impulseReactionLength - 1; j++)
                            {
                                element += impulseReactions[i] * impulseReactions[j] * SignalCorrelation(k + j - q - i);
                            }
                        }
                        return 2 * element;
                    }

                    else if (k >= fourierTransformBase + protectionIntervalSamplesNumber - lt && q >= fourierTransformBase + protectionIntervalSamplesNumber - lt)
                    {
                        for (int i = 0; i <= k + lt - fourierTransformBase - protectionIntervalSamplesNumber; i++)
                        {
                            for (int j = 0; j <= q + lt - fourierTransformBase - protectionIntervalSamplesNumber; j++)
                            {
                                element += impulseReactions[i] * impulseReactions[j] * SignalCorrelation(k + j - q - i);
                            }
                        }
                        return 2 * element;
                    }

                    else if (k <= impulseReactionLength - 2 - lt && q >= fourierTransformBase + protectionIntervalSamplesNumber - lt)
                    {
                        for (int i = k + lt + 1; i <= impulseReactionLength - 1; i++)
                        {
                            for (int j = 0; j <= q + lt - fourierTransformBase - protectionIntervalSamplesNumber; j++)
                            {
                                element += impulseReactions[i] * impulseReactions[j] * SignalCorrelation(2 * fourierTransformBase + protectionIntervalSamplesNumber + k + j - q - i);
                            }
                        }
                        return element;
                    }

                    else if (k >= fourierTransformBase + protectionIntervalSamplesNumber - lt && q <= impulseReactionLength - 2 - lt)
                    {
                        for (int i = 0; i <= k + lt - fourierTransformBase - protectionIntervalSamplesNumber; i++)
                        {
                            for (int j = q + lt + 1; j <= impulseReactionLength - 1; j++)
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

        /// Расчет мощности интерференционной помехи
        double Interf(int p)
        {
            double summ = 0;
            for (int k = 0; k <= fourierTransformBase - 1; k++)
            {
                for (int q = 0; q <= fourierTransformBase - 1; q++)
                {
                    summ += njus[k,q] * Math.Cos(2 * PI * (p + firstChannelNumber - 1) * (k - q) / fourierTransformBase);
                }
            }
            return summ;
        }

        /// Расчет мощности сигнала
        /// Переделать расчет на API от класса Complex
        /// Возможно, нужно явно(!) подключить Complex в настройках проекта.
        double Signal(int p)
{
            Complex summ = new Complex();
    const Complex  J = new Complex(0, 1);
    for (int i = 0; i <= fourierTransformBase - 1; i++)
    {
        summ += impulseReactions[i] * Math.Exp((-J) * 2.0 * PI * (double)(p + firstChannelNumber - 1) * (double)i / (double)fourierTransformBase);
    }
    return Math.Pow(Math.Abs(summ), 2) * fourierTransformBase * fourierTransformBase / 2.0 * signalPowers[p + firstChannelNumber - 1];
}

        /// Соотношение сигнал/шум
        double Ratio(int p)
        {
            double ratio;
            ratio = Math.Sqrt(Interf(p) / Signal(p));
            return ratio * 100.0;
        }

        /// Программа расчета для традиционных систем. Пример, файл - TWP_GFAST_150m_TR.cpp
        /// Для начала, реализовать выбор входных данных только через конфиги.
        /// Ввводные файлы должны быть прописаны в конфигах, а также должна быть возможность
        /// вводить кастомные имена через консоль.
        /// Начать работу с ввода информации (заполнение массивов).
        static void Main(string[] args)
        {
            Console.Write("Enter N ");
           fourierTransformBase =int.Parse(Console.ReadLine());
           Console.Write("Enter n ");
           carrierFrequencyMaxNumber = int.Parse(Console.ReadLine());

           Console.Write("Enter L ");
           protectionIntervalSamplesNumber = int.Parse(Console.ReadLine());
           Console.Write("Enter m ");
           firstChannelNumber = int.Parse(Console.ReadLine());
           Console.Write("Enter R ");
           impulseReactionLength = int.Parse(Console.ReadLine());
           Console.Write("Enter Start lt ");
           firstSample = int.Parse(Console.ReadLine());
           Console.Write("Enter end lt ");
           lastSample = int.Parse(Console.ReadLine());

           /// При чтении файлов, случайным образом возникает ошибка выхода за границы массива.
           /// Особенно, это происходит на 254 строке.
           /// N. B. Ошибка не возникает когда выполняется чтение без остальной логики.

           //"IRGFAST_twp_150m954"//заменить
           /// 954 и есть R - длительность импульсной реакции
           using (StreamReader sr = new StreamReader(@"IRGFAST_twp_150m954"))
           {
               string line;
               while ((line = sr.ReadLine()) != null)
               {
                   string[] text = line.Split(' ');

                   for (int i = 0; i < fourierTransformBase - 1; i++)
                   {
                       impulseReactions[i] = Double.Parse(text[i]);
                   }
               }
           }

           /// GfastPSD - это спектральная маска сигнала
           /// Этот файл существует по-умолчанию (почти не меняется). Другими словами, он обязан быть при выполнении.
           using (StreamReader sr = new StreamReader(@"GfastPSD_dB_0_2047"))
           {
               string line;
               while ((line = sr.ReadLine()) != null)
               {
                   string[] text = line.Split(' ');
                   for (int i = 0; i < firstChannelNumber + carrierFrequencyMaxNumber + 1; i++)
                   {
                       signalMask[i] = Double.Parse(text[i]);

                       /// не редактируй это, оставь как есть
                       signalPowers[i] = Math.Pow(10, 0.1 * (signalMask[i] + 80));  // прибавляем 80, чтобы не работать со слишком маленькими числами (результат работы программы зависит только от соотношения мощностей на несущих)
                   }
               }
           }

           /// Дописать логику вывода результатов.
           /// Файловый вывод (как в оригинале).
        }
    }
}
