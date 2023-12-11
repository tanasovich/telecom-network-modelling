using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ModelTelecomNetConsole;
using TelecomNetModelling.Readers;

namespace TelecomNetModelling
{
    class Program
    {
        private static ILoggerFactory loggerFactory = LoggerFactory.Create(
            builder => builder.
                SetMinimumLevel(LogLevel.Information).
                AddSimpleConsole()
        );
        private static readonly ILogger logger = loggerFactory.CreateLogger<Program>();

        private static readonly IGivenDataReader givenDataReader = new GivenDataPrnReader(
            loggerFactory.CreateLogger<IGivenDataReader>());
        
        public static void Main()
        {
            ConfigurationBuilder configurationBuilder = new();
            configurationBuilder.SetBasePath(
                AppDomain.CurrentDomain.BaseDirectory
            );
            configurationBuilder.AddXmlFile("appsettings.xml");

            IConfiguration configuration = configurationBuilder.Build();
            logger.LogInformation("Configuration data is loaded.");

            logger.LogInformation("Environment preparation for results storing.");
            Directory.CreateDirectory(configuration["AppSettings:resultsDirectory"]!);
            
            List<double> impulseReactions = givenDataReader.ReadFile(
                configuration["AppSettings:impulseReactionsFilename"]!
            );
            logger.LogInformation("Loaded impulse reactions.");

            List<double> signalMask = givenDataReader.ReadFile(
                configuration["AppSettings:maskFilename"]!
            );
            logger.LogInformation("Loaded spectral mask.");

            Dictionary<string, List<double>> inputs = new()
            {
                { "impulseReactions", impulseReactions },
                { "signalMask", signalMask }
            };

            GivenData given = new GivenData(
                int.Parse(configuration["AppSettings:fourierTransformBase"]!),
                int.Parse(configuration["AppSettings:carrierFrequencyMaxNumber"]!),
                int.Parse(configuration["AppSettings:protectionIntervalSamplesNumber"]!),
                int.Parse(configuration["AppSettings:firstChannelNumber"]!),
                int.Parse(configuration["AppSettings:firstSample"]!),
                int.Parse(configuration["AppSettings:lastSample"]!),
                int.Parse(configuration["AppSettings:impulseReactionLength"]!)
            );
            given.ImpulseReactions = impulseReactions.ToArray<double>();
            given.SignalMask = signalMask.ToArray<double>();

            NetworkValueCalculator calculator = new(
                given,
                loggerFactory.CreateLogger<NetworkValueCalculator>(),
                configuration["AppSettings:resultsDirectory"]!
            );

           DateTime computingStart = DateTime.Now;
           logger.LogInformation("Beginning of the calculation @ {start}", computingStart);
           calculator.Execute();
           DateTime computingEnd = DateTime.Now;
           TimeSpan consumedTime = computingEnd - computingStart;
           logger.LogInformation("End of calculation @ {end} Total consumed time is {total}", computingEnd, consumedTime);
        }
    }
}
