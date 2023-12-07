using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelTelecomNetConsole.Signal
{
    public interface ISignalStrategy
    {
        double SignalPower(int p);
    }
}
