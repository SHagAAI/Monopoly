using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Monopoly.Core.Enums;
using Monopoly.Core.Interfaces;

namespace Monopoly.Core.Models
{
    public class Token
    {
        public TokenColor Color { get; }
        public ISpace CurrentPosition { get; set; }

        public Token(TokenColor color, ISpace currentPosition)
        {
            Color = color;
            CurrentPosition = currentPosition;
        }
    }
}