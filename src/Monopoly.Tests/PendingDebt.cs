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
    public class PendingDebt
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
        public void PayPendingDebt_DebtorNull_ReturnFalse()
        {
            // Act & Assert
            bool result = _engine.PayPendingDebt(null!);
            Assert.That(result, Is.False, "Debtor cant be null");
        }

        [Test]
        public void PayPendingDebt_NoDebtOwed_ReturnFalse()
        {
            // Arrange: Player has no pending debts registered
            var player = players[0];

            // Act
            bool result = _engine.PayPendingDebt(player);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void PayPendingDebt_DebtOwedToPlayer_HaveMoney_PaysAndClearsDebt()
        {
            // Arrange: Player 1 owns Mediterranean Avenue with 1 house ($10 rent)
            var token1 = _engine.GetPlayerToken(players[0])!;

            var mediterranean = _engine.Board.Path[1].Property!;
            var baltic = _engine.Board.Path[3].Property!;

            _engine.Move(token1, steps: 1);
            _engine.BuyProperty(players[0], mediterranean);

            _engine.Move(token1, steps: 2);
            _engine.BuyProperty(players[0], baltic);

            mediterranean.BuildingLevel = 1; // $10 rent

            // Player 2 has $0 cash and incurs a $10 rent debt to Player 1
            var debtor = players[1];
            debtor.Money = 0;


            _engine.HandleRentPayment(debtor, mediterranean);
            Assert.That(_engine.HasPendingDebt(debtor), Is.True);

            // Player 2 raises cash (e.g. from mortgaging an asset)
            debtor.Money = 50;
            int creditorBalanceBefore = players[0].Money;

            // Act: Settle the pending debt
            bool result = _engine.PayPendingDebt(debtor);

            // Assert: Debt cleared, cash transferred from debtor to creditor
            Assert.That(result, Is.True);
            Assert.That(_engine.HasPendingDebt(debtor), Is.False);
            Assert.That(debtor.Money, Is.EqualTo(50 - 10)); // $40 remaining
            Assert.That(players[0].Money, Is.EqualTo(creditorBalanceBefore + 10));
        }

        [Test]
        public void PayPendingDebt_DebtOwedToBank_SufficientFunds_PaysBankAndClearsDebt()
        {
            // Arrange: Player 1 has $0 and lands on Space 4 (Income Tax: $200 owed to the Bank)
            var debtor = players[0];
            var token = _engine.GetPlayerToken(debtor)!;
            debtor.Money = 0;

            _engine.Move(token, steps: 4); // Space 4 is Income Tax
            Assert.That(_engine.HasPendingDebt(debtor), Is.True);

            // Player raises $250
            debtor.Money = 250;

            // Act: Pay the Bank debt
            bool result = _engine.PayPendingDebt(debtor);

            // Assert: Bank debt is settled and $200 deducted without a creditor player
            Assert.That(result, Is.True);
            Assert.That(_engine.HasPendingDebt(debtor), Is.False);
            Assert.That(debtor.Money, Is.EqualTo(250 - 200)); // $50 remaining
        }

        [Test]
        public void PayPendingDebt_InsufficientFunds_ReturnFalse()
        {
            // Arrange: Setup a $10 rent debt to Player 1
            var token1 = _engine.GetPlayerToken(players[0])!;
            var mediterranean = _engine.Board.Path[1].Property!;
            var baltic = _engine.Board.Path[3].Property!;

            _engine.Move(token1, steps: 1);
            _engine.BuyProperty(players[0], mediterranean);

            _engine.Move(token1, steps: 2);
            _engine.BuyProperty(players[0], baltic);

            mediterranean.BuildingLevel = 1;

            var debtor = players[1];
            debtor.Money = 0;

            _engine.HandleRentPayment(debtor, mediterranean);
            Assert.That(_engine.HasPendingDebt(debtor), Is.True);


            // Act: Attempt to pay debt with insufficient funds
            bool result = _engine.PayPendingDebt(debtor);

            // Assert: Must return false, debt stays active, and money untouched
            Assert.That(result, Is.False);
            Assert.That(_engine.HasPendingDebt(debtor), Is.True);
            Assert.That(debtor.Money, Is.EqualTo(0));
        }
    }
}