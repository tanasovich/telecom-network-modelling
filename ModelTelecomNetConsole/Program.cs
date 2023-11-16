using System.Globalization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

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
            
            List<double> impulseReactions = ReadPrnFile(
                configuration["AppSettings:impulseReactionsFilename"]!
            );
            logger.LogInformation("Loaded impulse reactions.");

            List<double> signalMask = ReadPrnFile(
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

        private static List<double> ReadPrnFile(string filename)
        {
            List<double> values = new();
            try
            {
                using (StreamReader sr = new StreamReader(@filename))
                {
                    logger.LogDebug($"Reading data from {filename}");
                    string? line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        string[] text = line.Split(' ');
                        foreach (string word in text)
                        {
                            try
                            {
                                values.Add(double.Parse(word, CultureInfo.InvariantCulture));
                                logger.LogTrace($"Parsed number {word}");
                            }
                            catch (FormatException)
                            {
                                logger.LogWarning("Invalid number data: '{word}' in {filename}", word, filename);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
            return values;
        }
    }
}
