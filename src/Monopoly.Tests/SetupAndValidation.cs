using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Monopoly.Core;
using Monopoly.Core.Enums;
using Monopoly.Core.Interfaces;
using Monopoly.Core.Models;
using NUnit.Framework;

namespace Monopoly.Tests
{
    [TestFixture]
    public class SetupAndValidation
    {
        private GameEngine _engine;

        [SetUp]
        public void Setup()
        {
            _engine = new GameEngine(new Board(), new Dice());
        }

        [Test]
        public void IsBoardSquare_Ready()
        {
            _engine.InitializeBoard();
            Assert.That(_engine.Board.Grid[10, 10].Name, Is.EqualTo("GO"), "Bottom right corner should be Go");

        }

        [Test]
        public void IsBoardPath_Ready_Return40()
        {
            _engine.InitializeBoard();

            var pathCount = _engine.Board.Path.Count();
            Assert.That(pathCount, Is.EqualTo(40), "Path should be 40 square");
        }

        [Test]
        public void InitializePlayer_WithoutBoard_ReturnFalse()
        {

            var players = new List<IPlayer>
            {
                new Player(id:1, name: "Ant"),
                new Player(id:2, name: "Thonny")
            };

            var tokens = new List<IToken>
            {
                new Token(color: Core.Enums.TokenColor.Red),
                new Token(color: Core.Enums.TokenColor.Blue)
            };


            Assert.Throws<InvalidOperationException>(() =>
            {
                _engine.InitializePlayers(players, tokens);
            }, "Should Initialize Board first before Players");


        }
        
        [TestCase(1)]
        [TestCase(7)]

        public void InitializePlayer_Below2_Above6_ReturnFalse(int playerCount)
        {

            var players = new List<IPlayer>();
            var tokens = new List<IToken>();

            TokenColor[] colors =
            {
                TokenColor.Red, TokenColor.Green, TokenColor.Blue, TokenColor.Yellow, TokenColor.Purple, TokenColor.Orange, TokenColor.Green
            };

            for (int i = 0; i < playerCount; i++)
            {
                players.Add(new Player(id: i + 1, name: $"Player{i + 1}"));
                tokens.Add(new Token(color: colors[i]));
            }

            _engine.InitializeBoard();

            Assert.That(_engine.InitializePlayers(players, tokens), Is.False ,"Players amount doesnt fit the constraint");


        }


        
    
    }
}