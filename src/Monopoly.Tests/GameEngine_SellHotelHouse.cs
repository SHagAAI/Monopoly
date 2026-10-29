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
    public class SellHotelHouse
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
        public void SellHouse_HasHouse_ReturnTrue()
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

            var result = _engine.SellHouse(player: player, property: Mediterranean);

            Assert.That(result, Is.True, "Can sell house");
        }

        [Test]
        public void SellHouse_HasNoHouses_ReturnFalse()
        {
            // 1. Arrange: Player owns Mediterranean Avenue, but has 0 houses
            var player = _engine.GetCurrentPlayer();
            var token = _engine.GetPlayerToken(player)!;

            var Mediterranean = _engine.Board.Path[1].Property!;

            _engine.Move(token: token, steps: 1);

            _engine.BuyProperty(player, Mediterranean);

            Mediterranean.BuildingLevel = 0;

            var result = _engine.SellHouse(player, Mediterranean);

            Assert.That(result, Is.False, "Cannot sell a house when building level is 0.");
        }

        [Test]
        public void SellHouse_ButNotEven_ReturnFalse()
        {
            var player = _engine.GetCurrentPlayer();
            var token = _engine.GetPlayerToken(player)!;

            var Mediterranean = _engine.Board.Path[1].Property!;
            var Baltic = _engine.Board.Path[3].Property!;

            _engine.Move(token, steps: 1);
            _engine.BuyProperty(player, Mediterranean);

            _engine.Move(token, steps: 2);
            _engine.BuyProperty(player, Baltic);

            Mediterranean.BuildingLevel = 2;
            Baltic.BuildingLevel = 1;

            var result = _engine.SellHouse(player: player, property: Baltic);
            Assert.That(result, Is.False, "Selling house should be even too");

        }


        [Test]
        public void SellHouse_PlayerNull_ReturnFalse()
        {
            // Arrange
            var Mediterranean = _engine.Board.Path[1].Property!;
            int startingHouses = _engine.HouseAmount;

            // Act: Attempt to sell a house with a null player reference
            bool result = _engine.SellHouse(null!, Mediterranean);

            // Assert: Must return false and bank supply remains untouched
            Assert.That(result, Is.False, "Cant sell house without player");
            Assert.That(_engine.HouseAmount, Is.EqualTo(startingHouses));
        }

        [Test]
        public void SellHouse_PropertyNull_ReturnFalse()
        {
            // Arrange
            var player = players[0];
            int startingMoney = player.Money;
            int startingHouses = _engine.HouseAmount;

            // Act: Attempt to sell a house with a null property reference
            bool result = _engine.SellHouse(player, null!);

            // Assert: Must return false, with no refund given or house count changed
            Assert.That(result, Is.False, "cant sell house without property");
            Assert.That(player.Money, Is.EqualTo(startingMoney));
            Assert.That(_engine.HouseAmount, Is.EqualTo(startingHouses));
        }

        [Test]
        public void SellHouse_PropertyNotStreet_ReturnFalse()
        {
            // Arrange
            var player = players[0];
            var token = _engine.GetPlayerToken(player)!;
            var readingRailroad = _engine.Board.Path[5].Property!; // Space 5 is a Railroad


            // Player 1 buys Reading Railroad
            _engine.Move(token, steps: 5);
            _engine.BuyProperty(player, readingRailroad);

            int balanceBeforeAttempt = player.Money;
            int housesBeforeAttempt = _engine.HouseAmount;

            // Act: Attempt to sell a house on a non-street property
            bool result = _engine.SellHouse(player, readingRailroad);

            // Assert: Must return false; only Street properties have houses to sell
            Assert.That(result, Is.False);
            Assert.That(player.Money, Is.EqualTo(balanceBeforeAttempt));
            Assert.That(_engine.HouseAmount, Is.EqualTo(housesBeforeAttempt));
        }


        [Test]
        public void SellHouse_PlayerIsBankrupt_ReturnFalse()
        {
            // Arrange
            var player = players[0];
            var token = _engine.GetPlayerToken(player)!;

            var mediterranean = _engine.Board.Path[1].Property!;
            var baltic = _engine.Board.Path[3].Property!;

            // Player 1 buys monopoly and builds 1 house
            _engine.Move(token, steps: 1);
            _engine.BuyProperty(player, mediterranean);

            _engine.Move(token, steps: 2);
            _engine.BuyProperty(player, baltic);

            _engine.BuyHouse(player, mediterranean);
            Assert.That(mediterranean.BuildingLevel, Is.EqualTo(1));

            // Player becomes bankrupt
            player.IsBankrupt = true;

            int startingMoney = player.Money;
            int startingHouses = _engine.HouseAmount;

            // Act: Bankrupt player attempts to sell a house
            bool result = _engine.SellHouse(player, mediterranean);

            // Assert: Must return false, building remains, and no refund is granted
            Assert.That(result, Is.False, "cant sell building when player is bankrupt");
            Assert.That(mediterranean.BuildingLevel, Is.EqualTo(1));
            Assert.That(player.Money, Is.EqualTo(startingMoney));
            Assert.That(_engine.HouseAmount, Is.EqualTo(startingHouses));
        }

        [Test]
        public void SellHouse_GameStatusNotInProgress_ReturnFalse()
        {
            var player = players[0];
            var token = _engine.GetPlayerToken(player)!;

            var mediterranean = _engine.Board.Path[1].Property!;
            var baltic = _engine.Board.Path[3].Property!;

            // Player 1 buys monopoly and builds 1 house
            _engine.Move(token, steps: 1);
            _engine.BuyProperty(player, mediterranean);

            _engine.Move(token, steps: 2);
            _engine.BuyProperty(player, baltic);

            _engine.BuyHouse(player, mediterranean);

            Assert.That(mediterranean.BuildingLevel, Is.EqualTo(1));

            // Transition game state to GameOver
            _engine.EndGame();
            int startingMoney = player.Money;
            int startingHouses = _engine.HouseAmount;

            // Act: Attempt to sell a house after the game has ended
            bool result = _engine.SellHouse(player, mediterranean);

            // Assert: Must return false; actions are forbidden when game is not InProgress
            Assert.That(result, Is.False);
            Assert.That(mediterranean.BuildingLevel, Is.EqualTo(1));
            Assert.That(player.Money, Is.EqualTo(startingMoney));
            Assert.That(_engine.HouseAmount, Is.EqualTo(startingHouses));
        }

        [Test]
        public void SellHouse_NotOnYourProperty_ReturnFalse()
        {
            var player1 = players[0];
            var player2 = players[1];

            var token1 = _engine.GetPlayerToken(player1)!;

            var mediterranean = _engine.Board.Path[1].Property!;
            var baltic = _engine.Board.Path[3].Property!;


            _engine.Move(token1, steps: 1);
            _engine.BuyProperty(player1, mediterranean);

            _engine.Move(token1, steps: 2);
            _engine.BuyProperty(player1, baltic);

            _engine.BuyHouse(player1, mediterranean);
            Assert.That(mediterranean.BuildingLevel, Is.EqualTo(1));

            int player2StartingMoney = player2.Money;
            int startingHouses = _engine.HouseAmount;


            // Act: Player 2 attempts to sell Player 1's house
            bool result = _engine.SellHouse(player2, mediterranean);

            // Assert: Must return false, house remains on property, and Player 2 receives no cash
            Assert.That(result, Is.False, "cant sell building at not your property");
            Assert.That(mediterranean.BuildingLevel, Is.EqualTo(1));
            Assert.That(player2.Money, Is.EqualTo(player2StartingMoney));
            Assert.That(_engine.HouseAmount, Is.EqualTo(startingHouses));
        }


        #region HOTEL



        [Test]
        public void SellHotel_PlayerNull_ReturnFalse()
        {
            var property = _engine.Board.Path[1].Property!;
            int startingHotels = _engine.HotelAmount;

            // Act: Attempt to sell hotel passing null player
            bool result = _engine.SellHotel(null!, property);

            // Must return false and leave hotel supply unchanged
            Assert.That(result, Is.False, "Player cant be null when selling property");
            Assert.That(_engine.HotelAmount, Is.EqualTo(startingHotels));
        }

        [Test]
        public void SellHotel_PropertyNull_ReturnFalse()
        {
            var player = players[0];
            int startingMoney = player.Money;
            int startingHotels = _engine.HotelAmount;

            // Act: Attempt to sell hotel passing null property
            bool result = _engine.SellHotel(player, null!);

            // Assert: Must return false and leave money and hotel supply unchanged
            Assert.That(result, Is.False, "Property cant be null");
            Assert.That(player.Money, Is.EqualTo(startingMoney));
            Assert.That(_engine.HotelAmount, Is.EqualTo(startingHotels));
        }

        [Test]
        public void SellHotel_PlayerBankrupt_ReturnFalse()
        {
            var player = players[0];
            var token = _engine.GetPlayerToken(player)!;

            var mediterranean = _engine.Board.Path[1].Property!;
            var baltic = _engine.Board.Path[3].Property!;

            // Player 1 buys monopoly and owns a hotel on Baltic
            _engine.Move(token, steps: 1);
            _engine.BuyProperty(player, mediterranean);

            _engine.Move(token, steps: 2);
            _engine.BuyProperty(player, baltic);

            mediterranean.BuildingLevel = 4;
            baltic.BuildingLevel = 5; // Hotel

            // Player goes bankrupt
            player.IsBankrupt = true;
            int startingMoney = player.Money;
            int startingHotels = _engine.HotelAmount;

            // Act: Bankrupt player attempts to sell hotel
            bool result = _engine.SellHotel(player, baltic);

            // Must return false, hotel remains on property, and no refund granted
            Assert.That(result, Is.False);
            Assert.That(baltic.BuildingLevel, Is.EqualTo(5));
            Assert.That(player.Money, Is.EqualTo(startingMoney));
            Assert.That(_engine.HotelAmount, Is.EqualTo(startingHotels));
        }

        [Test]
        public void SellHotel_GameIsNotInProgress_ReturnFalse()
        {
            var player = players[0];
            var token = _engine.GetPlayerToken(player)!;

            var mediterranean = _engine.Board.Path[1].Property!;
            var baltic = _engine.Board.Path[3].Property!;

            // Player 1 buys monopoly and owns a hotel on Baltic
            _engine.Move(token, steps: 1);
            _engine.BuyProperty(player, mediterranean);

            _engine.Move(token, steps: 2);
            _engine.BuyProperty(player, baltic);

            mediterranean.BuildingLevel = 4;
            baltic.BuildingLevel = 5; // Hotel

            // Match ends
            _engine.EndGame(); // game status is game over



            // Act: Attempt to sell hotel after match conclusion
            bool result = _engine.SellHotel(player, baltic);

            // Assert: Must return false and preserve board and balance state
            Assert.That(result, Is.False);
            Assert.That(baltic.BuildingLevel, Is.EqualTo(5));
        }

        [Test]
        public void SellHotel_PropertyNotStreet_ReturnFalse()
        {
            var player = players[0];
            var token = _engine.GetPlayerToken(player)!;
            var readingRailroad = _engine.Board.Path[5].Property!; // Space 5 is a Railroad

            // Player 1 buys Reading Railroad
            _engine.Move(token, steps: 5);
            _engine.BuyProperty(player, readingRailroad);


            // Act: Attempt to sell hotel on non-street property
            bool result = _engine.SellHotel(player, readingRailroad);

            // Assert: Must return false; only Street properties have hotels
            Assert.That(result, Is.False);
            Assert.That(readingRailroad.BuildingLevel, Is.EqualTo(0));
        }

        [Test]
        public void SellHotel_PropertyNotOwnedByYou_ReturnFalse()
        {
            var player1 = players[0];
            var player2 = players[1];
            var token1 = _engine.GetPlayerToken(player1)!;

            var mediterranean = _engine.Board.Path[1].Property!;
            var baltic = _engine.Board.Path[3].Property!;

            // Player 1 buys monopoly and owns a hotel on Baltic
            _engine.Move(token1, steps: 1);
            _engine.BuyProperty(player1, mediterranean);

            _engine.Move(token1, steps: 2);
            _engine.BuyProperty(player1, baltic);

            mediterranean.BuildingLevel = 4;
            baltic.BuildingLevel = 5; // Hotel


            // Act: Player 2 attempts to sell Player 1's hotel
            bool result = _engine.SellHotel(player2, baltic);

            // Must return false, hotel remains, and Player 2 receives no refund
            Assert.That(result, Is.False);
            Assert.That(baltic.BuildingLevel, Is.EqualTo(5));

        }

        [Test]
        public void SellHotel_HasNoHotel_ReturnFalse()
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

            var result = _engine.SellHotel(player, Mediterranean);

            Assert.That(result, Is.False, "Cant sell a hotel if you dont have hotel");
        }


        [Test]
        public void SellHotel_HasHotel_ReturnTrue()
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
            Baltic.BuildingLevel = 5;

            int moneyBefore = player.Money;

            var result = _engine.SellHotel(player, Baltic);
            Assert.That(result, Is.True, "Selling hotel ");
        }

        // =================================================

        [Test]
        public void SellHotel_BankHasFewerThan4Houses_VoluntarySale_ReturnFalse()
        {
            // Arrange: Player 1 has a Hotel on Baltic and 4 houses on Mediterranean
            var player = players[0];
            var token = _engine.GetPlayerToken(player)!;

            var mediterranean = _engine.Board.Path[1].Property!;
            var baltic = _engine.Board.Path[3].Property!;

            _engine.Move(token, steps: 1);
            _engine.BuyProperty(player, mediterranean);

            _engine.Move(token, steps: 2);
            _engine.BuyProperty(player, baltic);

            mediterranean.BuildingLevel = 4;
            baltic.BuildingLevel = 5; // Hotel

            // Bank has a housing shortage (fewer than 4 houses available to step down to)
            _engine.HouseAmount = 2;

            // Player has plenty of money and NO debt (this is a voluntary downgrade)
            Assert.That(_engine.HasPendingDebt(player), Is.False);

            int startingMoney = player.Money;

            // Act: Attempt to voluntarily sell the hotel when the Bank cannot supply 4 houses
            bool result = _engine.SellHotel(player, baltic);

            // Assert: Must return false (PRP-65: voluntary sale denied during housing shortage)
            Assert.That(result, Is.False, "bank has not enough house to replace your hotel");
            Assert.That(baltic.BuildingLevel, Is.EqualTo(5)); // Hotel remains
            Assert.That(player.Money, Is.EqualTo(startingMoney)); // No refund given
            Assert.That(_engine.HouseAmount, Is.EqualTo(2)); // Bank house supply unchanged
        }

        [Test]
        public void SellHotel_BankNoEnoughHouse_PlayerHasDebt_ForcesLiquidationAll_ReturnTrue()
        {
            // Arrange: Player 1 has a Hotel on Baltic (level 5) and 4 houses on Mediterranean (level 4)
            var player = players[0];
            var token = _engine.GetPlayerToken(player)!;

            var mediterranean = _engine.Board.Path[1].Property!;
            var baltic = _engine.Board.Path[3].Property!;

            _engine.Move(token, steps: 1);
            _engine.BuyProperty(player, mediterranean);

            _engine.Move(token, steps: 2);
            _engine.BuyProperty(player, baltic);

            mediterranean.BuildingLevel = 4;
            baltic.BuildingLevel = 5; // Hotel

            // 1. Simulate housing shortage in the Bank
            _engine.HouseAmount = 2;
            int startingHotels = _engine.HotelAmount;

            // 2. Put player in debt: Player has $0 and lands on Space 4 (Income Tax: $200 debt)
            player.Money = 0;
            _engine.Move(token, steps: 1); // Moves from Space 3 (Baltic) to Space 4 (Income Tax)

            Assert.That(_engine.HasPendingDebt(player), Is.True);

            // Act: Under debt pressure, selling the hotel triggers emergency group liquidation (PRP-66)
            bool result = _engine.SellHotel(player, baltic);

            // Assert:
            // 1. Sale succeeds
            Assert.That(result, Is.True, "force liquidation");

            // 2. Both properties are stripped completely back to level 0 (undeveloped ground)
            Assert.That(mediterranean.BuildingLevel, Is.EqualTo(0));
            Assert.That(baltic.BuildingLevel, Is.EqualTo(0));

            // 3. Hotel returns to the Bank (+1)
            Assert.That(_engine.HotelAmount, Is.EqualTo(startingHotels + 1));

            // 4. The 4 houses from Mediterranean return to the Bank supply (2 + 4 = 6)
            Assert.That(_engine.HouseAmount, Is.EqualTo(2 + 4));

            // 5. Player receives 50% refund on ALL 9 building levels:
            // Mediterranean (4 levels x $25 = $100) + Baltic (5 levels x $25 = $125) = $225 refund
            Assert.That(player.Money, Is.EqualTo(225));
        }

        #endregion





    }
}