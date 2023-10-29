using System.Globalization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

// N. B. Пока что просто берем константы из cpp-файла, описания сообщат позже.
namespace TelecomNetModelling
{
    class Program
    {
        private static ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddSimpleConsole());
        /// Программа расчета для традиционных систем. Пример, файл -
        /// TWP_GFAST_150m_TR.cpp
        /// Для начала, реализовать выбор входных данных только через конфиги.
        /// Ввводные файлы должны быть прописаны в конфигах, а также должна быть
        /// возможность
        /// вводить кастомные имена через консоль.
        /// Начать работу с ввода информации (заполнение массивов).
        public static void Main()
        {
            ILogger logger = loggerFactory.CreateLogger<Program>();
            logger.LogInformation("I created logger.");
            // Чтение конфигурациинных параметров.
            ConfigurationBuilder configurationBuilder = new();
            configurationBuilder.SetBasePath(
                AppDomain.CurrentDomain.BaseDirectory
            );
            configurationBuilder.AddXmlFile("appsettings.xml");

            IConfiguration configuration = configurationBuilder.Build();
            
            List<double> impulseReactions = ReadPrnFile(
                configuration["AppSettings:impulseReactionsFilename"]!
            );
            foreach (double impulse in impulseReactions)
            {
                Console.WriteLine(impulse);
            }

            List<double> signalMask = ReadPrnFile(
                configuration["AppSettings:maskFilename"]!
            );

            Dictionary<string, List<double>> inputs = new()
            {
                { "impulseReactions", impulseReactions },
                { "signalMask", signalMask }
            };


            NetworkValueCalculator calculator = new(
                configuration,
                inputs
            );

           calculator.Execute();
           
           // Вывод результатов в консоль.
           
           // Запись результатов в файл.
        }

        private static List<double> ReadPrnFile(string filename)
        {
            List<double> values = new();
            try
            {
                using (StreamReader sr = new StreamReader(@filename))
                {
                    string? line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        string[] text = line.Split(' ');
                        foreach (string word in text)
                        {
                            try
                            {
                                values.Add(double.Parse(word, CultureInfo.InvariantCulture));
                            }
                            catch (FormatException)
                            {
                                Console.WriteLine($"Invalid number data: '{word}'");
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
