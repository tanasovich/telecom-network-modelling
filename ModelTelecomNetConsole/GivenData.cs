using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelTelecomNetConsole
{
    public class GivenData
    {
        /// <summary>
        ///  <para>Fourier transformation base. The number of samples
        ///  at orthogonal interval.</para>
        ///  <para>Used as size of input matrix.</para>
        /// </summary>
        /// <remarks>Canonical name - <i>N</i></remarks>
        private readonly int fourierTransformBase;

        /// <summary>
        /// The number of maximum carrier frequency.
        /// </summary>
        /// <remarks>Canonical name - <i>n</i></remarks>
        private readonly int carrierFrequencyMaxNumber;

        /// <summary>
        /// The number of samples at proctecting interval.
        /// </summary>
        /// <remarks>Canonical name - <i>L</i></remarks>
        private readonly int protectionIntervalSamplesNumber;

        /// <summary>
        /// First channel number.
        /// </summary>
        /// <remarks>Canonical name - <i>m</i></remarks>
        private readonly int firstChannelNumber;

        /// <summary>
        /// Starting point.
        /// </summary>
        /// <remarks>Canonical name - <i>from_lt</i></remarks>
        private readonly int firstSample;

        /// <summary>
        /// Ending point.
        /// </summary>
        /// <remarks>Canonical name - <i>until_lt</i></remarks>
        private readonly int lastSample;

        /// <summary>
        /// Impulse reaction length. Value depends on file size.
        /// </summary>
        /// <remarks>Canonical name - <i>R</i></remarks>
        private readonly int impulseReactionLength;

        public GivenData(int fourierTransformBase, int carrierFrequencyMaxNumber, int protectionIntervalSamplesNumber, int firstChannelNumber, int firstSample, int lastSample, int impulseReactionLength)
        {
            this.fourierTransformBase = fourierTransformBase;
            this.carrierFrequencyMaxNumber = carrierFrequencyMaxNumber;
            this.protectionIntervalSamplesNumber = protectionIntervalSamplesNumber;
            this.firstChannelNumber = firstChannelNumber;
            this.firstSample = firstSample;
            this.lastSample = lastSample;
            this.impulseReactionLength = impulseReactionLength;
        }

        /// <inheritdoc cref="fourierTransformBase"/>
        public int FourierTransformBase => fourierTransformBase;

        /// <inheritdoc cref="carrierFrequencyMaxNumber"/>
        public int CarrierFrequencyMaxNumber => carrierFrequencyMaxNumber;

        /// <inheritdoc cref="protectionIntervalSamplesNumber"/>
        public int ProtectionIntervalSamplesNumber => protectionIntervalSamplesNumber;

        /// <inheritdoc cref="firstChannelNumber"/>
        public int FirstChannelNumber => firstChannelNumber;

        /// <inheritdoc cref="firstSample"/>
        public int FirstSample => firstSample;

        /// <inheritdoc cref="lastSample"/>
        public int LastSample => lastSample;

        /// <inheritdoc cref="impulseReactionLength"/>
        public int ImpulseReactionLength => impulseReactionLength;
    }
}
