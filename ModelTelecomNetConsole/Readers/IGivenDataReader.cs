using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelecomNetModelling.Readers
{
    internal interface IGivenDataReader
    {
        List<double> ReadFile(string filename);
    }
}
