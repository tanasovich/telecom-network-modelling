using System.Globalization;
using System.Numerics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace TelecomNetModelling
{
    public class NetworkValueCalculator
    {
        /// Нужно использовать именно такой PI (20 знаков, после запятой).
       public const double PI = 3.14159265358979323846;

       private readonly ILogger logger;
       
       /// <summary>
       ///  <para>Основа преобразования Фурье. Количество отсчетов на интервале
       ///  ортогональности.</para>
       ///  <para>Используется как размерность входяшей матрицы.</para>
       /// </summary>
       /// <remarks>Старое название - N</remarks>
       private readonly int fourierTransformBase;

       /// <summary>
       /// Номер максимальной несущей частоты.
       /// </summary>
       /// <remarks>Старое название - n</remarks>
       private readonly int carrierFrequencyMaxNumber;

       /// <summary>
       /// Количество отсчетов на защитном интервале.
       /// </summary>
       /// <remarks>Старое название - L</remarks>
       private readonly int protectionIntervalSamplesNumber;
       
       /// <summary>
       /// Номер первого канала.
       /// </summary>
       /// <remarks>Старое название - m</remarks>
       private readonly int firstChannelNumber;

       /// <summary>
       /// Начальная точка отсчета.
       /// </summary>
       /// <remarks>Старое название - from_lt</remarks>
       private readonly int firstSample;

       /// <summary>
       /// Конечная точка отсчета.
       /// </summary>
       /// <remarks>Старое название - until_lt</remarks>
       private readonly int lastSample;

       /// <summary>
       /// Длительность импульсной реакции. Зависит от размера файла.
       /// </summary>
       /// <remarks>Старое название - R</remarks>
       private readonly int impulseReactionLength;
       
       /// <summary>
       /// Вектор импульсных реакций.
       /// </summary>
       /// <remarks>Старое название - g</remarks>
       private List<double> impulseReactions;
       
       /// <summary>
       /// Вектор мощностей сигналов.
       /// </summary>
       /// <remarks>Старое название - power</remarks>
       private List<double> signalPowers;
       
       /// <summary>
       /// Маска сигнала.
       /// </summary>
       /// <remarks>Старое название - PSD</remarks>
       private List<double> signalMask;

       /// <summary>
       ///  Путь к директории с результатами расчетов.
       /// </summary>
       private readonly string resultsDirectory;

       private readonly NjuCalculator njuCalculator;
       
       /// <summary>
       /// Выходной массив чего-то.
       /// </summary>
       private  List<List<double>> njus;
       
       /// <summary>
       /// Промежуточный выходной массив.
       /// </summary>
       private  List<List<double>> currrentNjus;

       public NetworkValueCalculator(IConfiguration configuration,
           Dictionary<string, List<double>> inputs, ILogger logger)
       {
           this.logger = logger;
           
           fourierTransformBase = int.Parse(
               configuration["AppSettings:fourierTransformBase"]!);
           carrierFrequencyMaxNumber = int.Parse(
               configuration["AppSettings:carrierFrequencyMaxNumber"]!);
           protectionIntervalSamplesNumber = int.Parse(
               configuration["AppSettings:protectionIntervalSamplesNumber"]!);
           firstChannelNumber = int.Parse(
               configuration["AppSettings:firstChannelNumber"]!);
           firstSample = int.Parse(configuration["AppSettings:firstSample"]!);
           lastSample = int.Parse(configuration["AppSettings:lastSample"]!);
           impulseReactionLength = int.Parse(
               configuration["AppSettings:impulseReactionLength"]!);

           impulseReactions = inputs["impulseReactions"];
           signalMask = inputs["signalMask"];

           resultsDirectory = configuration["AppSettings:resultsDirectory"]!;

           signalPowers = new List<double>();
           for (int i = 0; i < firstChannelNumber + carrierFrequencyMaxNumber + 1; i++)
           {
               signalPowers.Add(Math.Pow(10, 0.1 * (signalMask[i] + 80)));
           }
           logger.LogInformation("Рассчитаны мощности сигналов на основе маски.");

           njuCalculator = new NjuCalculator(
            fourierTransformBase, impulseReactionLength,
            protectionIntervalSamplesNumber, carrierFrequencyMaxNumber,
            firstChannelNumber, impulseReactions, signalPowers
           );

           njus = new List<List<double>>(fourierTransformBase);
           currrentNjus = new List<List<double>>(fourierTransformBase);
           for (int i = 0; i < fourierTransformBase; i++)
           {
               njus.Add(new List<double>(fourierTransformBase));
               currrentNjus.Add(new List<double>(fourierTransformBase));
               for (int j = 0; j < fourierTransformBase; j++)
               {
                   njus[i].Add(default);
                   currrentNjus[i].Add(default);
               }
           }
       }

       // TODO: Нужно ещё выяснить какие данные возвращает функция.
       /// <summary>
       /// Фасадный метод, который запускает всю бизнес-логику приложения.
       /// </summary>
       public void Execute()
       {
           for (int currentSample = firstSample; currentSample <= lastSample; currentSample++)
           {
               logger.LogDebug("Вычисление для LT = {currentSample}", currentSample);
               for (int i = 0; i < fourierTransformBase; i++)
               {
                   // TODO: Переменной j можно присваивать i, ведь мы считаем только верхний треугольник.
                   for (int j = 0; j < fourierTransformBase; j++)
                   {
                       if (i < j)
                       {
                           continue;
                       }
                       // XXX: Я не понимаю эту проверку и передачу след. числа по диагонали.
                       if (currentSample != firstSample && i != fourierTransformBase - 1 && j != fourierTransformBase - 1)
                       {
                           currrentNjus[i][j] = njus[i + 1][j + 1];

                           logger.LogInformation("Выполнены мистическая проверка и выдача элемента по диагонали для LT {currentSample}", currentSample);
                           logger.LogDebug("Мистические параметры: i = {i}, j = {j}", i, j);

                           continue;
                       }
                       currrentNjus[i][j] = njuCalculator.Nju(i, j, currentSample);

                       logger.LogTrace("nju({i}, {j}) = {currentNju}", i, j, currrentNjus[i][j]);
                   }
               }

               // NOTE: Ни в коем случае не заполнять матрицу при расчете.
               for (int i = 0; i < fourierTransformBase; i++)
               {
                   for (int j = 0; j < fourierTransformBase; j++)
                   {
                       if (i >= j)
                       {
                           njus[i][j] = currrentNjus[i][j];
                       }
                       else
                       {
                           njus[i][j] = currrentNjus[j][i];
                       }
                   }
                   Console.WriteLine(string.Join(',', njus[i]));
               }

               bool firstEntry = true;
               for (int i = 0; i <= carrierFrequencyMaxNumber; i++)
               {
                   // TODO: Запись в файл
                   double ratio = SNR(i);

                   using (StreamWriter writer = new StreamWriter(Path.Combine(resultsDirectory, $"interf{currentSample}"), true))
                   {
                       if (firstEntry)
                       {
                           writer.Write(ratio.ToString(CultureInfo.InvariantCulture));
                           firstEntry = false;
                       }
                       else
                       {
                           writer.Write(" {0}", ratio.ToString(CultureInfo.InvariantCulture));
                       }
                   }

                   logger.LogInformation("Несущая частота №{carrier}: SNR = {ratio}", i, ratio);
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
            for (int i = 0; i < fourierTransformBase; i++)
            {
                for (int j = 0; j < fourierTransformBase; j++)
                {
                    sum += njus[i][j] * Math.Cos(2 * PI * (p + firstChannelNumber - 1) * (i - j) / fourierTransformBase);
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
            for (int i = 0; i < fourierTransformBase; i++)
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
