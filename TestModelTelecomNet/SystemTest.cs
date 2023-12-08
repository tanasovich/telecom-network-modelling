using Microsoft.Extensions.Logging;
using ModelTelecomNetConsole;
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
            if (Directory.Exists("results"))
            {
                Directory.Delete("results", true);
            }
            List<double> firstExpected = LoadDataFrom(Path.Combine("ExpectedData", "Traditional", "interf0.txt").ToString());
            List<double> secondExpected = LoadDataFrom(Path.Combine("ExpectedData", "Traditional", "interf150.txt").ToString());

            Directory.CreateDirectory("results");

            GivenData given = new GivenData(512, 200, 32, 30, 0, 150, 60);
            given.ImpulseReactions = LoadDataFrom(Path.Combine("TestData", "Traditional", "impulse-reactions.txt"));
            given.SignalMask = LoadDataFrom(Path.Combine("TestData", "Traditional", "mask.txt"));

            NetworkValueCalculator calculator = new NetworkValueCalculator(
                given,
                Mock.Of<ILogger<NetworkValueCalculator>>()
            );

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