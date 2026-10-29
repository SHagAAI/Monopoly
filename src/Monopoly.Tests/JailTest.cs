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
    public class JailTest
    {
        private GameEngine _engine;
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
        public void SendToJail_PlayerLandOnGoToJail_AndSetsInJailTrue()
        {
            // Arrange
            var player = players[0];
            var token = _engine.GetPlayerToken(player)!;

            // Act: Move directly to Space 30 ("Go to Jail")
            _engine.Move(token, steps: 30);

            // Assert
            // 1. Player is marked as in jail
            Assert.That(player.IsInJail, Is.True);

            // 2. Token is moved to Space 10 (Jail)
            var currentSpace = _engine.Board.Path[10];
            Assert.That(currentSpace.Tokens.Contains(token), Is.True);

            // 3. Turn ends immediately (current player rotated to Player 2)
            Assert.That(_engine.GetCurrentPlayer(), Is.EqualTo(players[1]));
        }

        #region HandleGetOutJail

        [Test]
        public void HandleGetOutOfJail_NotInJail_ReturnFalse()
        {
            // Arrange: Player is free and not in Jail
            var player = players[0];
            int startingMoney = player.Money;

            // Act: Attempt to release a player who is not incarcerated
            bool result = _engine.HandleGetOutOfJail(player);

            // Assert: Must return false and money remains untouched
            Assert.That(result, Is.False, "Player not in jail");
            Assert.That(player.Money, Is.EqualTo(startingMoney));
        }

        [Test]
        public void HandleGetOutOfJail_PayFine_EnoughMoney_ReturnTrue()
        {
            // Arrange: Put Player 1 in Jail
            var player = players[0];
            _engine.SendToJail(player);
            Assert.That(player.IsInJail, Is.True);

            int balanceBefore = player.Money;

            // Act: Pay the $50 fine to the Bank
            bool result = _engine.HandleGetOutOfJail(player, useCard: false);

            // Assert: Player is freed, returns true, and exactly $50 is deducted
            Assert.That(result, Is.True, "Freed, pay fine");
            Assert.That(player.IsInJail, Is.False);
            Assert.That(player.Money, Is.EqualTo(balanceBefore - 50));
        }

        [Test]
        public void HandleGetOutOfJail_PayFine_NotEnougMoney_ReturnFalse()
        {
            // Arrange: Put Player 1 in Jail with only $30 (cannot afford the $50 fine)
            var player = players[0];
            _engine.SendToJail(player);
            player.Money = 30;

            // Act: Attempt to pay fine with insufficient money
            bool result = _engine.HandleGetOutOfJail(player, useCard: false);

            // Assert: Must return false, stays in Jail, and money is untouched
            Assert.That(result, Is.False, "Stay in jail, cant pay fine");
            Assert.That(player.IsInJail, Is.True);
            Assert.That(player.Money, Is.EqualTo(30));
        }

        [Test]
        public void HandleGetOutOfJail_UseCard_HasCard_ReturnTrue()
        {
            // Arrange: Put Player 1 in Jail and give them 1 "Get Out of Jail Free" card
            var player = players[0];
            _engine.SendToJail(player);

            player.JailFreeCard = 1;
            int balanceBefore = player.Money;

            // Act: Use the card to get out of Jail
            bool result = _engine.HandleGetOutOfJail(player, useCard: true);

            // Assert: Player is freed, card count decrements to 0, and no money is deducted
            Assert.That(result, Is.True, "freed from jail use card");
            Assert.That(player.IsInJail, Is.False);
            Assert.That(player.JailFreeCard, Is.EqualTo(0));
            Assert.That(player.Money, Is.EqualTo(balanceBefore));
        }

        [Test]
        public void HandleGetOutOfJail_UseCard_HasNoCard_ReturnFalse()
        {
            // Arrange: Put Player 1 in Jail with 0 cards
            var player = players[0];
            _engine.SendToJail(player);
            player.JailFreeCard = 0;

            // Act: Try to force using a card when player has none
            bool result = _engine.HandleGetOutOfJail(player, useCard: true);

            // Assert: Must return false and remain in Jail
            Assert.That(result, Is.False, "no card to use");
            Assert.That(player.IsInJail, Is.True);
        }

        [Test]
        public void HandleGetOutOfJail_DefaultChoice_PrefersCardOverPayingFine()
        {
            // Arrange: Put Player 1 in Jail with both $1500 AND a JailFreeCard
            var player = players[0];
            _engine.SendToJail(player);
            
            player.JailFreeCard = 1;
            int balanceBefore = player.Money;

            // Call without specifying useCard (tests the `useCard ?? (player.JailFreeCard > 0)` fallback)
            bool result = _engine.HandleGetOutOfJail(player);

            // Assert: Should prioritize consuming the card to save the player's cash
            Assert.That(result, Is.True);
            Assert.That(player.IsInJail, Is.False);
            Assert.That(player.JailFreeCard, Is.EqualTo(0));
            Assert.That(player.Money, Is.EqualTo(balanceBefore)); // $0 paid
        }



        #endregion
    }
}