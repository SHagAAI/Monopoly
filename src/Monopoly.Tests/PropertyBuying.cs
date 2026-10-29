using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Monopoly.Core;
using Monopoly.Core.Interfaces;
using Monopoly.Core.Models;
using NUnit.Framework;

namespace Monopoly.Tests
{
    public class PropertyBuying
    {
        private GameEngine _engine;
        // List<IPlayer> players = new();
        List<IPlayer> players;

        List<IToken> tokens;
        [SetUp]
        public void Setup()
        {
            _engine = new GameEngine(new Board(), new Dice());
            players = new List<IPlayer>
            {
                new Player(id:1, name: "Ant"),
                new Player(id:2, name: "Thonny")
            };

            tokens = new List<IToken>
            {
                new Token(color: Core.Enums.TokenColor.Red),
                new Token(color: Core.Enums.TokenColor.Blue)
            };


            _engine.InitializeBoard();
            _engine.InitializeCardDecks();
            _engine.InitializePlayers(players, tokens);
            _engine.StartGame();


        }

        [Test]
        public void BuyProperty_PlayerNull_ReturnFalse()
        {
            // Arrange
            var property = _engine.Board.Path[1].Property!; // Mediterranean Avenue ($60)

            // Act: Attempt to purchase with null player
            bool result = _engine.BuyProperty(null!, property);

            // Assert: Must fail safely and leave property unowned
            Assert.That(result, Is.False, "Player cant be nul");
            Assert.That(_engine.GetPropertyOwner(property), Is.Null);
        }
        [Test]
        public void BuyProperty_PropertyNull_ReturnFalse()
        {
            // Arrange
            var player = players[0];
            int balanceBefore = player.Money;

            // Act: Attempt to purchase with null property
            bool result = _engine.BuyProperty(player, null!);

            // Assert: Must fail safely and leave player money untouched
            Assert.That(result, Is.False, "Property cant be null");
            Assert.That(player.Money, Is.EqualTo(balanceBefore));
        }

        [Test]
        public void BuyProperty_WhereWeStand_UnownedProperty_ReturnTrue()
        {
            _engine.Move(token: tokens[0], steps: 1, fromCard: false);

            var property = _engine.Board.Path[1].Property;
            var result = _engine.BuyProperty(player: players[0], property: property!);

            var currentPlayerMoney = _engine.GetCurrentPlayer().Money;
            // _engine.GetPropertyOwner(property!);

            Assert.That(result, Is.True);
            Assert.That(currentPlayerMoney, Is.EqualTo(1500 - 60));
            Assert.That(_engine.GetPropertyOwner(property!), Is.EqualTo(_engine.GetCurrentPlayer()));

        }

        [Test]
        public void BuyProperty_WhereWeStand_OpponentOwnedProperty_ReturnFalse()
        {
            _engine.Move(token: tokens[0], steps: 1, fromCard: false);
            var property = _engine.Board.Path[1].Property;
            _engine.BuyProperty(player: players[0], property: property!);

            // Move Player 2
            var token2 = _engine.GetPlayerToken(players[1])!;
            _engine.Move(token: token2, steps: 1);

            var result = _engine.BuyProperty(player: players[1], property: property!);

            Assert.That(result, Is.False, "Player 2 should NOT be able to buy an already-owned property.");

        }

        [Test]
        public void BuyProperty_NotWeStandOn_UnOwnedProperty_ReturnFalse()
        {
            // Arrange
            var player = players[0];
            var token = _engine.GetPlayerToken(player)!;

            // var mediterranean = _engine.Board.Path[1].Property!; // Space 1
            var baltic = _engine.Board.Path[3].Property!;        // Space 3

            // Move token 1 step onto Mediterranean Avenue
            _engine.Move(token, steps: 1);

            // Act: Attempt to buy Baltic Avenue while physically standing on Mediterranean Avenue
            var result = _engine.BuyProperty(player, baltic);

            // Assert: Rejected because token is on Mediterranean, not Baltic
            Assert.That(result, Is.False, "Player should NOT be able to buy a property their token is not standing on.");
            Assert.That(_engine.GetPropertyOwner(baltic), Is.Null);
            Assert.That(player.Money, Is.EqualTo(1500)); // No money deducted
        }


        [Test]
        public void BuyProperty_NotPropertySquare_ReturnFalse()
        {
            var player = _engine.GetCurrentPlayer();
            var token = _engine.GetPlayerToken(player)!;

            _engine.Move(token, steps: 2);

            // Board.Path[2] should be a community chest
            var result = _engine.BuyProperty(player, _engine.Board.Path[2].Property!);
            Assert.That(result, Is.False, "Cannot buy on a non-property square.");
        }

        [Test]
        public void BuyProperty_NotEnoughMoney_ReturnFalse()
        {
            _engine.Move(token: tokens[0], steps: 1);
            players[0].Money = 20;

            var result = _engine.BuyProperty(players[0], _engine.Board.Path[1].Property!);

            Assert.That(result, Is.False, "Player cannot buy a propert, Not Enough Money.");
        }

        [Test]
        public void BuyProperty_GameNotStarted_ReturnFalse()
        {
            // THERE ARE NO StartGame()
            var newEngine = new GameEngine(new Board(), new Dice());
            newEngine.InitializeBoard();
            newEngine.InitializePlayers(players, tokens);

            newEngine.Move(token: tokens[0], steps: 1);

            var result = newEngine.BuyProperty(players[0], newEngine.Board.Path[1].Property!);

            Assert.That(result, Is.False, "Cannot buy property before the match started.");
        }

        [Test]
        public void BuyProperty_BuyerIsBankrupt_ReturnFalse()
        {


            _engine.Move(token: tokens[0], steps: 1);
            players[0].IsBankrupt = true;

            var result = _engine.BuyProperty(players[0], _engine.Board.Path[1].Property!);

            Assert.That(result, Is.False, "Cannot buy property before the match started.");
        }
    }
}