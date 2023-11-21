using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

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


            NetworkValueCalculator calculator = new(
                configuration,
                inputs,
                loggerFactory.CreateLogger<NetworkValueCalculator>()
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
