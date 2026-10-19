using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Monopoly.Core.Interfaces;

namespace Monopoly.Core.Models
{
    public class Dice : IDice
    {
        public int Side1 { get; set; }
        public int Side2 { get; set; }
    }
}