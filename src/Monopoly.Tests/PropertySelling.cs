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
    public class SellingProperty
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
        public void SellProperty_PlayerNull_ReturnFalse()
        {
            var property = _engine.Board.Path[1].Property!;
            bool result = _engine.SellProperty(null!, property);
            Assert.That(result, Is.False, "player cant be null");
        }

        [Test]
        public void SellProperty_PropertyNull_ReturnFalse()
        {
            var player = players[0];
            bool result = _engine.SellProperty(player, null!);
            Assert.That(result, Is.False, "property cant be null");
        }

        [Test]
        public void SellProperty_PlayerBankrupt_ReturnFalse()
        {
            // Arrange
            var player = players[0];
            var token = _engine.GetPlayerToken(player)!;
            var mediterranean = _engine.Board.Path[1].Property!;

            _engine.Move(token, steps: 1);
            _engine.BuyProperty(player, mediterranean);

            player.IsBankrupt = true;
            int balanceBefore = player.Money;

            // Act: Bankrupt player attempts to sell deed
            bool result = _engine.SellProperty(player, mediterranean);

            // Assert: Must return false and ownership is retained
            Assert.That(result, Is.False);
            Assert.That(player.Money, Is.EqualTo(balanceBefore));
        }

        [Test]
        public void SellProperty_NotOwner_ReturnFalse()
        {
            // Arrange: Player 1 buys Mediterranean Avenue
            var player1 = players[0];
            var player2 = players[1];

            var token1 = _engine.GetPlayerToken(player1)!;
            var mediterranean = _engine.Board.Path[1].Property!;

            _engine.Move(token1, steps: 1);
            _engine.BuyProperty(player1, mediterranean);

            int player2BalanceBefore = player2.Money;

            // Act: Player 2 attempts to sell Player 1's property
            bool result = _engine.SellProperty(player2, mediterranean);

            // Assert: Must return false and ownership remains with Player 1
            Assert.That(result, Is.False, "cant sell other player property");
            Assert.That(_engine.GetPropertyOwner(mediterranean), Is.EqualTo(player1));
            Assert.That(player2.Money, Is.EqualTo(player2BalanceBefore));
        }

        [Test]
        public void SellProperty_ColorGroupHasBuildings_ReturnFalse()
        {
            // Arrange: Player 1 owns both Brown properties and builds a house on Mediterranean
            var player = players[0];
            var token = _engine.GetPlayerToken(player)!;

            var mediterranean = _engine.Board.Path[1].Property!;
            var baltic = _engine.Board.Path[3].Property!;

            _engine.Move(token, steps: 1);
            _engine.BuyProperty(player, mediterranean);

            _engine.Move(token, steps: 2);
            _engine.BuyProperty(player, baltic);

            _engine.BuyHouse(player, mediterranean);
            Assert.That(mediterranean.BuildingLevel, Is.EqualTo(1));

            // Act: Attempt to sell Baltic while Mediterranean has a house
            bool result = _engine.SellProperty(player, baltic);

            // Assert: PRP-31/PRP-32: Cannot sell a street if any street in its color group has buildings
            Assert.That(result, Is.False, "Cannot sell if any street in its color group has buildings");
            Assert.That(_engine.GetPropertyOwner(baltic), Is.EqualTo(player));
        }

        [Test]
        public void SellProperty_Unmortgaged_PaysMortgageValueAndReturnsDeedToBank()
        {
            // Arrange: Player 1 buys Mediterranean Avenue ($60 price, $30 mortgage value)
            var player = players[0];
            var token = _engine.GetPlayerToken(player)!;
            var mediterranean = _engine.Board.Path[1].Property!;

            _engine.Move(token, steps: 1);
            _engine.BuyProperty(player, mediterranean);

            int balanceBeforeSale = player.Money;

            // Act: Sell unmortgaged property back to the Bank
            bool result = _engine.SellProperty(player, mediterranean);

            // Assert: Returns true, deed returns to Bank (null owner), player receives MortgageValue ($30)
            Assert.That(result, Is.True);
            Assert.That(_engine.GetPropertyOwner(mediterranean), Is.Null);
            Assert.That(mediterranean.IsMortgaged, Is.False);
            Assert.That(player.Money, Is.EqualTo(balanceBeforeSale + mediterranean.MortgageValue));
        }

        [Test]
        public void SellProperty_AlreadyMortgaged_ReturnsDeedToBankWithoutDuplicatePayout()
        {
            // Arrange: Player 1 buys Mediterranean and mortgages it (already received $30 loan)
            var player = players[0];
            var token = _engine.GetPlayerToken(player)!;
            var property = _engine.Board.Path[1].Property!;

            _engine.Move(token, steps: 1);
            _engine.BuyProperty(player, property);
            
            _engine.HandleMortgage(player, property);
            Assert.That(property.IsMortgaged, Is.True);

            int balanceBeforeSale = player.Money;

            // Act: Sell an already-mortgaged property back to the Bank
            bool result = _engine.SellProperty(player, property);

            // Assert: Returns true, deed returns to Bank unmortgaged, but NO duplicate cash is granted
            Assert.That(result, Is.True);
            Assert.That(_engine.GetPropertyOwner(property), Is.Null);
            Assert.That(property.IsMortgaged, Is.False); // Reset to unmortgaged for next buyer
            Assert.That(player.Money, Is.EqualTo(balanceBeforeSale)); // No extra money
        }
    }
}