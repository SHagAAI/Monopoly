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
    public class TradeProperty_Test
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

        #region TradeProperty



        // =========================================================================
        // 1. BASIC VALIDATIONS & GUARD CLAUSES
        // =========================================================================

        [Test]
        public void TradeProperty_NullArguments_ReturnFalse()
        {
            var buyer = players[0];
            var seller = players[1];
            var properties = new List<IProperty> { _engine.Board.Path[1].Property! };

            // buyer null
            Assert.That(_engine.TradeProperty(null!, seller, properties, 100), Is.False, "buyer cant be null");

            // seller null
            Assert.That(_engine.TradeProperty(buyer, null!, properties, 100), Is.False);

            // properties null
            Assert.That(_engine.TradeProperty(buyer, seller, null!, 100), Is.False);
        }

        [Test]
        public void TradeProperty_TradeWithSelf_ReturnFalse()
        {
            // Arrange
            var player = players[0];
            var properties = new List<IProperty> { _engine.Board.Path[1].Property! };

            // Act: Attempt to trade with yourself (buyer == seller)
            bool result = _engine.TradeProperty(player, player, properties, 100);

            // Assert
            Assert.That(result, Is.False, "cant trade with yourself");
        }

        [Test]
        public void TradeProperty_BankruptPlayer_ReturnFalse()
        {
            var buyer = players[0];
            var seller = players[1];
            var properties = new List<IProperty> { _engine.Board.Path[1].Property! };

            buyer.IsBankrupt = true;
            Assert.That(_engine.TradeProperty(buyer, seller, properties, 100), Is.False, "cant traade if bankrupt");

            buyer.IsBankrupt = false;
            seller.IsBankrupt = true;
            Assert.That(_engine.TradeProperty(buyer, seller, properties, 100), Is.False, "cant traade if bankrupt");
        }

        [Test]
        public void TradeProperty_EmptyPropertiesOrNegativeOffer_ReturnFalse()
        {
            var buyer = players[0];
            var seller = players[1];
            var property = _engine.Board.Path[1].Property!;

            // Empty list
            Assert.That(_engine.TradeProperty(buyer, seller, new List<IProperty>(), 100), Is.False);

            // Negative offer
            Assert.That(_engine.TradeProperty(buyer, seller, new List<IProperty> { property }, -50), Is.False);
        }



        // =========================================================================
        // 2. FINANCIAL & OWNERSHIP VALIDATION
        // =========================================================================

        [Test]
        public void TradeProperty_BuyerCannotAffordOffer_ReturnFalse()
        {
            // Arrange: Buyer only has $50, but offer is $100
            var buyer = players[0];
            var seller = players[1];

            var token2 = _engine.GetPlayerToken(seller)!;

            var mediterranean = _engine.Board.Path[1].Property!;

            _engine.Move(token2, steps: 1);
            _engine.BuyProperty(seller, mediterranean);

            buyer.Money = 50;

            // Act: Buyer offers $100 while having only $50
            bool result = _engine.TradeProperty(buyer, seller, new List<IProperty> { mediterranean }, offer: 100);

            // Assert
            Assert.That(result, Is.False);
            Assert.That(_engine.GetPropertyOwner(mediterranean), Is.EqualTo(seller));
            Assert.That(buyer.Money, Is.EqualTo(50));
        }

        [Test]
        public void TradeProperty_SellerDoesNotOwnAllProperties_ReturnFalse()
        {
            // Arrange: Seller owns Mediterranean, but NOT Baltic (Bank owns Baltic)
            var buyer = players[0];
            var seller = players[1];

            var token2 = _engine.GetPlayerToken(seller)!;

            var mediterranean = _engine.Board.Path[1].Property!;
            var baltic = _engine.Board.Path[3].Property!;

            _engine.Move(token2, steps: 1);
            _engine.BuyProperty(seller, mediterranean);

            var tradeList = new List<IProperty> { mediterranean, baltic };

            // Act: Attempt to trade a list containing a property seller does not own
            bool result = _engine.TradeProperty(buyer, seller, tradeList, offer: 100);

            // Assert: Must fail validation
            Assert.That(result, Is.False, "you not own all of that property");
            Assert.That(_engine.GetPropertyOwner(mediterranean), Is.EqualTo(seller));
        }

        [Test]
        public void TradeProperty_ColorGroupHasBuildings_ReturnFalse()
        {
            // Arrange: Seller owns both Brown properties and builds a house on Mediterranean
            var buyer = players[0];
            var seller = players[1];

            var token2 = _engine.GetPlayerToken(seller)!;

            var mediterranean = _engine.Board.Path[1].Property!;
            var baltic = _engine.Board.Path[3].Property!;

            _engine.Move(token2, steps: 1);
            _engine.BuyProperty(seller, mediterranean);

            _engine.Move(token2, steps: 2);
            _engine.BuyProperty(seller, baltic);

            // build a house inside 1 property
            _engine.BuyHouse(seller, mediterranean);
            Assert.That(mediterranean.BuildingLevel, Is.EqualTo(1));

            // Act: Attempt to trade Baltic while Mediterranean has a house
            bool result = _engine.TradeProperty(buyer, seller, new List<IProperty> { baltic }, offer: 100);


            Assert.That(result, Is.False, "Cannot trade streets if color group has buildings");
        }

        // =========================================================================
        // 3. HAPPY PATHS (TRADING WITH OFFER, $0 OFFER, AND MORTGAGED DEEDS)
        // =========================================================================

        [Test]
        public void TradeProperty_ValidTradeWithOffer_TransfersCashAndOwnership()
        {
            // Arrange: Seller buys Mediterranean Avenue
            var buyer = players[0];
            var seller = players[1];

            var token2 = _engine.GetPlayerToken(seller)!;
            var property = _engine.Board.Path[1].Property!;

            _engine.Move(token2, steps: 1);
            _engine.BuyProperty(seller, property);

            int buyerCashBefore = buyer.Money;
            int sellerCashBefore = seller.Money;
            int offer = 200;

            // Act: Buyer pays $200 for Mediterranean Avenue
            bool result = _engine.TradeProperty(buyer, seller, new List<IProperty> { property }, offer);

            // Assert:
            Assert.That(result, Is.True, "trade success");

            // 1. Ownership transferred
            Assert.That(_engine.GetPropertyOwner(property), Is.EqualTo(buyer));

            // 2. Cash transferred
            Assert.That(buyer.Money, Is.EqualTo(buyerCashBefore - offer));
            Assert.That(seller.Money, Is.EqualTo(sellerCashBefore + offer));
        }

        [Test]
        public void TradeProperty_ZeroOffer_TransfersOwnershipWithoutMoney()
        {
            // Arrange: Seller transfers property to buyer for $0 (free gift or part of a deal)
            var buyer = players[0];
            var seller = players[1];

            var token2 = _engine.GetPlayerToken(seller)!;
            var property = _engine.Board.Path[1].Property!;

            _engine.Move(token2, steps: 1);
            _engine.BuyProperty(seller, property);

            int buyerCashBefore = buyer.Money;
            int sellerCashBefore = seller.Money;

            // Act: Trade for $0 offer
            bool result = _engine.TradeProperty(buyer, seller, new List<IProperty> { property }, offer: 0);

            // Assert: Ownership transferred, but neither player's balance changed
            Assert.That(result, Is.True);
            Assert.That(_engine.GetPropertyOwner(property), Is.EqualTo(buyer));
            Assert.That(buyer.Money, Is.EqualTo(buyerCashBefore));
            Assert.That(seller.Money, Is.EqualTo(sellerCashBefore));
        }

        [Test]
        public void TradeProperty_MortgagedProperty_PreservesMortgageStatusForBuyer()
        {
            // Arrange: Seller buys and mortgages Mediterranean Avenue
            var buyer = players[0];
            var seller = players[1];

            var token2 = _engine.GetPlayerToken(seller)!;
            var property = _engine.Board.Path[1].Property!;

            _engine.Move(token2, steps: 1);
            _engine.BuyProperty(seller, property);

            _engine.HandleMortgage(seller, property);
            Assert.That(property.IsMortgaged, Is.True);

            // Act: Buyer acquires the mortgaged property
            bool result = _engine.TradeProperty(buyer, seller, new List<IProperty> { property }, offer: 50);

            // Assert:
            Assert.That(result, Is.True);
            Assert.That(_engine.GetPropertyOwner(property), Is.EqualTo(buyer));
            // Mortgaged status stays intact; buyer inherits the mortgage!
            Assert.That(property.IsMortgaged, Is.True);
        }

        #endregion

        #region ProposeTrade

        // =========================================================================
        // PROPOSE TRADE UNIT TESTS
        // =========================================================================

        [Test]
        public void ProposeTrade_NullArguments_ReturnFalse()
        {
            var buyer = players[0];
            var seller = players[1];
            var properties = new List<IProperty> { _engine.Board.Path[1].Property! };

            // buyer null
            Assert.That(_engine.ProposeTrade(null!, seller, properties, 100), Is.False);

            // seller null
            Assert.That(_engine.ProposeTrade(buyer, null!, properties, 100), Is.False);

            // property null
            Assert.That(_engine.ProposeTrade(buyer, seller, null!, 100), Is.False);
        }

        [Test]
        public void ProposeTrade_BuyerEqualsSeller_ReturnFalse()
        {
            // Arrange
            var player = players[0];
            var properties = new List<IProperty> { _engine.Board.Path[1].Property! };

            // Act: Attempt to propose a trade to yourself (buyer == seller)
            bool result = _engine.ProposeTrade(player, player, properties, 100);

            // Assert
            Assert.That(result, Is.False, "buyer and seller cant be the same person");
        }

        [Test]
        public void ProposeTrade_BuyerOrSellerBankrupt_ReturnFalse()
        {
            var buyer = players[0];
            var seller = players[1];
            var properties = new List<IProperty> { _engine.Board.Path[1].Property! };

            // Buyer is bankrupt
            buyer.IsBankrupt = true;
            Assert.That(_engine.ProposeTrade(buyer, seller, properties, 100), Is.False);

            // Seller is bankrupt
            buyer.IsBankrupt = false;
            seller.IsBankrupt = true;
            Assert.That(_engine.ProposeTrade(buyer, seller, properties, 100), Is.False);
        }

        [Test]
        public void ProposeTrade_EmptyPropertiesOrNegativeOffer_ReturnFalse()
        {
            var buyer = players[0];
            var seller = players[1];


            var property = _engine.Board.Path[1].Property!;

            // Empty property list
            Assert.That(_engine.ProposeTrade(buyer, seller, new List<IProperty>(), 100), Is.False);

            // Negative cash offer
            Assert.That(_engine.ProposeTrade(buyer, seller, new List<IProperty> { property }, -50), Is.False);
        }

        [Test]
        public void ProposeTrade_BuyerInsufficientFunds_ReturnFalse()
        {
            // Arrange: Buyer only has $50 cash, but offers $100
            var buyer = players[0];
            var seller = players[1];

            var token2 = _engine.GetPlayerToken(seller)!;
            var property = _engine.Board.Path[1].Property!;

            _engine.Move(token2, steps: 1);
            _engine.BuyProperty(seller, property);

            buyer.Money = 50;

            // Act: Propose trade where offer exceeds buyer's funds
            bool result = _engine.ProposeTrade(buyer, seller, new List<IProperty> { property }, offer: 100);

            // Assert: Must return false and reject the proposal
            Assert.That(result, Is.False, "buyer has not enough money");
        }

        [Test]
        public void ProposeTrade_SellerDoesNotOwnAllProperties_ReturnFalse()
        {
            // Arrange: Seller owns Mediterranean, but NOT Baltic
            var buyer = players[0];
            var seller = players[1];

            var token2 = _engine.GetPlayerToken(seller)!;

            var mediterranean = _engine.Board.Path[1].Property!;
            var baltic = _engine.Board.Path[3].Property!;

            _engine.Move(token2, steps: 1);
            _engine.BuyProperty(seller, mediterranean);

            var tradeList = new List<IProperty> { mediterranean, baltic };

            // Act: Attempt to propose a trade containing a property seller does not own
            bool result = _engine.ProposeTrade(buyer, seller, tradeList, offer: 100);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void ProposeTrade_ColorGroupHasBuildings_ReturnFalse()
        {
            // Arrange: Seller owns Brown group and has built a house on Mediterranean
            var buyer = players[0];
            var seller = players[1];

            var token2 = _engine.GetPlayerToken(seller)!;

            var mediterranean = _engine.Board.Path[1].Property!;
            var baltic = _engine.Board.Path[3].Property!;

            _engine.Move(token2, steps: 1);
            _engine.BuyProperty(seller, mediterranean);

            _engine.Move(token2, steps: 2);
            _engine.BuyProperty(seller, baltic);

            _engine.BuyHouse(seller, mediterranean);
            Assert.That(mediterranean.BuildingLevel, Is.EqualTo(1));

            // Act: Attempt to propose trading Baltic while Mediterranean has a house
            bool result = _engine.ProposeTrade(buyer, seller, new List<IProperty> { baltic }, offer: 100);

            // Assert: PRP-31: Cannot trade streets if any property in color group has buildings
            Assert.That(result, Is.False);
        }

        [Test]
        public void ProposeTrade_ValidProposal_StoresPendingTradeAndFiresEvent_ReturnTrue()
        {
            // Arrange: Seller legitimately owns Mediterranean Avenue
            var buyer = players[0];
            var seller = players[1];

            var token2 = _engine.GetPlayerToken(seller)!;
            var property = _engine.Board.Path[1].Property!;

            _engine.Move(token2, steps: 1);
            _engine.BuyProperty(seller, property);

            IPlayer? eventBuyer = null;
            IPlayer? eventSeller = null;
            List<IProperty>? eventProps = null;
            int eventOffer = 0;

            _engine.OnTradeOffered += (b, s, props, off) =>
            {
                eventBuyer = b;
                eventSeller = s;
                eventProps = props;
                eventOffer = off;
            };

            var tradeList = new List<IProperty> { property };

            // Act: Buyer proposes $150
            bool result = _engine.ProposeTrade(buyer, seller, tradeList, offer: 150);

            // Assert:
            Assert.That(result, Is.True, "Trade success");

            // Verify event payload was broadcast to observers
            Assert.That(eventBuyer, Is.EqualTo(buyer));
            Assert.That(eventSeller, Is.EqualTo(seller));
            Assert.That(eventProps, Is.EqualTo(tradeList));
            Assert.That(eventOffer, Is.EqualTo(150));
        }

        #endregion

        #region Accept Trade

        // =========================================================================
        // ACCEPT TRADE UNIT TESTS
        // =========================================================================

        [Test]
        public void AcceptTrade_NoPendingTrade_ReturnFalse()
        {
            // Arrange: No trade proposal was ever initiated
            var seller = players[1];

            // Act: Attempt to accept when no trade is active
            bool result = _engine.AcceptTrade(seller);

            // Assert: Must return false safely
            Assert.That(result, Is.False);
        }

        [Test]
        public void AcceptTrade_CallerIsNotDesignatedSeller_ReturnFalse()
        {
            // Arrange: Buyer (Player 1) proposes trade to Seller (Player 2)
            var buyer = players[0];
            var seller = players[1];

            var token2 = _engine.GetPlayerToken(seller)!;
            var property = _engine.Board.Path[1].Property!;

            _engine.Move(token2, steps: 1);
            _engine.BuyProperty(seller, property);

            _engine.ProposeTrade(buyer, seller, new List<IProperty> { property }, offer: 100);

            // Act: The BUYER attempts to accept their own proposed trade
            bool result = _engine.AcceptTrade(buyer);

            // Assert: Must fail authorization check; deed remains with seller
            Assert.That(result, Is.False);
            Assert.That(_engine.GetPropertyOwner(property), Is.EqualTo(seller));
        }

        [Test]
        public void AcceptTrade_ValidSeller_ExecutesTradeAndClearsPendingOffer_ReturnTrue()
        {
            // Arrange: Buyer proposes $150 for Mediterranean Avenue
            var buyer = players[0];
            var seller = players[1];

            var token2 = _engine.GetPlayerToken(seller)!;
            var property = _engine.Board.Path[1].Property!;

            _engine.Move(token2, steps: 1);
            _engine.BuyProperty(seller, property);

            int buyerCashBefore = buyer.Money;
            int sellerCashBefore = seller.Money;
            int offer = 150;

            _engine.ProposeTrade(buyer, seller, new List<IProperty> { property }, offer);

            // Act: The designated seller accepts the trade
            bool result = _engine.AcceptTrade(seller);

            // Assert:
            Assert.That(result, Is.True);

            // 1. Ownership transferred to buyer
            Assert.That(_engine.GetPropertyOwner(property), Is.EqualTo(buyer));

            // 2. Money transferred from buyer to seller
            Assert.That(buyer.Money, Is.EqualTo(buyerCashBefore - offer));
            Assert.That(seller.Money, Is.EqualTo(sellerCashBefore + offer));
        }

        [Test]
        public void AcceptTrade_SecondCallAfterAcceptance_ReturnFalse()
        {
            // Arrange: Setup and accept a trade
            var buyer = players[0];
            var seller = players[1];

            var token2 = _engine.GetPlayerToken(seller)!;
            var property = _engine.Board.Path[1].Property!;

            _engine.Move(token2, steps: 1);
            _engine.BuyProperty(seller, property);

            _engine.ProposeTrade(buyer, seller, new List<IProperty> { property }, offer: 100);

            // First accept succeeds
            bool firstAccept = _engine.AcceptTrade(seller);
            Assert.That(firstAccept, Is.True);

            // Act: Attempt to accept again immediately
            bool secondAccept = _engine.AcceptTrade(seller);

            // Assert: Must return false because the pending proposal was cleared from memory
            Assert.That(secondAccept, Is.False, "Trade is done already");
        }

        [Test]
        public void AcceptTrade_BuyerSpentMoneyBeforeSellerAccepts_FailsAndClearsProposal()
        {
            // Arrange: Buyer proposes $100 when they have $1500
            var buyer = players[0];
            var seller = players[1];

            var token2 = _engine.GetPlayerToken(seller)!;
            var property = _engine.Board.Path[1].Property!;

            _engine.Move(token2, steps: 1);
            _engine.BuyProperty(seller, property);

            _engine.ProposeTrade(buyer, seller, new List<IProperty> { property }, offer: 100);

            // Sometime after the proposal, buyer loses/spends money and only has $20 left
            buyer.Money = 20;

            // Act: Seller accepts, but buyer can no longer afford the $100 offer
            bool result = _engine.AcceptTrade(seller);

            // Assert: Trade execution aborts, seller keeps property, and pending offer is cleared
            Assert.That(result, Is.False, "not enough money");
            Assert.That(_engine.GetPropertyOwner(property), Is.EqualTo(seller));
            Assert.That(buyer.Money, Is.EqualTo(20)); // Untouched
        }

        #endregion

        #region DeclineTrade

        // =========================================================================
        // DECLINE TRADE UNIT TESTS
        // =========================================================================

        [Test]
        public void DeclineTrade_DesignatedSellerDeclines_ClearsProposalWithoutTransferringAssets()
        {
            // Arrange: Buyer proposes $150 for Mediterranean Avenue
            var buyer = players[0];
            var seller = players[1];

            var token2 = _engine.GetPlayerToken(seller)!;
            var property = _engine.Board.Path[1].Property!;

            _engine.Move(token2, steps: 1);
            _engine.BuyProperty(seller, property);

            int buyerCashBefore = buyer.Money;
            int sellerCashBefore = seller.Money;

            _engine.ProposeTrade(buyer, seller, new List<IProperty> { property }, offer: 150);

            // Act: The designated seller declines the proposed trade
            _engine.DeclineTrade(seller);

            // Assert:
            // 1. Assets and money remain completely unchanged
            Assert.That(_engine.GetPropertyOwner(property), Is.EqualTo(seller));
            Assert.That(buyer.Money, Is.EqualTo(buyerCashBefore));
            Assert.That(seller.Money, Is.EqualTo(sellerCashBefore));

            // 2. The proposal is wiped from memory (subsequent accept fails)
            Assert.That(_engine.AcceptTrade(seller), Is.False);
        }

        [Test]
        public void DeclineTrade_UnauthorizedPlayerAttemptsToDecline_IgnoredAndProposalRemainsActive()
        {
            // Arrange: Buyer proposes $150 to Seller (Player 2)
            var buyer = players[0];
            var seller = players[1];

            var token2 = _engine.GetPlayerToken(seller)!;
            var property = _engine.Board.Path[1].Property!;

            _engine.Move(token2, steps: 1);
            _engine.BuyProperty(seller, property);

            _engine.ProposeTrade(buyer, seller, new List<IProperty> { property }, offer: 150);

            // Act: An unauthorized player (the BUYER) attempts to decline the trade
            _engine.DeclineTrade(buyer);

            // Assert:
            // The decline call was ignored because buyer != _tradeSeller.
            // The designated seller can still legitimately accept the trade!
            bool acceptResult = _engine.AcceptTrade(seller);
            
            Assert.That(acceptResult, Is.True);
            Assert.That(_engine.GetPropertyOwner(property), Is.EqualTo(buyer));
        }

        [Test]
        public void DeclineTrade_NoPendingTrade_DoesNothingSafely()
        {
            // Arrange: No trade proposal was ever initiated
            var player = players[0];

            // Act & Assert: Must return immediately without throwing an exception
            Assert.DoesNotThrow(() => _engine.DeclineTrade(player));
        }

        #endregion
    }
}