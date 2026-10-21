using Monopoly.Core;
using Monopoly.Core.Interfaces;
using Monopoly.Core.Models;

namespace Monopoly.Api.Services
{
    public class GameService
    {
           
        public GameEngine? Engine {get; private set;} 

        public GameEngine StartNewGame(List<IPlayer> players, List<IToken> tokens)
        {
            IBoard board = new Board();
            IDice dice = new Dice();

            Engine = new GameEngine(board, dice, players);

            // INITIALIZE COMPONENTS
            Engine.InitializeBoard();
            Engine.InitializePath();
            Engine.InitializeCardDecks();
            Engine.InitializePlayers(players, tokens);

            // START THE MATCH
            Engine.StartGame();


            return Engine;
        }
    }
}