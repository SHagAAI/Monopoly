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

    public class Mortgage
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
        public void HandleMortgage_PlayerNull_ReturnFalse()
        {
            // Arrange
            var property = _engine.Board.Path[1].Property!;

            // Act: Attempt to mortgage passing a null player reference
            bool result = _engine.HandleMortgage(null!, property);

            // Assert: Must return false and property remains unmortgaged
            Assert.That(result, Is.False, "Player cant be null");
            Assert.That(property.IsMortgaged, Is.False);
        }

        [Test]
        public void HandleMortgage_PropertyNull_ReturnFalse()
        {
            // Arrange
            var player = players[0];
            int balanceBefore = player.Money;

            // Act: Attempt to mortgage passing a null property reference
            bool result = _engine.HandleMortgage(player, null!);

            // Assert: Must return false and no cash paid to the player
            Assert.That(result, Is.False, "Property cant be null");
            Assert.That(player.Money, Is.EqualTo(balanceBefore));
        }

        [Test]
        public void HandleMortgage_PlayerBankrupt_ReturnFalse()
        {
            // Arrange
            var player = players[0];
            var token = _engine.GetPlayerToken(player)!;
            var property = _engine.Board.Path[1].Property!;

            // Player 1 legitimately buys Mediterranean Avenue
            _engine.Move(token, steps: 1);
            _engine.BuyProperty(player, property);

            // Player goes bankrupt
            player.IsBankrupt = true;
            int balanceBefore = player.Money;

            // Act: Bankrupt player attempts to mortgage
            bool result = _engine.HandleMortgage(player, property);

            // Assert: Must return false, deed remains unmortgaged, no cash awarded
            Assert.That(result, Is.False, "Bankrup Player cant do mortgage");
            Assert.That(property.IsMortgaged, Is.False);
            Assert.That(player.Money, Is.EqualTo(balanceBefore));
        }

        [Test]
        public void HandleMortgage_GameStatusNotInProgress_ReturnFalse()
        {
            // Arrange
            var player = players[0];
            var token = _engine.GetPlayerToken(player)!;
            var property = _engine.Board.Path[1].Property!;

            _engine.Move(token, steps: 1);
            _engine.BuyProperty(player, property);

            // Game concludes
            _engine.EndGame();
            int balanceBefore = player.Money;

            // Act: Attempt to mortgage after match ends
            bool result = _engine.HandleMortgage(player, property);

            // Assert: Must return false and state remains untouched
            Assert.That(result, Is.False, "Game not in progress");
            Assert.That(property.IsMortgaged, Is.False);
            Assert.That(player.Money, Is.EqualTo(balanceBefore));
        }



        [Test]
        public void HandleMortgage_OwnedByYou_ReturnTrue()
        {
            var player = _engine.GetCurrentPlayer();
            var token = _engine.GetPlayerToken(player)!;

            var Mediterranean = _engine.Board.Path[1].Property!;
            _engine.Move(token, steps: 1);
            _engine.BuyProperty(player, Mediterranean); // Owns the property

            int moneyBeforeMortgage = player.Money;
            int mortgageLoan = Mediterranean.MortgageValue; // $30 (half of $60)

            var result = _engine.HandleMortgage(player, Mediterranean);


            Assert.That(result, Is.True, "Mortgaging your own property success.");
            Assert.That(Mediterranean.IsMortgaged, Is.True, "Property should now be marked as mortgaged.");
            Assert.That(player.Money, Is.EqualTo(moneyBeforeMortgage + mortgageLoan), "Player should receive the 50% mortgage cash ($30).");
        }

        [Test]
        public void HandleMortgage_Twice_ReturnFalse()
        {

            var player = players[0];
            var token = _engine.GetPlayerToken(player)!;
            var Mediterranean = _engine.Board.Path[1].Property!;


            // Buy Mediterranean Avenue and successfully mortgage it the first time
            _engine.Move(token, steps: 1);
            _engine.BuyProperty(player, Mediterranean);

            bool firstResult = _engine.HandleMortgage(player, Mediterranean);
            Assert.That(firstResult, Is.True, "property fine to mortgage");

            int balanceAfterFirstMortgage = player.Money;


            // Act: Attempt to mortgage the same property a second time
            bool secondTimesMortgage = _engine.HandleMortgage(player, Mediterranean);

            // Assert: Second attempt must fail, stay mortgaged, and not grant extra money
            Assert.That(secondTimesMortgage, Is.False, "Cant mortgage a property that you have mortgaged");
            Assert.That(Mediterranean.IsMortgaged, Is.True);
            Assert.That(player.Money, Is.EqualTo(balanceAfterFirstMortgage));
        }

        [Test]
        public void HandleMortgage_OwnedByOther_ReturnFalse()
        {

            var player1 = players[0];
            var player2 = players[1];

            var token1 = _engine.GetPlayerToken(player1)!;
            var property = _engine.Board.Path[1].Property!;


            // Player 1 buys Mediterranean Avenue
            _engine.Move(token1, steps: 1);
            _engine.BuyProperty(player1, property);


            bool result = _engine.HandleMortgage(player2, property);

            // Assert: Must fail validation, property remains unmortgaged, and no cash is granted
            Assert.That(result, Is.False, "Cant mortgage other player property");

        }

        [Test]
        public void HandleMortgageRedemption_ColorGroupHasBuilding_ReturnFalse()
        {

            var player = players[0];
            var token = _engine.GetPlayerToken(player)!;

            var Mediterranean = _engine.Board.Path[1].Property!;
            var Baltic = _engine.Board.Path[3].Property!;

            // Player 1 buys both properties in the Brown color group
            _engine.Move(token, steps: 1); // Land on Mediterranean
            _engine.BuyProperty(player, Mediterranean);

            _engine.Move(token, steps: 2); // Land on Baltic
            _engine.BuyProperty(player, Baltic);


            // Build a house on Mediterranean (BuildingLevel = 1)
            bool houseBought = _engine.BuyHouse(player, Mediterranean);
            Assert.That(houseBought, Is.True);

            int balanceBeforeMortgage = player.Money;

            // Try to mortgage
            bool resultMortgageBaltic = _engine.HandleMortgage(player, Baltic);

            // Assert: Must return false because a property in the color group has buildings
            Assert.That(resultMortgageBaltic, Is.False, "Other property on same color group has building");
            Assert.That(Baltic.IsMortgaged, Is.False);
            Assert.That(player.Money, Is.EqualTo(balanceBeforeMortgage));


            bool resultMortgageMediterranean = _engine.HandleMortgage(player, Mediterranean);
            Assert.That(resultMortgageBaltic, Is.False, "Still false, has building");
        }

        #region MortgageRedemption

        [Test]
        public void HandleMortgageRedemption_PlayerNull_ReturnFalse()
        {
            var property = _engine.Board.Path[1].Property!;

            // Act: Attempt to mortgage passing a null player reference
            bool result = _engine.HandleMortgageRedemption(null!, property);

            // Must fail safely without NullReferenceException and leave deed unmortgaged
            Assert.That(result, Is.False, "Player cant be null when ");
            Assert.That(property.IsMortgaged, Is.False);
        }

        [Test]
        public void HandleMortgageRedemption_PropertyNull_ReturnFalse()
        {
            var player = players[0];
            int balanceBefore = player.Money;

            // Act: Attempt to mortgage passing a null property reference
            bool result = _engine.HandleMortgageRedemption(player, null!);

            // Must return false and not pay any loan to the player
            Assert.That(result, Is.False, "Property cant be null");
            Assert.That(player.Money, Is.EqualTo(balanceBefore));
        }

        [Test]
        public void HandleMortgageRedemption_PlayerBankrupt_ReturnFalse()
        {
            var player = players[0];
            var token = _engine.GetPlayerToken(player)!;
            var property = _engine.Board.Path[1].Property!;

            // Player 1 buys Mediterranean Avenue and mortgages it
            _engine.Move(token, steps: 1);
            _engine.BuyProperty(player, property);

            _engine.HandleMortgage(player, property);
            Assert.That(property.IsMortgaged, Is.True);

            // Player goes bankrupt
            player.IsBankrupt = true;

            int balanceBefore = player.Money;

            // Bankrupt player attempts to redeem the mortgage
            bool result = _engine.HandleMortgageRedemption(player, property);

            // Must return false, deed remains mortgaged, and no cash is deducted
            Assert.That(result, Is.False, "Player bankrupt cant unmortgage property");
            Assert.That(property.IsMortgaged, Is.True);
            Assert.That(player.Money, Is.EqualTo(balanceBefore));
        }

        [Test]
        public void HandleMortgageRedemption_GameStatusNotInProgress_ReturnFalse() { }

        [Test]
        public void HandleMortgageRedemption_YourMortgagedProperty_ReturnTrue()
        {

            var player = players[0];
            var token = _engine.GetPlayerToken(player)!;
            var Mediterranean = _engine.Board.Path[1].Property!;

            // Player 1 buys Mediterranean Avenue and mortgages it
            _engine.Move(token, steps: 1);
            _engine.BuyProperty(player, Mediterranean);

            _engine.HandleMortgage(player, Mediterranean);
            Assert.That(Mediterranean.IsMortgaged, Is.True);

            int balanceBeforeRedemption = player.Money;

            // 10% interest on mortgage value ($30 + $3 = $33)
            int interest = (int)Math.Ceiling(Mediterranean.MortgageValue * 0.10);
            int redemptionCost = Mediterranean.MortgageValue + interest;

            // Act: Player 1 redeems / unmortgages their Mediterranean
            bool result = _engine.HandleMortgageRedemption(player, Mediterranean);

            // Assert: Must return true, lift the mortgage flag, and deduct principal + 10% interest
            Assert.That(result, Is.True, "Unmortgaged success");
            Assert.That(Mediterranean.IsMortgaged, Is.False);
            Assert.That(player.Money, Is.EqualTo(balanceBeforeRedemption - redemptionCost));
        }

        [Test]
        public void HandleMortgageRedemption_NotEnoughMoney_ReturnTrue()
        {
            var player = players[0];
            var token = _engine.GetPlayerToken(player)!;
            var Mediterranean = _engine.Board.Path[1].Property!;

            // Player 1 buys Mediterranean Avenue and mortgages it
            _engine.Move(token, steps: 1);
            _engine.BuyProperty(player, Mediterranean);

            _engine.HandleMortgage(player, Mediterranean);

            // Redemption cost is $30 (principal) + $3 (10% interest) = $33.
            // Set player money lower than $33 to simulate insufficient funds.
            player.Money = 10;

            // Act: Attempt to redeem the mortgage without enough money
            bool result = _engine.HandleMortgageRedemption(player, Mediterranean);

            // Assert: Must return false, property remains mortgaged, and player's money is untouched
            Assert.That(result, Is.False, "not enough money");
            Assert.That(Mediterranean.IsMortgaged, Is.True);
            Assert.That(player.Money, Is.EqualTo(10));
        }


        [Test]
        public void HandleMortgageRedemption_NotMortgaged_ReturnFalse()
        {
            var player = players[0];
            var token = _engine.GetPlayerToken(player)!;
            var Mediterranean = _engine.Board.Path[1].Property!;

            // Player 1 buys Mediterranean Avenue but does NOT mortgage it
            _engine.Move(token, steps: 1);
            _engine.BuyProperty(player, Mediterranean);

            int balanceBeforeRedemption = player.Money;

            // Act: Attempt to redeem a Mediterranean that is already unmortgaged
            bool result = _engine.HandleMortgageRedemption(player, Mediterranean);

            // Assert: Must return false, remain unmortgaged, and charge no money
            Assert.That(result, Is.False, "Cant unmortgaged property that is not mortgaged");
            Assert.That(Mediterranean.IsMortgaged, Is.False);
            Assert.That(player.Money, Is.EqualTo(balanceBeforeRedemption));
        }

        [Test]
        public void HandleMortgageRedemption_OwnedByOther_ReturnFalse()
        {
            // Arrange
            var player1 = players[0];
            var player2 = players[1];
            var token1 = _engine.GetPlayerToken(player1)!;

            var Mediterranean = _engine.Board.Path[1].Property!;

            // Player 1 buys Mediterranean Avenue and mortgages it
            _engine.Move(token1, steps: 1);
            _engine.BuyProperty(player1, Mediterranean);
            _engine.HandleMortgage(player1, Mediterranean);

            Assert.That(Mediterranean.IsMortgaged, Is.True);


            int player2BalanceBefore = player2.Money;


            // Player 2 attempts to unmortgage Player 1's property
            bool result = _engine.HandleMortgageRedemption(player2, Mediterranean);


            // Assert: Must return false, deed remains mortgaged, and Player 2 is not charged
            Assert.That(result, Is.False);
            Assert.That(Mediterranean.IsMortgaged, Is.True);
            Assert.That(player2.Money, Is.EqualTo(player2BalanceBefore));
        }


        #endregion
    }
}