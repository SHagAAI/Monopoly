using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Monopoly.Core.Enums;
using Monopoly.Core.Interfaces;
using Monopoly.Core.Models;

namespace Monopoly.Core
{
    public class GameController
    {

        // public IBoard Board { get; }
        // public IDice Dice { get; }
        // public List<IPlayer> Players { get; }

        // private readonly ICardDeck _chanceDeck = new CardDeck();
        // private readonly ICardDeck _communityChestDeck = new CardDeck();

        // Random _random = new Random();

        // private int _currentPlayerIndex;

        // private const int MinPlayers = 2;
        // private const int MaxPlayers = 6;

        // public GameStatus Status { get; private set; } = GameStatus.NotStarted;

        // private readonly Dictionary<IPlayer, IToken> _playerTokens = new Dictionary<IPlayer, IToken>();
        // Dictionary<IToken, ISpace> _tokenPositions = new Dictionary<IToken, ISpace>();
        // Dictionary<IPlayer, int> _jailTurns = new Dictionary<IPlayer, int>();

        // public Action<IPlayer, int, int>? OnDiceRolled;

        // private readonly List<PendingDebts> _pendingDebts = new();


        // private bool _hasRolled;

        // public bool HasPendingDebt() => _pendingDebts.Count > 0;

        // /// <summary>Returns true if a specific player owes an unpaid debt.</summary>
        // public bool HasPendingDebt(IPlayer player) => _pendingDebts.Any(d => d.Debtor == player);


        // /// FLAG, Bug fixing, (Bug : Game cant continue when everyone decline buying property)
        // private bool _propertyDecisionResolved = true;

        // private readonly Dictionary<IProperty, IPlayer> _propertyOwners = new Dictionary<IProperty, IPlayer>();

        // private int _doubleCount;

        // public Action<IPlayer>? OnPlayerMoneyChanged;

        // public Action<IToken, ISpace>? OnPlayerMoved;

        // public ICard? LastDrawnCard { get; private set; }
        // public void ClearLastDrawnCard() => LastDrawnCard = null;


        // private const int StartingHouses = 32;
        // private const int StartingHotels = 12;


        // public int HouseAmount { get; set; } = StartingHouses;
        // public int HotelAmount { get; set; } = StartingHotels;


        // public GameController(IBoard board, IDice dice, List<IPlayer> players)
        // {
        //     Board = board;
        //     Dice = dice;
        //     Players = players;
        // }


        // public void InitializeBoard()
        // {
        //     Array.Clear(Board.Grid);
        //     Board.Path.Clear();

        //     InitializeSpace();
        //     InitializePath();
        // }

        // public void InitializeSpace()
        // {
        //     List<ISpace> spaces = BoardData.CreateSpaces();

        //     for (int index = 0; index < BoardData.SquareCount; index++)
        //     {
        //         (int row, int column) = GetGridPosition(index);
        //         Board.Grid[row, column] = spaces[index];
        //     }
        // }

        // public void InitializePath()
        // {
        //     for (int i = 0; i < BoardData.SquareCount; i++)
        //     {
        //         (int row, int col) = GetGridPosition(i);
        //         Board.Path.Add(Board.Grid[row, col]);

        //     }
        // }

        // private static (int row, int column) GetGridPosition(int index)
        // {
        //     // GridSize = 11, Monopoly size
        //     int last = Models.Board.GridSize - 1;

        //     if (index <= last)
        //     {
        //         return (last, last - index);          // bottom edge, right to left
        //     }

        //     if (index <= 2 * last)
        //     {
        //         return (2 * last - index, 0);         // left edge, bottom to top
        //     }

        //     if (index <= 3 * last)
        //     {
        //         return (0, index - 2 * last);         // top edge, left to right
        //     }

        //     return (index - 3 * last, last);
        // }

        // public bool InitializePlayers(List<IPlayer> players, List<IToken> tokens)
        // {
        //     if (Board.Path.Count != BoardData.SquareCount)
        //     {
        //         throw new InvalidOperationException("Initialize the board before the players.");
        //     }

        //     // ADD NEW VALIDATION
        //     if (players == null || tokens == null) return false;
        //     if (players.Count < MinPlayers || players.Count > MaxPlayers) return false;
        //     if (players.Count != tokens.Count) return false;

        //     ISpace go = Board.Path[0];

        //     // 1. Copy to local list first so clearing doesn't wipe the source
        //     var playerList = players.ToList();

        //     Players.Clear();
        //     _playerTokens.Clear();
        //     _tokenPositions.Clear();
        //     _jailTurns.Clear();

        //     // Reset everything left over from a previous game.
        //     foreach (ISpace space in Board.Path)
        //     {

        //         space.Tokens.Clear();
        //     }

        //     for (int i = 0; i < playerList.Count; i++)
        //     {
        //         IPlayer player = playerList[i];
        //         IToken token = tokens[i];

        //         // token.CurrentPosition = go;
        //         _tokenPositions[token] = go;
        //         go.Tokens.Add(token);

        //         Players.Add(player);
        //         _playerTokens[player] = token;
        //     }

        //     _currentPlayerIndex = 0;
        //     return true;
        // }

        // public void InitializeCardDecks()
        // {
        //     _chanceDeck.Cards = new Queue<ICard>(BoardData.CreateChanceCards());
        //     _communityChestDeck.Cards = new Queue<ICard>(BoardData.CreateCommunityChestCards());
        //     ShuffleCard(CardType.Chance);
        //     ShuffleCard(CardType.CommunityChest);
        // }


        // public void ShuffleCard(CardType cardType)
        // {
        //     ICardDeck deck = cardType == CardType.Chance ? _chanceDeck : _communityChestDeck;

        //     List<ICard> cardList = deck.Cards.ToList();

        //     // Fisher-Yates Shuffle
        //     for (int i = cardList.Count - 1; i > 0; i--)
        //     {
        //         int j = _random.Next(i + 1);
        //         // (cardList[i], cardList[j]) = (cardList[j], cardList[i]);
        //         ICard temp = cardList[i];
        //         cardList[i] = cardList[j];
        //         cardList[j] = temp;
        //     }

        //     deck.Cards = new Queue<ICard>(cardList);
        // }


        // public IPlayer GetCurrentPlayer()
        // {
        //     return Players[_currentPlayerIndex];
        // }

        // public void StartGame()
        // {
        //     if (Players.Count < MinPlayers)
        //     {
        //         throw new InvalidOperationException("Initialize the players before starting the game.");
        //     }

        //     Status = GameStatus.InProgress;
        // }


        // private int RollDice(IPlayer player)
        // {
        //     {
        //         Dice.Side1 = _random.Next(1, 7);
        //         Dice.Side2 = _random.Next(1, 7);
        //         OnDiceRolled?.Invoke(player, Dice.Side1, Dice.Side2);
        //         return Dice.Side1 + Dice.Side2;
        //     }
        // }

        // public void RollandMoveChecking()
        // {
        //     // Block if not allowed to roll right now
        //     if (!CanRoll()) return;

        //     _hasRolled = true;
        //     LastDrawnCard = null;


        //     // if there are someone who hasnt paid his Debt, the game cant continue
        //     if (Status != GameStatus.InProgress || HasPendingDebt()) return;

        //     IPlayer player = GetCurrentPlayer();
        //     IToken token = _playerTokens[player];

        //     int total = RollDice(player);
        //     bool isDouble = Dice.Side1 == Dice.Side2;

        //     // --- JAIL LOGIC (PLR-23, PLR-27, DCE-06) ---
        //     if (player.IsInJail)
        //     {
        //         if (isDouble)
        //         {
        //             // Freed by rolling doubles: moves forward, but does NOT get another roll (DCE-06)
        //             player.IsInJail = false;
        //             _jailTurns[player] = 0;
        //             _doubleCount = 0;
        //             Move(token, total);
        //         }
        //         else
        //         {
        //             int turns = _jailTurns.GetValueOrDefault(player, 0) + 1;
        //             _jailTurns[player] = turns;

        //             if (turns >= 3)
        //             {
        //                 // 3rd failed turn in jail: must pay $50, leaves jail, and moves by this roll (PLR-27)
        //                 if (!SendMoney(player, 50))
        //                 {
        //                     // Cannot pay $50 -> record debt to Bank and stay in jail
        //                     RecordDebt(player, 50, creditor: null);

        //                 }


        //                 player.IsInJail = false;
        //                 _jailTurns[player] = 0;
        //                 _doubleCount = 0;
        //                 Move(token, total);
        //             }
        //             // Otherwise, stays in jail and turn ends without moving
        //         }
        //         return;
        //     }

        //     // --- NORMAL MOVEMENT LOGIC (DCE-04, DCE-05) ---
        //     if (isDouble)
        //     {
        //         _doubleCount++;

        //         // 3 consecutive doubles sends the player directly to Jail (DCE-05)
        //         if (_doubleCount >= 3)
        //         {
        //             _doubleCount = 0;
        //             SendToJail(player);
        //             return;
        //         }

        //         // Moves, and player will be allowed to roll again on this turn
        //         Move(token, total);
        //     }
        //     else
        //     {
        //         _doubleCount = 0;
        //         Move(token, total);
        //     }
        // }

        // public bool CanRoll()
        // {
        //     if (Status != GameStatus.InProgress || HasPendingDebt()) return false;
        //     if (HasUnresolvedPropertyUnderfoot())
        //     {
        //         return false;
        //     }

        //     IPlayer player = GetCurrentPlayer();
        //     if (player.IsBankrupt) return false;

        //     // Allowed to roll if haven't rolled yet, OR if earned an extra roll via doubles
        //     return !_hasRolled || CanRollAgain();
        // }

        // public bool CanRollAgain()
        // {
        //     if (Status != GameStatus.InProgress || HasPendingDebt()) return false;

        //     IPlayer player = GetCurrentPlayer();

        //     // If the player is in jail or bankrupt, they can NEVER roll again on this turn
        //     if (player.IsInJail || player.IsBankrupt)
        //     {
        //         return false;
        //     }


        //     // Must be double, but strictly less than 3 consecutive doubles
        //     return _doubleCount > 0 && _doubleCount < 3;
        // }

        // public bool HasUnresolvedPropertyUnderfoot()
        // {
        //     if (_propertyDecisionResolved) return false; // auction is done already/bought
        //     var player = GetCurrentPlayer();
        //     var space = GetPlayerPosition(player);
        //     return space?.Property != null && GetPropertyOwner(space.Property) == null;
        // }

        // /// <summary>The owner of a property, or null if the Bank still owns it.</summary>
        // public IPlayer? GetPropertyOwner(IProperty property)
        // {
        //     return _propertyOwners.TryGetValue(property, out IPlayer? owner) ? owner : null;
        // }

        // public ISpace? GetPlayerPosition(IPlayer player)
        //    => _playerTokens.TryGetValue(player, out var token) && _tokenPositions.TryGetValue(token, out var space) ? space : null;

        // public void Move(IToken token, int steps, bool fromCard = false)
        // {
        //     if (!_tokenPositions.TryGetValue(token, out ISpace? currentSpace))
        //     {
        //         return;
        //     }

        //     int currentIndex = Board.Path.IndexOf(currentSpace);

        //     // the square doesnt exist or smth
        //     if (currentIndex == -1) return;

        //     // Find the player who owns this token
        //     IPlayer? player = _playerTokens.FirstOrDefault(kvp => kvp.Value == token).Key;
        //     if (player == null) return;

        //     int pathCount = Board.Path.Count;

        //     // Check if player passed or landed on GO (BRD-10, BRD-11)
        //     if (steps > 0 && currentIndex + steps >= pathCount)
        //     {
        //         ReceiveMoney(player, 200);
        //     }

        //     // Wrap-around calculation (handles positive steps or negative steps from cards)
        //     int targetIndex = (currentIndex + steps) % pathCount;

        //     // if below 0 means you still not pass Go
        //     if (targetIndex < 0)
        //     {
        //         targetIndex += pathCount;
        //     }

        //     // above zero means you pass go
        //     ISpace newSpace = Board.Path[targetIndex];

        //     // Update positions
        //     // remove the previous space the token stand on
        //     currentSpace.Tokens.Remove(token);

        //     // move to new space
        //     newSpace.Tokens.Add(token);

        //     // update the dictionary
        //     _tokenPositions[token] = newSpace;

        //     // reset FLAG, auction helper to continue the turn
        //     _propertyDecisionResolved = false;

        //     // Notify observers
        //     OnPlayerMoved?.Invoke(token, newSpace);

        //     // Resolve landing on the square
        //     ResolveLanding(player, fromCard);
        // }

        // public bool ReceiveMoney(IPlayer player, int amount, IPlayer otherPlayer)
        // {
        //     return SendMoney(otherPlayer, amount, player);
        // }

        // public void ReceiveMoney(IPlayer player, int amount)
        // {
        //     if (amount < 0) return;

        //     player.Money += amount;
        //     OnPlayerMoneyChanged?.Invoke(player);
        // }

        // public bool SendMoney(IPlayer player, int amount)
        // {
        //     if (amount < 0 || player.Money < amount) return false;

        //     player.Money -= amount;
        //     OnPlayerMoneyChanged?.Invoke(player);
        //     return true;
        // }

        // public bool SendMoney(IPlayer player, int amount, IPlayer otherPlayer)
        // {
        //     if (amount < 0 || player.Money < amount) return false;

        //     player.Money -= amount;
        //     otherPlayer.Money += amount;
        //     OnPlayerMoneyChanged?.Invoke(player);
        //     OnPlayerMoneyChanged?.Invoke(otherPlayer);
        //     return true;
        // }

        // public void ResolveLanding(IPlayer player, bool fromCard = false)
        // {
        //     IToken token = _playerTokens[player];
        //     ISpace space = _tokenPositions[token];

        //     switch (space.Type)
        //     {
        //         case SpaceType.Street:
        //         case SpaceType.Railroad:
        //         case SpaceType.Utility:
        //             if (space.Property != null)
        //             {
        //                 IPlayer? owner = GetPropertyOwner(space.Property);

        //                 if (owner != null && owner != player)
        //                 {
        //                     // Owned by another player: pay rent (SPC-03)
        //                     HandleRentPayment(player, space.Property, fromCard);
        //                 }
        //                 // If unowned: player can buy or trigger auction via BuyProperty() / HandleAuction()
        //             }
        //             break;

        //         case SpaceType.IncomeTax:
        //             // Flat $200 payment to the Bank (SPC-06)
        //             // If player can't pay the $200 Income Tax, record debt to the Bank (creditor = null)
        //             if (!SendMoney(player, BoardData.IncomeTaxAmount))
        //             {
        //                 RecordDebt(player, BoardData.IncomeTaxAmount, creditor: null);
        //             }

        //             break;

        //         case SpaceType.LuxuryTax:
        //             // Flat $100 payment to the Bank (SPC-07)
        //             // If player can't pay the $200 Income Tax, record debt to the Bank (creditor = null)
        //             if (!SendMoney(player, BoardData.LuxuryTaxAmount))
        //             {
        //                 RecordDebt(player, BoardData.LuxuryTaxAmount, creditor: null);
        //             }

        //             break;

        //         case SpaceType.GoToJail:
        //             // Sent directly to Jail (SPC-09)
        //             SendToJail(player);
        //             break;

        //         case SpaceType.Chance:
        //             DrawCard(player, CardType.Chance);
        //             break;

        //         case SpaceType.CommunityChest:
        //             DrawCard(player, CardType.CommunityChest);
        //             break;

        //         case SpaceType.Go:
        //         case SpaceType.Jail:
        //         case SpaceType.FreeParking:
        //             // No additional action needed (SPC-01, SPC-08, SPC-10)
        //             break;
        //     }
        // }

        // public void HandleRentPayment(IPlayer player, IProperty property, bool fromCard = false)
        // {
        //     if (player == null || property == null) return;

        //     IPlayer? owner = GetPropertyOwner(property);

        //     // Unowned or owned by self collects no rent (SPC-04)
        //     if (owner == null || owner == player) return;

        //     // Mortgaged properties collect no rent (PRP-24)
        //     if (property.IsMortgaged) return;

        //     int rent = 0;

        //     switch (property.PropertyType)
        //     {
        //         case PropertyType.Street:
        //             // Rent based on building level 0-5 (PRP-21)
        //             rent = property.Rents[property.BuildingLevel];

        //             // If unimproved (level 0), check for complete color group monopoly (PRP-22, PRP-23)
        //             if (property.BuildingLevel == 0 && property.ColorGroup.HasValue)
        //             {
        //                 bool ownsMonopoly = Board.Path
        //                     .Where(s => s.Property != null && s.Property.ColorGroup == property.ColorGroup)
        //                     .All(s => GetPropertyOwner(s.Property!) == owner);

        //                 if (ownsMonopoly)
        //                 {
        //                     rent *= 2; // Doubled rent on full unimproved color group
        //                 }
        //             }
        //             break;

        //         case PropertyType.Railroad:
        //             // Rent scales by total railroads owned: 1=$25, 2=$50, 3=$100, 4=$200 (PRP-29a, PRP-29b)
        //             // Note: Mortgaged railroads still count toward the total owned count (PRP-29b)
        //             int railroadsOwned = _propertyOwners.Count(kvp =>
        //                 kvp.Value == owner && kvp.Key.PropertyType == PropertyType.Railroad);

        //             if (railroadsOwned <= 0)
        //             {
        //                 rent = 0;
        //             }
        //             else
        //             {


        //                 int[] railroadRents = [25, 50, 100, 200];
        //                 rent = railroadRents[railroadsOwned - 1];
        //                 // rent = railroadRents[Math.Clamp(railroadsOwned - 1, 0, 3)];

        //                 if (fromCard)
        //                 {
        //                     rent *= 2;
        //                 }
        //             }
        //             break;

        //         case PropertyType.Utility:
        //             // Rent is 4x or 10x the dice roll total (PRP-26, PRP-27, PRP-29c)
        //             int utilitiesOwned = _propertyOwners.Count(kvp =>
        //                 kvp.Value == owner && kvp.Key.PropertyType == PropertyType.Utility);

        //             if (utilitiesOwned <= 0)
        //             {
        //                 rent = 0;
        //             }
        //             else if (fromCard)
        //             {
        //                 // int reroll = _random.Next(1, 7) + _random.Next(1, 7);
        //                 /// if you land on utility because of card, you got 
        //                 /// automatically charge 10x

        //                 int reroll = RollDice(player);
        //                 rent = reroll * 10;
        //             }
        //             else
        //             {

        //                 int diceTotal = Dice.Side1 + Dice.Side2;
        //                 int multiplier = (utilitiesOwned >= 2) ? 10 : 4;
        //                 rent = diceTotal * multiplier;
        //             }
        //             break;
        //     }

        //     // Transfer rent from the landing player to the owner


        //     if (!SendMoney(player, rent, owner))
        //     {
        //         // _pendingDebtAmount = rent;
        //         // _pendingDebtor = player;
        //         // _pendingCreditor = owner;

        //         RecordDebt(debtor: player, amount: rent, creditor: owner);
        //     }
        // }
        // private void RecordDebt(IPlayer debtor, int amount, IPlayer? creditor)
        // {

        //     _pendingDebts.Add(new PendingDebts(debtor, creditor, amount));
        // }
        // public void SendToJail(IPlayer player)
        // {
        //     player.IsInJail = true;
        //     _jailTurns[player] = 0;
        //     _doubleCount = 0;

        //     IToken token = _playerTokens[player];
        //     ISpace currentSpace = _tokenPositions[token];
        //     currentSpace.Tokens.Remove(token);

        //     // Jail is square index 10 in the standard board (BRD-04)
        //     ISpace jailSpace = Board.Path[10];
        //     jailSpace.Tokens.Add(token);
        //     _tokenPositions[token] = jailSpace;

        //     OnPlayerMoved?.Invoke(token, jailSpace);

        //     // When sent to Jail, the turn ends immediately (PLR-21)
        //     EndTurn();
        // }

        // public bool HandleGetOutOfJail(IPlayer player, bool? useCard = null)
        // {
        //     if (!player.IsInJail) return false;

        //     // If choice is not specified: use card if owned, otherwise pay cash
        //     bool shouldUseCard = useCard ?? (player.JailFreeCard > 0);

        //     if (shouldUseCard)
        //     {
        //         if (player.JailFreeCard <= 0) return false;

        //         player.JailFreeCard--;
        //         player.IsInJail = false;
        //         _jailTurns[player] = 0;
        //         return true;
        //     }
        //     else
        //     {
        //         // Pay the $50 fine to the Bank (PLR-26)
        //         if (!SendMoney(player, 50))
        //         {
        //             return false;
        //         }

        //         player.IsInJail = false;
        //         _jailTurns[player] = 0;
        //         return true;
        //     }
        // }
        // public void EndTurn()
        // {

        //     if (HasPendingDebt())
        //     {
        //         return;
        //     }
        //     _doubleCount = 0;
        //     _hasRolled = false;
        //     LastDrawnCard = null;
        //     NextTurn();
        // }


        // public Action<IPlayer>? OnTurnChanged;
        // public bool NextTurn()
        // {
        //     if (Status != GameStatus.InProgress) return false;

        //     for (int step = 1; step <= Players.Count; step++)
        //     {
        //         int index = (_currentPlayerIndex + step) % Players.Count;

        //         if (index == _currentPlayerIndex) break;   // came back to the same player, no other player
        //         if (Players[index].IsBankrupt) continue;

        //         _currentPlayerIndex = index;
        //         OnTurnChanged?.Invoke(Players[index]);
        //         return true;
        //     }

        //     return false;
        // }

        // public Action<IPlayer, ICard>? OnCardEffect;
        // public void DrawCard(IPlayer player, CardType cardType)
        // {
        //     if (player == null || player.IsBankrupt || Status != GameStatus.InProgress) return;

        //     ICardDeck deck = cardType == CardType.Chance ? _chanceDeck : _communityChestDeck;

        //     if (deck.Cards.Count == 0) return;

        //     // 1. Draw top card (CRD-02)
        //     ICard card = deck.Cards.Dequeue();
        //     LastDrawnCard = card;

        //     // 2. Notify observers / UI to display the card popup
        //     OnCardEffect?.Invoke(player, card);

        //     // 3. Special Case: Get Out of Jail Free is kept by the player (CRD-05)
        //     if (card.CardEffect == CardEffect.GetOutOfJailFree)
        //     {
        //         player.JailFreeCard++;
        //         // Not returned to the deck until used or player goes bankrupt to Bank
        //     }
        //     else
        //     {
        //         // 4. Follow the card's instructions
        //         ExecuteCardEffect(player, card);

        //         // 5. Return card facedown to the bottom of the deck (CRD-03)
        //         ReturnCard(card, cardType);
        //     }
        // }

        // public void ReturnCard(ICard card, CardType cardType)
        // {
        //     if (card == null) return;

        //     ICardDeck deck = cardType == CardType.Chance ? _chanceDeck : _communityChestDeck;
        //     deck.Cards.Enqueue(card);
        // }

        // public void ExecuteCardEffect(IPlayer player, ICard card)
        // {
        //     IToken token = _playerTokens[player];
        //     ISpace currentSpace = _tokenPositions[token];
        //     int currentIndex = Board.Path.IndexOf(currentSpace);

        //     switch (card.CardEffect)
        //     {
        //         case CardEffect.MoveToPosition:
        //             if (card.Value.HasValue)
        //             {
        //                 //  Ex : 24 - Illinois Avenue
        //                 int targetIndex = card.Value.Value;

        //                 // Ex : currentIndex - 2
        //                 // steps = (24 - 2 + 40) % 40
        //                 int steps = (targetIndex - currentIndex + Board.Path.Count) % Board.Path.Count;
        //                 if (steps == 0)
        //                 {

        //                     steps = Board.Path.Count; // Advance to GO, when player already on GO
        //                 }



        //                 Move(token, steps, fromCard: true);
        //             }
        //             break;

        //         case CardEffect.MoveToNearestRailroad:
        //             // Railroads are at spaces 5, 15, 25, 35
        //             int[] railroads = [5, 15, 25, 35];
        //             int nextRailroad = railroads.FirstOrDefault(r => r > currentIndex, railroads[0]);
        //             int rrSteps = (nextRailroad - currentIndex + Board.Path.Count) % Board.Path.Count;
        //             if (rrSteps == 0) rrSteps = Board.Path.Count;

        //             // Passing GO checker inside Move()
        //             Move(token, rrSteps, fromCard: true);
        //             break;

        //         case CardEffect.MoveToNearestUtility:
        //             // Utilities are at spaces 12 (Electric Co) and 28 (Water Works)
        //             int[] utilities = [12, 28];
        //             int nextUtility = utilities.FirstOrDefault(u => u > currentIndex, utilities[0]);
        //             int utilSteps = (nextUtility - currentIndex + Board.Path.Count) % Board.Path.Count;
        //             if (utilSteps == 0) utilSteps = Board.Path.Count;

        //             Move(token, utilSteps, fromCard: true);
        //             break;

        //         case CardEffect.MoveSteps:
        //             // E.g. "Go Back 3 Spaces" (Value = -3)
        //             if (card.Value.HasValue)
        //             {
        //                 Move(token, card.Value.Value, fromCard: true);
        //             }
        //             break;

        //         case CardEffect.ReceiveMoney:
        //             // Bank pays dividend, loan maturity, tax refund, etc.
        //             if (card.Value.HasValue)
        //             {
        //                 ReceiveMoney(player, card.Value.Value);
        //             }
        //             break;

        //         case CardEffect.PayMoney:
        //             // Fines, doctor fees, school fees to the Bank (CRD-07)
        //             if (card.Value.HasValue)
        //             {
        //                 int fine = card.Value.Value;
        //                 if (!SendMoney(player, fine))
        //                 {
        //                     // If player cannot pay immediately, enter debt state

        //                     RecordDebt(debtor: player, amount: fine, creditor: null); // owed to Bank
        //                 }
        //             }
        //             break;

        //         case CardEffect.PayEachPlayer:
        //             // E.g. "Chairman of the Board: pay each player $50"
        //             if (card.Value.HasValue)
        //             {
        //                 int amountPerPlayer = card.Value.Value;
        //                 foreach (IPlayer other in Players)
        //                 {
        //                     if (other != player && !other.IsBankrupt)
        //                     {
        //                         if (!SendMoney(player, amountPerPlayer, other))
        //                         {
        //                             RecordDebt(debtor: player, amount: amountPerPlayer, creditor: other);
        //                         }
        //                     }
        //                 }
        //             }
        //             break;

        //         case CardEffect.ReceiveFromEachPlayer:
        //             // E.g. "Grand Opera Opening" or "Birthday: collect $10 from each player"
        //             if (card.Value.HasValue)
        //             {
        //                 int gift = card.Value.Value;
        //                 foreach (IPlayer other in Players)
        //                 {
        //                     if (other != player && !other.IsBankrupt)
        //                     {

        //                         if (!SendMoney(other, gift, player))
        //                         {
        //                             RecordDebt(debtor: other, amount: gift, creditor: player);
        //                         }
        //                     }
        //                 }
        //             }
        //             break;

        //         case CardEffect.GoToJail:
        //             // Directly to Jail, do not pass GO, do not collect $200 (PLR-20, PLR-21)
        //             SendToJail(player);
        //             break;

        //         case CardEffect.GetOutOfJailFree:
        //             // Handled directly inside DrawCard (card kept by player)
        //             break;

        //         case CardEffect.Repairs:
        //             // Chance: $25/house, $100/hotel. Community Chest: $40/house, $115/hotel
        //             int houseRate = card.CardType == CardType.Chance ? 25 : 40;
        //             int hotelRate = card.CardType == CardType.Chance ? 100 : 115;

        //             int totalHouses = 0;
        //             int totalHotels = 0;

        //             // Count all buildings owned across all properties of this player
        //             foreach (var prop in _propertyOwners.Where(kvp => kvp.Value == player).Select(kvp => kvp.Key))
        //             {
        //                 if (prop.BuildingLevel == 5)
        //                 {
        //                     totalHotels++;
        //                 }
        //                 else if (prop.BuildingLevel > 0)
        //                 {
        //                     totalHouses += prop.BuildingLevel;
        //                 }
        //             }

        //             int repairBill = (totalHouses * houseRate) + (totalHotels * hotelRate);
        //             if (repairBill > 0)
        //             {
        //                 if (!SendMoney(player, repairBill))
        //                 {

        //                     RecordDebt(debtor: player, amount: repairBill, creditor: null);

        //                 }
        //             }
        //             break;
        //     }
        // }

        // public bool BuyProperty(IPlayer player, IProperty property)
        // {
        //     if (player == null || property == null) return false;
        //     if (player.IsBankrupt || Status != GameStatus.InProgress) return false;

        //     // 1. Must be unowned (Bank-owned)
        //     if (GetPropertyOwner(property) != null)
        //     {
        //         return false;
        //     }

        //     // validation, to buy a property, the token should be on top of it
        //     if (!_tokenPositions.TryGetValue(_playerTokens[player], out ISpace? currentSpace) || currentSpace.Property != property)
        //     {
        //         return false;
        //     }

        //     // 2. Player must be able to afford the printed price (PRP-10)
        //     // SendMoney deducts the cash and fires OnPlayerMoneyChanged
        //     if (!SendMoney(player, property.Price))
        //     {
        //         return false;
        //     }

        //     // 3. Register ownership in the dictionary (PRP-11)
        //     _propertyOwners[property] = player;

        //     // RESET, FLAG for auction
        //     _propertyDecisionResolved = true;

        //     return true;
        // }

        // public void DeclineProperty(IPlayer player, IProperty property)
        // {
        //     _propertyDecisionResolved = true;
        //     // Rule PRP-12: If the player declines, the Bank auctions it!
        //     StartAuction(property);
        // }


        // private IProperty? _auctionProperty;
        // private IPlayer? _highestBidder;
        // private int _highestBid;
        // // Events for the React client / SignalR
        // public Action<IProperty>? OnAuctionStarted;
        // public Action<IPlayer, int>? OnBidPlaced; // bidder, amount
        // public Action<IProperty, IPlayer?, int>? OnAuctionEnded; // property, winner (or null), winningBid

        // private List<IPlayer> _auctionBidders = new();
        // private int _auctionTurnIndex;
        // public bool StartAuction(IProperty property)
        // {
        //     if (property == null || GetPropertyOwner(property) != null)
        //     {
        //         return false;
        //     }

        //     _auctionProperty = property;
        //     _highestBidder = null;
        //     _highestBid = 0;

        //     // Initialize all active (non-bankrupt) players as bidders
        //     _auctionBidders = Players.Where(p => !p.IsBankrupt).ToList();

        //     // Turn starts with the player who declined
        //     _auctionTurnIndex = _currentPlayerIndex;

        //     OnAuctionStarted?.Invoke(property);
        //     return true;
        // }

        // public IPlayer? CurrentAuctionBidder =>
        //     _auctionBidders.Count > 0 ? _auctionBidders[_auctionTurnIndex % _auctionBidders.Count] : null;

        // public bool PlaceBid(IPlayer player, int amount)
        // {
        //     if (_auctionProperty == null)
        //     {
        //         return false;
        //     }
        //     if (player == null || player.IsBankrupt)
        //     {
        //         return false; // to make sure it is your turn
        //     }

        //     if (player != CurrentAuctionBidder) return false;

        //     // Cannot bid against yourself!
        //     if (player == _highestBidder)
        //     {
        //         return false;
        //     }

        //     // Bid must be higher than current top bid, and player must actually have the cash
        //     if (amount <= _highestBid || player.Money < amount)
        //     {
        //         return false;
        //     }

        //     _highestBidder = player;
        //     _highestBid = amount;

        //     OnBidPlaced?.Invoke(player, amount);

        //     if (_auctionBidders.Count <= 1)
        //     {
        //         HandleAuction(_auctionProperty);
        //         return true;
        //     }

        //     // Advance bidding turn to the next remaining bidder
        //     // if (_auctionBidders.Count > 1)
        //     // {
        //     //     _auctionTurnIndex = (_auctionTurnIndex + 1) % _auctionBidders.Count;
        //     // }

        //     _auctionTurnIndex = (_auctionTurnIndex + 1) % _auctionBidders.Count;
        //     return true;
        // }

        // public IPlayer? HandleAuction(IProperty property)
        // {
        //     if (property == null || _auctionProperty != property) return null;

        //     IPlayer? winner = _highestBidder;
        //     int winningBid = _highestBid;

        //     if (winner != null && winningBid > 0)
        //     {
        //         // 1. Winner pays cash to the Bank (PRP-14)
        //         SendMoney(winner, winningBid);

        //         // 2. Winner receives the Title Deed
        //         _propertyOwners[property] = winner;
        //     }

        //     // 3. Notify players of the auction result
        //     OnAuctionEnded?.Invoke(property, winner, winningBid);

        //     // 4. Clear auction state
        //     _auctionProperty = null;
        //     _highestBidder = null;
        //     _highestBid = 0;

        //     return winner;
        // }

        // public bool PassBid(IPlayer player)
        // {
        //     if (_auctionProperty == null || player != CurrentAuctionBidder) return false;

        //     // Remove this player from the auction
        //     _auctionBidders.Remove(player);

        //     // Case 1: Nobody participate in bidding, Auction ends, stays with Bank
        //     if (_auctionBidders.Count == 0)
        //     {
        //         HandleAuction(_auctionProperty);
        //         return true;
        //     }

        //     // Case 2: Only 1 bidder remains AND someone placed a bid ➔ Remaining player wins!
        //     if (_auctionBidders.Count == 1 && _highestBidder != null)
        //     {
        //         HandleAuction(_auctionProperty);
        //         return true;
        //     }

        //     // Case 3: More bidders remain ➔ Advance turn
        //     _auctionTurnIndex = _auctionTurnIndex % _auctionBidders.Count;
        //     return true;
        // }
        // public IToken? GetPlayerToken(IPlayer player)
        //     => _playerTokens.TryGetValue(player, out var token) ? token : null;


        // public bool TradeCard(IPlayer buyer, IPlayer seller, int offer)
        // {
        //     // 1. Basic Validation
        //     if (buyer == null || seller == null) return false;
        //     if (buyer == seller) return false; // Cannot trade with yourself
        //     if (buyer.IsBankrupt || seller.IsBankrupt) return false;
        //     if (offer < 0) return false;

        //     // 2. Seller must actually possess a Get Out of Jail Free card (CRD-06)
        //     if (seller.JailFreeCard <= 0)
        //     {
        //         return false;
        //     }

        //     // 3. Buyer must be able to afford the cash offer
        //     if (buyer.Money < offer)
        //     {
        //         return false;
        //     }

        //     // 4. Transfer cash offer from Buyer to Seller
        //     if (offer > 0)
        //     {
        //         if (!SendMoney(buyer, offer, seller))
        //         {
        //             return false;
        //         }
        //     }

        //     // 5. Transfer the Get Out of Jail Free card
        //     seller.JailFreeCard--;
        //     buyer.JailFreeCard++;

        //     return true;
        // }


        // /// <summary>
        // /// Proposes to buy a Get Out of Jail Free card from another player for an agreed offer (CRD-06).
        // /// Validates that seller has a card and buyer has the cash, then fires OnCardTradeOffered.
        // /// </summary>
        // private IPlayer? _cardTradeBuyer;
        // private IPlayer? _cardTradeSeller;
        // private int _cardTradeOffer;
        // public Action<IPlayer, IPlayer, int>? OnCardTradeOffered;
        // public bool ProposeCardTrade(IPlayer buyer, IPlayer seller, int offer)
        // {
        //     // 1. Validation
        //     if (buyer == null || seller == null) return false;
        //     if (buyer == seller) return false;
        //     if (buyer.IsBankrupt || seller.IsBankrupt) return false;
        //     if (offer < 0) return false;

        //     // 2. Seller must actually have a card
        //     if (seller.JailFreeCard <= 0) return false;

        //     // 3. Buyer must be able to afford the offer
        //     if (buyer.Money < offer) return false;

        //     // 4. Save the pending proposal
        //     _cardTradeBuyer = buyer;
        //     _cardTradeSeller = seller;
        //     _cardTradeOffer = offer;

        //     // 5. Notify the seller through the event
        //     OnCardTradeOffered?.Invoke(buyer, seller, offer);

        //     return true;
        // }


        // /// <summary>
        // /// Called when the seller accepts the card offer.
        // /// Executes TradeCard and transfers the cash and card.
        // /// </summary>
        // public bool AcceptCardTrade(IPlayer seller)
        // {
        //     if (_cardTradeSeller == null || _cardTradeBuyer == null) return false;
        //     if (seller != _cardTradeSeller) return false;

        //     // Execute the trade!
        //     bool success = TradeCard(_cardTradeBuyer, _cardTradeSeller, _cardTradeOffer);

        //     // Clear the pending offer
        //     _cardTradeBuyer = null;
        //     _cardTradeSeller = null;
        //     _cardTradeOffer = 0;

        //     return success;
        // }

        // /// <summary>
        // /// Called when the seller declines the card offer.
        // /// Clears the offer without moving any money or cards.
        // /// </summary>
        // public void DeclineCardTrade(IPlayer seller)
        // {
        //     if (seller != _cardTradeSeller) return;

        //     // Cancel and clear the offer
        //     _cardTradeBuyer = null;
        //     _cardTradeSeller = null;
        //     _cardTradeOffer = 0;
        // }

        // public bool SellProperty(IPlayer player, IProperty property)
        // {
        //     if (player == null || property == null) return false;

        //     if (player.IsBankrupt)
        //     {
        //         return false;
        //     }

        //     // 1. Player must be the owner
        //     if (GetPropertyOwner(property) != player)
        //     {
        //         return false;
        //     }

        //     // 2. The "No Buildings in Color Group" rule (PRP-31, PRP-32):
        //     // A street cannot be sold while ANY street in its color group has buildings.
        //     if (property.PropertyType == PropertyType.Street && property.ColorGroup.HasValue)
        //     {
        //         bool hasBuildingsInGroup = Board.Path
        //             .Where(s => s.Property != null && s.Property.ColorGroup == property.ColorGroup)
        //             .Any(s => s.Property!.BuildingLevel > 0);

        //         if (hasBuildingsInGroup)
        //         {
        //             return false; // All houses/hotels in the group must be sold to the Bank first (PRP-32)
        //         }
        //     }

        //     // 3. Bank payout:
        //     // If unmortgaged, Bank pays the MortgageValue (half the printed price per BRD-19).
        //     // (If already mortgaged, the player already received this loan from the Bank).
        //     if (!property.IsMortgaged)
        //     {
        //         ReceiveMoney(player, property.MortgageValue);
        //     }

        //     // 4. Return Title Deed to the Bank
        //     property.IsMortgaged = false;
        //     _propertyOwners.Remove(property);

        //     return true;
        // }

        // private IPlayer? _tradeBuyer;
        // private IPlayer? _tradeSeller;
        // private List<IProperty>? _tradeProperties;
        // private int _tradeOffer;

        // public IPlayer? TradeBuyer => _tradeBuyer;
        // public IPlayer? TradeSeller => _tradeSeller;
        // public List<IProperty>? TradeProperties => _tradeProperties;
        // public int TradeOffer => _tradeOffer;
        // public bool HasPendingTrade => _tradeSeller != null;

        // public bool TradeProperty(IPlayer buyer, IPlayer seller, List<IProperty> properties, int offer)
        // {
        //     // 1. Basic Validation
        //     if (buyer == null || seller == null || properties == null) return false;
        //     if (buyer == seller) return false; // Cannot trade with yourself
        //     if (buyer.IsBankrupt || seller.IsBankrupt) return false;
        //     if (properties.Count == 0 || offer < 0) return false;

        //     // 2. Buyer must be able to afford the cash offer
        //     if (buyer.Money < offer)
        //     {
        //         return false;
        //     }

        //     // 3. Seller must own every property being traded
        //     foreach (IProperty prop in properties)
        //     {
        //         if (GetPropertyOwner(prop) != seller)
        //         {
        //             return false; // Seller does not own this property
        //         }
        //     }

        //     // 4. The "No Buildings in Color Group" rule (PRP-31, PRP-32):
        //     // A street cannot be traded if ANY street in its color group has buildings.
        //     foreach (IProperty prop in properties)
        //     {
        //         if (prop.PropertyType == PropertyType.Street && prop.ColorGroup != null)
        //         {
        //             bool hasBuildingsInGroup = Board.Path
        //                 .Where(s => s.Property != null && s.Property.ColorGroup == prop.ColorGroup)
        //                 .Any(s => s.Property!.BuildingLevel > 0);

        //             if (hasBuildingsInGroup)
        //             {
        //                 return false; // All houses/hotels in the group must be sold to the Bank first (PRP-32)
        //             }
        //         }
        //     }

        //     // 5. Transfer cash offer from Buyer to Seller (PRP-30)
        //     if (offer > 0)
        //     {
        //         if (!SendMoney(buyer, offer, seller))
        //         {
        //             return false;
        //         }
        //     }

        //     // 6. Transfer ownership of all properties to Buyer
        //     // (Mortgaged status is preserved as-is; buyer can redeem it later)
        //     foreach (IProperty prop in properties)
        //     {
        //         _propertyOwners[prop] = buyer;
        //     }

        //     return true;
        // }

        // /// <summary>
        // /// Proposes a trade to the seller. Validates that the trade is legal before offering (PRP-30 to PRP-32).
        // /// Fires OnTradeOffered so the seller's UI can prompt them to Accept or Decline.
        // /// </summary>
        // /// 
        // public Action<IPlayer, IPlayer, List<IProperty>, int>? OnTradeOffered;
        // public bool ProposeTrade(IPlayer buyer, IPlayer seller, List<IProperty> properties, int offer)
        // {
        //     // 1. Validate that the offer is valid
        //     if (buyer == null || seller == null || properties == null)
        //     {
        //         return false;
        //     }

        //     if (buyer == seller)
        //     {
        //         return false;
        //     }

        //     if (buyer.IsBankrupt || seller.IsBankrupt)
        //     {
        //         return false;
        //     }

        //     if (properties.Count == 0 || offer < 0)
        //     {
        //         return false;
        //     }


        //     // 2. Buyer must actually have the cash they are offering
        //     if (buyer.Money < offer) return false;

        //     // 3. Seller must actually own all properties
        //     if (properties.Any(p => GetPropertyOwner(p) != seller)) return false;

        //     // 4. No buildings in the color group rule (PRP-31)
        //     foreach (IProperty prop in properties)
        //     {
        //         if (prop.PropertyType == PropertyType.Street && prop.ColorGroup != null)
        //         {
        //             bool hasBuildings = Board.Path
        //                 .Where(s => s.Property != null && s.Property.ColorGroup == prop.ColorGroup)
        //                 .Any(s => s.Property!.BuildingLevel > 0);

        //             if (hasBuildings) return false;
        //         }
        //     }

        //     // 5. Store the pending trade proposal
        //     _tradeBuyer = buyer;
        //     _tradeSeller = seller;
        //     _tradeProperties = properties;
        //     _tradeOffer = offer;

        //     // 6. Notify the seller through the event
        //     OnTradeOffered?.Invoke(buyer, seller, properties, offer);

        //     return true;
        // }

        // /// <summary>
        // /// Called when the seller accepts the proposed trade offer.
        // /// Executes the transaction via TradeProperty and clears the offer.
        // /// </summary>
        // public bool AcceptTrade(IPlayer seller)
        // {
        //     // Only the seller who was offered the trade can accept it
        //     if (_tradeSeller == null || _tradeBuyer == null || _tradeProperties == null) return false;
        //     if (seller != _tradeSeller) return false;

        //     // Execute the trade!
        //     bool success = TradeProperty(_tradeBuyer, _tradeSeller, _tradeProperties, _tradeOffer);

        //     // Clear the pending offer
        //     _tradeBuyer = null;
        //     _tradeSeller = null;
        //     _tradeProperties = null;
        //     _tradeOffer = 0;

        //     return success;
        // }

        // /// <summary>
        // /// Called when the seller declines the proposed trade offer.
        // /// Cancels the offer without changing any game state.
        // /// </summary>
        // public void DeclineTrade(IPlayer seller)
        // {
        //     if (seller != _tradeSeller) return;

        //     // Cancel and clear the offer
        //     _tradeBuyer = null;
        //     _tradeSeller = null;
        //     _tradeProperties = null;
        //     _tradeOffer = 0;
        // }

        // public bool CanEndTurn()
        // {
        //     if (Status != GameStatus.InProgress || HasPendingDebt()) return false;

        //     if (HasUnresolvedPropertyUnderfoot())
        //     {
        //         return false;
        //     }
        //     IPlayer player = GetCurrentPlayer();

        //     // var space = GetPlayerPosition(player);
        //     // if (space?.Property != null && GetPropertyOwner(space.Property) == null)
        //     // {
        //     //     return false;
        //     // }
        //     if (player.IsBankrupt) return true;

        //     // Allowed to end turn only after rolling, and NOT while holding an extra double-roll
        //     return _hasRolled && !CanRollAgain();
        // }

    }
}