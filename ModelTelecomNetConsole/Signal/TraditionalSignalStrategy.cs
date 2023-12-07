using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ModelTelecomNetConsole.Signal
{
    public class TraditionalSignalStrategy : AccurateMathematic, ISignalStrategy
    {
        private GivenData given;
        private List<double> impulseReactions;
        private List<double> signalPowers;

        public TraditionalSignalStrategy(GivenData given, List<double> impulseReactions, List<double> signalPowers)
        {
            this.given = given;
            this.impulseReactions = impulseReactions;
            this.signalPowers = signalPowers;
        }

        /// <summary>
        /// The power of active signal.
        /// </summary>
        /// <param name="p">power</param>
        /// <returns>active signal power</returns>
        /// <remarks>Canonical name - <i>Signal</i></remarks>
        public double SignalPower(int p)
        {
            Complex sum = new Complex();
            Complex J = new Complex(0, 1);
            for (int i = 0; i < given.FourierTransformBase; i++)
            {
                sum += Complex.Multiply(impulseReactions[i],
                    Complex.Exp(
                        Complex.Multiply(-J,
                            2.0 * PI * (double)(p + given.FirstChannelNumber - 1) * (double)i / (double)given.FourierTransformBase
                        )
                    )
                );
            }
            return Math.Pow(Complex.Abs(sum), 2) * given.FourierTransformBase * given.FourierTransformBase / 2.0 * signalPowers[p + given.FirstChannelNumber - 1];
        }
    }
}
