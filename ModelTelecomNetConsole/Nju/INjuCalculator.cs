using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelTelecomNetConsole.Nju
{
    public interface INjuCalculator
    {
        /// <summary>
        /// Compute impulse reactions' integral
        /// </summary>
        /// <param name="k">first impulse reaction index</param>
        /// <param name="q">second impulse reaction index</param>
        /// <param name="currentSample">current sample, canonical name - <i>lt</i></param>
        /// <returns>imulse reaction integral</returns>
        double Nju(int k, int q, int currentSample);
    }
}
