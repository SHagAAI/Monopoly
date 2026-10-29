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

    public class TradeCard_Test
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

        // =========================================================================
        // 1. BASIC VALIDATIONS & GUARD CLAUSES
        // =========================================================================

        #region TradeCard



        [Test]
        public void TradeCard_NullArguments_ReturnFalse()
        {
            var buyer = players[0];
            var seller = players[1];

            Assert.That(_engine.TradeCard(null!, seller, 50), Is.False);
            Assert.That(_engine.TradeCard(buyer, null!, 50), Is.False);
        }

        [Test]
        public void TradeCard_TradeWithSelf_ReturnFalse()
        {
            var player = players[0];
            player.JailFreeCard = 1;

            // Act: Attempt to trade a card to yourself (buyer == seller)
            bool result = _engine.TradeCard(player, player, 50);

            // Assert
            Assert.That(result, Is.False);
            Assert.That(player.JailFreeCard, Is.EqualTo(1));
        }

        [Test]
        public void TradeCard_BankruptPlayer_ReturnFalse()
        {
            var buyer = players[0];
            var seller = players[1];
            seller.JailFreeCard = 1;

            // Buyer is bankrupt
            buyer.IsBankrupt = true;
            Assert.That(_engine.TradeCard(buyer, seller, 50), Is.False);

            // Seller is bankrupt
            buyer.IsBankrupt = false;
            seller.IsBankrupt = true;
            Assert.That(_engine.TradeCard(buyer, seller, 50), Is.False);
        }

        [Test]
        public void TradeCard_NegativeOffer_ReturnFalse()
        {
            var buyer = players[0];
            var seller = players[1];
            seller.JailFreeCard = 1;

            // Act: Offer is negative (-50)
            bool result = _engine.TradeCard(buyer, seller, offer: -50);

            // Assert
            Assert.That(result, Is.False);
            Assert.That(seller.JailFreeCard, Is.EqualTo(1));
            Assert.That(buyer.JailFreeCard, Is.EqualTo(0));
        }

        // =========================================================================
        // 2. INVENTORY & SOLVENCY VALIDATION
        // =========================================================================

        [Test]
        public void TradeCard_SellerHasNoJailCard_ReturnFalse()
        {
            var buyer = players[0];
            var seller = players[1];

            // Seller possesses 0 Get Out of Jail Free cards
            seller.JailFreeCard = 0;

            // Act: Attempt to buy a card the seller does not possess (CRD-06)
            bool result = _engine.TradeCard(buyer, seller, offer: 50);

            // Assert
            Assert.That(result, Is.False);
            Assert.That(buyer.JailFreeCard, Is.EqualTo(0));
        }

        [Test]
        public void TradeCard_BuyerInsufficientFunds_ReturnFalse()
        {
            var buyer = players[0];
            var seller = players[1];
            seller.JailFreeCard = 1;

            // Buyer only has $30 cash, but offer is $50
            buyer.Money = 30;

            // Act: Attempt to buy when buyer cannot afford the offer
            bool result = _engine.TradeCard(buyer, seller, offer: 50);

            // Assert: Must fail validation; no card or cash transferred
            Assert.That(result, Is.False);
            Assert.That(seller.JailFreeCard, Is.EqualTo(1));
            Assert.That(buyer.JailFreeCard, Is.EqualTo(0));
            Assert.That(buyer.Money, Is.EqualTo(30));
        }

        // =========================================================================
        // 3. HAPPY PATHS (TRADING WITH OFFER AND $0 GIFT)
        // =========================================================================

        [Test]
        public void TradeCard_ValidTradeWithOffer_TransfersCardAndCash()
        {
            // Arrange
            var buyer = players[0];
            var seller = players[1];
            seller.JailFreeCard = 1;
            buyer.JailFreeCard = 0;

            int buyerCashBefore = buyer.Money;
            int sellerCashBefore = seller.Money;
            int offer = 75;

            // Act: Buyer buys the card for $75
            bool result = _engine.TradeCard(buyer, seller, offer);

            // Assert:
            Assert.That(result, Is.True);
            // 1. Card transferred
            Assert.That(seller.JailFreeCard, Is.EqualTo(0));
            Assert.That(buyer.JailFreeCard, Is.EqualTo(1));
            // 2. Cash transferred
            Assert.That(buyer.Money, Is.EqualTo(buyerCashBefore - offer));
            Assert.That(seller.Money, Is.EqualTo(sellerCashBefore + offer));
        }

        [Test]
        public void TradeCard_ZeroOffer_TransfersCardWithoutCash()
        {
            // Arrange: Seller transfers card to buyer for $0 (free gift)
            var buyer = players[0];
            var seller = players[1];
            seller.JailFreeCard = 1;
            buyer.JailFreeCard = 0;

            int buyerCashBefore = buyer.Money;
            int sellerCashBefore = seller.Money;

            // Act: Trade for $0 offer
            bool result = _engine.TradeCard(buyer, seller, offer: 0);

            // Assert: Card is transferred, but neither player's balance changed
            Assert.That(result, Is.True);
            Assert.That(seller.JailFreeCard, Is.EqualTo(0));
            Assert.That(buyer.JailFreeCard, Is.EqualTo(1));
            Assert.That(buyer.Money, Is.EqualTo(buyerCashBefore));
            Assert.That(seller.Money, Is.EqualTo(sellerCashBefore));
        }

        #endregion

        #region ProposeCardTrade

        // =========================================================================
        // PROPOSE CARD TRADE UNIT TESTS
        // =========================================================================

        [Test]
        public void ProposeCardTrade_BuyerSellerNull_ReturnFalse()
        {
            var buyer = players[0];
            var seller = players[1];
            seller.JailFreeCard = 1;

            // Assert: Buyer null
            Assert.That(_engine.ProposeCardTrade(null!, seller, 50), Is.False);

            // Assert: Seller null
            Assert.That(_engine.ProposeCardTrade(buyer, null!, 50), Is.False);
        }

        [Test]
        public void ProposeCardTrade_BuyerIsSeller_ReturnFalse()
        {
            var player = players[0];
            player.JailFreeCard = 1;

            // Act: Attempt to propose a card trade to yourself
            bool result = _engine.ProposeCardTrade(player, player, 50);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void ProposeCardTrade_BankruptPlayer_ReturnFalse()
        {
            var buyer = players[0];
            var seller = players[1];
            seller.JailFreeCard = 1;

            // Case 1: Buyer is bankrupt
            buyer.IsBankrupt = true;
            Assert.That(_engine.ProposeCardTrade(buyer, seller, 50), Is.False);

            // Case 2: Seller is bankrupt
            buyer.IsBankrupt = false;
            seller.IsBankrupt = true;
            Assert.That(_engine.ProposeCardTrade(buyer, seller, 50), Is.False);
        }

        [Test]
        public void ProposeCardTrade_NegativeOffer_ReturnFalse()
        {
            var buyer = players[0];
            var seller = players[1];
            seller.JailFreeCard = 1;

            // Act: Offer is negative
            bool result = _engine.ProposeCardTrade(buyer, seller, offer: -10);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void ProposeCardTrade_SellerHasNoJailCard_ReturnFalse()
        {
            var buyer = players[0];
            var seller = players[1];

            // Seller has 0 Get Out of Jail Free cards
            seller.JailFreeCard = 0;

            // Act: Buyer proposes to buy a card seller doesn't have
            bool result = _engine.ProposeCardTrade(buyer, seller, offer: 50);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void ProposeCardTrade_BuyerInsufficientFunds_ReturnFalse()
        {
            var buyer = players[0];
            var seller = players[1];
            seller.JailFreeCard = 1;

            // Buyer only has $40 cash, but offer is $100
            buyer.Money = 40;

            // Act
            bool result = _engine.ProposeCardTrade(buyer, seller, offer: 100);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void ProposeCardTrade_ValidOffer_StoresPendingProposalAndFiresOnCardTradeOffered()
        {
            // Arrange
            var buyer = players[0];
            var seller = players[1];

            // seller has the jail free card
            seller.JailFreeCard = 1;


            buyer.Money = 500;

            IPlayer? eventBuyer = null;
            IPlayer? eventSeller = null;
            int eventOffer = 0;

            _engine.OnCardTradeOffered += (b, s, off) =>
            {
                eventBuyer = b;
                eventSeller = s;
                eventOffer = off;
            };

            // Act: Buyer proposes $75 to Seller for the card
            bool result = _engine.ProposeCardTrade(buyer, seller, offer: 75);

            // Assert:
            Assert.That(result, Is.True, "propose success");
            // Verify event payload was broadcasted correctly
            Assert.That(eventBuyer, Is.EqualTo(buyer));
            Assert.That(eventSeller, Is.EqualTo(seller));
            Assert.That(eventOffer, Is.EqualTo(75));
        }

        #endregion


        #region AcceptCardTrade

        // =========================================================================
        // ACCEPT CARD TRADE UNIT TESTS
        // =========================================================================

        [Test]
        public void AcceptCardTrade_NoPendingTrade_ReturnFalse()
        {
            var seller = players[1];

            // Act: Attempt to accept when no card trade proposal is active
            bool result = _engine.AcceptCardTrade(seller);

            // Assert: Must fail safely
            Assert.That(result, Is.False);
        }

        [Test]
        public void AcceptCardTrade_UnauthorizedCallerNotSeller_ReturnFalse()
        {
            // Arrange: Buyer proposes trade to Seller
            var buyer = players[0];
            var seller = players[1];

            seller.JailFreeCard = 1;
            buyer.Money = 500;

            _engine.ProposeCardTrade(buyer, seller, offer: 50);

            // Act: The buyer attempts to accept their own proposal
            bool result = _engine.AcceptCardTrade(buyer);

            // Assert: Authorization fails; card remains with seller
            Assert.That(result, Is.False);
            Assert.That(seller.JailFreeCard, Is.EqualTo(1));
            Assert.That(buyer.JailFreeCard, Is.EqualTo(0));
        }

        [Test]
        public void AcceptCardTrade_ValidSeller_ExecutesTradeTransfersCardAndCash_ReturnTrue()
        {
            // Arrange: Buyer proposes $80 for the seller's card
            var buyer = players[0];
            var seller = players[1];

            seller.JailFreeCard = 1;
            buyer.JailFreeCard = 0;

            int buyerCashBefore = buyer.Money;
            int sellerCashBefore = seller.Money;

            int offer = 50;

            _engine.ProposeCardTrade(buyer, seller, offer);

            // Act: Designated seller accepts the proposal
            bool result = _engine.AcceptCardTrade(seller);

            // Assert:
            Assert.That(result, Is.True);

            // 1. Card transferred
            Assert.That(seller.JailFreeCard, Is.EqualTo(0));
            Assert.That(buyer.JailFreeCard, Is.EqualTo(1));

            // 2. Cash transferred
            Assert.That(buyer.Money, Is.EqualTo(buyerCashBefore - offer));
            Assert.That(seller.Money, Is.EqualTo(sellerCashBefore + offer));
        }

        [Test]
        public void AcceptCardTrade_SecondCallAfterAcceptance_ReturnFalse()
        {
            // Arrange: Setup and accept a trade
            var buyer = players[0];
            var seller = players[1];
            seller.JailFreeCard = 1;

            _engine.ProposeCardTrade(buyer, seller, offer: 50);

            bool firstAccept = _engine.AcceptCardTrade(seller);
            Assert.That(firstAccept, Is.True);

            // Act: Attempt to accept the same trade a second time
            bool secondAccept = _engine.AcceptCardTrade(seller);

            // Assert: Must return false because the proposal was cleared from memory
            Assert.That(secondAccept, Is.False);
        }

        [Test]
        public void AcceptCardTrade_BuyerSpentMoneyBeforeSellerAccepts_FailsAndClearsProposal()
        {
            // Arrange: Buyer proposes $100 when they have cash
            var buyer = players[0];
            var seller = players[1];

            seller.JailFreeCard = 1;

            _engine.ProposeCardTrade(buyer, seller, offer: 100);

            // Buyer's cash drops before the seller accepts (e.g. paid rent or fine)
            buyer.Money = 20;

            // Act: Seller accepts, but buyer can no longer afford the $100 offer
            bool result = _engine.AcceptCardTrade(seller);

            // Assert: Trade execution aborts, seller keeps card, and pending offer is cleared
            Assert.That(result, Is.False);
            Assert.That(seller.JailFreeCard, Is.EqualTo(1));
            Assert.That(buyer.JailFreeCard, Is.EqualTo(0));
            Assert.That(buyer.Money, Is.EqualTo(20)); // Untouched
        }

        #endregion

        #region DeclineCardTrade

        // =========================================================================
        // DECLINE CARD TRADE UNIT TESTS
        // =========================================================================

        [Test]
        public void DeclineCardTrade_DesignatedSellerDeclines_ClearsProposalWithoutTransferringAssets()
        {
            // Arrange
            var buyer = players[0];
            var seller = players[1];

            seller.JailFreeCard = 1;
            buyer.JailFreeCard = 0;

            int buyerCashBefore = buyer.Money;
            int sellerCashBefore = seller.Money;

            _engine.ProposeCardTrade(buyer, seller, offer: 100);

            // Act: The designated seller declines the proposal
            _engine.DeclineCardTrade(seller);

            // Assert:
            // 1. Assets and money remain unchanged
            // jail card remains unchanged
            Assert.That(seller.JailFreeCard, Is.EqualTo(1));
            Assert.That(buyer.JailFreeCard, Is.EqualTo(0));

            Assert.That(buyer.Money, Is.EqualTo(buyerCashBefore));
            Assert.That(seller.Money, Is.EqualTo(sellerCashBefore));

            // 2. Proposal is cleared (subsequent accept fails)
            Assert.That(_engine.AcceptCardTrade(seller), Is.False);
        }

        [Test]
        public void DeclineCardTrade_UnauthorizedCallerDeclines_IgnoredAndProposalRemainsActive()
        {
            // Arrange: Buyer proposes trade to Seller
            var buyer = players[0];
            var seller = players[1];
            seller.JailFreeCard = 1;

            _engine.ProposeCardTrade(buyer, seller, offer: 100);

            // Act: Buyer attempts to decline instead of seller
            _engine.DeclineCardTrade(buyer);

            // Assert: Call is ignored, proposal remains active and seller can still accept
            bool acceptResult = _engine.AcceptCardTrade(seller);
            
            Assert.That(acceptResult, Is.True);
            Assert.That(buyer.JailFreeCard, Is.EqualTo(1));
            Assert.That(seller.JailFreeCard, Is.EqualTo(0));
        }

        #endregion

    }
}