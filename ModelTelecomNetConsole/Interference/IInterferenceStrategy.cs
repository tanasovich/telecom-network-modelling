using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelTelecomNetConsole.Interference
{
    public interface IInterferenceStrategy
    {
        double InterferationNoisePower(int p);
    }
}
