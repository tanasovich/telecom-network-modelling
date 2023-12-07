using System.Globalization;
using System.Numerics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ModelTelecomNetConsole;
using ModelTelecomNetConsole.Interference;
using ModelTelecomNetConsole.Signal;

namespace TelecomNetModelling
{
    public class NetworkValueCalculator
    {

       private readonly ILogger logger;
       private GivenData given;
       
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
       
       private IInterferenceStrategy InterferenceStrategy;
       private ISignalStrategy SignalStrategy;

       public NetworkValueCalculator(IConfiguration configuration,
           Dictionary<string, List<double>> inputs, ILogger logger)
        {
            this.logger = logger;

            given = new GivenData(
                int.Parse(configuration["AppSettings:fourierTransformBase"]!),
                int.Parse(configuration["AppSettings:carrierFrequencyMaxNumber"]!),
                int.Parse(configuration["AppSettings:protectionIntervalSamplesNumber"]!),
                int.Parse(configuration["AppSettings:firstChannelNumber"]!),
                int.Parse(configuration["AppSettings:firstSample"]!),
                int.Parse(configuration["AppSettings:lastSample"]!),
                int.Parse(configuration["AppSettings:impulseReactionLength"]!)
            );

            impulseReactions = inputs["impulseReactions"];
            signalMask = inputs["signalMask"];

            resultsDirectory = configuration["AppSettings:resultsDirectory"]!;

            GenerateSignalPowers();

            njuCalculator = new NjuCalculator(
             given, impulseReactions, signalPowers
            );

            BuildNjuMatrixes();

            InterferenceStrategy = new TraditionalInterferenceStrategy(given, njus);
            SignalStrategy = new TraditionalSignalStrategy(given, impulseReactions, signalPowers);
        }

        public NetworkValueCalculator(Dictionary<string, List<double>> inputs, ILogger logger)
        {
            given = new GivenData(512, 200, 32, 30, 0, 150, 60);
            resultsDirectory = "results";
            this.logger = logger;

            impulseReactions = inputs["impulseReactions"];
            signalMask = inputs["signalMask"];

            GenerateSignalPowers();

            njuCalculator = new NjuCalculator(
             given, impulseReactions, signalPowers
            );

            BuildNjuMatrixes();

            InterferenceStrategy = new TraditionalInterferenceStrategy(given, njus);
            SignalStrategy = new TraditionalSignalStrategy(given, impulseReactions, signalPowers);
        }

        private void BuildNjuMatrixes()
        {
            njus = new List<List<double>>(given.FourierTransformBase);
            currrentNjus = new List<List<double>>(given.FourierTransformBase);
            for (int i = 0; i < given.FourierTransformBase; i++)
            {
                njus.Add(new List<double>(given.FourierTransformBase));
                currrentNjus.Add(new List<double>(given.FourierTransformBase));
                for (int j = 0; j < given.FourierTransformBase; j++)
                {
                    njus[i].Add(default);
                    currrentNjus[i].Add(default);
                }
            }
        }

        private void GenerateSignalPowers()
        {
            signalPowers = new List<double>();
            for (int i = 0; i < given.FirstChannelNumber + given.CarrierFrequencyMaxNumber; i++)
            {
                signalPowers.Add(Math.Pow(10, 0.1 * (signalMask[i] + 80)));
            }
            logger.LogInformation("Calculated sinal powers (using mask).");
        }

        /// <summary>
        /// Facade method. Performs whole business logic.
        /// </summary>
        public void Execute()
       {
           for (int currentSample = given.FirstSample; currentSample <= given.LastSample; currentSample++)
           {
               DateTime sampleComputeStart = DateTime.Now;
               logger.LogInformation("Calculating LT = {currentSample} @ {start}", currentSample, sampleComputeStart);
               
               for (int i = 0; i < given.FourierTransformBase; i++)
               {
                   logger.LogInformation("Current array segment: {index}", i);
                   for (int j = 0; j <= i; j++)
                   {
                       // XXX: Nobody understands this check and action. Don't try to modify this section
                       if (currentSample != given.FirstSample && i != given.FourierTransformBase - 1 && j != given.FourierTransformBase - 1)
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
               for (int i = 0; i < given.FourierTransformBase; i++)
               {
                   for (int j = 0; j < given.FourierTransformBase; j++)
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
               }

               bool firstEntry = true;
               for (int i = 0; i <= given.CarrierFrequencyMaxNumber; i++)
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

               DateTime sampleComputeEnd = DateTime.Now;
               TimeSpan consumedBySample = sampleComputeEnd - sampleComputeStart;
               logger.LogInformation("Calculation for LT = {sample} is done @ {end}", currentSample, sampleComputeEnd);
               logger.LogInformation("LT = {sample} consumed {time}", currentSample, consumedBySample);
           }
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
            ratio = Math.Sqrt(InterferenceStrategy.InterferationNoisePower(power) / SignalStrategy.SignalPower(power));
            return ratio * 100.0;
        }
    }
}
