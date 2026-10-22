namespace Monopoly.Core.Interfaces;

/// <summary>
/// Basic contract for player entity.
/// </summary>
public interface IPlayer
{
    int Id { get; }
    string Name { get; }

    // IToken Token { get; }

    /// <summary>The player's cash. There are no bill denominations, only one whole-number balance.</summary>
    int Money { get; set; }

    bool IsBankrupt { get; set; }

    bool IsInJail { get; set; }

    /// <summary>Number of Get Out of Jail Free cards the player is holding.</summary>
    int JailFreeCard { get; set; }
}
