using System.Globalization;
using System.Numerics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace TelecomNetModelling
{
    public class NetworkValueCalculator
    {
        // Accuracy of built-in PI constant is not enough.
       public const double PI = 3.14159265358979323846;

       private readonly ILogger logger;
       
       /// <summary>
       ///  <para>Fourier transformation base. The number of samples
       ///  at orthogonal interval.</para>
       ///  <para>Used as size of input matrix.</para>
       /// </summary>
       /// <remarks>Canonical name - <i>N</i></remarks>
       private readonly int fourierTransformBase;

       /// <summary>
       /// The number of maximum carrier frequency.
       /// </summary>
       /// <remarks>Canonical name - <i>n</i></remarks>
       private readonly int carrierFrequencyMaxNumber;

       /// <summary>
       /// The number of samples at proctecting interval.
       /// </summary>
       /// <remarks>Canonical name - <i>L</i></remarks>
       private readonly int protectionIntervalSamplesNumber;
       
       /// <summary>
       /// First channel number.
       /// </summary>
       /// <remarks>Canonical name - <i>m</i></remarks>
       private readonly int firstChannelNumber;

       /// <summary>
       /// Starting point.
       /// </summary>
       /// <remarks>Canonical name - <i>from_lt</i></remarks>
       private readonly int firstSample;

       /// <summary>
       /// Ending point.
       /// </summary>
       /// <remarks>Canonical name - <i>until_lt</i></remarks>
       private readonly int lastSample;

       /// <summary>
       /// Impulse reaction length. Value depends on file size.
       /// </summary>
       /// <remarks>Canonical name - <i>R</i></remarks>
       private readonly int impulseReactionLength;
       
       /// <summary>
       /// Impulse reactions' list.
       /// </summary>
       /// <remarks>Canonical name - <i>g</i></remarks>
       private List<double> impulseReactions;
       
       /// <summary>
       /// Signal powers' list.
       /// </summary>
       /// <remarks>Canonical name - <i>power</i></remarks>
       private List<double> signalPowers;
       
       /// <summary>
       /// Signal mask.
       /// </summary>
       /// <remarks>Canonical name - <i>PSD</i></remarks>
       private List<double> signalMask;

       /// <summary>
       ///  Path to file results. Relative to working directory.
       /// </summary>
       private readonly string resultsDirectory;

       private readonly NjuCalculator njuCalculator;
       
       /// <summary>
       /// Interferation model matrix for previous sample.
       /// </summary>
       private  List<List<double>> njus;
       
       /// <summary>
       /// Interferation model matrix for current sample.
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

           if (signalMask.Count < firstChannelNumber + carrierFrequencyMaxNumber + 1)
           {
               logger.LogError(
                    "Signal mask length {mask} is not enough to generate {boundary} powers.",
                    signalMask.Count,
                    firstChannelNumber + carrierFrequencyMaxNumber + 1
                );
                Environment.Exit(1);
           }

           signalPowers = new List<double>();
           for (int i = 0; i < firstChannelNumber + carrierFrequencyMaxNumber + 1; i++)
           {
               signalPowers.Add(Math.Pow(10, 0.1 * (signalMask[i] + 80)));
           }
           logger.LogInformation("Calculated sinal powers (using mask).");

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

       /// <summary>
       /// Facade method. Performs whole business logic.
       /// </summary>
       public void Execute()
       {
           for (int currentSample = firstSample; currentSample <= lastSample; currentSample++)
           {
               logger.LogInformation("Calculating LT = {currentSample}", currentSample);
               for (int i = 0; i < fourierTransformBase; i++)
               {
                   // TODO: j could be initialized by i. We traverse top triangle only.
                   for (int j = 0; j < fourierTransformBase; j++)
                   {
                       if (i < j)
                       {
                           continue;
                       }
                       
                       // XXX: Nobody understands this check and action. Don't try to modify this section
                       if (currentSample != firstSample && i != fourierTransformBase - 1 && j != fourierTransformBase - 1)
                       {
                           currrentNjus[i][j] = njus[i + 1][j + 1];

                           logger.LogTrace("Mystical check is done. Take bottom-right element from previous Nju matrix LT {currentSample}", currentSample);
                           logger.LogTrace("Mystical check indexes: i = {i}, j = {j}", i, j);

                           continue;
                       }
                       
                       currrentNjus[i][j] = njuCalculator.Nju(i, j, currentSample);

                       logger.LogTrace("nju({i}, {j}) = {currentNju}", i, j, currrentNjus[i][j]);
                   }
               }

               // NOTE: To "mirror" matrix triangles, we use separate(!) for loops.
               // It's non-destructive approach, until I receive proper consultation from scientists.
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

                   logger.LogDebug("Carrier frequency №{carrier}: SNR = {ratio}", i, ratio);
               }
           }
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
        /// The power of active signal.
        /// </summary>
        /// <param name="p">power</param>
        /// <returns>active signal power</returns>
        /// <remarks>Canonical name - <i>Signal</i></remarks>
        public double SignalPower(int p)
        {
            Complex sum = new Complex();
            Complex J = new Complex(0, 1);
            for (int i = 0; i < fourierTransformBase; i++)
            {
                sum += Complex.Multiply(impulseReactions[i],
                    Complex.Exp(
                        Complex.Multiply(-J,
                            2.0 * PI * (double) (p + firstChannelNumber - 1) * (double) i / (double) fourierTransformBase
                        )
                    )
                );
            }
            return Math.Pow(Complex.Abs(sum), 2) * fourierTransformBase * fourierTransformBase / 2.0 * signalPowers[p + firstChannelNumber - 1];
        }

        /// <summary>
        /// Signal-to-noise ratio.
        /// </summary>
        /// <param name="power">power</param>
        /// <returns>SNR ration</returns>
        /// <remarks>Canonical name - <i>Ratio</i></remarks>>
        public double SNR(int power)
        {
            double ratio;
            ratio = Math.Sqrt(InterferationNoisePower(power) / SignalPower(power));
            return ratio * 100.0;
        }
    }
}
