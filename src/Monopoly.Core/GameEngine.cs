using Monopoly.Core.Enums;
using Monopoly.Core.Interfaces;
using Monopoly.Core.Models;
using Monopoly.Enums;

namespace Monopoly.Core
{
    public class GameEngine
    {
        private const int MinPlayers = 2;
        private const int MaxPlayers = 6;
        private const int StartingHouses = 32;
        private const int StartingHotels = 12;

        public IBoard Board { get; }
        public IDice Dice { get; }
        Random _random = new Random();
        public List<IPlayer> Players { get; }

        private readonly Dictionary<IProperty, IPlayer> _propertyOwners = new Dictionary<IProperty, IPlayer>();

        private readonly Dictionary<IPlayer, IToken> _playerTokens = new Dictionary<IPlayer, IToken>();

        Dictionary<IToken, ISpace> _tokenPositions = new Dictionary<IToken, ISpace>();

        /// <summary>
        /// store counter Jail attempt with dice
        /// </summary>

        Dictionary<IPlayer, int> _jailTurns = new Dictionary<IPlayer, int>();

        private int _currentPlayerIndex;

        /// <summary>Houses still in the Bank.</summary>
        public int houseAmount { get; set; } = StartingHouses;

        /// <summary>Hotels still in the Bank.</summary>
        public int hotelAmount { get; set; } = StartingHotels;

        private int _doubleCount;
        private readonly ICardDeck _chanceDeck = new Carddeck();
        private readonly ICardDeck _communityChestDeck = new Carddeck();
        private bool _isGameOver;

        /// FLAG, Bug fixing, (API can call RollDice again and again)
        private bool _hasRolled;

        /// FLAG, Bug fixing, (Bug : Game cant continue when everyone decline buying property)
        private bool _propertyDecisionResolved = true;

        // ========================================================================================
        // CHANCE & COMMUNITY CHEST HELPER
        // ========================================================================================

        public ICard? LastDrawnCard { get; private set; }
        public void ClearLastDrawnCard() => LastDrawnCard = null;



        /// =========================================================================================
        /// ------------------------------- DEBT AND BANKRUPTCY HELPER ------------------------------
        /// =========================================================================================

        // Replaces the old scalar fields with a single chronological list (zero new classes)
        private readonly List<(IPlayer Debtor, IPlayer? Creditor, int Amount)> _pendingDebts = new();

        public Action<IToken, ISpace>? OnPlayerMoved;
        public Action<IPlayer>? OnTurnChanged;
        public Action<IPlayer>? OnPlayerMoneyChanged;
        public Action<IPlayer, ICard>? OnCardEffect;
        public Action<IPlayer>? OnPlayerBankrupt;
        // new event next to the others
        public Action<IPlayer, int, int>? OnDiceRolled;     // player, side1, side2

        /// =========================================================================================
        /// ------------------------------- Trading & Auction ---------------------------------------
        /// =========================================================================================

        // Pending trade state (zero new classes needed)
        private IPlayer? _tradeBuyer;
        private IPlayer? _tradeSeller;
        private List<IProperty>? _tradeProperties;
        private int _tradeOffer;

        // Event to notify the client/UI that an offer was made
        public Action<IPlayer, IPlayer, List<IProperty>, int>? OnTradeOffered; // buyer, seller, properties, offer

        /// =========================================================================================
        /// ------------------------------- Card Trade ----------------------------------------------
        /// =========================================================================================


        // Pending card trade state
        private IPlayer? _cardTradeBuyer;
        private IPlayer? _cardTradeSeller;
        private int _cardTradeOffer;

        // Event to notify the UI that someone is offering to buy a card
        public Action<IPlayer, IPlayer, int>? OnCardTradeOffered; // buyer, seller, offer



        /// =========================================================================================
        /// --------------------------------------- AUCTION -----------------------------------------
        /// =========================================================================================

        // Current auction state (zero new classes)
        private IProperty? _auctionProperty;
        private IPlayer? _highestBidder;
        private int _highestBid;
        // Events for the React client / SignalR
        public Action<IProperty>? OnAuctionStarted;
        public Action<IPlayer, int>? OnBidPlaced; // bidder, amount
        public Action<IProperty, IPlayer?, int>? OnAuctionEnded; // property, winner (or null), winningBid

        /// =========================================================================================
        /// AUCTION HELPER, TAKE TURN TO BID
        /// =========================================================================================
        private List<IPlayer> _auctionBidders = new();
        private int _auctionTurnIndex;

        /// =========================================================================================
        /// GETTER, SETTER.
        /// =========================================================================================


        public IPlayer? CurrentAuctionBidder =>
            _auctionBidders.Count > 0 ? _auctionBidders[_auctionTurnIndex % _auctionBidders.Count] : null;

        public GameEngine(IBoard board, IDice dice, List<IPlayer> players)
        {
            Board = board;
            Dice = dice;
            Players = players;
        }

        /// <summary>
        /// Builds the whole board: creates the 40 squares and puts them in the grid and on the path.
        /// Safe to call more than once. It starts from an empty board each time.
        /// </summary>
        public void InitializeBoard()
        {
            Array.Clear(Board.Grid);
            Board.Path.Clear();

            InitializeSpace();
            InitializePath();
        }

        /// <summary>Creates the 40 hard-coded squares and places each one in the grid.</summary>
        public void InitializeSpace()
        {
            List<ISpace> spaces = BoardData.CreateSpaces();

            for (int index = 0; index < BoardData.SquareCount; index++)
            {
                (int row, int column) = GetGridPosition(index);
                Board.Grid[row, column] = spaces[index];
            }
        }


        /// <summary>
        /// Builds the path: the squares in the order tokens walk, starting at GO.
        /// It reads the squares from the grid, so the path and the grid hold the same objects.
        /// </summary>
        public void InitializePath()
        {
            Board.Path.Clear();

            for (int index = 0; index < BoardData.SquareCount; index++)
            {
                (int row, int column) = GetGridPosition(index);
                Board.Path.Add(Board.Grid[row, column]);
            }
        }

        /// <summary>
        /// Creates the players, one per name, in join order, each with the matching token color on GO.
        /// Returns false (and creates nobody) if the names or colors are not valid.
        /// The board must be initialized first.
        /// </summary>
        public bool InitializePlayers(List<IPlayer> players, List<IToken> tokens)
        {
            if (Board.Path.Count != BoardData.SquareCount)
            {
                throw new InvalidOperationException("Initialize the board before the players.");
            }

            // ADD NEW VALIDATION
            if (players == null || tokens == null) return false;
            if (players.Count < MinPlayers || players.Count > MaxPlayers) return false;
            if (players.Count != tokens.Count) return false;

            ISpace go = Board.Path[0];

            // 1. Copy to local list first so clearing doesn't wipe the source
            var playerList = players.ToList();

            Players.Clear();
            _playerTokens.Clear();
            _tokenPositions.Clear();
            _jailTurns.Clear();

            // Reset everything left over from a previous game.
            foreach (ISpace space in Board.Path)
            {

                space.Tokens.Clear();
            }

            for (int i = 0; i < playerList.Count; i++)
            {
                IPlayer player = playerList[i];
                IToken token = tokens[i];

                // token.CurrentPosition = go;
                _tokenPositions[token] = go;
                go.Tokens.Add(token);

                Players.Add(player);
                _playerTokens[player] = token;
            }

            _currentPlayerIndex = 0;
            return true;
        }

        public void InitializeCardDecks()
        {
            _chanceDeck.Cards = new Queue<ICard>(CreateChanceCards());
            _communityChestDeck.Cards = new Queue<ICard>(CreateCommunityChestCards());
            ShuffleCard(CardType.Chance);
            ShuffleCard(CardType.CommunityChest);
        }

        /// <summary>
        /// Take around the Grid. Starting from GO Square
        /// 
        /// (   row     column)
        ///     10          10      GO
        ///     10          9       Mediteranean Avenue
        ///     10          8       CC
        ///     10          7       Baltic Avenue
        /// </summary>
        private static (int row, int column) GetGridPosition(int index)
        {
            int last = Models.Board.GridSize - 1;

            if (index <= last)
            {
                return (last, last - index);          // bottom edge, right to left
            }

            if (index <= 2 * last)
            {
                return (2 * last - index, 0);         // left edge, bottom to top
            }

            if (index <= 3 * last)
            {
                return (0, index - 2 * last);         // top edge, left to right
            }

            return (index - 3 * last, last);          // right edge, top to bottom
        }

        /// =========================================================================================
        /// -------------------------------- CARD PREPARATION ---------------------------------------
        /// =========================================================================================


        private static List<ICard> CreateChanceCards()
        {
            return new List<ICard>
            {
                new Card(CardType.Chance, CardEffect.MoveToPosition, "Advance to Boardwalk", 39),
                new Card(CardType.Chance, CardEffect.MoveToPosition, "Advance to GO (Collect $200)", 0),
                new Card(CardType.Chance, CardEffect.MoveToPosition, "Advance to Illinois Avenue", 24),
                new Card(CardType.Chance, CardEffect.MoveToPosition, "Advance to St. Charles Place. If you pass GO, collect $200", 11),
                new Card(CardType.Chance, CardEffect.MoveToNearestRailroad, "Advance to nearest Railroad. If unowned, you may buy it. If owned, pay owner twice the rent", null),
                new Card(CardType.Chance, CardEffect.MoveToNearestRailroad, "Advance to nearest Railroad. If unowned, you may buy it. If owned, pay owner twice the rent", null),
                new Card(CardType.Chance, CardEffect.MoveToNearestUtility, "Advance to nearest Utility. If unowned, you may buy it. If owned, throw dice and pay owner 10 times the dice throw", null),
                new Card(CardType.Chance, CardEffect.ReceiveMoney, "Bank pays you dividend of $50", 50),
                new Card(CardType.Chance, CardEffect.GetOutOfJailFree, "Get Out of Jail Free. This card may be kept until needed or sold", null),
                new Card(CardType.Chance, CardEffect.MoveSteps, "Go Back 3 Spaces", -3),
                new Card(CardType.Chance, CardEffect.GoToJail, "Go to Jail. Go directly to Jail, do not pass GO, do not collect $200", null),
                new Card(CardType.Chance, CardEffect.Repairs, "Make general repairs on all your property. For each house pay $25, for each hotel pay $100", null),
                new Card(CardType.Chance, CardEffect.PayMoney, "Speeding fine $15", 15),
                new Card(CardType.Chance, CardEffect.MoveToPosition, "Take a trip to Reading Railroad. If you pass GO, collect $200", 5),
                new Card(CardType.Chance, CardEffect.PayEachPlayer, "You have been elected Chairman of the Board. Pay each player $50", 50),
                new Card(CardType.Chance, CardEffect.ReceiveMoney, "Your building loan matures. Collect $150", 150)
            };
        }

        private static List<ICard> CreateCommunityChestCards()
        {
            return new List<ICard>
            {
                new Card(CardType.CommunityChest, CardEffect.MoveToPosition, "Advance to GO (Collect $200)", 0),
                new Card(CardType.CommunityChest, CardEffect.ReceiveMoney, "Bank error in your favor. Collect $200", 200),
                new Card(CardType.CommunityChest, CardEffect.PayMoney, "Doctor’s fee. Pay $50", 50),
                new Card(CardType.CommunityChest, CardEffect.ReceiveMoney, "From sale of stock you get $50", 50),
                new Card(CardType.CommunityChest, CardEffect.GetOutOfJailFree, "Get Out of Jail Free. This card may be kept until needed or sold", null),
                new Card(CardType.CommunityChest, CardEffect.GoToJail, "Go to Jail. Go directly to Jail, do not pass GO, do not collect $200", null),
                new Card(CardType.CommunityChest, CardEffect.ReceiveMoney, "Holiday fund matures. Receive $100", 100),
                new Card(CardType.CommunityChest, CardEffect.ReceiveMoney, "Income tax refund. Collect $20", 20),
                new Card(CardType.CommunityChest, CardEffect.ReceiveFromEachPlayer, "It is your birthday. Collect $10 from every player", 10),
                new Card(CardType.CommunityChest, CardEffect.ReceiveMoney, "Life insurance matures. Collect $100", 100),
                new Card(CardType.CommunityChest, CardEffect.PayMoney, "Pay hospital fees of $100", 100),
                new Card(CardType.CommunityChest, CardEffect.PayMoney, "Pay school fees of $50", 50),
                new Card(CardType.CommunityChest, CardEffect.ReceiveMoney, "Receive $25 consultancy fee", 25),
                new Card(CardType.CommunityChest, CardEffect.Repairs, "You are assessed for street repairs. $40 per house, $115 per hotel", null),
                new Card(CardType.CommunityChest, CardEffect.ReceiveMoney, "You have won second prize in a beauty contest. Collect $10", 10),
                new Card(CardType.CommunityChest, CardEffect.ReceiveMoney, "You inherit $100", 100)
            };
        }

        /// <summary>
        /// Shuffles the specified card deck (CRD-04).
        /// </summary>
        public void ShuffleCard(CardType cardType)
        {
            ICardDeck deck = cardType == CardType.Chance ? _chanceDeck : _communityChestDeck;

            List<ICard> cardList = deck.Cards.ToList();

            // Fisher-Yates Shuffle
            for (int i = cardList.Count - 1; i > 0; i--)
            {
                int j = _random.Next(i + 1);
                (cardList[i], cardList[j]) = (cardList[j], cardList[i]);
            }

            deck.Cards = new Queue<ICard>(cardList);
        }

        /// <summary>
        /// Draws the top card from the specified deck, invokes OnCardEffect,
        /// and executes its effect (CRD-02, CRD-03, CRD-05).
        /// </summary>
        public void DrawCard(IPlayer player, CardType cardType)
        {
            if (player == null || player.IsBankrupt || _isGameOver) return;

            ICardDeck deck = cardType == CardType.Chance ? _chanceDeck : _communityChestDeck;

            if (deck.Cards.Count == 0) return;

            // 1. Draw top card (CRD-02)
            ICard card = deck.Cards.Dequeue();
            LastDrawnCard = card;

            // 2. Notify observers / UI to display the card popup
            OnCardEffect?.Invoke(player, card);

            // 3. Special Case: Get Out of Jail Free is kept by the player (CRD-05)
            if (card.CardEffect == CardEffect.GetOutOfJailFree)
            {
                player.JailFreeCard++;
                // Not returned to the deck until used or player goes bankrupt to Bank
            }
            else
            {
                // 4. Follow the card's instructions
                ExecuteCardEffect(player, card);

                // 5. Return card facedown to the bottom of the deck (CRD-03)
                ReturnCard(card, cardType);
            }
        }


        /// <summary>
        /// Executes the specific game action defined on the drawn card (CRD-08, CRD-09).
        /// </summary>
        public void ExecuteCardEffect(IPlayer player, ICard card)
        {
            IToken token = _playerTokens[player];
            ISpace currentSpace = _tokenPositions[token];
            int currentIndex = Board.Path.IndexOf(currentSpace);

            switch (card.CardEffect)
            {
                case CardEffect.MoveToPosition:
                    if (card.Value.HasValue)
                    {
                        //  Ex : 24 - Illinois Avenue
                        int targetIndex = card.Value.Value;

                        // Ex : currentIndex - 2
                        // steps = (24 - 2 + 40) % 40
                        int steps = (targetIndex - currentIndex + Board.Path.Count) % Board.Path.Count;
                        if (steps == 0) steps = Board.Path.Count; // e.g. Advance to GO when already on GO



                        Move(token, steps, fromCard: true);
                    }
                    break;

                case CardEffect.MoveToNearestRailroad:
                    // Railroads are at spaces 5, 15, 25, 35
                    int[] railroads = [5, 15, 25, 35];
                    int nextRailroad = railroads.FirstOrDefault(r => r > currentIndex, railroads[0]);
                    int rrSteps = (nextRailroad - currentIndex + Board.Path.Count) % Board.Path.Count;
                    if (rrSteps == 0) rrSteps = Board.Path.Count;

                    // Passing GO checker inside Move()
                    Move(token, rrSteps, fromCard: true);
                    break;

                case CardEffect.MoveToNearestUtility:
                    // Utilities are at spaces 12 (Electric Co) and 28 (Water Works)
                    int[] utilities = [12, 28];
                    int nextUtility = utilities.FirstOrDefault(u => u > currentIndex, utilities[0]);
                    int utilSteps = (nextUtility - currentIndex + Board.Path.Count) % Board.Path.Count;
                    if (utilSteps == 0) utilSteps = Board.Path.Count;

                    Move(token, utilSteps, fromCard: true);
                    break;

                case CardEffect.MoveSteps:
                    // E.g. "Go Back 3 Spaces" (Value = -3)
                    if (card.Value.HasValue)
                    {
                        Move(token, card.Value.Value, fromCard: true);
                    }
                    break;

                case CardEffect.ReceiveMoney:
                    // Bank pays dividend, loan maturity, tax refund, etc.
                    if (card.Value.HasValue)
                    {
                        ReceiveMoney(player, card.Value.Value);
                    }
                    break;

                case CardEffect.PayMoney:
                    // Fines, doctor fees, school fees to the Bank (CRD-07)
                    if (card.Value.HasValue)
                    {
                        int fine = card.Value.Value;
                        if (!SendMoney(player, fine))
                        {
                            // If player cannot pay immediately, enter debt state

                            RecordDebt(debtor: player, amount: fine, creditor: null); // owed to Bank
                        }
                    }
                    break;

                case CardEffect.PayEachPlayer:
                    // E.g. "Chairman of the Board: pay each player $50"
                    if (card.Value.HasValue)
                    {
                        int amountPerPlayer = card.Value.Value;
                        foreach (IPlayer other in Players)
                        {
                            if (other != player && !other.IsBankrupt)
                            {
                                if (!SendMoney(player, amountPerPlayer, other))
                                {
                                    RecordDebt(debtor: player, amount: amountPerPlayer, creditor: other);
                                }
                            }
                        }
                    }
                    break;

                case CardEffect.ReceiveFromEachPlayer:
                    // E.g. "Grand Opera Opening" or "Birthday: collect $10 from each player"
                    if (card.Value.HasValue)
                    {
                        int gift = card.Value.Value;
                        foreach (IPlayer other in Players)
                        {
                            if (other != player && !other.IsBankrupt)
                            {

                                if (!SendMoney(other, gift, player))
                                {
                                    RecordDebt(debtor: other, amount: gift, creditor: player);
                                }
                            }
                        }
                    }
                    break;

                case CardEffect.GoToJail:
                    // Directly to Jail, do not pass GO, do not collect $200 (PLR-20, PLR-21)
                    SendToJail(player);
                    break;

                case CardEffect.GetOutOfJailFree:
                    // Handled directly inside DrawCard (card kept by player)
                    break;

                case CardEffect.Repairs:
                    // Chance: $25/house, $100/hotel. Community Chest: $40/house, $115/hotel
                    int houseRate = card.CardType == CardType.Chance ? 25 : 40;
                    int hotelRate = card.CardType == CardType.Chance ? 100 : 115;

                    int totalHouses = 0;
                    int totalHotels = 0;

                    // Count all buildings owned across all properties of this player
                    foreach (var prop in _propertyOwners.Where(kvp => kvp.Value == player).Select(kvp => kvp.Key))
                    {
                        if (prop.BuildingLevel == 5)
                        {
                            totalHotels++;
                        }
                        else if (prop.BuildingLevel > 0)
                        {
                            totalHouses += prop.BuildingLevel;
                        }
                    }

                    int repairBill = (totalHouses * houseRate) + (totalHotels * hotelRate);
                    if (repairBill > 0)
                    {
                        if (!SendMoney(player, repairBill))
                        {

                            RecordDebt(debtor: player, amount: repairBill, creditor: null);
                            // _pendingDebtAmount = repairBill;
                            // _pendingDebtor = player;
                            // _pendingCreditor = null; // owed to Bank
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// Returns a card facedown to the bottom of its deck (CRD-03).
        /// </summary>
        public void ReturnCard(ICard card, CardType cardType)
        {
            if (card == null) return;

            ICardDeck deck = cardType == CardType.Chance ? _chanceDeck : _communityChestDeck;
            deck.Cards.Enqueue(card);
        }



        /// <summary>The player whose turn it is.</summary>
        public IPlayer GetCurrentPlayer()
        {
            return Players[_currentPlayerIndex];
        }

        /// <summary>
        /// Starts the game with the first player (the one who joined first).
        /// Throws if the players are not set up yet.
        /// </summary>
        public void StartGame()
        {
            if (Players.Count < MinPlayers)
            {
                throw new InvalidOperationException("Initialize the players before starting the game.");
            }

            _isGameOver = false;
            _doubleCount = 0;
            _currentPlayerIndex = 0;
            _hasRolled = false;

            OnTurnChanged?.Invoke(GetCurrentPlayer());
        }

        /// <summary>Ends the game. Nothing more can be played.</summary>
        public void EndGame()
        {
            _isGameOver = true;
        }

        /// <summary>Ends the current player's turn and passes the turn on.</summary>
        public void EndTurn()
        {

            if (HasPendingDebt())
            {
                return;
            }
            _doubleCount = 0;
            _hasRolled = false;
            LastDrawnCard = null;
            NextTurn();
        }

        /// <summary>
        /// Passes the turn to the next player who is not bankrupt, in join order.
        /// Returns false (and keeps the turn where it is) if there is nobody else to play.
        // count the index to determine who's next with formula
        /// </summary>
        public bool NextTurn()
        {
            if (_isGameOver) return false;

            for (int step = 1; step <= Players.Count; step++)
            {
                int index = (_currentPlayerIndex + step) % Players.Count;

                if (index == _currentPlayerIndex) break;   // came back to the same player
                if (Players[index].IsBankrupt) continue;

                _currentPlayerIndex = index;
                OnTurnChanged?.Invoke(Players[index]);
                return true;
            }

            return false;
        }

        /// =========================================================================================
        /// ------------------------------- SEND & RECEIVE MONEY ------------------------------------
        /// =========================================================================================

        /// <summary>
        /// The player pays the BANK. Returns false (and changes nothing) if the amount is negative
        /// or the player does not have enough money. The caller decides what happens then
        /// (sell or mortgage, or bankruptcy).
        /// --> player = who want to pay
        /// --> amount = the amount player has to pay
        /// </summary>
        public bool SendMoney(IPlayer player, int amount)
        {
            if (amount < 0 || player.Money < amount) return false;

            player.Money -= amount;
            OnPlayerMoneyChanged?.Invoke(player);
            return true;
        }

        /// <summary>
        /// The PLAYER pays PLAYER. Returns false (and changes nothing) if the amount is negative
        /// or the paying player does not have enough money.
        /// </summary>
        public bool SendMoney(IPlayer player, int amount, IPlayer otherPlayer)
        {
            if (amount < 0 || player.Money < amount) return false;

            player.Money -= amount;
            otherPlayer.Money += amount;
            OnPlayerMoneyChanged?.Invoke(player);
            OnPlayerMoneyChanged?.Invoke(otherPlayer);
            return true;
        }

        /// <summary>The player receives money from the Bank. The Bank has unlimited money.</summary>
        public void ReceiveMoney(IPlayer player, int amount)
        {
            if (amount < 0) return;

            player.Money += amount;
            OnPlayerMoneyChanged?.Invoke(player);
        }

        /// <summary>
        /// The player receives money from another player. Returns false (and changes nothing)
        /// if the amount is negative or the other player does not have enough money.
        /// </summary>
        public bool ReceiveMoney(IPlayer player, int amount, IPlayer otherPlayer)
        {
            return SendMoney(otherPlayer, amount, player);
        }



        /// =========================================================================================
        /// ------------------------------- MOVE, DICE ROLL, & --------------------------------------
        /// =========================================================================================

        private int RollDiceValues(IPlayer player)
        {
            Dice.Side1 = _random.Next(1, 7);
            Dice.Side2 = _random.Next(1, 7);
            OnDiceRolled?.Invoke(player, Dice.Side1, Dice.Side2);
            return Dice.Side1 + Dice.Side2;
        }

        /// <summary>
        /// Rolls the two dice for the current player, resolves jail/doubles rules, and initiates movement.
        /// </summary>
        public void RollDice()
        {

            // Block if not allowed to roll right now
            if (!CanRoll()) return;

            _hasRolled = true;
            LastDrawnCard = null;


            // if there are someone who hasnt paid his Debt, the game cant continue
            if (_isGameOver || HasPendingDebt()) return;

            IPlayer player = GetCurrentPlayer();
            IToken token = _playerTokens[player];

            // Roll 2 six-sided dice (1 to 6)

            // Dice.Side1 = _random.Next(1, 7);
            // Dice.Side2 = _random.Next(1, 7);
            // int total = Dice.Side1 + Dice.Side2;

            int total = RollDiceValues(player);
            bool isDouble = Dice.Side1 == Dice.Side2;

            // --- JAIL LOGIC (PLR-23, PLR-27, DCE-06) ---
            if (player.IsInJail)
            {
                if (isDouble)
                {
                    // Freed by rolling doubles: moves forward, but does NOT get another roll (DCE-06)
                    player.IsInJail = false;
                    _jailTurns[player] = 0;
                    _doubleCount = 0;
                    Move(token, total);
                }
                else
                {
                    int turns = _jailTurns.GetValueOrDefault(player, 0) + 1;
                    _jailTurns[player] = turns;

                    if (turns >= 3)
                    {
                        // 3rd failed turn in jail: must pay $50, leaves jail, and moves by this roll (PLR-27)
                        if (!SendMoney(player, 50))
                        {
                            // Cannot pay $50 -> record debt to Bank and stay in jail
                            RecordDebt(player, 50, creditor: null);

                        }


                        player.IsInJail = false;
                        _jailTurns[player] = 0;
                        _doubleCount = 0;
                        Move(token, total);
                    }
                    // Otherwise, stays in jail and turn ends without moving
                }
                return;
            }

            // --- NORMAL MOVEMENT LOGIC (DCE-04, DCE-05) ---
            if (isDouble)
            {
                _doubleCount++;

                // 3 consecutive doubles sends the player directly to Jail (DCE-05)
                if (_doubleCount >= 3)
                {
                    _doubleCount = 0;
                    SendToJail(player);
                    return;
                }

                // Moves, and player will be allowed to roll again on this turn
                Move(token, total);
            }
            else
            {
                _doubleCount = 0;
                Move(token, total);
            }
        }


        /// <summary>
        /// Determines whether the current player is entitled to roll again on this turn (e.g., after rolling doubles).
        /// </summary>
        public bool CanRollAgain()
        {
            if (_isGameOver || HasPendingDebt()) return false;

            IPlayer player = GetCurrentPlayer();

            // If the player is in jail or bankrupt, they can NEVER roll again on this turn
            if (player.IsInJail || player.IsBankrupt)
            {
                return false;
            }

            // Must be doubles (and not dice default 0 before rolling)
            // bool isDouble = Dice.Side1 == Dice.Side2 && Dice.Side1 > 0;

            // return isDouble && _doubleCount > 0 && _doubleCount < 3;
            // Must be double, but strictly less than 3 consecutive doubles
            return _doubleCount > 0 && _doubleCount < 3;
        }

        // Helper: checks if the current player is standing on an unowned purchasable property
        // need to buy or auction
        public bool HasUnresolvedPropertyUnderfoot()
        {
            if (_propertyDecisionResolved) return false; // auction is done already/bought
            var player = GetCurrentPlayer();
            var space = GetPlayerPosition(player);
            return space?.Property != null && GetPropertyOwner(space.Property) == null;
        }

        public bool CanRoll()
        {
            if (_isGameOver || HasPendingDebt()) return false;
            if (HasUnresolvedPropertyUnderfoot())
            {
                return false;
            }
            // IPlayer player = GetCurrentPlayer();

            // // checks if the current player is standing on an unowned purchasable property
            // var space = GetPlayerPosition(player);

            // if (space?.Property != null && GetPropertyOwner(space.Property) == null)
            // {
            //     return false;
            // }
            IPlayer player = GetCurrentPlayer();
            if (player.IsBankrupt) return false;

            // Allowed to roll if haven't rolled yet, OR if earned an extra roll via doubles
            return !_hasRolled || CanRollAgain();
        }

        public bool CanEndTurn()
        {
            if (_isGameOver || HasPendingDebt()) return false;

            if (HasUnresolvedPropertyUnderfoot())
            {
                return false;
            }
            IPlayer player = GetCurrentPlayer();

            // var space = GetPlayerPosition(player);
            // if (space?.Property != null && GetPropertyOwner(space.Property) == null)
            // {
            //     return false;
            // }
            if (player.IsBankrupt) return true;

            // Allowed to end turn only after rolling, and NOT while holding an extra double-roll
            return _hasRolled && !CanRollAgain();
        }


        /// <summary>
        /// Moves the token forward (or backward) by the given number of steps,
        /// checks for passing GO ($200), updates token positions, and triggers HandleLanding.
        /// </summary>
        public void Move(IToken token, int steps, bool fromCard = false)
        {
            if (!_tokenPositions.TryGetValue(token, out ISpace? currentSpace))
            {
                return;
            }

            int currentIndex = Board.Path.IndexOf(currentSpace);

            // the square doesnt exist or smth
            if (currentIndex == -1) return;

            // Find the player who owns this token
            IPlayer? player = _playerTokens.FirstOrDefault(kvp => kvp.Value == token).Key;
            if (player == null) return;

            int pathCount = Board.Path.Count;

            // Check if player passed or landed on GO (BRD-10, BRD-11)
            if (steps > 0 && currentIndex + steps >= pathCount)
            {
                ReceiveMoney(player, 200);
            }

            // Wrap-around calculation (handles positive steps or negative steps from cards)
            int targetIndex = (currentIndex + steps) % pathCount;

            // if below 0 means you still not pass Go
            if (targetIndex < 0)
            {
                targetIndex += pathCount;
            }

            // above zero means you pass go
            ISpace newSpace = Board.Path[targetIndex];

            // Update positions
            // remove the previous space the token stand on
            currentSpace.Tokens.Remove(token);

            // move to new space
            newSpace.Tokens.Add(token);

            // update the dictionary
            _tokenPositions[token] = newSpace;

            // reset FLAG, auction helper to continue the turn
            _propertyDecisionResolved = false;

            // Notify observers
            OnPlayerMoved?.Invoke(token, newSpace);

            // Resolve landing on the square
            ResolveLanding(player, fromCard);
        }

        /// <summary>
        /// Resolves the effect of the space the player's token just landed on (SPC-01 to SPC-10).
        /// </summary>
        public void ResolveLanding(IPlayer player, bool fromCard = false)
        {
            IToken token = _playerTokens[player];
            ISpace space = _tokenPositions[token];

            switch (space.Type)
            {
                case SpaceType.Street:
                case SpaceType.Railroad:
                case SpaceType.Utility:
                    if (space.Property != null)
                    {
                        IPlayer? owner = GetPropertyOwner(space.Property);

                        if (owner != null && owner != player)
                        {
                            // Owned by another player: pay rent (SPC-03)
                            HandleRentPayment(player, space.Property, fromCard);
                        }
                        // If unowned: player can buy or trigger auction via BuyProperty() / HandleAuction()
                    }
                    break;

                case SpaceType.IncomeTax:
                    // Flat $200 payment to the Bank (SPC-06)
                    // If player can't pay the $200 Income Tax, record debt to the Bank (creditor = null)
                    if (!SendMoney(player, BoardData.IncomeTaxAmount))
                    {
                        RecordDebt(player, BoardData.IncomeTaxAmount, creditor: null);
                    }

                    break;

                case SpaceType.LuxuryTax:
                    // Flat $100 payment to the Bank (SPC-07)
                    // If player can't pay the $200 Income Tax, record debt to the Bank (creditor = null)
                    if (!SendMoney(player, BoardData.LuxuryTaxAmount))
                    {
                        RecordDebt(player, BoardData.LuxuryTaxAmount, creditor: null);
                    }

                    break;

                case SpaceType.GoToJail:
                    // Sent directly to Jail (SPC-09)
                    SendToJail(player);
                    break;

                case SpaceType.Chance:
                    DrawCard(player, CardType.Chance);
                    break;

                case SpaceType.CommunityChest:
                    DrawCard(player, CardType.CommunityChest);
                    break;

                case SpaceType.Go:
                case SpaceType.Jail:
                case SpaceType.FreeParking:
                    // No additional action needed (SPC-01, SPC-08, SPC-10)
                    break;
            }
        }

        /// <summary>
        /// Sends the player directly to Jail (index 10), without passing GO or collecting $200 (BRD-13, PLR-21).
        /// Their turn ends immediately.
        /// </summary>
        public void SendToJail(IPlayer player)
        {
            player.IsInJail = true;
            _jailTurns[player] = 0;
            _doubleCount = 0;

            IToken token = _playerTokens[player];
            ISpace currentSpace = _tokenPositions[token];
            currentSpace.Tokens.Remove(token);

            // Jail is square index 10 in the standard board (BRD-04)
            ISpace jailSpace = Board.Path[10];
            jailSpace.Tokens.Add(token);
            _tokenPositions[token] = jailSpace;

            OnPlayerMoved?.Invoke(token, jailSpace);

            // When sent to Jail, the turn ends immediately (PLR-21)
            EndTurn();
        }

        /// <summary>
        /// Releases the player from Jail before rolling (PLR-24, PLR-26).
        /// If useCard is true (or null and player has a card), consumes one Get Out of Jail Free card.
        /// Otherwise, pays the $50 fine to the Bank.
        /// Returns true if the player successfully exited Jail and is now free to roll.
        /// </summary>
        public bool HandleGetOutOfJail(IPlayer player, bool? useCard = null)
        {
            if (!player.IsInJail) return false;

            // If choice is not specified: use card if owned, otherwise pay cash
            bool shouldUseCard = useCard ?? (player.JailFreeCard > 0);

            if (shouldUseCard)
            {
                if (player.JailFreeCard <= 0) return false;

                player.JailFreeCard--;
                player.IsInJail = false;
                _jailTurns[player] = 0;
                return true;
            }
            else
            {
                // Pay the $50 fine to the Bank (PLR-26)
                if (!SendMoney(player, 50))
                {
                    return false;
                }

                player.IsInJail = false;
                _jailTurns[player] = 0;
                return true;
            }
        }


        /// =========================================================================================
        /// ---------------------------------- PROPERTY STUFFS --------------------------------------
        /// =========================================================================================

        /// <summary>
        /// Buys an unowned property from the Bank at its printed price (PRP-10, PRP-11).
        /// Returns true if the purchase succeeded, or false if already owned, bankrupt, or insufficient funds.
        /// </summary>
        public bool BuyProperty(IPlayer player, IProperty property)
        {
            if (player == null || property == null) return false;
            if (player.IsBankrupt || _isGameOver) return false;

            // 1. Must be unowned (Bank-owned)
            if (GetPropertyOwner(property) != null)
            {
                return false;
            }

            // validation, to buy a property, the token should be on top of it
            if (!_tokenPositions.TryGetValue(_playerTokens[player], out ISpace? currentSpace) || currentSpace.Property != property)
            {
                return false;
            }

            // 2. Player must be able to afford the printed price (PRP-10)
            // SendMoney deducts the cash and fires OnPlayerMoneyChanged
            if (!SendMoney(player, property.Price))
            {
                return false;
            }

            // 3. Register ownership in the dictionary (PRP-11)
            _propertyOwners[property] = player;

            // RESET, FLAG for auction
            _propertyDecisionResolved = true;

            return true;
        }

        /// <summary>
        /// Calculates and transfers rent from player to property owner (PRP-20 to PRP-29c).
        /// </summary>
        public void HandleRentPayment(IPlayer player, IProperty property, bool fromCard = false)
        {
            if (player == null || property == null) return;

            IPlayer? owner = GetPropertyOwner(property);

            // Unowned or owned by self collects no rent (SPC-04)
            if (owner == null || owner == player) return;

            // Mortgaged properties collect no rent (PRP-24)
            if (property.IsMortgaged) return;

            int rent = 0;

            switch (property.PropertyType)
            {
                case PropertyType.Street:
                    // Rent based on building level 0-5 (PRP-21)
                    rent = property.Rents[property.BuildingLevel];

                    // If unimproved (level 0), check for complete color group monopoly (PRP-22, PRP-23)
                    if (property.BuildingLevel == 0 && property.ColorGroup.HasValue)
                    {
                        bool ownsMonopoly = Board.Path
                            .Where(s => s.Property != null && s.Property.ColorGroup == property.ColorGroup)
                            .All(s => GetPropertyOwner(s.Property!) == owner);

                        if (ownsMonopoly)
                        {
                            rent *= 2; // Doubled rent on full unimproved color group
                        }
                    }
                    break;

                case PropertyType.Railroad:
                    // Rent scales by total railroads owned: 1=$25, 2=$50, 3=$100, 4=$200 (PRP-29a, PRP-29b)
                    // Note: Mortgaged railroads still count toward the total owned count (PRP-29b)
                    int railroadsOwned = _propertyOwners.Count(kvp =>
                        kvp.Value == owner && kvp.Key.PropertyType == PropertyType.Railroad);

                    if (railroadsOwned <= 0)
                    {
                        rent = 0;
                    }
                    else
                    {


                        int[] railroadRents = [25, 50, 100, 200];
                        rent = railroadRents[Math.Clamp(railroadsOwned - 1, 0, 3)];

                        if (fromCard)
                        {
                            rent *= 2;
                        }
                    }
                    break;

                case PropertyType.Utility:
                    // Rent is 4x or 10x the dice roll total (PRP-26, PRP-27, PRP-29c)
                    int utilitiesOwned = _propertyOwners.Count(kvp =>
                        kvp.Value == owner && kvp.Key.PropertyType == PropertyType.Utility);

                    if (utilitiesOwned <= 0)
                    {
                        rent = 0;
                    }
                    else if (fromCard)
                    {
                        // int reroll = _random.Next(1, 7) + _random.Next(1, 7);
                        /// if you land on utility because of card, you got 
                        /// automatically charge 10x

                        int reroll = RollDiceValues(player);
                        rent = reroll * 10;
                    }
                    else
                    {

                        int diceTotal = Dice.Side1 + Dice.Side2;
                        int multiplier = (utilitiesOwned >= 2) ? 10 : 4;
                        rent = diceTotal * multiplier;
                    }
                    break;
            }

            // Transfer rent from the landing player to the owner


            if (!SendMoney(player, rent, owner))
            {
                // _pendingDebtAmount = rent;
                // _pendingDebtor = player;
                // _pendingCreditor = owner;

                RecordDebt(debtor: player, amount: rent, creditor: owner);
            }
        }

        /// <summary>
        /// Pays the pending debt once the player has raised enough cash through mortgages or sales.
        /// </summary>
        public bool PayPendingDebt(IPlayer debtor)
        {
            if (debtor == null)
            {
                return false;
            }

            // if (_pendingDebtAmount == 0 || _pendingDebtor == null) return false;

            bool anyPaid = false;
            for (int i = 0; i < _pendingDebts.Count; i++)
            {
                var (theDebtor, theCreditor, amount) = _pendingDebts[i];
                if (theDebtor != debtor)
                {
                    continue;
                }

                // can the debtor pay
                if (debtor.Money >= amount)
                {
                    bool paid = theCreditor != null ? SendMoney(debtor, amount, theCreditor) : SendMoney(debtor, amount);

                    if (paid)
                    {
                        _pendingDebts.RemoveAt(i);
                        i--;
                        anyPaid = true;
                    }
                }
                else
                {
                    // cant afford it yet
                    // need to raise money, debt is paid in order
                    break;
                }


            }




            return anyPaid; // Still not enough cash
        }

        /// <summary>
        /// Mortgages a property to the Bank in exchange for its printed MortgageValue (PRP-40 to PRP-42).
        /// All buildings in that color group must be sold back to the Bank before mortgaging (PRP-41).
        /// Returns true if mortgaging succeeded, false otherwise.
        /// </summary>
        public bool HandleMortgage(IPlayer player, IProperty property)
        {
            if (player == null || property == null) return false;
            if (player.IsBankrupt || _isGameOver) return false;

            // 1. Player must own the property
            if (GetPropertyOwner(property) != player)
            {
                return false;
            }

            // 2. Cannot mortgage an already mortgaged property
            if (property.IsMortgaged)
            {
                return false;
            }

            // 3. Unimproved rule (PRP-40, PRP-41):
            // If it is a street, neither this property nor ANY other street in its color group can have houses/hotels.
            if (property.PropertyType == PropertyType.Street && property.ColorGroup.HasValue)
            {
                bool hasBuildingsInGroup = Board.Path
                    .Where(s => s.Property != null && s.Property.ColorGroup == property.ColorGroup)
                    .Any(s => s.Property!.BuildingLevel > 0);

                if (hasBuildingsInGroup)
                {
                    // All houses and hotels in the color group must be sold before mortgaging (PRP-41)
                    return false;
                }
            }

            // 4. Mark as mortgaged (PRP-44)
            property.IsMortgaged = true;

            // 5. Bank lends the mortgage value to the player (PRP-42)
            ReceiveMoney(player, property.MortgageValue);

            return true;
        }


        /// <summary>
        /// Lifts a mortgage by repaying the principal plus 10% interest to the Bank (PRP-43).
        /// Returns true if successfully unmortgaged, or false if not owned, not mortgaged, or insufficient funds.
        /// </summary>
        public bool HandleMortgageRedemption(IPlayer player, IProperty property)
        {
            if (player == null || property == null) return false;
            if (player.IsBankrupt || _isGameOver) return false;

            // 1. Player must own the property
            if (GetPropertyOwner(property) != player)
            {
                return false;
            }

            // 2. Property must currently be mortgaged
            if (!property.IsMortgaged)
            {
                return false;
            }

            // 3. Calculate total redemption cost: Principal (MortgageValue) + 10% interest (PRP-43)
            // Using Math.Ceiling handles odd amounts like Utilities ($75 mortgage -> $8 interest)
            int interest = (int)Math.Ceiling(property.MortgageValue * 0.10);
            int redemptionCost = property.MortgageValue + interest;

            // 4. Player pays the Bank (SendMoney checks balance and fires OnPlayerMoneyChanged)
            if (!SendMoney(player, redemptionCost))
            {
                return false; // Insufficient funds to lift mortgage
            }

            // 5. Turn Title Deed faceup again (PRP-44)
            property.IsMortgaged = false;

            return true;
        }

        /// <summary>The owner of a property, or null if the Bank still owns it.</summary>
        public IPlayer? GetPropertyOwner(IProperty property)
        {
            return _propertyOwners.TryGetValue(property, out IPlayer? owner) ? owner : null;
        }

        /// =========================================================================================
        /// ------------------------------- BUY & SELL BUILDING -------------------------------------
        /// =========================================================================================

        /// <summary>
        /// Buys one house on a street property (PRP-49 to PRP-56).
        /// Enforces: complete color group monopoly, no mortgaged properties in the group,
        /// even building, maximum 4 houses, and available Bank house supply.
        /// </summary>
        public bool BuyHouse(IPlayer player, IProperty property)
        {
            if (player == null || property == null) return false;
            if (player.IsBankrupt || _isGameOver) return false;

            // 1. Must be a street property with a valid color group and house cost
            if (property.PropertyType != PropertyType.Street ||
                !property.ColorGroup.HasValue ||
                !property.HouseCost.HasValue)
            {
                return false;
            }

            // 2. BuyHouse only builds up to 4 houses (level 4). Level 5 is a Hotel (BuyHotel) (PRP-49)
            if (property.BuildingLevel >= 4)
            {
                return false;
            }

            // 3. The Bank must have houses in stock (PRP-50, PRP-56)
            if (houseAmount <= 0)
            {
                return false;
            }

            // Get all street properties in this color group
            List<IProperty> groupProperties = Board.Path
                .Where(s => s.Property != null && s.Property.ColorGroup == property.ColorGroup)
                .Select(s => s.Property!)
                .ToList();

            // 4. Player must own the COMPLETE color group (PRP-51)
            bool ownsMonopoly = groupProperties.All(p => GetPropertyOwner(p) == player);
            if (!ownsMonopoly)
            {
                return false;
            }

            // 5. Cannot build if ANY property in the color group is mortgaged (PRP-54)
            bool anyMortgaged = groupProperties.Any(p => p.IsMortgaged);
            if (anyMortgaged)
            {
                return false;
            }

            // 6. Even Building Rule (PRP-53):
            // You cannot build on this street if another street in the group is at a lower building level
            int minLevel = groupProperties.Min(p => p.BuildingLevel);
            if (property.BuildingLevel > minLevel)
            {
                return false; // Must build on the lower properties in the group first!
            }

            // 7. Pay the Bank for the house (PRP-55)
            if (!SendMoney(player, property.HouseCost.Value))
            {
                return false; // Insufficient funds
            }

            // 8. Update building level and deduct 1 house from the Bank supply
            property.BuildingLevel++;
            houseAmount--;

            return true;
        }

        /// <summary>
        /// Sells one house from a street property back to the Bank at half price (PRP-58, PRP-59).
        /// Enforces: player owns the property, has at least 1 house, even selling,
        /// returns 1 house to the Bank supply, and credits half the house cost.
        /// </summary>
        public bool SellHouse(IPlayer player, IProperty property)
        {
            if (player == null || property == null) return false;
            if (player.IsBankrupt || _isGameOver) return false;

            // 1. Must be a street property with a valid color group and house cost
            if (property.PropertyType != PropertyType.Street ||
                !property.ColorGroup.HasValue ||
                !property.HouseCost.HasValue)
            {
                return false;
            }

            // 2. Player must be the owner
            if (GetPropertyOwner(property) != player)
            {
                return false;
            }

            // 3. Must have houses to sell (levels 1 to 4). If level 5 (Hotel), call SellHotel (PRP-49, PRP-64)
            if (property.BuildingLevel < 1 || property.BuildingLevel > 4)
            {
                return false;
            }

            // Get all street properties in this color group
            List<IProperty> groupProperties = Board.Path
                .Where(s => s.Property != null && s.Property.ColorGroup == property.ColorGroup)
                .Select(s => s.Property!)
                .ToList();

            // 4. Even Selling Rule (PRP-58):
            // You cannot sell from this street if another street in the group is at a higher building level
            int maxLevel = groupProperties.Max(p => p.BuildingLevel);
            if (property.BuildingLevel < maxLevel)
            {
                return false; // Must sell from the highest-improved streets in the group first!
            }

            // 5. Bank pays half the original house purchase price (PRP-59)
            int refundAmount = property.HouseCost.Value / 2;
            ReceiveMoney(player, refundAmount);

            // 6. Step down building level and return 1 house to the Bank's supply
            property.BuildingLevel--;
            houseAmount++;

            return true;
        }

        /// <summary>
        /// Converts 4 houses on a street property into 1 hotel (level 5) (PRP-60 to PRP-63).
        /// Enforces: complete color group monopoly, no mortgaged properties in the group,
        /// every street in the group must have at least 4 houses (PRP-61), and available Bank hotel supply.
        /// Returns the 4 houses to the Bank supply (PRP-62).
        /// </summary>
        public bool BuyHotel(IPlayer player, IProperty property)
        {
            if (player == null || property == null) return false;
            if (player.IsBankrupt || _isGameOver) return false;

            // 1. Must be a street property with valid color group and house cost
            if (property.PropertyType != PropertyType.Street ||
                !property.ColorGroup.HasValue ||
                !property.HouseCost.HasValue)
            {
                return false;
            }

            // 2. Player must be the owner
            if (GetPropertyOwner(property) != player)
            {
                return false;
            }

            // 3. Must currently have exactly 4 houses (level 4) (PRP-61, PRP-63)
            if (property.BuildingLevel != 4)
            {
                return false;
            }

            // 4. The Bank must have a hotel in stock (PRP-60: Bank has 12 hotels total)
            if (hotelAmount <= 0)
            {
                return false;
            }

            // Get all street properties in this color group
            List<IProperty> groupProperties = Board.Path
                .Where(s => s.Property != null && s.Property.ColorGroup == property.ColorGroup)
                .Select(s => s.Property!)
                .ToList();

            // 5. Must own the complete color group (PRP-51)
            bool ownsMonopoly = groupProperties.All(p => GetPropertyOwner(p) == player);
            if (!ownsMonopoly)
            {
                return false;
            }

            // 6. No property in the color group can be mortgaged (PRP-54)
            if (groupProperties.Any(p => p.IsMortgaged))
            {
                return false;
            }

            // 7. Even building check for hotels (PRP-61):
            // Every street in the color group must have at least 4 houses (level 4 or 5)
            if (groupProperties.Any(p => p.BuildingLevel < 4))
            {
                return false; // Cannot buy a hotel until all streets in the group reach 4 houses!
            }

            // 8. Pay the Bank the hotel price (which equals the house price per BRD-18)
            if (!SendMoney(player, property.HouseCost.Value))
            {
                return false; // Insufficient funds
            }

            // 9. Exchange buildings with the Bank (PRP-62):
            // The 4 houses on this property return to the Bank's supply
            houseAmount += 4;

            // 1 hotel is taken from the Bank's supply
            hotelAmount--;

            // Upgrade to level 5 (Hotel)
            property.BuildingLevel = 5;

            return true;
        }

        /// <summary>
        /// Steps down a hotel (level 5) to 4 houses on a street property (PRP-64).
        /// Returns the hotel to the Bank, takes 4 houses from the Bank supply,
        /// and refunds half the hotel purchase price to the player.
        /// Denied if the Bank has fewer than 4 houses available (PRP-65).
        /// </summary>
        public bool SellHotel(IPlayer player, IProperty property)
        {
            if (player == null || property == null) return false;
            if (player.IsBankrupt || _isGameOver) return false;

            // 1. Must be a street property with a valid house cost
            if (property.PropertyType != PropertyType.Street || !property.HouseCost.HasValue)
            {
                return false;
            }

            // 2. Player must be the owner
            if (GetPropertyOwner(property) != player)
            {
                return false;
            }

            // 3. Must currently have a Hotel (level 5) (PRP-63, PRP-64)
            if (property.BuildingLevel != 5)
            {
                return false;
            }

            // 4. Housing Shortage Check (PRP-65):
            // Stepping down to 4 houses requires the Bank to supply 4 physical houses.
            // If the Bank has fewer than 4 houses, the voluntary sale is DENIED.
            if (houseAmount < 4)
            {

                // If it's a voluntary sale, it's denied (PRP-65)
                // if (_pendingDebtAmount <= 0)
                if (!HasPendingDebt(player))
                {
                    return false;
                }

                // FORCED SELL-OFF FOR A DEBT (PRP-66):
                // Player owes a debt and Bank has < 4 houses -> liquidate the whole color group!
                return ForceSellColorGroupForDebt(player, property.ColorGroup!.Value);

                // return false;
            }

            // 5. The Bank refunds half of the hotel price (PRP-59, PRP-64, BRD-18)
            int refund = property.HouseCost.Value / 2;
            ReceiveMoney(player, refund);

            // 6. Exchange buildings with the Bank:
            // 1 hotel returns to the Bank's supply
            hotelAmount++;

            // 4 houses are taken from the Bank's supply to place on this property
            houseAmount -= 4;

            // 7. Property steps down from level 5 (Hotel) to level 4 (4 Houses)
            property.BuildingLevel = 4;

            return true;
        }

        /// <summary>
        /// Emergency liquidation when a player owes a debt and the Bank has fewer than 4 houses (PRP-66).
        /// Sells all houses and hotels across the entire color group back to level 0 at half price.
        /// </summary>
        private bool ForceSellColorGroupForDebt(IPlayer player, ColorGroup colorGroup)
        {
            // Find all streets in this color group
            List<IProperty> groupProperties = Board.Path
                .Where(s => s.Property != null && s.Property.ColorGroup == colorGroup)
                .Select(s => s.Property!)
                .ToList();

            int totalRefund = 0;

            foreach (IProperty street in groupProperties)
            {
                if (street.BuildingLevel == 0) continue;

                // If it has a hotel (level 5), return 1 hotel to Bank
                if (street.BuildingLevel == 5)
                {
                    hotelAmount++;
                }
                else
                {
                    // Return houses to Bank
                    houseAmount += street.BuildingLevel;
                }

                // Each level is refunded at half price (PRP-59, PRP-66)
                int levels = street.BuildingLevel;

                // take the house cost price (hotel and houses same value)
                int refundPerLevel = street.HouseCost!.Value / 2;
                totalRefund += levels * refundPerLevel;

                // Reset street back to undeveloped (level 0)
                street.BuildingLevel = 0;
            }

            // Give the liquidation cash to the player so they can pay their debt
            ReceiveMoney(player, totalRefund);

            return true;
        }


        /// =========================================================================================
        /// ------------------------------- BANKRUPTCY & Game Over ----------------------------------
        /// =========================================================================================

        /// <summary>
        /// Eliminates a bankrupt player and transfers all assets to the creditor or the Bank (PLR-30 to PLR-34).
        /// Returns true if bankruptcy was successfully processed.
        /// </summary>
        public bool HandleBankruptcy(IPlayer player)
        {
            if (player == null || player.IsBankrupt) return false;

            // Determine creditor from our pending debt (null = Bank)
            var firstDebt = _pendingDebts.FirstOrDefault(d => d.Debtor == player);
            IPlayer? creditor = firstDebt.Creditor;

            // 1. FORCED LIQUIDATION OF ALL BUILDINGS (PLR-31, PLR-34):
            // All houses and hotels on all streets owned by player are sold to the Bank at half price.
            List<IProperty> ownedProperties = _propertyOwners
                .Where(kvp => kvp.Value == player)
                .Select(kvp => kvp.Key)
                .ToList();

            int buildingRefundTotal = 0;

            foreach (IProperty street in ownedProperties.Where(p => p.PropertyType == PropertyType.Street))
            {
                // IF WE HAVE BUILDING
                if (street.BuildingLevel > 0)
                {
                    if (street.BuildingLevel == 5)
                    {
                        hotelAmount++; // Hotel returns to Bank
                    }
                    else
                    {
                        houseAmount += street.BuildingLevel; // Houses return to Bank
                    }

                    buildingRefundTotal += street.BuildingLevel * (street.HouseCost!.Value / 2);
                    street.BuildingLevel = 0;
                }
            }

            player.Money += buildingRefundTotal;

            // 2. TRANSFER ASSETS TO CREDITOR OR BANK:
            if (creditor != null)
            {
                // --- DEBT TO ANOTHER PLAYER (PLR-31) ---
                // A. Cash goes to creditor
                int cash = player.Money;
                if (cash > 0)
                {
                    player.Money = 0;
                    creditor.Money += cash;
                    OnPlayerMoneyChanged?.Invoke(player);
                    OnPlayerMoneyChanged?.Invoke(creditor);
                }

                // B. Properties go to creditor
                foreach (IProperty prop in ownedProperties)
                {
                    _propertyOwners[prop] = creditor;
                }

                // C. Get Out of Jail Free cards go to creditor
                if (player.JailFreeCard > 0)
                {
                    creditor.JailFreeCard += player.JailFreeCard;
                    player.JailFreeCard = 0;
                }
            }
            else
            {
                // --- DEBT TO THE BANK (PLR-32) ---
                // A. Cash goes to Bank
                player.Money = 0;
                OnPlayerMoneyChanged?.Invoke(player);

                // B. Properties become unowned (Bank holds them)
                foreach (IProperty prop in ownedProperties)
                {
                    _propertyOwners.Remove(prop);
                    prop.IsMortgaged = false; // Reset mortgage status
                }

                // C. Jail cards return to supply
                player.JailFreeCard = 0;
            }

            // 3. REMOVE TOKEN FROM THE BOARD (PLR-33)
            if (_playerTokens.TryGetValue(player, out IToken? token))
            {
                if (_tokenPositions.TryGetValue(token, out ISpace? space))
                {
                    space.Tokens.Remove(token);
                    _tokenPositions.Remove(token);
                }
            }

            // 4. MARK BANKRUPT AND CLEAR DEBT
            player.IsBankrupt = true;
            _jailTurns.Remove(player);
            _doubleCount = 0;

            // player bankrupt, remove debt
            _pendingDebts.RemoveAll(d => d.Debtor == player || d.Creditor == player);

            // 5. NOTIFY OBSERVERS
            OnPlayerBankrupt?.Invoke(player);

            // 6. CHECK FOR GAME OVER (PLR-40)
            if (CheckGameOver())
            {
                EndGame();
            }
            else if (GetCurrentPlayer() == player)
            {
                // Pass turn to next active player if the bankrupt player was currently playing
                NextTurn();
            }

            return true;
        }

        /// <summary>
        /// Checks whether the game has ended (only 1 non-bankrupt player remains) (PLR-40).
        /// </summary>
        public bool CheckGameOver()
        {
            if (_isGameOver) return true;

            int activePlayers = Players.Count(p => !p.IsBankrupt);

            if (activePlayers <= 1)
            {
                _isGameOver = true;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns the last non-bankrupt player who won the game, or null if the game is still ongoing (PLR-40).
        /// </summary>
        public IPlayer? GetWinner()
        {
            if (!CheckGameOver())
            {
                return null; // Game still in progress
            }

            return Players.FirstOrDefault(p => !p.IsBankrupt);
        }

        /// =========================================================================================
        /// --------------------------------- DEBT BANKRUPTCY HELPERD -------------------------------
        /// =========================================================================================

        /// <summary>
        /// Records an unpaid obligation as a pending debt (creditor = null means owed to the Bank).
        /// </summary>
        private void RecordDebt(IPlayer debtor, int amount, IPlayer? creditor)
        {

            _pendingDebts.Add((debtor, creditor, amount));
        }


        /// <summary>Returns true if ANY unresolved debt exists in the game.</summary>
        public bool HasPendingDebt() => _pendingDebts.Count > 0;

        /// <summary>Returns true if a specific player owes an unpaid debt.</summary>
        public bool HasPendingDebt(IPlayer player) => _pendingDebts.Any(d => d.Debtor == player);

        /// =========================================================================================
        /// ------------------------------- API (GetState) ---------------------------------------
        /// =========================================================================================

        public IToken? GetPlayerToken(IPlayer player)
            => _playerTokens.TryGetValue(player, out var token) ? token : null;
        public ISpace? GetPlayerPosition(IPlayer player)
            => _playerTokens.TryGetValue(player, out var token) && _tokenPositions.TryGetValue(token, out var space) ? space : null;



        /// =========================================================================================
        /// ------------------------------- Trading & Auction ---------------------------------------
        /// =========================================================================================


        public IPlayer? TradeBuyer => _tradeBuyer;
        public IPlayer? TradeSeller => _tradeSeller;
        public List<IProperty>? TradeProperties => _tradeProperties;
        public int TradeOffer => _tradeOffer;
        public bool HasPendingTrade => _tradeSeller != null;


        /// <summary>
        /// Sells an undeveloped property back to the Bank (PRP-30 to PRP-32).
        /// The Bank pays the property's MortgageValue (half the printed price per BRD-19).
        /// All buildings in that color group must be sold back to the Bank first (PRP-31).
        /// </summary>
        public bool SellProperty(IPlayer player, IProperty property)
        {
            if (player == null || property == null) return false;
            if (player.IsBankrupt || _isGameOver) return false;

            // 1. Player must be the owner
            if (GetPropertyOwner(property) != player)
            {
                return false;
            }

            // 2. The "No Buildings in Color Group" rule (PRP-31, PRP-32):
            // A street cannot be sold while ANY street in its color group has buildings.
            if (property.PropertyType == PropertyType.Street && property.ColorGroup.HasValue)
            {
                bool hasBuildingsInGroup = Board.Path
                    .Where(s => s.Property != null && s.Property.ColorGroup == property.ColorGroup)
                    .Any(s => s.Property!.BuildingLevel > 0);

                if (hasBuildingsInGroup)
                {
                    return false; // All houses/hotels in the group must be sold to the Bank first (PRP-32)
                }
            }

            // 3. Bank payout:
            // If unmortgaged, Bank pays the MortgageValue (half the printed price per BRD-19).
            // (If already mortgaged, the player already received this loan from the Bank).
            if (!property.IsMortgaged)
            {
                ReceiveMoney(player, property.MortgageValue);
            }

            // 4. Return Title Deed to the Bank
            property.IsMortgaged = false;
            _propertyOwners.Remove(property);

            return true;
        }


        /// <summary>
        /// Executes a private property sale between two players for an agreed cash offer (PRP-30 to PRP-32).
        /// Enforces: seller owns all properties, no buildings exist on any streets in the color group,
        /// and buyer has enough cash for the offer. Mortgaged properties remain mortgaged under the new owner.
        /// </summary>
        public bool TradeProperty(IPlayer buyer, IPlayer seller, List<IProperty> properties, int offer)
        {
            // 1. Basic Validation
            if (buyer == null || seller == null || properties == null) return false;
            if (buyer == seller) return false; // Cannot trade with yourself
            if (buyer.IsBankrupt || seller.IsBankrupt || _isGameOver) return false;
            if (properties.Count == 0 || offer < 0) return false;

            // 2. Buyer must be able to afford the cash offer
            if (buyer.Money < offer)
            {
                return false;
            }

            // 3. Seller must own every property being traded
            foreach (IProperty prop in properties)
            {
                if (GetPropertyOwner(prop) != seller)
                {
                    return false; // Seller does not own this property
                }
            }

            // 4. The "No Buildings in Color Group" rule (PRP-31, PRP-32):
            // A street cannot be traded if ANY street in its color group has buildings.
            foreach (IProperty prop in properties)
            {
                if (prop.PropertyType == PropertyType.Street && prop.ColorGroup != null)
                {
                    bool hasBuildingsInGroup = Board.Path
                        .Where(s => s.Property != null && s.Property.ColorGroup == prop.ColorGroup)
                        .Any(s => s.Property!.BuildingLevel > 0);

                    if (hasBuildingsInGroup)
                    {
                        return false; // All houses/hotels in the group must be sold to the Bank first (PRP-32)
                    }
                }
            }

            // 5. Transfer cash offer from Buyer to Seller (PRP-30)
            if (offer > 0)
            {
                if (!SendMoney(buyer, offer, seller))
                {
                    return false;
                }
            }

            // 6. Transfer ownership of all properties to Buyer
            // (Mortgaged status is preserved as-is; buyer can redeem it later)
            foreach (IProperty prop in properties)
            {
                _propertyOwners[prop] = buyer;
            }

            return true;
        }

        /// <summary>
        /// Proposes a trade to the seller. Validates that the trade is legal before offering (PRP-30 to PRP-32).
        /// Fires OnTradeOffered so the seller's UI can prompt them to Accept or Decline.
        /// </summary>
        public bool ProposeTrade(IPlayer buyer, IPlayer seller, List<IProperty> properties, int offer)
        {
            // 1. Validate that the offer is valid
            if (buyer == null || seller == null || properties == null) return false;
            if (buyer == seller) return false;
            if (buyer.IsBankrupt || seller.IsBankrupt || _isGameOver) return false;
            if (properties.Count == 0 || offer < 0) return false;

            // 2. Buyer must actually have the cash they are offering
            if (buyer.Money < offer) return false;

            // 3. Seller must actually own all properties
            if (properties.Any(p => GetPropertyOwner(p) != seller)) return false;

            // 4. No buildings in the color group rule (PRP-31)
            foreach (IProperty prop in properties)
            {
                if (prop.PropertyType == PropertyType.Street && prop.ColorGroup != null)
                {
                    bool hasBuildings = Board.Path
                        .Where(s => s.Property != null && s.Property.ColorGroup == prop.ColorGroup)
                        .Any(s => s.Property!.BuildingLevel > 0);

                    if (hasBuildings) return false;
                }
            }

            // 5. Store the pending trade proposal
            _tradeBuyer = buyer;
            _tradeSeller = seller;
            _tradeProperties = properties;
            _tradeOffer = offer;

            // 6. Notify the seller through the event
            OnTradeOffered?.Invoke(buyer, seller, properties, offer);

            return true;
        }

        /// <summary>
        /// Called when the seller accepts the proposed trade offer.
        /// Executes the transaction via TradeProperty and clears the offer.
        /// </summary>
        public bool AcceptTrade(IPlayer seller)
        {
            // Only the seller who was offered the trade can accept it
            if (_tradeSeller == null || _tradeBuyer == null || _tradeProperties == null) return false;
            if (seller != _tradeSeller) return false;

            // Execute the trade!
            bool success = TradeProperty(_tradeBuyer, _tradeSeller, _tradeProperties, _tradeOffer);

            // Clear the pending offer
            _tradeBuyer = null;
            _tradeSeller = null;
            _tradeProperties = null;
            _tradeOffer = 0;

            return success;
        }

        /// <summary>
        /// Called when the seller declines the proposed trade offer.
        /// Cancels the offer without changing any game state.
        /// </summary>
        public void DeclineTrade(IPlayer seller)
        {
            if (seller != _tradeSeller) return;

            // Cancel and clear the offer
            _tradeBuyer = null;
            _tradeSeller = null;
            _tradeProperties = null;
            _tradeOffer = 0;
        }



        /// =========================================================================================
        /// ------------------------------------ TRADING CARD ---------------------------------------
        /// =========================================================================================

        public IPlayer? CardTradeBuyer => _cardTradeBuyer;
        public IPlayer? CardTradeSeller => _cardTradeSeller;
        public int CardTradeOffer => _cardTradeOffer;
        public bool HasPendingCardTrade => _cardTradeSeller != null;

        /// <summary>
        /// Buys a Get Out of Jail Free card from another player for an agreed cash amount (CRD-06, PLR-25).
        /// Enforces: seller has at least 1 card, buyer has enough cash, and neither player is bankrupt.
        /// </summary>
        public bool TradeCard(IPlayer buyer, IPlayer seller, int offer)
        {
            // 1. Basic Validation
            if (buyer == null || seller == null) return false;
            if (buyer == seller) return false; // Cannot trade with yourself
            if (buyer.IsBankrupt || seller.IsBankrupt || _isGameOver) return false;
            if (offer < 0) return false;

            // 2. Seller must actually possess a Get Out of Jail Free card (CRD-06)
            if (seller.JailFreeCard <= 0)
            {
                return false;
            }

            // 3. Buyer must be able to afford the cash offer
            if (buyer.Money < offer)
            {
                return false;
            }

            // 4. Transfer cash offer from Buyer to Seller
            if (offer > 0)
            {
                if (!SendMoney(buyer, offer, seller))
                {
                    return false;
                }
            }

            // 5. Transfer the Get Out of Jail Free card
            seller.JailFreeCard--;
            buyer.JailFreeCard++;

            return true;
        }


        /// <summary>
        /// Proposes to buy a Get Out of Jail Free card from another player for an agreed offer (CRD-06).
        /// Validates that seller has a card and buyer has the cash, then fires OnCardTradeOffered.
        /// </summary>
        public bool ProposeCardTrade(IPlayer buyer, IPlayer seller, int offer)
        {
            // 1. Validation
            if (buyer == null || seller == null) return false;
            if (buyer == seller) return false;
            if (buyer.IsBankrupt || seller.IsBankrupt || _isGameOver) return false;
            if (offer < 0) return false;

            // 2. Seller must actually have a card
            if (seller.JailFreeCard <= 0) return false;

            // 3. Buyer must be able to afford the offer
            if (buyer.Money < offer) return false;

            // 4. Save the pending proposal
            _cardTradeBuyer = buyer;
            _cardTradeSeller = seller;
            _cardTradeOffer = offer;

            // 5. Notify the seller through the event
            OnCardTradeOffered?.Invoke(buyer, seller, offer);

            return true;
        }


        /// <summary>
        /// Called when the seller accepts the card offer.
        /// Executes TradeCard and transfers the cash and card.
        /// </summary>
        public bool AcceptCardTrade(IPlayer seller)
        {
            if (_cardTradeSeller == null || _cardTradeBuyer == null) return false;
            if (seller != _cardTradeSeller) return false;

            // Execute the trade!
            bool success = TradeCard(_cardTradeBuyer, _cardTradeSeller, _cardTradeOffer);

            // Clear the pending offer
            _cardTradeBuyer = null;
            _cardTradeSeller = null;
            _cardTradeOffer = 0;

            return success;
        }

        /// <summary>
        /// Called when the seller declines the card offer.
        /// Clears the offer without moving any money or cards.
        /// </summary>
        public void DeclineCardTrade(IPlayer seller)
        {
            if (seller != _cardTradeSeller) return;

            // Cancel and clear the offer
            _cardTradeBuyer = null;
            _cardTradeSeller = null;
            _cardTradeOffer = 0;
        }

        /// =========================================================================================
        /// --------------------------------------- AUCTION -----------------------------------------
        /// =========================================================================================

        public IProperty? AuctionProperty => _auctionProperty;
        public IPlayer? HighestBidder => _highestBidder;
        public int HighestBid => _highestBid;
        public bool HasActiveAuction => _auctionProperty != null;

        ///  <summary>
        /// Called when a player lands on an unowned property and declines to buy it at the printed price (PRP-12).
        /// Immediately puts the property up for auction among all players (PRP-13).
        /// </summary>
        public void DeclineProperty(IPlayer player, IProperty property)
        {
            _propertyDecisionResolved = true;
            // Rule PRP-12: If the player declines, the Bank auctions it!
            StartAuction(property);
        }

        /// <summary>
        /// Opens an auction for an unowned property (PRP-12, PRP-13).
        /// Resets the highest bid to 0 and notifies the players.
        /// </summary>
        public bool StartAuction(IProperty property)
        {
            if (property == null || GetPropertyOwner(property) != null || _isGameOver)
            {
                return false;
            }

            _auctionProperty = property;
            _highestBidder = null;
            _highestBid = 0;

            // Initialize all active (non-bankrupt) players as bidders
            _auctionBidders = Players.Where(p => !p.IsBankrupt).ToList();

            // Turn starts with the player who declined
            _auctionTurnIndex = _currentPlayerIndex;

            OnAuctionStarted?.Invoke(property);
            return true;
        }

        /// <summary>
        /// Places a bid in the current auction (PRP-13).
        /// Validates that the player is active, has enough cash, and bids higher than the current top bid.
        /// </summary>
        public bool PlaceBid(IPlayer player, int amount)
        {
            if (_auctionProperty == null) return false;
            if (player == null || player.IsBankrupt || _isGameOver) return false; // to make sure it is your turn

            if (player != CurrentAuctionBidder) return false;

            // Cannot bid against yourself!
            if (player == _highestBidder) return false;

            // Bid must be higher than current top bid, and player must actually have the cash
            if (amount <= _highestBid || player.Money < amount)
            {
                return false;
            }

            _highestBidder = player;
            _highestBid = amount;

            OnBidPlaced?.Invoke(player, amount);

            if (_auctionBidders.Count <= 1)
            {
                HandleAuction(_auctionProperty);
                return true;
            }

            // Advance bidding turn to the next remaining bidder
            // if (_auctionBidders.Count > 1)
            // {
            //     _auctionTurnIndex = (_auctionTurnIndex + 1) % _auctionBidders.Count;
            // }

            _auctionTurnIndex = (_auctionTurnIndex + 1) % _auctionBidders.Count;
            return true;
        }

        /// <summary>
        /// A player passes their turn in the auction and drops out (PRP-13).
        /// </summary>
        public bool PassBid(IPlayer player)
        {
            if (_auctionProperty == null || player != CurrentAuctionBidder) return false;

            // Remove this player from the auction
            _auctionBidders.Remove(player);

            // Case 1: Nobody left at all ➔ Auction ends, stays with Bank
            if (_auctionBidders.Count == 0)
            {
                HandleAuction(_auctionProperty);
                return true;
            }

            // Case 2: Only 1 bidder remains AND someone placed a bid ➔ Remaining player wins!
            if (_auctionBidders.Count == 1 && _highestBidder != null)
            {
                HandleAuction(_auctionProperty);
                return true;
            }

            // Case 3: More bidders remain ➔ Advance turn
            _auctionTurnIndex = _auctionTurnIndex % _auctionBidders.Count;
            return true;
        }

        /// <summary>
        /// Closes the auction for the specified property (PRP-14).
        /// Collects the winning bid from the highest bidder, awards the Title Deed,
        /// and returns the winning player (or null if no bids were placed).
        /// </summary>
        public IPlayer? HandleAuction(IProperty property)
        {
            if (property == null || _auctionProperty != property) return null;

            IPlayer? winner = _highestBidder;
            int winningBid = _highestBid;

            if (winner != null && winningBid > 0)
            {
                // 1. Winner pays cash to the Bank (PRP-14)
                SendMoney(winner, winningBid);

                // 2. Winner receives the Title Deed
                _propertyOwners[property] = winner;
            }

            // 3. Notify players of the auction result
            OnAuctionEnded?.Invoke(property, winner, winningBid);

            // 4. Clear auction state
            _auctionProperty = null;
            _highestBidder = null;
            _highestBid = 0;

            return winner;
        }




        /// <summary>
        /// Direct execution overload: awards the auctioned property to the specified winner for the winning bid.
        /// </summary>
        // public IPlayer? HandleAuction(IProperty property, IPlayer winner, int winningBid)
        // {
        //     if (property == null || winner == null || winningBid < 0) return null;
        //     if (winner.IsBankrupt || _isGameOver) return null;

        //     if (!SendMoney(winner, winningBid))
        //     {
        //         return null; // Winner cannot afford the bid
        //     }

        //     _propertyOwners[property] = winner;
        //     return winner;
        // }


    }
}