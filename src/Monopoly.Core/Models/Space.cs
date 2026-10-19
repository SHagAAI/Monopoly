using Monopoly.Core.Enums;
using Monopoly.Core.Interfaces;

namespace Monopoly.Core.Models;

/// <summary>
/// Concrete implementation of a board square (SPC-01 to SPC-10).
/// </summary>
public class Space : ISpace
{
    public string Name { get; }
    public SpaceType Type { get; }
    public IProperty? Property { get; set; }
    public List<IToken> Tokens { get; } = new List<IToken>();
 
    public Space(string name, SpaceType type)
    {
        Name = name;
        Type = type;
    }
}
