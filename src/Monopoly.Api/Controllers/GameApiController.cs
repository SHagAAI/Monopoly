using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Monopoly.Api.Dtos;
using Monopoly.Api.Services;
using Monopoly.Core.Enums;
using Monopoly.Core.Interfaces;
using Monopoly.Core.Models;

namespace Monopoly.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GameApiController : ControllerBase
    {
        private readonly GameService _gameService;

        public GameApiController(GameService gameService)
        {
            _gameService = gameService;
        }

        [HttpPost("start")]
        public IActionResult StartGame([FromBody] StartGameRequest request)
        {
            if (request.Players.Count < 2 || request.Players.Count > 6)
            {
                return BadRequest("Must have 2 to 6 players.");
            }
            var players = new List<IPlayer>();
            var tokens = new List<IToken>();
            // foreach (var p in request.Players)
            // {
            //     players.Add(new Player(p.Name));
            //     tokens.Add(new Token(p.Color));
            // }

            for (int i = 0; i < request.Players.Count; i++)
            {
                var p = request.Players[i];
                players.Add(new Player(i + 1, p.Name)); // Id: 1, 2, ...
                tokens.Add(new Token(p.Color));
            }
            _gameService.StartNewGame(players, tokens);
            return Ok(GetState());
        }

        [HttpGet("state")]
        public IActionResult GetCurrentState()
        {
            if (_gameService.Engine == null) return NotFound("Game has not started yet.");
            return Ok(GetState());
        }

        [HttpPost("roll")]
        public IActionResult RollDice()
        {
            if (_gameService.Engine == null) return NotFound("Game has not started.");
            if (!_gameService.Engine.CanRoll()) return BadRequest("Cannot roll right now. Please end your turn or resolve debts.");
            _gameService.Engine.RollDice();
            return Ok(GetState());
        }

        [HttpPost("end-turn")]
        public IActionResult EndTurn()
        {
            if (_gameService.Engine == null) return NotFound("Game has not started.");
            if (!_gameService.Engine.CanEndTurn()) return BadRequest("Cannot end turn. You must roll first or roll again for doubles.");

            _gameService.Engine.EndTurn();
            return Ok(GetState());
        }

        [HttpPost("buy-property")]
        public IActionResult BuyProperty()
        {
            var engine = _gameService.Engine;
            if (engine == null) return NotFound();

            var player = engine.GetCurrentPlayer();
            // var space = engine.Board.Path.FirstOrDefault(s => s.Tokens.Any(t => t.Color == engine.GetPlayerToken(player).Color));
            var space = engine.GetPlayerPosition(player);

            if (space?.Property == null) return BadRequest("Not standing on a property.");

            bool bought = engine.BuyProperty(player, space.Property);
            return bought ? Ok(GetState()) : BadRequest("Cannot buy property.");
        }

        [HttpPost("pay-debt")]
        public IActionResult PayDebt()
        {
            var engine = _gameService.Engine;
            if (engine == null) return NotFound();

            bool paid = engine.PayPendingDebt(engine.GetCurrentPlayer());
            return paid ? Ok(GetState()) : BadRequest("Not enough money to clear debt.");
        }


        /// =========================================================================================
        /// ------------------------------------ JAIL SECTION ---------------------------------------
        /// =========================================================================================
        /// 


        [HttpPost("jail/pay")]
        public IActionResult PayJailFine()
        {
            var engine = _gameService.Engine;
            if (engine == null) return NotFound("Game has not started.");
            var player = engine.GetCurrentPlayer();
            if (!player.IsInJail) return BadRequest("Player is not in Jail.");
            bool freed = engine.HandleGetOutOfJail(player, useCard: false);
            return freed ? Ok(GetState()) : BadRequest("Cannot pay $50 jail fine. Not enough money.");
        }

        [HttpPost("jail/card")]
        public IActionResult UseJailCard()
        {
            var engine = _gameService.Engine;
            if (engine == null) return NotFound("Game has not started.");

            var player = engine.GetCurrentPlayer();
            if (!player.IsInJail) return BadRequest("Player is not in Jail.");

            if (player.JailFreeCard <= 0) return BadRequest("Player does not own a Get Out of Jail Free card.");
            bool freed = engine.HandleGetOutOfJail(player, useCard: true);

            return freed ? Ok(GetState()) : BadRequest("Failed to use card.");
        }

        /// =========================================================================================
        /// ---------------------------------------- BANKRUPT ---------------------------------------
        /// =========================================================================================

        [HttpPost("bankrupt")]
        public IActionResult DeclareBankruptcy()
        {
            var engine = _gameService.Engine;
            if (engine == null) return NotFound("Game has not started.");

            var player = engine.GetCurrentPlayer();
            if (player.IsBankrupt) return BadRequest("Player is already bankrupt.");

            bool success = engine.HandleBankruptcy(player);
            return success ? Ok(GetState()) : BadRequest("Cannot declare bankruptcy.");
        }


        /// =========================================================================
        /// --------------------- BUILDING & PROPERTY MANAGEMENT --------------------
        /// =========================================================================

        [HttpPost("property/buy-house")]
        public IActionResult BuyHouse([FromQuery] int spaceIndex)
        {
            var engine = _gameService.Engine;
            if (engine == null) return NotFound("Game has not started.");

            if (spaceIndex < 0 || spaceIndex >= engine.Board.Path.Count) return BadRequest("Invalid space index (0-39).");

            var space = engine.Board.Path[spaceIndex];
            if (space.Property == null) return BadRequest("This space is not a property.");

            var player = engine.GetCurrentPlayer();
            bool success = engine.BuyHouse(player, space.Property);
            if (!success) return BadRequest("Cannot buy house (check monopoly ownership, funds, even-building rule, or bank supply).");

            return Ok(GetState());
        }

        [HttpPost("property/sell-house")]
        public IActionResult SellHouse([FromQuery] int spaceIndex)
        {
            var engine = _gameService.Engine;
            if (engine == null) return NotFound("Game has not started.");

            if (spaceIndex < 0 || spaceIndex >= engine.Board.Path.Count) return BadRequest("Invalid space index (0-39).");

            var space = engine.Board.Path[spaceIndex];
            if (space.Property == null) return BadRequest("This space is not a property.");

            var player = engine.GetCurrentPlayer();
            bool success = engine.SellHouse(player, space.Property);
            if (!success) return BadRequest("Cannot sell house on this property.");

            return Ok(GetState());
        }


        [HttpPost("property/buy-hotel")]
        public IActionResult BuyHotel([FromQuery] int spaceIndex)
        {
            var engine = _gameService.Engine;
            if (engine == null) return NotFound("Game has not started.");

            if (spaceIndex < 0 || spaceIndex >= engine.Board.Path.Count) return BadRequest("Invalid space index (0-39).");

            var space = engine.Board.Path[spaceIndex];
            if (space.Property == null) return BadRequest("This space is not a property.");

            var player = engine.GetCurrentPlayer();
            bool success = engine.BuyHotel(player, space.Property);
            if (!success) return BadRequest("Cannot buy hotel (requires 4 houses on all properties in this color group).");

            return Ok(GetState());
        }


        [HttpPost("property/sell-hotel")]
        public IActionResult SellHotel([FromQuery] int spaceIndex)
        {
            var engine = _gameService.Engine;
            if (engine == null) return NotFound("Game has not started.");

            if (spaceIndex < 0 || spaceIndex >= engine.Board.Path.Count) return BadRequest("Invalid space index (0-39).");

            var space = engine.Board.Path[spaceIndex];
            if (space.Property == null) return BadRequest("This space is not a property.");

            var player = engine.GetCurrentPlayer();
            bool success = engine.SellHotel(player, space.Property);
            if (!success) return BadRequest("Cannot sell hotel on this property.");

            return Ok(GetState());
        }

        [HttpPost("property/mortgage")]
        public IActionResult MortgageProperty([FromQuery] int spaceIndex)
        {
            var engine = _gameService.Engine;
            if (engine == null) return NotFound("Game has not started.");

            if (spaceIndex < 0 || spaceIndex >= engine.Board.Path.Count) return BadRequest("Invalid space index (0-39).");

            var space = engine.Board.Path[spaceIndex];
            if (space.Property == null) return BadRequest("This space is not a property.");

            var player = engine.GetCurrentPlayer();
            bool success = engine.HandleMortgage(player, space.Property);
            if (!success) return BadRequest("Cannot mortgage property (check ownership, unbuilt group rule, or if already mortgaged).");

            return Ok(GetState());
        }


        [HttpPost("property/unmortgage")]
        public IActionResult UnmortgageProperty([FromQuery] int spaceIndex)
        {
            var engine = _gameService.Engine;
            if (engine == null) return NotFound("Game has not started.");

            if (spaceIndex < 0 || spaceIndex >= engine.Board.Path.Count) return BadRequest("Invalid space index (0-39).");

            var space = engine.Board.Path[spaceIndex];
            if (space.Property == null) return BadRequest("This space is not a property.");

            var player = engine.GetCurrentPlayer();

            bool success = engine.HandleMortgageRedemption(player, space.Property);
            if (!success) return BadRequest("Cannot unmortgage (insufficient funds or property is not mortgaged).");
            return Ok(GetState());
        }

        /// <summary>Sells an undeveloped property back to the Bank for half price (MortgageValue) to raise emergency cash.</summary>
        [HttpPost("property/sell")]
        public IActionResult SellProperty([FromQuery] int spaceIndex)
        {
            var engine = _gameService.Engine;
            if (engine == null) return NotFound("Game has not started.");
            if (spaceIndex < 0 || spaceIndex >= engine.Board.Path.Count) return BadRequest("Invalid space index (0-39).");

            var space = engine.Board.Path[spaceIndex];
            if (space.Property == null) return BadRequest("This space is not a property.");

            var player = engine.GetCurrentPlayer();
            bool success = engine.SellProperty(player, space.Property);
            if (!success) return BadRequest("Cannot sell property back to Bank (check ownership, or ensure all houses/hotels in that color group are sold first).");

            return Ok(GetState());
        }

        /// =========================================================================
        /// ----------------------- TRADING ENDPOINTS -------------------------------
        /// =========================================================================

        [HttpPost("trade/propose")]
        public IActionResult ProposeTrade([FromBody] TradeProposalRequest request)
        {
            var engine = _gameService.Engine;
            if (engine == null) return NotFound("Game has not started.");

            var buyer = engine.Players.FirstOrDefault(p => p.Id == request.BuyerId);
            var seller = engine.Players.FirstOrDefault(p => p.Id == request.SellerId);
            if (buyer == null || seller == null) return BadRequest("Invalid buyer or seller ID.");

            var properties = new List<IProperty>();
            foreach (var idx in request.SpaceIndices)
            {
                if (idx < 0 || idx >= engine.Board.Path.Count) return BadRequest($"Invalid space index: {idx}");

                var prop = engine.Board.Path[idx].Property;
                if (prop == null) return BadRequest($"Space {idx} is not a property.");

                properties.Add(prop);
            }

            bool success = engine.ProposeTrade(buyer, seller, properties, request.CashOffer);
            if (!success) return BadRequest("Trade rejected (check buyer funds, seller ownership, or unbuilt group rule).");
            return Ok(GetState());
        }


        [HttpPost("trade/accept")]
        public IActionResult AcceptTrade([FromQuery] int sellerId)
        {
            var engine = _gameService.Engine;
            if (engine == null) return NotFound("Game has not started.");

            var seller = engine.Players.FirstOrDefault(p => p.Id == sellerId);
            if (seller == null) return BadRequest("Seller not found.");

            bool success = engine.AcceptTrade(seller);
            if (!success) return BadRequest("Failed to accept trade. No matching active proposal.");
            return Ok(GetState());
        }

        [HttpPost("trade/decline")]
        public IActionResult DeclineTrade([FromQuery] int sellerId)
        {
            var engine = _gameService.Engine;
            if (engine == null) return NotFound("Game has not started.");

            var seller = engine.Players.FirstOrDefault(p => p.Id == sellerId);
            if (seller == null) return BadRequest("Seller not found.");

            engine.DeclineTrade(seller);

            return Ok(GetState());
        }

        /// =========================================================================
        /// ----------------------- AUCTION ENDPOINTS -------------------------------
        /// =========================================================================

        /// <summary>
        /// Declines to buy the property the player is standing on.
        /// Trigger Auction
        /// </summary>
        [HttpPost("decline-property")]
        public IActionResult DeclineProperty()
        {
            var engine = _gameService.Engine;
            if (engine == null) return NotFound("Game has not started.");

            var player = engine.GetCurrentPlayer();
            var space = engine.GetPlayerPosition(player);

            if (space?.Property == null) return BadRequest("Not standing on a property.");
            if (engine.GetPropertyOwner(space.Property) != null) return BadRequest("Property is already owned.");

            // Triggers the official auction
            engine.DeclineProperty(player, space.Property);

            return Ok(GetState());
        }


        [HttpPost("auction/start")]
        public IActionResult StartAuction([FromQuery] int spaceIndex)
        {
            var engine = _gameService.Engine;
            if (engine == null) return NotFound("Game has not started.");

            if (spaceIndex < 0 || spaceIndex >= engine.Board.Path.Count) return BadRequest("Invalid space index.");
            var space = engine.Board.Path[spaceIndex];
            if (space.Property == null) return BadRequest("Space is not a property.");

            bool success = engine.StartAuction(space.Property);
            if (!success) return BadRequest("Cannot start auction (property may already be owned).");
            return Ok(GetState());
        }

        [HttpPost("auction/bid")]
        public IActionResult PlaceBid([FromBody] AuctionBidRequest request)
        {
            var engine = _gameService.Engine;
            if (engine == null) return NotFound("Game has not started.");

            var player = engine.Players.FirstOrDefault(p => p.Id == request.PlayerId);
            if (player == null) return BadRequest("Player not found.");

            bool success = engine.PlaceBid(player, request.Amount);
            if (!success) return BadRequest("Bid rejected. Must exceed current highest bid and player must have enough cash.");
            return Ok(GetState());
        }

        /// <summary>The current bidder passes their turn in the auction.</summary>
        [HttpPost("auction/pass")]
        public IActionResult PassAuction([FromQuery] int playerId)
        {
            var engine = _gameService.Engine;
            if (engine == null) return NotFound("Game has not started.");
            if (!engine.HasActiveAuction) return BadRequest("No active auction.");

            var player = engine.Players.FirstOrDefault(p => p.Id == playerId);
            if (player == null) return BadRequest("Player not found.");

            bool success = engine.PassBid(player);
            if (!success) return BadRequest("Cannot pass (either not your turn or not in auction).");

            return Ok(GetState());
        }

        [HttpPost("auction/end")]
        // public IActionResult EndAuction([FromQuery] int spaceIndex)
        public IActionResult EndAuction()
        {
            var engine = _gameService.Engine;
            if (engine == null) return NotFound("Game has not started.");

            if (!engine.HasActiveAuction || engine.AuctionProperty == null)
            {
                return BadRequest("No active auction to end.");
            }
            // if (spaceIndex < 0 || spaceIndex >= engine.Board.Path.Count) return BadRequest("Invalid space index.");
            // var space = engine.Board.Path[spaceIndex];
            // if (space.Property == null) return BadRequest("Space is not a property.");


            var winner = engine.HandleAuction(engine.AuctionProperty);
            return Ok(GetState());
        }


        /// =========================================================================
        /// ----------------------- JAIL CARD TRADING -------------------------------
        /// =========================================================================

        /// <summary>Proposes to buy a Get Out of Jail Free card from another player for cash.</summary>
        [HttpPost("trade/card/propose")]
        public IActionResult ProposeCardTrade([FromBody] CardTradeProposalRequest request)
        {
            var engine = _gameService.Engine;
            if (engine == null) return NotFound("Game has not started.");

            var buyer = engine.Players.FirstOrDefault(p => p.Id == request.BuyerId);
            var seller = engine.Players.FirstOrDefault(p => p.Id == request.SellerId);
            if (buyer == null || seller == null) return BadRequest("Invalid buyer or seller ID.");

            bool success = engine.ProposeCardTrade(buyer, seller, request.CashOffer);
            if (!success) return BadRequest("Cannot propose card trade (check buyer cash or seller card count).");

            return Ok(GetState());
        }

        /// <summary>The seller accepts the card trade offer, transferring cash and the Jail card.</summary>
        [HttpPost("trade/card/accept")]
        public IActionResult AcceptCardTrade([FromQuery] int sellerId)
        {
            var engine = _gameService.Engine;
            if (engine == null) return NotFound("Game has not started.");

            var seller = engine.Players.FirstOrDefault(p => p.Id == sellerId);
            if (seller == null) return BadRequest("Seller not found.");

            bool success = engine.AcceptCardTrade(seller);
            if (!success) return BadRequest("Failed to accept card trade. No matching active proposal.");

            return Ok(GetState());
        }

        /// <summary>The seller declines the card trade offer.</summary>
        [HttpPost("trade/card/decline")]
        public IActionResult DeclineCardTrade([FromQuery] int sellerId)
        {
            var engine = _gameService.Engine;
            if (engine == null) return NotFound("Game has not started.");

            var seller = engine.Players.FirstOrDefault(p => p.Id == sellerId);
            if (seller == null) return BadRequest("Seller not found.");

            engine.DeclineCardTrade(seller);
            return Ok(GetState());
        }


        /// <summary>Dismisses the last drawn card notification.</summary>
        [HttpPost("card/dismiss")]
        public IActionResult DismissCard()
        {
            var engine = _gameService.Engine;
            if (engine == null) return NotFound("Game has not started.");
            engine.ClearLastDrawnCard();
            return Ok(GetState());
        }

        // =========================================================================
        // DEBUG / CHEAT ENDPOINTS (For Testing)
        // =========================================================================

        /// <summary>Instantly sends a player to Jail.</summary>
        [HttpPost("debug/send-to-jail")]
        public IActionResult DebugSendToJail([FromQuery] int playerId)
        {
            var engine = _gameService.Engine;
            if (engine == null) return NotFound("Game has not started.");

            var player = engine.Players.FirstOrDefault(p => p.Id == playerId);
            if (player == null) return BadRequest($"Player with Id {playerId} not found.");

            engine.SendToJail(player);
            return Ok(GetState());
        }

        /// <summary>Gives the current player 1 Get Out of Jail Free card.</summary>
        [HttpPost("debug/give-jail-card")]
        public IActionResult DebugGiveJailCard()
        {
            var engine = _gameService.Engine;
            if (engine == null) return NotFound("Game has not started.");

            var player = engine.GetCurrentPlayer();
            player.JailFreeCard++;

            return Ok(GetState());
        }

        /// <summary>Teleports the current player to any space (0-39) and triggers its landing effect.</summary>
        [HttpPost("debug/teleport")]
        public IActionResult DebugTeleport([FromQuery] int spaceIndex)
        {
            var engine = _gameService.Engine;
            if (engine == null) return NotFound("Game has not started.");
            if (spaceIndex < 0 || spaceIndex >= engine.Board.Path.Count) return BadRequest("Invalid space index (0-39).");

            var player = engine.GetCurrentPlayer();
            var token = engine.GetPlayerToken(player);
            if (token == null) return BadRequest();

            var currentSpace = engine.GetPlayerPosition(player);
            int currentIndex = currentSpace != null ? engine.Board.Path.IndexOf(currentSpace) : 0;

            int steps = (spaceIndex - currentIndex + engine.Board.Path.Count) % engine.Board.Path.Count;
            if (steps == 0) steps = engine.Board.Path.Count;

            engine.Move(token, steps, fromCard: false);
            return Ok(GetState());
        }

        /// <summary>Adds money to the current player.</summary>
        [HttpPost("debug/add-money")]
        public IActionResult DebugAddMoney([FromQuery] int? amount = null)
        {
            var engine = _gameService.Engine;
            if (engine == null) return NotFound("Game has not started.");

            int cash = amount ?? 1000;
            var player = engine.GetCurrentPlayer();
            engine.ReceiveMoney(player, cash);

            return Ok(GetState());
        }


        /// <summary>Cheat: Gives the current player ownership of the Brown group (Space 1: Mediterranean & Space 3: Baltic).</summary>
        [HttpPost("debug/give-brown-monopoly")]
        public IActionResult DebugGiveBrownMonopoly()
        {
            var engine = _gameService.Engine;
            if (engine == null) return NotFound("Game has not started.");
            var player = engine.GetCurrentPlayer();
            var med = engine.Board.Path[1].Property!;
            var bal = engine.Board.Path[3].Property!;
            engine.BuyProperty(player, med);
            engine.BuyProperty(player, bal);
            return Ok(GetState());
        }

        // Helper: Map GameEngine state into clean GameStateDto for React
        private GameStateDto GetState()
        {
            var engine = _gameService.Engine!;
            var currPlayer = engine.GetCurrentPlayer();
            return new GameStateDto
            {
                CurrentPlayerId = currPlayer.Id,
                CurrentPlayerName = currPlayer.Name,

                BankHouses = engine.houseAmount,
                BankHotels = engine.hotelAmount,

                CanRollAgain = engine.CanRollAgain(),
                CanRoll = engine.CanRoll(),
                CanEndTurn = engine.CanEndTurn(),
                HasPendingDebt = engine.HasPendingDebt(),
                IsGameOver = engine.CheckGameOver(),
                WinnerName = engine.GetWinner()?.Name,
                Dice = new DiceDto
                {
                    Side1 = engine.Dice.Side1,
                    Side2 = engine.Dice.Side2
                },
                Players = engine.Players.Select(p =>
                {
                    var token = engine.GetPlayerToken(p);
                    var space = engine.Board.Path.FirstOrDefault(s => s.Tokens.Contains(token));
                    int pos = space != null ? engine.Board.Path.IndexOf(space) : 0;
                    return new PlayerDto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Money = p.Money,
                        Position = pos,
                        PositionName = space?.Name ?? "GO",
                        TokenColor = token.Color.ToString(),
                        IsInJail = p.IsInJail,
                        IsBankrupt = p.IsBankrupt,
                        JailFreeCards = p.JailFreeCard,
                        OwnedProperties = engine.Board.Path
                            .Where(s => s.Property != null && engine.GetPropertyOwner(s.Property) == p)
                            .Select(s => s.Property!.Name)
                            .ToList()
                    };
                }).ToList(),
                Spaces = engine.Board.Path.Select((s, idx) => new SpaceDto
                {
                    Index = idx,
                    Name = s.Name,
                    Type = s.Type.ToString(),
                    ColorGroup = s.Property?.ColorGroup?.ToString(),
                    Price = s.Property?.Price,
                    BuildingLevel = s.Property?.BuildingLevel ?? 0,
                    IsMortgaged = s.Property?.IsMortgaged ?? false,
                    OwnerName = s.Property != null ? engine.GetPropertyOwner(s.Property)?.Name : null,
                    TokenColors = s.Tokens.Select(t => t.Color.ToString()).ToList()
                }).ToList(),
                ActiveAuction = engine.HasActiveAuction ? new AuctionDto
                {
                    SpaceIndex = engine.Board.Path.FindIndex(s => s.Property == engine.AuctionProperty),
                    PropertyName = engine.AuctionProperty!.Name,
                    HighestBidderId = engine.HighestBidder?.Id,
                    HighestBidderName = engine.HighestBidder?.Name,
                    HighestBid = engine.HighestBid,
                    CurrentBidderId = engine.CurrentAuctionBidder?.Id,      // helper for take turn when bidding
                    CurrentBidderName = engine.CurrentAuctionBidder?.Name   // helper for take turn when bidding
                } : null,
                PendingCardTrade = engine.HasPendingCardTrade ? new CardTradeDto
                {
                    BuyerId = engine.CardTradeBuyer!.Id,
                    BuyerName = engine.CardTradeBuyer.Name,
                    SellerId = engine.CardTradeSeller!.Id,
                    SellerName = engine.CardTradeSeller.Name,
                    CashOffer = engine.CardTradeOffer
                } : null,
                PendingTrade = engine.HasPendingTrade ? new TradeDto
                {
                    BuyerId = engine.TradeBuyer!.Id,
                    BuyerName = engine.TradeBuyer.Name,
                    SellerId = engine.TradeSeller!.Id,
                    SellerName = engine.TradeSeller.Name,
                    SpaceIndices = engine.TradeProperties?
                        .Select(p => engine.Board.Path.FindIndex(s => s.Property == p))
                        .Where(idx => idx >= 0)
                        .ToList() ?? new List<int>(),
                    PropertyNames = engine.TradeProperties?
                        .Select(p => p.Name)
                        .ToList() ?? new List<string>(),
                    CashOffer = engine.TradeOffer
                } : null,
                LastDrawnCard = engine.LastDrawnCard != null ? new CardDto
                {
                    CardType = engine.LastDrawnCard.CardType.ToString(),
                    CardEffect = engine.LastDrawnCard.CardEffect.ToString(),
                    Description = engine.LastDrawnCard.Description,
                    Value = engine.LastDrawnCard.Value
                } : null,
            };
        }

        /// <summary>Debug cheat: forcefully draws a card for the current player without needing to roll.</summary>
        [HttpPost("debug/draw-card")]
        public IActionResult DebugDrawCard([FromQuery] CardType cardType)
        {
            var engine = _gameService.Engine;
            if (engine == null) return NotFound("Game has not started.");
            var player = engine.GetCurrentPlayer();
            engine.DrawCard(player, cardType);
            return Ok(GetState());
        }


    }
}