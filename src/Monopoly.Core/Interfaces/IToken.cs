
using Monopoly.Core.Enums;

namespace Monopoly.Core.Interfaces
{
    public interface IToken
    {
        TokenColor Color { get; }

        /// <summary>The square the token is standing on. The game controller changes it when the token moves.</summary>
    }
}