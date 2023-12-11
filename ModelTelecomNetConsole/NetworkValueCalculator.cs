using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ModelTelecomNetConsole;
using ModelTelecomNetConsole.Interference;
using ModelTelecomNetConsole.Nju;
using ModelTelecomNetConsole.Signal;

namespace TelecomNetModelling
{
    public class NetworkValueCalculator
    {

       private readonly ILogger logger;
       private GivenData given;

       /// <summary>
       ///  Path to file results. Relative to working directory.
       /// </summary>
       private readonly string resultsDirectory;

       private readonly INjuCalculator njuCalculator;
       
       /// <summary>
       /// Interferation model matrix for previous sample.
       /// </summary>
       private  double[,] njus;
       
       /// <summary>
       /// Interferation model matrix for current sample.
       /// </summary>
       private  double[,] currrentNjus;
       
       private IInterferenceStrategy _interferenceStrategy;
       private ISignalStrategy _signalStrategy;
       
       public IInterferenceStrategy InterferenceStrategy
        {
            get
            {
                return _interferenceStrategy;
            }
            set
            {
                _interferenceStrategy = value;
            }
        }

        public ISignalStrategy SignalStrategy
        {
            get
            {
                return _signalStrategy;
            }
            set
            {
                _signalStrategy = value;
            }
        }

       public NetworkValueCalculator(GivenData given, ILogger logger, string resultsDirectory)
        {
            this.logger = logger;

            this.given = given;

            this.resultsDirectory = resultsDirectory;

            njuCalculator = new TraditionalNjuCalculator(given);

            BuildNjuMatrixes(out njus, out currrentNjus);

            _interferenceStrategy = new TraditionalInterferenceStrategy(given);
            _signalStrategy = new TraditionalSignalStrategy(given);
        }

        private void BuildNjuMatrixes(out double[,] mu, out double[,] currentMu)
        {
            mu = new double[given.FourierTransformBase, given.FourierTransformBase];
            currentMu = new double[given.FourierTransformBase, given.FourierTransformBase];
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
                           currrentNjus[i, j] = njus[i + 1, j + 1];
                           njus[i, j] = currrentNjus[i, j];
                           njus[j, i] = currrentNjus[i, j];

                           logger.LogTrace("Mystical check is done. Take bottom-right element from previous Nju matrix LT {currentSample}", currentSample);
                           logger.LogTrace("Mystical check indexes: i = {i}, j = {j}", i, j);

                           continue;
                       }
                       
                       currrentNjus[i, j] = njuCalculator.Nju(i, j, currentSample);
                       njus[i, j] = currrentNjus[i, j];
                       njus[j, i] = currrentNjus[i, j];

                       logger.LogTrace("nju({i}, {j}) = {currentNju}", i, j, currrentNjus[i, j]);
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
            ratio = Math.Sqrt(_interferenceStrategy.InterferationNoisePower(power, njus) / _signalStrategy.SignalPower(power));
            return ratio * 100.0;
        }
    }
}
