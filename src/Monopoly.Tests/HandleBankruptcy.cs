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
    public class HandleBankruptcy
    {
        private GameEngine _engine;
        private List<IPlayer> players;
        private List<IToken> tokens;

        [SetUp]
        public void Setup()
        {
            _engine = new GameEngine(new Board(), new Dice());
            players = new List<IPlayer>
            {
                new Player(id: 1, name: "Ant"),
                new Player(id: 2, name: "Thonny")
            };

            tokens = new List<IToken>
            {
                new Token(color: TokenColor.Red),
                new Token(color: TokenColor.Blue)
            };

            _engine.InitializeBoard();
            _engine.InitializeCardDecks();
            _engine.InitializePlayers(players, tokens);
            _engine.StartGame();
        }

        [Test]
        public void HandleBankruptcy_PlayerNull_ReturnFalse()
        {
            bool result = _engine.HandleBankruptcy(null!);
            Assert.That(result, Is.False, "player null");
        }

        [Test]
        public void HandleBankruptcy_AlreadyBankrupt_ReturnFalse()
        {
            var player = players[0];
            player.IsBankrupt = true;

            bool result = _engine.HandleBankruptcy(player);
            Assert.That(result, Is.False, "already bankrupt");
        }

        [Test]
        public void HandleBankruptcy_DebtToPlayer_TransfersCashPropertiesAndJailCardToCreditor()
        {
            // Arrange
            var debtor = players[0];
            var creditor = players[1];

            var token1 = _engine.GetPlayerToken(debtor)!;
            var token2 = _engine.GetPlayerToken(creditor)!;

            // 1. Debtor owns Mediterranean Avenue
            var mediterranean = _engine.Board.Path[1].Property!;
            _engine.Move(token1, steps: 1);
            _engine.BuyProperty(debtor, mediterranean);

            // 2. Debtor holds 1 Get Out of Jail Free
            debtor.JailFreeCard = 1;

            // 3. Creditor buys Baltic Avenue and builds 1 house ($10 rent)
            var baltic = _engine.Board.Path[3].Property!;

            _engine.Move(token2, steps: 3);
            _engine.BuyProperty(creditor, baltic);

            // 4. Debtor incurs rent debt to Creditor
            debtor.Money = 0; // Insolvent

            _engine.HandleRentPayment(debtor, baltic);
            Assert.That(_engine.HasPendingDebt(debtor), Is.True);

            // Give debtor $50 cash before declaring bankruptcy (to verify cash transfer)
            debtor.Money = 50;
            int creditorCashBefore = creditor.Money;

            // Act: Debtor declares bankruptcy to Creditor
            bool result = _engine.HandleBankruptcy(debtor);

            // Assert:
            // 1. Debtor marked bankrupt & token removed from board
            Assert.That(result, Is.True);
            Assert.That(debtor.IsBankrupt, Is.True);
            Assert.That(debtor.Money, Is.EqualTo(0));
            Assert.That(_engine.GetPlayerPosition(debtor), Is.Null);

            // 2. Creditor receives Debtor's cash ($50)
            Assert.That(creditor.Money, Is.EqualTo(creditorCashBefore + 50));

            // 3. Creditor receives ownership of Debtor's properties
            Assert.That(_engine.GetPropertyOwner(mediterranean), Is.EqualTo(creditor));

            // 4. Creditor receives Debtor's Get Out of Jail Free card
            Assert.That(creditor.JailFreeCard, Is.EqualTo(1));
            Assert.That(debtor.JailFreeCard, Is.EqualTo(0));

            // 5. Debt cleared and Game Over triggered (only 1 player remains)
            Assert.That(_engine.HasPendingDebt(debtor), Is.False);
            Assert.That(_engine.Status, Is.EqualTo(GameStatus.GameOver));
        }

        [Test]
        public void HandleBankruptcy_DebtToBank_ReturnsAssetsToBankAndClearsOwnership()
        {
            // Arrange
            var debtor = players[0];
            var token = _engine.GetPlayerToken(debtor)!;

            // 1. Debtor owns Mediterranean Avenue
            var mediterranean = _engine.Board.Path[1].Property!;

            _engine.Move(token, steps: 1);
            _engine.BuyProperty(debtor, mediterranean);
            debtor.JailFreeCard = 1;

            // 2. Debtor incurs a debt to the BANK (lands on Income Tax with $0 cash)
            debtor.Money = 0;
            _engine.Move(token, steps: 3); // Moves from Space 1 to Space 4 (Income Tax)
            Assert.That(_engine.HasPendingDebt(debtor), Is.True);

            // Act: Debtor declares bankruptcy to the Bank
            bool result = _engine.HandleBankruptcy(debtor);

            // Assert:
            Assert.That(result, Is.True);
            Assert.That(debtor.IsBankrupt, Is.True);
            Assert.That(debtor.Money, Is.EqualTo(0));

            // Properties become unowned Bank properties
            Assert.That(_engine.GetPropertyOwner(mediterranean), Is.Null);
            Assert.That(mediterranean.IsMortgaged, Is.False);

            // Jail cards returned to supply
            Assert.That(debtor.JailFreeCard, Is.EqualTo(0));

            // Token removed from board
            Assert.That(_engine.GetPlayerPosition(debtor), Is.Null);
            Assert.That(_engine.Status, Is.EqualTo(GameStatus.GameOver));
        }

        [Test]
        public void HandleBankruptcy_LiquidatesBuildingsAtHalfPrice()
        {
            // Arrange: Debtor owns Brown monopoly with a Hotel on Baltic (level 5)
            var debtor = players[0];
            var creditor = players[1];

            var token1 = _engine.GetPlayerToken(debtor)!;
            var token2 = _engine.GetPlayerToken(creditor)!;

            var mediterranean = _engine.Board.Path[1].Property!;
            var baltic = _engine.Board.Path[3].Property!;

            // simulate debtor has property
            _engine.Move(token1, steps: 1);
            _engine.BuyProperty(debtor, mediterranean);

            _engine.Move(token1, steps: 2);
            _engine.BuyProperty(debtor, baltic);

            // simulate debtor has buildings
            mediterranean.BuildingLevel = 4;
            baltic.BuildingLevel = 5; // Hotel

            int startingHotels = _engine.HotelAmount;
            int startingHouses = _engine.HouseAmount;

            // Debtor incurs rent debt to Creditor
            debtor.Money = 0;
            var readingRailroad = _engine.Board.Path[5].Property!;


            _engine.Move(token2, steps: 5);
            _engine.BuyProperty(creditor, readingRailroad);

            _engine.HandleRentPayment(debtor, readingRailroad);
            Assert.That(_engine.HasPendingDebt(debtor), Is.True);

            int creditorCashBefore = creditor.Money;

            // Act: Liquidate on bankruptcy
            _engine.HandleBankruptcy(debtor);

            // Assert:
            // 1. Hotel returned to Bank
            Assert.That(_engine.HotelAmount, Is.EqualTo(startingHotels + 1));

            // 2. Both buildings stripped back to level 0
            Assert.That(mediterranean.BuildingLevel, Is.EqualTo(0));
            Assert.That(baltic.BuildingLevel, Is.EqualTo(0));

            // 3. Refund for buildings: Mediterranean (4 x $25 = $100) + Baltic (5 x $25 = $125) = $225
            // Cash from building liquidation is awarded to Creditor!
            Assert.That(creditor.Money, Is.EqualTo(creditorCashBefore + 225));
        }

        [Test]
        public void HandleBankruptcy_OnPlayerBankruptEvent()
        {
            // Arrange
            var debtor = players[0];
            IPlayer? bankruptEventPlayer = null;

            _engine.OnPlayerBankrupt += (p) =>
            {
                bankruptEventPlayer = p;
            };

            // Act
            _engine.HandleBankruptcy(debtor);

            // Assert
            Assert.That(bankruptEventPlayer, Is.EqualTo(debtor));
        }
    }
}