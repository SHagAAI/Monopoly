using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Monopoly.Core.Interfaces
{
    public interface IDice
    {
        /// <summary>Value of the first die (1 to 6). It is 0 before the first roll.</summary>
        int Side1 { get; set; }

        /// <summary>Value of the second die (1 to 6). It is 0 before the first roll.</summary>
        int Side2 { get; set; }
    }
}