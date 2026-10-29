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
    public class ExecuteCardEffect_Test
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


        // =========================================================================
        // 1. MOVEMENT CARDS
        // =========================================================================
        #region ExecuteCardEffect
        [Test]
        public void ExecuteCardEffect_MoveToPosition_AdvancesTokenToTargetSquare()
        {
            // Arrange: Advance to Illinois Avenue (Space index 24)
            var player = players[0];
            var card = new Card(CardType.Chance, CardEffect.MoveToPosition, "Advance to Illinois Ave", value: 24);

            // Act
            _engine.ExecuteCardEffect(player, card);

            // Assert
            Assert.That(_engine.GetPlayerPosition(player), Is.EqualTo(_engine.Board.Path[24]));
        }

        [Test]
        public void ExecuteCardEffect_MoveToNearestRailroad_AdvancesToNextRailroad()
        {
            // Arrange: Move token to Space 7 (Chance). Next railroad ahead is Space 15 (Pennsylvania RR)
            var player = players[0];
            var token = _engine.GetPlayerToken(player)!;
            _engine.Move(token, steps: 6);

            var card = new Card(CardType.Chance, CardEffect.MoveToNearestRailroad, "Advance to nearest railroad");

            // Act
            _engine.ExecuteCardEffect(player, card);

            // Assert: Advances to Space 15 (Pennsylvania RR)
            Assert.That(_engine.GetPlayerPosition(player), Is.EqualTo(_engine.Board.Path[15]));
        }

        [Test]
        public void ExecuteCardEffect_MoveToNearestUtility_AdvancesToNextUtility()
        {
            // Arrange: Move token to Space 7 (Chance). Next utility ahead is Space 12 (Electric Company)
            var player = players[0];
            var token = _engine.GetPlayerToken(player)!;
            _engine.Move(token, steps: 6);

            var card = new Card(CardType.Chance, CardEffect.MoveToNearestUtility, "Advance to nearest utility");

            // Act
            _engine.ExecuteCardEffect(player, card);

            // Assert: Advances to Space 12 (Electric Company)
            Assert.That(_engine.GetPlayerPosition(player), Is.EqualTo(_engine.Board.Path[12]));
        }

        [Test]
        public void ExecuteCardEffect_MoveSteps_MovesBackThreeSpaces()
        {
            // Arrange: Move to Space 7 (Chance)
            var player = players[0];
            var token = _engine.GetPlayerToken(player)!;
            _engine.Move(token, steps: 6);

            var card = new Card(CardType.Chance, CardEffect.MoveSteps, "Go Back 3 Spaces", value: -3);

            // Act: 7 - 3 = 4 (Income Tax)
            _engine.ExecuteCardEffect(player, card);

            // Assert
            Assert.That(_engine.GetPlayerPosition(player), Is.EqualTo(_engine.Board.Path[3]));
        }

        // =========================================================================
        // 2. CASH AWARDS & FINES
        // =========================================================================

        [Test]
        public void ExecuteCardEffect_ReceiveMoney_IncreasesPlayerMoney()
        {
            // Arrange
            var player = players[0];
            int balanceBefore = player.Money;
            var card = new Card(CardType.CommunityChest, CardEffect.ReceiveMoney, "Bank error in your favor", value: 200);

            // Act
            _engine.ExecuteCardEffect(player, card);

            // Assert
            Assert.That(player.Money, Is.EqualTo(balanceBefore + 200));
        }

        [Test]
        public void ExecuteCardEffect_PayMoney_HaveEnoughMoney_DeductsMoney()
        {
            // Arrange
            var player = players[0];
            int balanceBefore = player.Money;
            var card = new Card(CardType.CommunityChest, CardEffect.PayMoney, "Doctor's fees", value: 50);

            // Act
            _engine.ExecuteCardEffect(player, card);

            // Assert
            Assert.That(player.Money, Is.EqualTo(balanceBefore - 50));
        }

        [Test]
        public void ExecuteCardEffect_PayMoney_HaveNOMoney_RecordsPendingDebt()
        {
            // Arrange: Player has $0 and must pay a $50 fine to the Bank
            var player = players[0];
            player.Money = 0;
            var card = new Card(CardType.CommunityChest, CardEffect.PayMoney, "School fees", value: 50);

            // Act
            _engine.ExecuteCardEffect(player, card);

            // Assert: Cannot pay -> recorded as debt owed to Bank (null creditor)
            Assert.That(_engine.HasPendingDebt(player), Is.True);
            Assert.That(player.Money, Is.EqualTo(0));
        }

        // =========================================================================
        // 3. PLAYER-TO-PLAYER TRANSACTIONS
        // =========================================================================

        [Test]
        public void ExecuteCardEffect_PayEachPlayer_TransfersCashToAllActiveOpponents()
        {
            // Arrange: Player 1 pays $50 to each other player
            var player1 = players[0];
            var player2 = players[1];

            var card = new Card(CardType.Chance, CardEffect.PayEachPlayer, "Chairman of the Board", value: 50);

            // Act
            _engine.ExecuteCardEffect(player1, card);

            // Assert: Player 1 loses $50, Player 2 gains $50
            Assert.That(player1.Money, Is.EqualTo(1500 - 50));
            Assert.That(player2.Money, Is.EqualTo(1500 + 50));
        }

        [Test]
        public void ExecuteCardEffect_ReceiveFromEachPlayer_CollectsCashFromAllOpponents()
        {
            // Arrange: Player 1 collects $10 from each other player for Birthday
            var player1 = players[0];
            var player2 = players[1];

            var card = new Card(CardType.CommunityChest, CardEffect.ReceiveFromEachPlayer, "It's your birthday", value: 10);

            // Act
            _engine.ExecuteCardEffect(player1, card);

            // Assert: Player 1 gains $10, Player 2 pays $10
            Assert.That(player1.Money, Is.EqualTo(1500 + 10));
            Assert.That(player2.Money, Is.EqualTo(1500 - 10));
        }

        // =========================================================================
        // 4. JAIL & REPAIRS
        // =========================================================================

        [Test]
        public void ExecuteCardEffect_GoToJail_SendsPlayerToJailSquare10()
        {
            // Arrange
            var player = players[0];
            var card = new Card(CardType.Chance, CardEffect.GoToJail, "Go directly to Jail");

            // Act
            _engine.ExecuteCardEffect(player, card);

            // Assert
            Assert.That(player.IsInJail, Is.True);
            Assert.That(_engine.GetPlayerPosition(player), Is.EqualTo(_engine.Board.Path[10]));
        }

        [Test]
        public void ExecuteCardEffect_Repairs_ChargesBasedOnHousesAndHotelsOwned()
        {
            // Arrange: Player 1 owns Mediterranean (2 houses) and Baltic (1 hotel = level 5)
            var player = players[0];
            var token = _engine.GetPlayerToken(player)!;

            var mediterranean = _engine.Board.Path[1].Property!;
            var baltic = _engine.Board.Path[3].Property!;

            _engine.Move(token, steps: 1);
            _engine.BuyProperty(player, mediterranean);

            _engine.Move(token, steps: 2);
            _engine.BuyProperty(player, baltic);

            mediterranean.BuildingLevel = 4; // 2 houses
            baltic.BuildingLevel = 5;        // 1 hotel

            int balanceBefore = player.Money;

            // Chance repairs: $25 per house, $100 per hotel
            // Expected bill: (4 houses * $25) + (1 hotel * $100) = $100 + $100 = $200
            var card = new Card(CardType.Chance, CardEffect.Repairs, "Make general repairs on all your property");

            // Act
            _engine.ExecuteCardEffect(player, card);

            // Assert
            Assert.That(player.Money, Is.EqualTo(balanceBefore - 200));
        }





        #endregion


        // =========================================================================
        // 5. DRAW CARD & DECK LIFECYCLE
        // =========================================================================

        [Test]
        public void DrawCard_PlayerNull_DoesNothing()
        {
            // Act: Draw card with null player
            _engine.DrawCard(null!, CardType.Chance);

            // Assert: Does not crash and no card is drawn
            Assert.That(_engine.LastDrawnCard, Is.Null);
        }

        [Test]
        public void DrawCard_PlayerBankrupt_DoesNothing()
        {
            // Arrange
            var player = players[0];
            player.IsBankrupt = true;

            // Act: Bankrupt player tries to draw
            _engine.DrawCard(player, CardType.Chance);

            // Assert: No card drawn
            Assert.That(_engine.LastDrawnCard, Is.Null);
        }

        [Test]
        public void DrawCard_GameNotInProgress_DoesNothing()
        {
            // Arrange
            var player = players[0];
            _engine.EndGame();

            // Act: Attempt to draw after game has ended
            _engine.DrawCard(player, CardType.Chance);

            // Assert: No card drawn
            Assert.That(_engine.LastDrawnCard, Is.Null);
        }

        [Test]
        public void DrawCard_ValidCall_UpdatesLastDrawnCard_OnCardEffectEvent()
        {
            // Arrange: Subscribe to the engine's OnCardEffect event
            var player = players[0];
            IPlayer? eventPlayer = null;
            ICard? eventCard = null;

            _engine.OnCardEffect += (p, c) =>
            {
                eventPlayer = p;
                eventCard = c;
            };

            // Place a known cash card on top of the deck (avoids random GoToJail card wiping LastDrawnCard)
            _engine.CommunityChestDeck.Cards.Clear();

            var testCard = new Card(CardType.CommunityChest, CardEffect.ReceiveMoney, "Bank error in your favor", value: 200);

            _engine.CommunityChestDeck.Cards.Enqueue(testCard);

            // Act: Draw the card from Community Chest
            _engine.DrawCard(player, CardType.CommunityChest);
            
            // Assert: LastDrawnCard is recorded and observer event was notified with the exact card
            Assert.That(_engine.LastDrawnCard, Is.EqualTo(testCard));
            Assert.That(eventCard, Is.EqualTo(testCard));
            Assert.That(eventPlayer, Is.EqualTo(player));
        }

        [Test]
        public void DrawCard_NormalCard_ExecutesEffectAndReturnsCardToBottomOfDeck()
        {
            // Arrange: Setup Chance deck with exactly 1 cash card
            var player = players[0];
            int balanceBefore = player.Money;

            _engine.ChanceDeck.Cards.Clear();
            var dividendCard = new Card(CardType.Chance, CardEffect.ReceiveMoney, "Bank dividend", value: 50);
            _engine.ChanceDeck.Cards.Enqueue(dividendCard);

            // Act: Draw the card
            _engine.DrawCard(player, CardType.Chance);

            // Assert:
            // 1. Effect was executed (player received $50)
            Assert.That(player.Money, Is.EqualTo(balanceBefore + 50));

            // 2. Card was returned to the bottom of the deck (deck still has 1 card)
            Assert.That(_engine.ChanceDeck.Cards.Count, Is.EqualTo(1));
            Assert.That(_engine.ChanceDeck.Cards.Peek(), Is.EqualTo(dividendCard));
        }

        [Test]
        public void DrawCard_GetOutOfJailFree_PlayerKeepsCardAndDeckDecrements()
        {
            // Arrange: Setup Chance deck with a "Get Out of Jail Free" card
            var player = players[0];
            player.JailFreeCard = 0;

            _engine.ChanceDeck.Cards.Clear();
            var jailCard = new Card(CardType.Chance, CardEffect.GetOutOfJailFree, "Get Out of Jail Free");
            _engine.ChanceDeck.Cards.Enqueue(jailCard);

            // Act: Draw the card
            _engine.DrawCard(player, CardType.Chance);

            // Assert:
            // 1. Player keeps the card in their possession (CRD-05)
            Assert.That(player.JailFreeCard, Is.EqualTo(1));

            // 2. Card is NOT returned to the deck (deck is now empty: 0)
            Assert.That(_engine.ChanceDeck.Cards.Count, Is.EqualTo(0));
        }
    }
}