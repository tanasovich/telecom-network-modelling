using System.Globalization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

// N. B. Пока что просто берем константы из cpp-файла, описания сообщат позже.
namespace TelecomNetModelling
{
    class Program
    {
        private static ILoggerFactory loggerFactory = LoggerFactory.Create(
            builder => builder.
                SetMinimumLevel(LogLevel.Debug).
                AddSimpleConsole()
        );
        private static readonly ILogger logger = loggerFactory.CreateLogger<Program>();
        /// Программа расчета для традиционных систем. Пример, файл -
        /// TWP_GFAST_150m_TR.cpp
        /// Для начала, реализовать выбор входных данных только через конфиги.
        /// Ввводные файлы должны быть прописаны в конфигах, а также должна быть
        /// возможность
        /// вводить кастомные имена через консоль.
        /// Начать работу с ввода информации (заполнение массивов).
        public static void Main()
        {
            ConfigurationBuilder configurationBuilder = new();
            configurationBuilder.SetBasePath(
                AppDomain.CurrentDomain.BaseDirectory
            );
            configurationBuilder.AddXmlFile("appsettings.xml");

            IConfiguration configuration = configurationBuilder.Build();
            logger.LogInformation("Загружена конфигурация.");
            
            List<double> impulseReactions = ReadPrnFile(
                configuration["AppSettings:impulseReactionsFilename"]!
            );
            logger.LogInformation("Загружены импульсные реакции.");

            List<double> signalMask = ReadPrnFile(
                configuration["AppSettings:maskFilename"]!
            );
            logger.LogInformation("Загружена спектральная маска");

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

           logger.LogInformation("Начало расчета...");
           calculator.Execute();
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
