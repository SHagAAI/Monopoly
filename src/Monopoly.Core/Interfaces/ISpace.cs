using Monopoly.Core.Enums;

namespace Monopoly.Core.Interfaces;

/// <summary>
/// Contract representing a square on the Monopoly board (SPC-01 to SPC-10).
/// </summary>
public interface ISpace
{
    string Name { get; }
 
    SpaceType Type { get; }
 
    /// <summary>
    /// The property on this square. A street, railroad or utility square has one.
    /// All other squares have none (null).
    /// </summary>
    IProperty? Property { get; set; }
 
    /// <summary>The tokens currently standing on this square.</summary>
    List<IToken> Tokens { get; }
}
