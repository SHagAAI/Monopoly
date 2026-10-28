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
    public class Purchasing
    {
        private GameEngine _engine;
        List<IPlayer> players = new();
        [SetUp]
        public void Setup()
        {
            _engine = new GameEngine(new Board(), new Dice());

            players = new List<IPlayer>
            {
                new Player(id:1, name: "Ant"),
                new Player(id:2, name: "Thonny")
            };

            var tokens = new List<IToken>
            {
                new Token(color: Core.Enums.TokenColor.Red),
                new Token(color: Core.Enums.TokenColor.Blue)
            };

            _engine.InitializeBoard();
            _engine.InitializeCardDecks();
            _engine.InitializePlayers(players, tokens);

            _engine.Move(token: tokens[0], steps: 1, fromCard: false);
        }

        [Test]
        public void BuyProperty_WhereWeStand_UnownedProperty_()
        {
            var property = _engine.Board.Path[1].Property;
            var result = _engine.BuyProperty(player: players[0], property: property!);
            var currentPlayerMoney = _engine.GetCurrentPlayer().Money - property!.Price;
            _engine.GetPropertyOwner(property);

            Assert.That(result, Is.True);
            Assert.That(currentPlayerMoney, Is.EqualTo(1500 - 60));
            Assert.That(_engine.GetPropertyOwner(property), Is.EqualTo(_engine.GetCurrentPlayer()));

        }

    }
}