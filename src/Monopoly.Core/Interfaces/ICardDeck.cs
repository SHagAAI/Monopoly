namespace Monopoly.Core.Interfaces;

/// <summary>
/// Contract for card decks (CRD-01 to CRD-04).
/// </summary>
public interface ICardDeck
{
    /// <summary>The cards in the deck, top card first.</summary>
    Queue<ICard> Cards { get; set; }
}
