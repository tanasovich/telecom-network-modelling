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
            Directory.Delete("results", true);
            List<double> firstExpected = LoadDataFrom(Path.Combine("ExpectedData", "Traditional", "interf0.txt").ToString());
            List<double> secondExpected = LoadDataFrom(Path.Combine("ExpectedData", "Traditional", "interf150.txt").ToString());

            Dictionary<string, List<double>> inputs = new Dictionary<string, List<double>>();
            inputs["impulseReactions"] = LoadDataFrom(Path.Combine("TestData", "Traditional", "impulse-reactions.txt"));
            inputs["signalMask"] = LoadDataFrom(Path.Combine("TestData", "Traditional", "mask.txt"));

            Directory.CreateDirectory("results");

            NetworkValueCalculator calculator = new NetworkValueCalculator(inputs, Mock.Of<ILogger<NetworkValueCalculator>>());

            // Act
            calculator.Execute();

            // Assert
            List<double> firstActual = LoadDataFrom(Path.Combine("results", "interf0"));
            Assert.Equal(firstExpected.Count, firstActual.Count);
            for (int i = 0; i < firstExpected.Count; i++)
            {
                Assert.Equal(firstExpected[i], firstActual[i], 4);
            }
            List<double> secondActual = LoadDataFrom(Path.Combine("results", "interf150"));
            Assert.Equal(secondExpected.Count, secondActual.Count);
            for (int i = 0; i < secondExpected.Count; i++)
            {
                Assert.Equal(secondExpected[i], secondActual[i], 4);
            }
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