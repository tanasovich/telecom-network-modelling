using Microsoft.Extensions.Logging;
using Moq;
using System.Globalization;
using TelecomNetModelling;

namespace TestModelTelecomNet
{
    public class PositiveSystemTest
    {
        [Fact]
        public void TestTraditionalSystemComputation()
        {
            // Arrange
            List<double> expected = LoadDataFrom(Path.Combine("ExpectedData", "Traditional", "interf.txt").ToString());
            Dictionary<string, List<double>> inputs = new Dictionary<string, List<double>>();
            inputs["impulseReactions"] = LoadDataFrom(Path.Combine("TestData", "Traditional", "impulse-reactions.txt"));
            inputs["signalMask"] = LoadDataFrom(Path.Combine("TestData", "Traditional", "mask.txt"));
            NetworkValueCalculator calculator = new NetworkValueCalculator(inputs, Mock.Of<ILogger<NetworkValueCalculator>>());

            // Act
            calculator.Execute();

            // Assert
        }

        private List<double> LoadDataFrom(string filename)
        {
            List<double> values = new();
            using (StreamReader sr = new StreamReader(@filename))
            {
                string? line;
                while ((line = sr.ReadLine()) != null)
                {
                    string[] text = line.Split(' ');
                    foreach (string word in text)
                    {
                        values.Add(double.Parse(word, CultureInfo.InvariantCulture));
                    }
                }
            }
            return values;
        }
    }
}