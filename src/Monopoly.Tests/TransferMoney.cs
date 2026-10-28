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
    public class TransferMoney
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
        }

        // SEND MONEY PLAYER TO PLAYER
        [Test]
        public void SendMoney_ToPlayer_WeHaveNoMoney_ReturnFalse()
        {

            var result = _engine.SendMoney(player: players[0], amount: 2000, otherPlayer: players[1]);
            Assert.That(result, Is.False, "Not Enough Money");
        }

        [Test]
        public void SendMoney_ToPlayer_Send0Zero_ReturnFalse()
        {
            var result = _engine.SendMoney(player: players[0], amount: 0, otherPlayer: players[1]);

            Assert.That(result, Is.False, "Send money should be above 0");
        }

        [Test]
        public void SendMoney_SenderPlayerNull_ReturnFalse()
        {
            var result = _engine.SendMoney(player: null!, amount: 0, otherPlayer: players[1]);

            Assert.That(result, Is.False, "Player cant be null");
        }

        [Test]
        public void SendMoney_ReceiverPlayerNull_ReturnFalse()
        {
            var result = _engine.SendMoney(player: players[0], amount: 0, otherPlayer: null!);

            Assert.That(result, Is.False, "Player cant be null");
        }


        // SEND MONEY FROM BANK
        [Test]
        public void SendMoney_Zero0_ToBank_ReturnFalse()
        {

            var result = _engine.SendMoney(player: players[0], amount: 0);
            Assert.That(result, Is.False, "Send Money should be above 0");
        }

        [Test]
        public void SendMoney_FromBank_PlayerNull_ReturnFalse()
        {

            var result = _engine.SendMoney(player: null!, amount: 200);
            Assert.That(result, Is.False, "Send Money should be above 0");
        }

        [Test]
        public void ReceiveMoney_FromBank_PlayerNull_ReturnFalse()
        {

            var result = _engine.ReceiveMoney(amount: 200, player: null!);
            Assert.That(result, Is.False, "Player can't be null");
        }

        [Test]
        public void ReceiveMoney_Zero0_FromBank_ReturnFalse()
        {

            var result = _engine.ReceiveMoney(amount: 0, player: players[0]);
            Assert.That(result, Is.False, "Send Money should be above 0");
        }
    }
}