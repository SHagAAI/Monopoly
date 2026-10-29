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
    public class RollDice
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

        #region Guard Clauses & Pre-Conditions

        // =========================================================================
        // 1. GUARD CLAUSES & PRE-CONDITIONS
        // =========================================================================

        [Test]
        public void RollandMoveChecking_GameNotStarted_DoesNotRollOrMove()
        {
            // Arrange: Recreate engine without calling StartGame()
            var unstartedEngine = new GameEngine(new Board(), new Dice());
            unstartedEngine.InitializeBoard();
            unstartedEngine.InitializeCardDecks();
            unstartedEngine.InitializePlayers(players, tokens);

            var player = players[0];

            // Act: Attempt to roll before game starts
            unstartedEngine.RollandMoveChecking();

            // Assert: Dice remain unrolled (0) and token remains at GO (Space 0)
            Assert.That(unstartedEngine.Dice.Side1, Is.EqualTo(0));
            Assert.That(unstartedEngine.Dice.Side2, Is.EqualTo(0));
            Assert.That(unstartedEngine.GetPlayerPosition(player), Is.EqualTo(unstartedEngine.Board.Path[0]));
        }

        [Test]
        public void RollandMoveChecking_PlayerHasPendingDebt_BlockedFromRolling()
        {
            // Arrange: Player 1 has an unresolved pending debt of $100 to the Bank
            var player1 = players[0];
            _engine.RecordDebt(player1, amount: 100, creditor: null);

            Assert.That(_engine.HasPendingDebt(), Is.True);
            Assert.That(_engine.CanRoll(), Is.False);

            // Act: Attempt to roll while in debt
            _engine.RollandMoveChecking();

            // Assert: Blocked! Dice remain unrolled and token remains at GO
            Assert.That(_engine.Dice.Side1, Is.EqualTo(0));
            Assert.That(_engine.Dice.Side2, Is.EqualTo(0));
            Assert.That(_engine.GetPlayerPosition(player1), Is.EqualTo(_engine.Board.Path[0]));
        }

        #endregion



        // =========================================================================
        // 2. DICE GENERATION, OBSERVER EVENT, & MOVEMENT
        // =========================================================================

        [Test]
        public void RollandMoveChecking_ValidRoll_DiceSidesWithinValidRange()
        {
            // Act: Current player rolls and moves
            _engine.RollandMoveChecking();

            // Assert: Both 6-sided dice must produce integers between 1 and 6 inclusive
            Assert.That(_engine.Dice.Side1, Is.InRange(1, 6));
            Assert.That(_engine.Dice.Side2, Is.InRange(1, 6));

            int total = _engine.Dice.Side1 + _engine.Dice.Side2;
            Assert.That(total, Is.InRange(2, 12));
        }

        [Test]
        public void RollandMoveChecking_ValidRoll_FiresOnDiceRolledEvent()
        {
            // Arrange: Subscribe to the dice roll event
            var player1 = players[0];
            IPlayer? eventPlayer = null;
            int eventSide1 = 0;
            int eventSide2 = 0;

            _engine.OnDiceRolled += (p, s1, s2) =>
            {
                eventPlayer = p;
                eventSide1 = s1;
                eventSide2 = s2;
            };

            // Act: Roll
            _engine.RollandMoveChecking();

            // Assert: Observer received the exact player and dice results
            Assert.That(eventPlayer, Is.EqualTo(player1));
            Assert.That(eventSide1, Is.EqualTo(_engine.Dice.Side1));
            Assert.That(eventSide2, Is.EqualTo(_engine.Dice.Side2));
        }

        [Test]
        public void RollandMoveChecking_ValidRoll_AdvancesPlayerTokenByDiceSum()
        {
            // Arrange: Player starts at Space 0 (GO)
            var player1 = players[0];
            Assert.That(_engine.GetPlayerPosition(player1), Is.EqualTo(_engine.Board.Path[0]));

            // Act: Roll and advance
            _engine.RollandMoveChecking();

            int expectedSteps = _engine.Dice.Side1 + _engine.Dice.Side2;
            var expectedSpace = _engine.Board.Path[expectedSteps];

            // Assert: Token landed exactly on Space[Dice.Side1 + Dice.Side2]
            Assert.That(_engine.GetPlayerPosition(player1), Is.EqualTo(expectedSpace));
        }

        [Test]
        public void RollandMoveChecking_ValidRoll_ResetsLastDrawnCardToNull()
        {
            // Arrange: Simulate a previously drawn card stored in engine
            _engine.DrawCard(players[0], CardType.CommunityChest);
            Assert.That(_engine.LastDrawnCard, Is.Not.Null);

            // Act: New turn roll begins
            _engine.RollandMoveChecking();

            // Assert: Engine wipes previous turn's card state
            Assert.That(_engine.LastDrawnCard, Is.Null);
        }

        // =========================================================================
        // 3. TURN RESTRICTIONS
        // =========================================================================

        [Test]
        public void RollandMoveChecking_NonDoublesRoll_CannotRollAgainInSameTurn()
        {
            // Arrange: Current player rolls
            var player1 = players[0];
            _engine.RollandMoveChecking();

            // If the roll was NOT doubles, the player is not allowed to roll again
            if (!_engine.CanRollAgain())
            {
                Assert.That(_engine.CanRoll(), Is.False);

                var spaceAfterFirstRoll = _engine.GetPlayerPosition(player1);

                // Act: Attempt to roll again on the same turn
                _engine.RollandMoveChecking();

                // Assert: Position does not move
                Assert.That(_engine.GetPlayerPosition(player1), Is.EqualTo(spaceAfterFirstRoll));
            }
        }

        [Test]
        public void RollandMoveChecking_StandingOnUnresolvedProperty_CannotRollAgain()
        {
            // Arrange: Move Player 1 to Space 1 (Mediterranean Avenue - Unowned)
            var player1 = players[0];
            var token1 = _engine.GetPlayerToken(player1)!;
            _engine.Move(token1, steps: 1);

            // Assert: Player has unresolved property underfoot (must buy or auction first)
            Assert.That(_engine.HasUnresolvedPropertyUnderfoot(), Is.True);
            Assert.That(_engine.CanRoll(), Is.False);

            // Act: Attempt to roll while standing on unbought/unauctioned property
            _engine.RollandMoveChecking();

            // Assert: Player is BLOCKED from moving further until decision is resolved
            Assert.That(_engine.GetPlayerPosition(player1), Is.EqualTo(_engine.Board.Path[1]));
        }

        #region EndTurn & NextTurn

        // =========================================================================
        // END TURN & NEXT TURN UNIT TESTS
        // =========================================================================

        [Test]
        public void EndTurn_PlayerHasPendingDebt_BlockedFromEndingTurn()
        {
            // Arrange: Player 1 has an unresolved debt of $50
            var player1 = players[0];
            _engine.RecordDebt(player1, amount: 50, creditor: null);

            Assert.That(_engine.HasPendingDebt(), Is.True);
            Assert.That(_engine.GetCurrentPlayer(), Is.EqualTo(player1));

            // Act: Attempt to end turn while in debt
            _engine.EndTurn();

            // Assert: Turn does NOT end; Player 1 remains the current player
            Assert.That(_engine.GetCurrentPlayer(), Is.EqualTo(player1));
        }

        [Test]
        public void EndTurn_ValidCall_ResetsRollStateAndAdvancesToNextPlayer()
        {
            // Arrange: Player 1 rolls first
            var player1 = players[0];
            var player2 = players[1];

            _engine.RollandMoveChecking();

            // Act: Player 1 ends their turn
            _engine.EndTurn();

            // Assert:
            // 1. Current player advances to Player 2
            Assert.That(_engine.GetCurrentPlayer(), Is.EqualTo(player2));

            // 2. Roll state is reset (_hasRolled = false), allowing Player 2 to roll
            Assert.That(_engine.CanRoll(), Is.True);

            // 3. Card state is wiped
            Assert.That(_engine.LastDrawnCard, Is.Null);
        }

        // [Test]
        // public void EndTurn_TwoConsecutiveEndTurns_WrapsAroundBackToFirstPlayer()
        // {
        //     // Arrange: Start with Player 1 (Ant)
        //     Assert.That(_engine.GetCurrentPlayer(), Is.EqualTo(players[0]));

        //     // Act 1: Player 1 ends turn -> moves to Player 2 (Thonny)
        //     _engine.EndTurn();
        //     Assert.That(_engine.GetCurrentPlayer(), Is.EqualTo(players[1]));

        //     // Act 2: Player 2 ends turn -> wraps back around to Player 1 (Ant)
        //     _engine.EndTurn();

        //     // Assert
        //     Assert.That(_engine.GetCurrentPlayer(), Is.EqualTo(players[0]));
        // }

        [Test]
        public void NextTurn_GameNotInProgress_ReturnsFalse()
        {
            // Arrange: Game engine created but not started
            var unstartedEngine = new GameEngine(new Board(), new Dice());
            unstartedEngine.InitializeBoard();
            unstartedEngine.InitializePlayers(players, tokens);

            // Act: Attempt to advance turn when game is NotStarted
            bool result = unstartedEngine.NextTurn();

            // Assert: Must return false safely
            Assert.That(result, Is.False);
        }

        [Test]
        public void NextTurn_ValidCall_AdvancesNewTurnPlayer()
        {
            // Arrange: Subscribe to OnTurnChanged event
            var player2 = players[1];
            IPlayer? eventNewPlayer = null;

            _engine.OnTurnChanged += (newPlayer) =>
            {
                eventNewPlayer = newPlayer;
            };

            // Act: Advance turn
            bool result = _engine.NextTurn();

            // Assert:
            Assert.That(result, Is.True);
            Assert.That(_engine.GetCurrentPlayer(), Is.EqualTo(player2));
            // Observer was notified with Player 2
            Assert.That(eventNewPlayer, Is.EqualTo(player2));
        }

        [Test]
        public void NextTurn_NextPlayerIsBankrupt_SkipsBankruptPlayer()
        {
            // Arrange: Setup a 3-player game to test skipping
            var threePlayers = new List<IPlayer>
            {
                new Player(id: 1, name: "Ant"),
                new Player(id: 2, name: "Thonny"),
                new Player(id: 3, name: "Bella")
            };
            var threeTokens = new List<IToken>
            {
                new Token(color: TokenColor.Red),
                new Token(color: TokenColor.Blue),
                new Token(color: TokenColor.Green)
            };

            var testEngine = new GameEngine(new Board(), new Dice());
            testEngine.InitializeBoard();
            testEngine.InitializeCardDecks();
            testEngine.InitializePlayers(threePlayers, threeTokens);
            testEngine.StartGame();

            // Player 2 (Thonny) goes bankrupt!
            threePlayers[1].IsBankrupt = true;

            // Currently Player 1's turn
            Assert.That(testEngine.GetCurrentPlayer(), Is.EqualTo(threePlayers[0]));

            // Act: Call NextTurn
            bool result = testEngine.NextTurn();

            // Assert: Skips bankrupt Player 2 (Thonny) and jumps straight to Player 3 (Bella)!
            Assert.That(result, Is.True);
            Assert.That(testEngine.GetCurrentPlayer(), Is.EqualTo(threePlayers[2]));
        }

        #endregion
    }
}