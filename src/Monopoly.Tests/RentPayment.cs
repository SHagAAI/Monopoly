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
    public class RentPayment
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
        public void HandleRentPayment_PlayerNull_ReturnNothing()
        {
            // Arrange
            var property = _engine.Board.Path[1].Property!;

            // Act & Assert: Must return immediately without throwing an exception
            Assert.DoesNotThrow(() => _engine.HandleRentPayment(null!, property));
        }

        [Test]
        public void HandleRentPayment_PropertyNull_ReturnNothing()
        {
            // Arrange
            var player = players[0];
            int balanceBefore = player.Money;

            // Act: Call with null property
            _engine.HandleRentPayment(player, null!);

            // Assert: Player balance must remain completely untouched
            Assert.That(player.Money, Is.EqualTo(balanceBefore));
        }

        [Test]
        public void HandleRentPayment_OwnerNull_NoRentCharged()
        {
            // Arrange: Mediterranean Avenue is unowned (Bank-owned, owner == null)
            var player = players[0];
            var mediterranean = _engine.Board.Path[1].Property!;
            int balanceBefore = player.Money;

            // Act: Player lands on unowned property
            _engine.HandleRentPayment(player, mediterranean);

            // Assert: Unowned properties collect $0 rent (SPC-04)
            Assert.That(player.Money, Is.EqualTo(balanceBefore));
        }
        [Test]
        public void HandleRentPayment_OwnerIsPlayer_NoRentCharged()
        {
            // Arrange: Player 1 buys Mediterranean Avenue
            var player = players[0];
            var token = _engine.GetPlayerToken(player)!;

            var mediterranean = _engine.Board.Path[1].Property!;

            _engine.Move(token, steps: 1);
            _engine.BuyProperty(player, mediterranean);

            int balanceAfterPurchase = player.Money;

            // Act: Player 1 lands on their own property (owner == player)
            _engine.HandleRentPayment(player, mediterranean);

            // Assert: You do not pay rent to yourself
            Assert.That(player.Money, Is.EqualTo(balanceAfterPurchase));
        }
        [Test]
        public void HandleRentPayment_Railroad_FromCard_DoublesRent()
        {
            // Arrange: Player 2 buys Reading Railroad (Space 5)
            var player1 = players[0];
            var player2 = players[1];

            var token2 = _engine.GetPlayerToken(player2)!;
            var readingRailroad = _engine.Board.Path[5].Property!; // Base rent for 1 railroad is $25

            _engine.Move(token2, steps: 5);
            _engine.BuyProperty(player2, readingRailroad);

            int player1StartingMoney = player1.Money;
            int player2StartingMoney = player2.Money;

            // Act: Player 1 advances to Reading Railroad from a Chance Card (fromCard: true)
            _engine.HandleRentPayment(player1, readingRailroad, fromCard: true);
            
            // Assert: Normal rent is $25, but Chance card mandates paying 2x rent ($50)
            int expectedRent = 25 * 2; // $50
            Assert.That(player1.Money, Is.EqualTo(player1StartingMoney - expectedRent));
            Assert.That(player2.Money, Is.EqualTo(player2StartingMoney + expectedRent));
        }

        [Test]
        public void HandleRentPayment_SinglePropertyInColorGroup_PayBaseRent()
        {
            var player = players[0];
            var token = _engine.GetPlayerToken(players[0])!;

            var Mediterranean = _engine.Board.Path[1].Property!;
            _engine.Move(token, steps: 1);
            _engine.BuyProperty(player, Mediterranean);

            var player2 = players[1];
            var token2 = _engine.GetPlayerToken(players[1])!;


            var player2StartingMoney = player2.Money;
            var baseRent = Mediterranean.Rents[0];

            _engine.HandleRentPayment(player: player2, property: Mediterranean);
            Assert.That(player2.Money, Is.EqualTo(player2StartingMoney - baseRent), "Player 2 money should be decrease ($2) for rent.");

        }

        [Test]
        public void HandleRentPayment_CompleteColorGroupUnImproved_PayBaseRentMultipliedBy2()
        {
            var player = players[0];
            var token = _engine.GetPlayerToken(player)!;

            var Mediterranean = _engine.Board.Path[1].Property!;
            var Baltic = _engine.Board.Path[3].Property!;

            _engine.Move(token, steps: 1);
            _engine.BuyProperty(player, Mediterranean);

            _engine.Move(token, steps: 2);
            _engine.BuyProperty(player, Baltic);

            var player2 = players[1];
            var player2StartingMoney = player2.Money;
            // _engine.Move(token: tokens[1], steps: 1);

            var doubleRent = Mediterranean.Rents[0] * 2; // $2 * 2
            _engine.HandleRentPayment(player: player2, property: Mediterranean);

            Assert.That(player2.Money, Is.EqualTo(player2StartingMoney - doubleRent), "Player 2 should be deducted doubled rent ($4).");
        }

        [TestCase(1)] // 1 House  -> $10 rent
        [TestCase(2)] // 2 Houses -> $30 rent
        [TestCase(3)] // 3 Houses -> $90 rent
        [TestCase(4)] // 4 Houses -> $160 rent
        public void HandleRentPayment_WithHouse_PayHouseRent(int houseLevel)
        {
            var player = players[0];
            var token = _engine.GetPlayerToken(player)!;

            var Mediterranean = _engine.Board.Path[1].Property!;
            var Baltic = _engine.Board.Path[3].Property!;

            _engine.Move(token, steps: 1);
            _engine.BuyProperty(player, Mediterranean);

            _engine.Move(token, steps: 2);
            _engine.BuyProperty(player, Baltic);

            Mediterranean.BuildingLevel = houseLevel;

            var player2 = players[1];
            var player2StartingMoney = player2.Money;
            int houseRent = Mediterranean.Rents[houseLevel]; // $10, $30, $90, or $160

            _engine.HandleRentPayment(player: player2, property: Mediterranean);
            Assert.That(player2.Money, Is.EqualTo(player2StartingMoney - houseRent), "Player 2 should pay the 1-house rent ($10).");

        }

        [Test]
        public void HandleRentPayment_NoMoney_AddPendingDebt()
        {
            var token = _engine.GetPlayerToken(players[0])!;
            var Mediterranean = _engine.Board.Path[1].Property!;
            var Baltic = _engine.Board.Path[3].Property!;

            _engine.Move(token, steps: 1);
            _engine.BuyProperty(players[0], Mediterranean);

            _engine.Move(token, steps: 2);
            _engine.BuyProperty(players[0], Baltic);

            Mediterranean.BuildingLevel = 1;
            // int rentCost = Mediterranean.Rents[0]; // $2

            var player2 = players[1];
            player2.Money = 0;

            _engine.HandleRentPayment(player: player2, property: Mediterranean);

            Assert.That(_engine.HasPendingDebt(player2), Is.True, "Player 2 should be in a Pending Debt state.");
            Assert.That(_engine.CanEndTurn(), Is.False, "Player in debt must NOT be allowed to end their turn!");
        }


        [Test]
        public void HandleRentPayment_MortgagedProperty_Collects0ZeroRent()
        {

            var token = _engine.GetPlayerToken(players[0])!;
            var Mediterranean = _engine.Board.Path[1].Property!;

            _engine.Move(token, steps: 1);
            _engine.BuyProperty(players[0], Mediterranean);

            // Mortgage the property
            Mediterranean.IsMortgaged = true;

            var player2 = players[1];
            int player2StartingMoney = player2.Money; // $1500

            // 2. Act: Player 2 lands on / pays rent on the mortgaged property
            _engine.HandleRentPayment(player: player2, property: Mediterranean);

            // 3. Assert:
            Assert.That(player2.Money, Is.EqualTo(player2StartingMoney), "No rent on mortgaged property.");

        }

        [TestCase(1, 25)]  // 1 Railroad  -> $25
        [TestCase(2, 50)]  // 2 Railroads -> $50
        [TestCase(3, 100)] // 3 Railroads -> $100
        [TestCase(4, 200)] // 4 Railroads -> $200
        public void HandleRentPayment_Railroad_RentScalesByRailroadsOwned(int count, int rentCost)
        {
            // 1. Arrange: Have Player 1 buy 'count' railroads
            var token = _engine.GetPlayerToken(players[0])!;
            int[] railRoadIndices = { 5, 15, 25, 35 };

            int currentPos = 0;
            for (int i = 0; i < count; i++)
            {
                int target = railRoadIndices[i];
                _engine.Move(token, steps: target - currentPos);
                currentPos = target;

                var railroadProperty = _engine.Board.Path[target].Property!;
                _engine.BuyProperty(players[0], railroadProperty); // $200 each
            }

            // Target Reading Railroad (Space 5) for the rent check
            var readingRailroad = _engine.Board.Path[5].Property!;

            var player2 = players[1];
            int player2StartingMoney = player2.Money; // $1500

            // 2. Act: Player 2 pays rent on Reading Railroad
            _engine.HandleRentPayment(player: player2, property: readingRailroad);

            // 3. Assert:
            Assert.That(player2.Money, Is.EqualTo(player2StartingMoney - rentCost),
                $"Owning {count} railroad(s) should charge ${rentCost} rent.");

        }

        [TestCase(1, 4)]  // 1 Utility owned  -> Dice Total x 4  (7 x 4 = $28)
        [TestCase(2, 10)] // 2 Utilities owned -> Dice Total x 10 (7 x 10 = $70)
        public void HandleRentPayment_Utility_RentMultipliesDiceTotal(int utilityCount, int multiplier)
        {
            // 1. Arrange: Player 1 buys 'utilityCount' utilities (Space 12: Electric Co, Space 28: Water Works)
            var token = _engine.GetPlayerToken(players[0])!;
            int[] utilIndices = { 12, 28 };


            int currentPos = 0;
            for (int i = 0; i < utilityCount; i++)
            {
                int target = utilIndices[i];
                _engine.Move(token, steps: target - currentPos);
                currentPos = target;

                var utilityProperty = _engine.Board.Path[target].Property!;
                _engine.BuyProperty(players[0], utilityProperty); // $150 each
            }


            _engine.Dice.Side1 = 3;
            _engine.Dice.Side2 = 4;

            int diceTotal = _engine.Dice.Side1 + _engine.Dice.Side2;  // 3 + 4 = 7 

            var electricCompany = _engine.Board.Path[12].Property!;

            var player2 = players[1];

            int player2StartingMoney = player2.Money; // $1500
            int expectedRent = diceTotal * multiplier; // 7 x 4 = $28, or 7 x 10 = $70

            // 2. Act: Player 2 pays rent on Electric Company
            _engine.HandleRentPayment(player: player2, property: electricCompany);

            // 3. Assert:
            Assert.That(players[1].Money, Is.EqualTo(player2StartingMoney - expectedRent),
                $"With {utilityCount} utility owned and dice 7, rent should be ${expectedRent}.");
        }



    }
}