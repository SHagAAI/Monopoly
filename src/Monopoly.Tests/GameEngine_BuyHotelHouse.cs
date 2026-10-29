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
    public class PurchasingHotelHouse
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
        public void BuyHouse_CompleteColorGroup_ReturnTrue()
        {
            var Mediterranean = _engine.Board.Path[1].Property!;
            var Baltic = _engine.Board.Path[3].Property!;

            _engine.Move(token: tokens[0], steps: 1);
            _engine.BuyProperty(player: players[0], Mediterranean);

            _engine.Move(token: tokens[0], steps: 2);
            _engine.BuyProperty(player: players[0], Baltic);

            var result = _engine.BuyHouse(players[0], Mediterranean);

            Assert.That(result, Is.True, "Complete color group, can buy house");
        }

        [Test]
        public void BuyHouse_NotCompleteColorGroup_ReturnFalse()
        {
            var player = _engine.GetCurrentPlayer();
            var token = _engine.GetPlayerToken(player)!;

            var Mediterranean = _engine.Board.Path[1].Property!;
            _engine.Move(token: tokens[0], steps: 1);
            _engine.BuyProperty(player: players[0], Mediterranean);

            var result = _engine.BuyHouse(players[0], Mediterranean);

            Assert.That(result, Is.False, "Cant buy house, color group not complete");
        }

        [Test]
        public void BuyHouse_ButConcentrateOn1Property_ReturnFalse()
        {

            var player = _engine.GetCurrentPlayer();
            var token = _engine.GetPlayerToken(player)!;

            var Mediterranean = _engine.Board.Path[1].Property!;
            var Baltic = _engine.Board.Path[3].Property!;

            _engine.Move(token: token, steps: 1);
            _engine.BuyProperty(player: players[0], Mediterranean);

            _engine.Move(token: token, steps: 2);
            _engine.BuyProperty(player: players[0], Baltic);

            _engine.BuyHouse(player: player, Mediterranean);

            var result = _engine.BuyHouse(player, Mediterranean);

            Assert.That(result, Is.False, "House should be built equally spread out");
        }

        [Test]
        public void BuyHouse_AlreadyHas4House_ReturnFalse()
        {
            var player = _engine.GetCurrentPlayer();
            var token = _engine.GetPlayerToken(player)!;

            var Mediterranean = _engine.Board.Path[1].Property!;
            var Baltic = _engine.Board.Path[3].Property!;

            Mediterranean.BuildingLevel = 4;
            Baltic.BuildingLevel = 4;

            var result = _engine.BuyHouse(player, Mediterranean);

            Assert.That(result, Is.False, "House amount is maxed out. Buy Hotel instead");
        }

        [Test]
        public void BuyHouse_BankHasNoHouseLeft_ReturnFalse()
        {
            var player = _engine.GetCurrentPlayer();
            var token = _engine.GetPlayerToken(player)!;

            var Mediterranean = _engine.Board.Path[1].Property!;
            var Baltic = _engine.Board.Path[3].Property!;

            _engine.Move(token, steps: 1);
            _engine.BuyProperty(player, Mediterranean);

            _engine.Move(token, steps: 2);
            _engine.BuyProperty(player, Baltic);

            Mediterranean.BuildingLevel = 3;
            Baltic.BuildingLevel = 3;
            _engine.HouseAmount = 0;

            var result = _engine.BuyHouse(player: player, property: Mediterranean);

            Assert.That(result, Is.False, "Cannot buy a house if the Bank has 0 house in stock.");

        }

        [Test]
        public void BuyHouse_PlayerNull_ReturnFalse()
        {

            var player = _engine.GetCurrentPlayer();
            var token = _engine.GetPlayerToken(player)!;
            var Mediterranean = _engine.Board.Path[1].Property!;

            // Act: Attempt to buy a house passing a null property reference
            bool result = _engine.BuyHouse(null!, Mediterranean);

            // Assert: Must return false and keep player money & bank house supply untouched
            Assert.That(result, Is.False, "BuyHouse fail without player");

        }




        [Test]
        public void BuyHouse_PropertyNull_ReturnFalse()
        {

            var player = players[0];
            int startingMoney = player.Money;
            int startingHouses = _engine.HouseAmount;

            // Act: Attempt to buy a house passing a null property reference
            bool result = _engine.BuyHouse(player, null!);

            // Assert: Must return false and keep player money & bank house supply untouched
            Assert.That(result, Is.False);
            Assert.That(player.Money, Is.EqualTo(startingMoney));
            Assert.That(_engine.HouseAmount, Is.EqualTo(startingHouses));
        }


        [Test]
        public void BuyHouse_PlayerBankrupt_ReturnFalse()
        {
            // Arrange
            var player = players[0];
            var token = _engine.GetPlayerToken(player)!;

            var mediterranean = _engine.Board.Path[1].Property!;
            var baltic = _engine.Board.Path[3].Property!;


            // Player 1 legitimately owns the full Brown monopoly
            _engine.Move(token, steps: 1);
            _engine.BuyProperty(player, mediterranean);

            _engine.Move(token, steps: 2);
            _engine.BuyProperty(player, baltic);

            // Player becomes bankrupt
            player.IsBankrupt = true;
            int startingHouses = _engine.HouseAmount;

            // Act: Bankrupt player attempts to buy a house
            bool result = _engine.BuyHouse(player, mediterranean);

            // Assert: Must return false, property remains unimproved, and bank supply is unchanged
            Assert.That(result, Is.False, "Bankrupt player cant buy house");
            Assert.That(mediterranean.BuildingLevel, Is.EqualTo(0));
            Assert.That(_engine.HouseAmount, Is.EqualTo(startingHouses));

        }


        [Test]
        public void BuyHouse_PropertyNotStreet_ReturnFalse()
        {


            var player = players[0];
            var token = _engine.GetPlayerToken(player)!;
            var readingRailroad = _engine.Board.Path[5].Property!; // Space 5 is a Railroad

            // Player 1 buys Reading Railroad
            _engine.Move(token, steps: 5);
            _engine.BuyProperty(player, readingRailroad);


            // Act: Attempt to build a house on a Railroad property
            bool result = _engine.BuyHouse(player, readingRailroad);

            // Assert: Must return false; houses can only be built on Street properties
            Assert.That(result, Is.False, "Cant buy building on not street property");
            Assert.That(readingRailroad.BuildingLevel, Is.EqualTo(0));

        }

        [Test]
        public void BuyHouse_AnyPropertyIsMortgaged_ReturnFalse()
        {

            var player = players[0];
            var token = _engine.GetPlayerToken(player)!;

            var mediterranean = _engine.Board.Path[1].Property!;
            var baltic = _engine.Board.Path[3].Property!;

            // Player 1 buys complete Brown monopoly
            _engine.Move(token, steps: 1);
            _engine.BuyProperty(player, mediterranean);

            _engine.Move(token, steps: 2);
            _engine.BuyProperty(player, baltic);

            // Baltic is mortgaged
            baltic.IsMortgaged = true;

            int startingMoney = player.Money;
            int startingHouses = _engine.HouseAmount;

            // Act: Attempt to build a house on Mediterranean while Baltic is mortgaged
            bool result = _engine.BuyHouse(player, mediterranean);

            // Must return false, building level stays 0, and bank inventory unchanged
            Assert.That(result, Is.False);
            Assert.That(mediterranean.BuildingLevel, Is.EqualTo(0));
            Assert.That(player.Money, Is.EqualTo(startingMoney));
            Assert.That(_engine.HouseAmount, Is.EqualTo(startingHouses));
        }

        [Test]
        public void BuyHouse_NotEnoughMoney_ReturnFalse()
        {
            var player = players[0];
            var token = _engine.GetPlayerToken(player)!;

            var mediterranean = _engine.Board.Path[1].Property!;
            var baltic = _engine.Board.Path[3].Property!;


            // Player 1 buys complete Brown monopoly
            _engine.Move(token, steps: 1);
            _engine.BuyProperty(player, mediterranean);

            _engine.Move(token, steps: 2);
            _engine.BuyProperty(player, baltic);

            // House cost for Mediterranean is $50.
            // Set player money lower than $50 to simulate insufficient funds.
            player.Money = 20;
            int startingHouses = _engine.HouseAmount;

            // Act: Attempt to buy a house without enough cash
            bool result = _engine.BuyHouse(player, mediterranean);

            // Assert: Must return false, level remains 0, and player balance untouched
            Assert.That(result, Is.False,  "not enough money");
            Assert.That(mediterranean.BuildingLevel, Is.EqualTo(0));
            Assert.That(player.Money, Is.EqualTo(20));
            Assert.That(_engine.HouseAmount, Is.EqualTo(startingHouses));
        }

        /// ==========================================
        /// HOTEL
        /// ==========================================

        [Test]
        public void BuyHotel_CompleteColorGroupAnd4Houses_ReturnTrue()
        {
            var player = _engine.GetCurrentPlayer();
            var token = _engine.GetPlayerToken(player)!;

            var Mediterranean = _engine.Board.Path[1].Property!;
            var Baltic = _engine.Board.Path[3].Property!;

            _engine.Move(token, steps: 1);
            _engine.BuyProperty(player, Mediterranean);

            _engine.Move(token, steps: 2);
            _engine.BuyProperty(player, Baltic);

            // Both properties must have 4 houses
            Mediterranean.BuildingLevel = 4;
            Baltic.BuildingLevel = 4;


            var result = _engine.BuyHotel(player, Mediterranean);
            Assert.That(result, Is.True, "House amount is sufficient => 4 houses on all properties on same color group");
        }

        [Test]
        public void BuyHotel_FewerThan4Houses_ReturnFalse()
        {
            var player = _engine.GetCurrentPlayer();
            var token = _engine.GetPlayerToken(player)!;

            var Mediterranean = _engine.Board.Path[1].Property!;
            var Baltic = _engine.Board.Path[3].Property!;

            _engine.Move(token: tokens[0], steps: 1);
            _engine.BuyProperty(player: players[0], Mediterranean);

            _engine.Move(token: tokens[0], steps: 2);
            _engine.BuyProperty(player: players[0], Baltic);

            Mediterranean.BuildingLevel = 3;
            Baltic.BuildingLevel = 4;

            var result = _engine.BuyHotel(player: player, property: Baltic);
            Assert.That(result, Is.False, "Cannot buy a hotel. Building level should be even out");
        }

        [Test]
        public void BuyHotel_BankHasNoHotelLeft_ReturnFalse()
        {
            var player = _engine.GetCurrentPlayer();
            var token = _engine.GetPlayerToken(player)!;

            var Mediterranean = _engine.Board.Path[1].Property!;
            var Baltic = _engine.Board.Path[3].Property!;

            _engine.Move(token, steps: 1);
            _engine.BuyProperty(player, Mediterranean);

            _engine.Move(token, steps: 2);
            _engine.BuyProperty(player, Baltic);

            Mediterranean.BuildingLevel = 4;
            Baltic.BuildingLevel = 4;

            _engine.HotelAmount = 0;

            var result = _engine.BuyHotel(player: player, property: Mediterranean);

            Assert.That(result, Is.False, "Cannot buy a hotel if the Bank has 0 hotels in stock.");
        }

        [Test]
        public void BuyHotel_PlayerNull_ReturnFalse()
        {

            var Mediterranean = _engine.Board.Path[1].Property!;
            int startingHotels = _engine.HotelAmount;

            // Act: Attempt to buy a hotel with a null player reference
            bool result = _engine.BuyHotel(null!, Mediterranean);

            // Assert: Must return false and leave bank hotel supply untouched
            Assert.That(result, Is.False, "player cant be null when buying hotel");
            Assert.That(_engine.HotelAmount, Is.EqualTo(startingHotels));
        }

        [Test]
        public void BuyHotel_PropertyNull_ReturnFalse()
        {
            var player = players[0];
            int startingMoney = player.Money;
            int startingHotels = _engine.HotelAmount;

            // Act: Attempt to buy a hotel with a null property reference
            bool result = _engine.BuyHotel(player, null!);

            // Assert: Must return false and leave player balance & hotel supply untouched
            Assert.That(result, Is.False, "cant buy hotel without property");
            Assert.That(player.Money, Is.EqualTo(startingMoney));
            Assert.That(_engine.HotelAmount, Is.EqualTo(startingHotels));
        }

        [Test]
        public void BuyHotel_PropertyNotStreet_ReturnFalse()
        {
            // Arrange
            var player = players[0];
            var token = _engine.GetPlayerToken(player)!;
            var readingRailroad = _engine.Board.Path[5].Property!; // Space 5 Reading Railroad

            // Player 1 buys Reading Railroad
            _engine.Move(token, steps: 5);
            _engine.BuyProperty(player, readingRailroad);

            int balanceBeforeAttempt = player.Money;
            int hotelsBeforeAttempt = _engine.HotelAmount;

            // Act: Attempt to build a hotel on a non-street property (Railroad)
            bool result = _engine.BuyHotel(player, readingRailroad);

            // Assert: Must return false; hotels can only be erected on Street properties
            Assert.That(result, Is.False, "property not street, cant buy hotel here");
            Assert.That(readingRailroad.BuildingLevel, Is.EqualTo(0));
            Assert.That(player.Money, Is.EqualTo(balanceBeforeAttempt));
            Assert.That(_engine.HotelAmount, Is.EqualTo(hotelsBeforeAttempt));
        }

        [Test]
        public void BuyHotel_PropertyNotYours_ReturnFalse()
        {
            var player1 = players[0];
            var player2 = players[1];

            var token1 = _engine.GetPlayerToken(player1)!;

            var mediterranean = _engine.Board.Path[1].Property!;
            var baltic = _engine.Board.Path[3].Property!;

            // Player 1 buys both Brown properties and develops them to 4 houses each
            _engine.Move(token1, steps: 1);
            _engine.BuyProperty(player1, mediterranean);

            _engine.Move(token1, steps: 2);
            _engine.BuyProperty(player1, baltic);

            mediterranean.BuildingLevel = 4;
            baltic.BuildingLevel = 4;

            int player2StartingMoney = player2.Money;
            int startingHotels = _engine.HotelAmount;

            // Player 2 want to buy a hotel on Player 1's property
            bool result = _engine.BuyHotel(player2, mediterranean);

            // Assert: Must return false, level remains 4 houses, and Player 2 is not charged
            Assert.That(result, Is.False, "cant buy hotel on other player property");
            Assert.That(mediterranean.BuildingLevel, Is.EqualTo(4));
            Assert.That(player2.Money, Is.EqualTo(player2StartingMoney));
            Assert.That(_engine.HotelAmount, Is.EqualTo(startingHotels));
        }

        [Test]
        public void BuyHotel_PlayerBankrupt_ReturnFalse()
        {
            var player = players[0];
            var token = _engine.GetPlayerToken(player)!;

            var mediterranean = _engine.Board.Path[1].Property!;
            var baltic = _engine.Board.Path[3].Property!;

            // Player 1 buys Brown monopoly and reaches 4 houses on each
            _engine.Move(token, steps: 1);
            _engine.BuyProperty(player, mediterranean);

            _engine.Move(token, steps: 2);
            _engine.BuyProperty(player, baltic);

            mediterranean.BuildingLevel = 4;
            baltic.BuildingLevel = 4;

            // Player goes bankrupt
            player.IsBankrupt = true;
            int startingHotels = _engine.HotelAmount;

            // Act: Bankrupt player attempts to upgrade to a hotel
            bool result = _engine.BuyHotel(player, mediterranean);

            // Must return false, stays at 4 houses, and hotel supply is untouched
            Assert.That(result, Is.False, "Player bankrupt, cant buy building/hotel");
            Assert.That(_engine.HotelAmount, Is.EqualTo(startingHotels));
        }

        [Test]
        public void BuyHotel_GameStatusNotInProgress_ReturnFalse()
        {
            var player = players[0];
            var token = _engine.GetPlayerToken(player)!;

            var mediterranean = _engine.Board.Path[1].Property!;
            var baltic = _engine.Board.Path[3].Property!;


            // Player 1 buys Brown monopoly and reaches 4 houses on each
            _engine.Move(token, steps: 1);
            _engine.BuyProperty(player, mediterranean);

            _engine.Move(token, steps: 2);
            _engine.BuyProperty(player, baltic);

            mediterranean.BuildingLevel = 4;
            baltic.BuildingLevel = 4;

            // End game to put Status in GameOver (not InProgress)
            _engine.EndGame();

            // Act: Attempt to buy a hotel after the game has concluded
            bool result = _engine.BuyHotel(player, mediterranean);

            // Assert: Must return false; building actions are blocked when game is inactive
            Assert.That(result, Is.False, "Game status not in progress");
            Assert.That(mediterranean.BuildingLevel, Is.EqualTo(4));
        }

        [Test]
        public void BuyHotel_HasNotEnoughHouse_ReturnFalse()
        {
            var player = players[0];
            var token = _engine.GetPlayerToken(player)!;

            var mediterranean = _engine.Board.Path[1].Property!;
            var baltic = _engine.Board.Path[3].Property!;

            _engine.Move(token, steps: 1);
            _engine.BuyProperty(player, mediterranean);

            _engine.Move(token, steps: 2);
            _engine.BuyProperty(player, baltic);

            // Mediterranean only has 3 houses (needs 4 houses to qualify for a hotel)
            mediterranean.BuildingLevel = 3;
            baltic.BuildingLevel = 4;

            int startingMoney = player.Money;
            int startingHotels = _engine.HotelAmount;

            // Act: Attempt to buy a hotel when fewer than 4 houses are built
            bool result = _engine.BuyHotel(player, mediterranean);

            // Assert: Must return false, level stays at 3, and hotel count remains unchanged
            Assert.That(result, Is.False, "Building level not even out");
            Assert.That(mediterranean.BuildingLevel, Is.EqualTo(3));
            Assert.That(player.Money, Is.EqualTo(startingMoney));
            Assert.That(_engine.HotelAmount, Is.EqualTo(startingHotels));
        }

        [Test]
        public void BuyHotel_PlayerNotOwnColorGroup_ReturnFalse()
        {
            var player = players[0];
            var token = _engine.GetPlayerToken(player)!;

            var mediterranean = _engine.Board.Path[1].Property!;

            // Player 1 only buys Mediterranean, leaving Baltic unowned
            _engine.Move(token, steps: 1);
            _engine.BuyProperty(player, mediterranean);

            mediterranean.BuildingLevel = 4;

            int startingMoney = player.Money;
            int startingHotels = _engine.HotelAmount;

            // Act: Attempt to buy a hotel without owning the entire color group
            bool result = _engine.BuyHotel(player, mediterranean);

            // Must return false; hotel development requires full monopoly
            Assert.That(result, Is.False, "you not own the color group, cant buy building");
            Assert.That(mediterranean.BuildingLevel, Is.EqualTo(4));
            Assert.That(player.Money, Is.EqualTo(startingMoney));
            Assert.That(_engine.HotelAmount, Is.EqualTo(startingHotels));
        }

        [Test]
        public void BuyHotel_AnyPropertyIsMortgaged_ReturnFalse()
        {
            var player = players[0];
            var token = _engine.GetPlayerToken(player)!;

            var mediterranean = _engine.Board.Path[1].Property!;
            var baltic = _engine.Board.Path[3].Property!;

            _engine.Move(token, steps: 1);
            _engine.BuyProperty(player, mediterranean);

            _engine.Move(token, steps: 2);
            _engine.BuyProperty(player, baltic);

            mediterranean.BuildingLevel = 4;
            baltic.BuildingLevel = 4;

            // Baltic is mortgaged
            baltic.IsMortgaged = true;


            // Act: Attempt to upgrade Mediterranean while another property in the group is mortgaged
            bool result = _engine.BuyHotel(player, mediterranean);

            // Must return false; no improvements allowed if any deed in the color group is mortgaged
            Assert.That(result, Is.False);
            Assert.That(mediterranean.BuildingLevel, Is.EqualTo(4));

        }

        [Test]
        public void BuyHotel_NotEnoughMoney_ReturnFalse()
        {
            var player = players[0];
            var token = _engine.GetPlayerToken(player)!;

            var mediterranean = _engine.Board.Path[1].Property!;
            var baltic = _engine.Board.Path[3].Property!;

            // Complete Brown monopoly with 4 houses on each property
            _engine.Move(token, steps: 1);
            _engine.BuyProperty(player, mediterranean);

            _engine.Move(token, steps: 2);
            _engine.BuyProperty(player, baltic);

            mediterranean.BuildingLevel = 4;
            baltic.BuildingLevel = 4;

            // Hotel cost equals house cost ($50 for Brown group).
            // Set player money lower than $50 to simulate insufficient funds.
            player.Money = 20;


            // Act: Attempt to purchase a hotel without enough cash
            bool result = _engine.BuyHotel(player, mediterranean);

            // Assert: Must return false, stays at 4 houses, money untouched, and bank inventory unchanged
            Assert.That(result, Is.False);
            Assert.That(mediterranean.BuildingLevel, Is.EqualTo(4));
            Assert.That(player.Money, Is.EqualTo(20));

        }
    }
}