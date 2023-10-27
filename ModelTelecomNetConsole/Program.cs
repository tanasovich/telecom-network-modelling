using Microsoft.Extensions.Configuration;

// N. B. Пока что просто берем константы из cpp-файла, описания сообщат позже.
namespace TelecomNetModelling
{
    class Program
    {
        /// Программа расчета для традиционных систем. Пример, файл - TWP_GFAST_150m_TR.cpp
        /// Для начала, реализовать выбор входных данных только через конфиги.
        /// Ввводные файлы должны быть прописаны в конфигах, а также должна быть возможность
        /// вводить кастомные имена через консоль.
        /// Начать работу с ввода информации (заполнение массивов).
        static void Main(string[] args)
        {
            // Чтение конфигурациинных параметров.
            ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.SetBasePath(AppDomain.CurrentDomain.BaseDirectory);
            configurationBuilder.AddXmlFile("appsettings.xml");

            // Чтение матрицы длительностей импульсных реакций.

           // Чтение матрицы спектральной маски.

            IConfiguration configuration = configurationBuilder.Build();
            NetworkValueCalculator calculator = new NetworkValueCalculator(
                configuration,
                Array.Empty<double>(),
                Array.Empty<double>(),
                Array.Empty<double>()
            );

           calculator.Execute();
           
           // Вывод результатов в консоль.
           
           // Запись результатов в файл.
        }
    }
}
