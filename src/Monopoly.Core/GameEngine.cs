using Monopoly.Core.Interfaces;
using Monopoly.Core.Models;

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
        public List<IPlayer> Players { get; }

        private readonly Dictionary<IProperty, IPlayer> _propertyOwners = new Dictionary<IProperty, IPlayer>();

        private readonly Dictionary<IPlayer, IToken> _playerTokens = new Dictionary<IPlayer, IToken>();

        private int _currentPlayerIndex;

        /// <summary>Houses still in the Bank.</summary>
        public int houseAmount { get; set; } = StartingHouses;

        /// <summary>Hotels still in the Bank.</summary>
        public int hotelAmount { get; set; } = StartingHotels;

        private int _doubleCount;
        private readonly ICardDeck _chanceDeck = new Carddeck();
        private readonly ICardDeck _communityChestDeck = new Carddeck();
        private bool _isGameOver;

        public Action<IToken, ISpace>? OnPlayerMoved;
        public Action<IPlayer>? OnTurnChanged;
        public Action<IPlayer>? OnPlayerMoneyChanged;
        public Action<IPlayer, ICard>? OnCardEffect;
        public Action<IPlayer>? OnPlayerBankrupt;

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
            // InitializePath();
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


            // if (playerNames.Count < MinPlayers || playerNames.Count > MaxPlayers)
            // {
            //     return false;
            // }

            // if (playerNames.Count != tokenColors.Count)
            // {
            //     return false;
            // }

            // if (playerNames.Any(string.IsNullOrWhiteSpace))
            // {
            //     return false;
            // }

            // if (playerNames.Distinct().Count() != playerNames.Count)
            // {
            //     return false;
            // }

            // if (tokenColors.Distinct().Count() != tokenColors.Count)
            // {
            //     return false;
            // }

            ISpace go = Board.Path[0];

            Players.Clear();

            for (int i = 0; i < players.Count; i++)
            {
                IPlayer player = players[i];
                IToken token = tokens[i];

                token.CurrentPosition=go;
                go.Tokens.Add(token);

                Players.Add(player);
                _playerTokens[player] = token;
            }

            // for (int i = 0; i < playerNames.Count; i++)
            // {
            //     IToken token = new Token(tokenColors[i], go);
            //     go.Tokens.Add(token);
            //     Players.Add(new Player(playerNames[i], token));
            // }

            _currentPlayerIndex = 0;
            return true;
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

    }
}