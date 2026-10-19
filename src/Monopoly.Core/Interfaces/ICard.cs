using Monopoly.Core.Enums;

namespace Monopoly.Core.Interfaces;

/// <summary>
/// Basic contract for Chance and Community Chest cards (CRD-01 to CRD-09).
/// </summary>

     /// <summary>
/// A Chance or Community Chest card. This is only data.
/// </summary>
public interface ICard
{
    /// <summary>Which deck the card belongs to.</summary>
    CardType CardType { get; }
 
    /// <summary>What the card does when it is drawn.</summary>
    CardEffect CardEffect { get; }
 
    /// <summary>The text shown to the player, for example "Advance to Illinois Avenue. If you pass Go, collect $200."</summary>
    string Description { get; }
 
    /// <summary>
    /// The number that gives the effect its detail, or null when the effect needs none.
    /// MoveToPosition: path index of the target square.
    /// MoveSteps: number of steps (negative = backwards).
    /// ReceiveMoney, PayMoney, PayEachPlayer, ReceiveFromEachPlayer: the amount.
    /// GoToJail, GetOutOfJailFree, Repairs: null (repair amounts are set in the game controller).
    /// </summary>
    int? Value { get; }
}
