using Microsoft.Extensions.Logging;
using System.Globalization;

namespace TelecomNetModelling.Readers
{
    public class GivenDataPrnReader: IGivenDataReader
    {
        private readonly ILogger logger;

        public GivenDataPrnReader(ILogger logger)
        {
            this.logger = logger;
        }

        public List<double> ReadFile(string filename)
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
