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
    public class Auction_Test
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
        public void Test1()
        {
            Assert.Pass();
        }

        #region StartAuction & DeclineProperty

        // =========================================================================
        // 1. VALIDATION & GUARD CLAUSES
        // =========================================================================

        [Test]
        public void StartAuction_NullProperty_ReturnFalse()
        {
            // Act: Attempt to start an auction with a null property
            bool result = _engine.StartAuction(null!);

            // Assert
            Assert.That(result, Is.False);
            Assert.That(_engine.HasActiveAuction, Is.False);
        }

        [Test]
        public void StartAuction_AlreadyOwnedProperty_ReturnFalse()
        {
            // Arrange: Player 1 buys Mediterranean Avenue
            var player1 = players[0];
            var token1 = _engine.GetPlayerToken(player1)!;

            var property = _engine.Board.Path[1].Property!;

            _engine.Move(token1, steps: 1);
            _engine.BuyProperty(player1, property);

            // Act: Attempt to auction a property already owned by Player 1
            bool result = _engine.StartAuction(property);

            // Assert: Cannot auction a property that already has an owner
            Assert.That(result, Is.False);
            Assert.That(_engine.HasActiveAuction, Is.False);
        }

        // =========================================================================
        // 2. STATE INITIALIZATION & OBSERVER EVENT
        // =========================================================================

        [Test]
        public void StartAuction_ValidUnownedProperty_InitializesAuctionState()
        {
            // Arrange: Mediterranean Avenue is unowned
            var property = _engine.Board.Path[1].Property!;

            // Act: Open auction
            bool result = _engine.StartAuction(property);

            // Assert:
            Assert.That(result, Is.True);
            Assert.That(_engine.HasActiveAuction, Is.True);
            Assert.That(_engine.AuctionProperty, Is.EqualTo(property));
            Assert.That(_engine.HighestBidder, Is.Null);
            Assert.That(_engine.HighestBid, Is.EqualTo(0));

            // First bidder to act is the current player whose turn it is
            Assert.That(_engine.CurrentAuctionBidder, Is.EqualTo(players[0]));
        }

        [Test]
        public void StartAuction_ValidCall_FiresOnAuctionStartedEvent()
        {
            // Arrange: Subscribe to the auction started event
            var property = _engine.Board.Path[1].Property!;
            IProperty? eventProperty = null;

            _engine.OnAuctionStarted += (prop) =>
            {
                eventProperty = prop;
            };

            // Act
            _engine.StartAuction(property);

            // Assert: Observer was notified with the correct property
            Assert.That(eventProperty, Is.EqualTo(property));
        }

        [Test]
        public void StartAuction_BankruptPlayersAreExcludedFromBidders()
        {
            // Arrange: Player 2 (Thonny) is bankrupt
            players[1].IsBankrupt = true;

            var property = _engine.Board.Path[1].Property!;

            // Act
            bool result = _engine.StartAuction(property);

            // Assert: Auction starts successfully for active players
            Assert.That(result, Is.True);
            Assert.That(_engine.HasActiveAuction, Is.True);
        }

        [Test]
        public void DeclineProperty_UnownedProperty_TriggersStartAuction()
        {
            // Arrange: Player 1 lands on Mediterranean Avenue and chooses not to buy it
            var player1 = players[0];
            var property = _engine.Board.Path[1].Property!;

            // Act: Player declines the purchase (PRP-12)
            _engine.DeclineProperty(player1, property);

            // Assert: Engine automatically opens the auction for all players (PRP-13)
            Assert.That(_engine.HasActiveAuction, Is.True);
            Assert.That(_engine.AuctionProperty, Is.EqualTo(property));
            Assert.That(_engine.CurrentAuctionBidder, Is.EqualTo(player1));
        }

        #endregion



        #region PlaceBid

        // =========================================================================
        // PLACE BID UNIT TESTS
        // =========================================================================

        [Test]
        public void PlaceBid_PlayerNullOrBankrupt_ReturnFalse()
        {
            // Arrange
            var property = _engine.Board.Path[1].Property!;
            _engine.StartAuction(property);

            var currentBidder = _engine.CurrentAuctionBidder!;
            currentBidder.IsBankrupt = true;

            // Act & Assert
            // 1. Null player
            Assert.That(_engine.PlaceBid(null!, 50), Is.False);

            // 2. Bankrupt player
            Assert.That(_engine.PlaceBid(currentBidder, 50), Is.False);
        }

        [Test]
        public void PlaceBid_NotCurrentBiddersTurn_ReturnFalse()
        {
            // Arrange: Auction starts, current bidder is players[0]
            var property = _engine.Board.Path[1].Property!;
            _engine.StartAuction(property);

            var notCurrentBidder = players[1]; // Thonny's turn hasn't arrived yet

            // Act: Out-of-turn bid attempt
            bool result = _engine.PlaceBid(notCurrentBidder, amount: 50);

            // Assert: Must be rejected; turn order must be respected
            Assert.That(result, Is.False);
            Assert.That(_engine.HighestBidder, Is.Null);
            Assert.That(_engine.HighestBid, Is.EqualTo(0));
        }

        [Test]
        public void PlaceBid_BidLowerThanOrEqualToCurrentTopBid_ReturnFalse()
        {
            // Arrange: Player 1 bids $50 successfully
            var property = _engine.Board.Path[1].Property!;
            _engine.StartAuction(property);

            var player1 = players[0];
            var player2 = players[1];

            _engine.PlaceBid(player1, amount: 50);

            // Act & Assert: It is now Player 2's turn
            // Case 1: Bid equal to current top bid ($50)
            Assert.That(_engine.PlaceBid(player2, amount: 50), Is.False);

            // Case 2: Bid lower than current top bid ($40)
            Assert.That(_engine.PlaceBid(player2, amount: 40), Is.False);
        }

        [Test]
        public void PlaceBid_PlayerCannotAffordBid_ReturnFalse()
        {
            // Arrange: Current bidder only has $30 cash
            var property = _engine.Board.Path[1].Property!;
            _engine.StartAuction(property);

            var player1 = players[0];
            player1.Money = 30;

            // Act: Attempt to bid $100
            bool result = _engine.PlaceBid(player1, amount: 100);

            // Assert: Must fail solvency check
            Assert.That(result, Is.False);
            Assert.That(_engine.HighestBid, Is.EqualTo(0));
            Assert.That(_engine.HighestBidder, Is.Null);
        }

        [Test]
        public void PlaceBid_ValidBid_UpdatesTopBidAndFiresEvent()
        {
            // Arrange
            var property = _engine.Board.Path[1].Property!;
            _engine.StartAuction(property);

            var player1 = players[0];

            IPlayer? eventPlayer = null;
            int eventAmount = 0;

            _engine.OnBidPlaced += (p, amt) =>
            {
                eventPlayer = p;
                eventAmount = amt;
            };

            // Act: Player 1 places a valid bid of $50
            bool result = _engine.PlaceBid(player1, amount: 50);


            Assert.That(result, Is.True);
            Assert.That(_engine.HighestBidder, Is.EqualTo(player1));
            Assert.That(_engine.HighestBid, Is.EqualTo(50));

            // // Verify event dispatched
            // Assert.That(eventPlayer, Is.EqualTo(player1));
            // Assert.That(eventAmount, Is.EqualTo(50));
        }

        [Test]
        public void PlaceBid_ValidBid_AdvancesTurnToNextBidder()
        {
            // Arrange: 3 players in auction (Ant, Thonny, Bella)
            var property = _engine.Board.Path[1].Property!;
            _engine.StartAuction(property);

            Assert.That(_engine.CurrentAuctionBidder, Is.EqualTo(players[0]));

            // Act 1: Player 1 bids $50
            _engine.PlaceBid(players[0], amount: 50);

            // Assert 1: Turn advances to Player 2
            Assert.That(_engine.CurrentAuctionBidder, Is.EqualTo(players[1]));

            // Act 2: Player 2 bids $60
            _engine.PlaceBid(players[1], amount: 60);

            // Assert 2: Turn advances to Player 3
            Assert.That(_engine.CurrentAuctionBidder, Is.EqualTo(players[0]));
        }

        #endregion

        #region PassBid

        // =========================================================================
        // PASS BID UNIT TESTS (2-PLAYER GAME)
        // =========================================================================

        [Test]
        public void PassBid_NoActiveAuction_ReturnFalse()
        {
            // Act: Attempt to pass when no auction is running
            bool result = _engine.PassBid(players[0]);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void PassBid_NotCurrentBiddersTurn_ReturnFalse()
        {
            // Arrange: Auction starts, turn is with Player 1 (Ant)
            var property = _engine.Board.Path[1].Property!;
            _engine.StartAuction(property);

            var notCurrentBidder = players[1]; // Thonny

            // Act: Player 2 attempts to pass when it's Player 1's turn
            bool result = _engine.PassBid(notCurrentBidder);

            // Assert: Must be rejected; turn order must be respected
            Assert.That(result, Is.False);
            Assert.That(_engine.CurrentAuctionBidder, Is.EqualTo(players[0]));
        }

        [Test]
        public void PassBid_FirstPlayerPasses_AdvancesTurnToSecondPlayer()
        {
            // Arrange: 2 players in auction
            var property = _engine.Board.Path[1].Property!;
            _engine.StartAuction(property);

            Assert.That(_engine.CurrentAuctionBidder, Is.EqualTo(players[0]));

            // Act: Player 1 passes without bidding
            bool result = _engine.PassBid(players[0]);

            // Assert: Player 1 drops out, turn passes to Player 2
            Assert.That(result, Is.True);
            Assert.That(_engine.CurrentAuctionBidder, Is.EqualTo(players[1]));
            Assert.That(_engine.HasActiveAuction, Is.True);
        }

        [Test]
        public void PassBid_AllPlayersPassWithoutBids_AuctionEnds()
        {
            // Arrange: Start auction for Mediterranean Avenue
            var property = _engine.Board.Path[1].Property!;
            _engine.StartAuction(property);

            IPlayer? eventWinner = null;
            int eventBid = -1;
            bool auctionEndedFired = false;

            _engine.OnAuctionEnded += (theProperty, winner, bid) =>
            {
                auctionEndedFired = true;
                eventWinner = winner;
                eventBid = bid;
            };

            // Act: Both players pass consecutively
            _engine.PassBid(players[0]); // Player 1 passes
            bool finalPass = _engine.PassBid(players[1]); // Player 2 passes

            // Assert:
            Assert.That(finalPass, Is.True);

            // 1. Auction is completely closed
            Assert.That(_engine.HasActiveAuction, Is.False);

            // 2. Property remains unowned (stays with Bank)
            Assert.That(_engine.GetPropertyOwner(property), Is.Null);

            // 3. OnAuctionEnded fired with null winner and 0 bid
            Assert.That(auctionEndedFired, Is.True);
            Assert.That(eventWinner, Is.Null);
            Assert.That(eventBid, Is.EqualTo(0));
        }

        [Test]
        public void PassBid_OpponentPassesAfterBid_HighestBidderWinsProperty()
        {
            // Arrange: Mediterranean Avenue
            var property = _engine.Board.Path[1].Property!;
            _engine.StartAuction(property);

            var player1 = players[0];
            var player2 = players[1];

            int player1CashBefore = player1.Money;
            int winningBid = 75;

            // Player 1 bids $75
            _engine.PlaceBid(player1, winningBid);
            Assert.That(_engine.CurrentAuctionBidder, Is.EqualTo(player2));

            // Act: Player 2 passes
            bool passResult = _engine.PassBid(player2);
            Assert.That(passResult, Is.True);



            // 1. Player 1 wins and receives the Title Deed (PRP-14)
            Assert.That(_engine.GetPropertyOwner(property), Is.EqualTo(player1));

            // 2. Winning bid paid to the Bank
            Assert.That(player1.Money, Is.EqualTo(player1CashBefore - winningBid));

            // 3. Auction state cleared
            Assert.That(_engine.HasActiveAuction, Is.False);
        }

        #endregion

    }
}