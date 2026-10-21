using Monopoly.Core.Enums;
using Monopoly.Core.Interfaces;

namespace Monopoly.Core.Models
{
    public class Token : IToken
    {
        public TokenColor Color { get; }
       

        public Token(TokenColor color)
        {
            Color = color;
            
        }
    }
}