using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Monopoly.Api.Dtos
{
    public class GameStateDto
    {
        public int CurrentPlayerId { get; set; }
        public string CurrentPlayerName { get; set; } = string.Empty;


        public int BankHouses { get; set; }
        public int BankHotels { get; set; }

        public bool CanRollAgain { get; set; }
        public bool CanRoll { get; set; }

        public bool CanEndTurn { get; set; }
        public bool HasPendingDebt { get; set; }
        public bool IsGameOver { get; set; }
        public string? WinnerName { get; set; }
        public DiceDto Dice { get; set; } = new();
        public List<PlayerDto> Players { get; set; } = new();
        public List<SpaceDto> Spaces { get; set; } = new();

        public AuctionDto? ActiveAuction { get; set; }

        public CardTradeDto? PendingCardTrade { get; set; }
        public TradeDto? PendingTrade { get; set; }
        public CardDto? LastDrawnCard { get; set; }
    }
    public class DiceDto
    {
        public int Side1 { get; set; }
        public int Side2 { get; set; }
        public int Total => Side1 + Side2;
        public bool IsDouble => Side1 == Side2 && Side1 > 0;
    }

    public class PlayerDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Money { get; set; }
        public int Position { get; set; }
        public string PositionName { get; set; } = string.Empty;
        public string TokenColor { get; set; } = string.Empty;
        public bool IsInJail { get; set; }
        public bool IsBankrupt { get; set; }
        public int JailFreeCards { get; set; }
        public List<string> OwnedProperties { get; set; } = new();
    }

    public class SpaceDto
    {
        public int Index { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string? ColorGroup { get; set; }
        public int? Price { get; set; }
        public int? Rent { get; set; }
        public int BuildingLevel { get; set; }
        public bool IsMortgaged { get; set; }
        public string? OwnerName { get; set; }
        public List<string> TokenColors { get; set; } = new();
    }

    public class TradeProposalRequest
    {
        public int BuyerId { get; set; }
        public int SellerId { get; set; }
        public List<int> SpaceIndices { get; set; } = new();
        public int CashOffer { get; set; }
    }

    public class AuctionBidRequest
    {
        public int PlayerId { get; set; }
        public int Amount { get; set; }
    }

    public class AuctionDto
    {
        public int SpaceIndex { get; set; }
        public string PropertyName { get; set; } = string.Empty;
        public int? HighestBidderId { get; set; }
        public string? HighestBidderName { get; set; }
        public int HighestBid { get; set; }

        public int? CurrentBidderId { get; set; }
        public string? CurrentBidderName { get; set; }
    }

    public class CardTradeProposalRequest
    {
        public int BuyerId { get; set; }
        public int SellerId { get; set; }
        public int CashOffer { get; set; }
    }

    public class CardTradeDto
    {
        public int BuyerId { get; set; }
        public string BuyerName { get; set; } = string.Empty;
        public int SellerId { get; set; }
        public string SellerName { get; set; } = string.Empty;
        public int CashOffer { get; set; }
    }

    public class TradeDto
    {
        public int BuyerId { get; set; }
        public string BuyerName { get; set; } = string.Empty;
        public int SellerId { get; set; }
        public string SellerName { get; set; } = string.Empty;
        public List<int> SpaceIndices { get; set; } = new();
        public List<string> PropertyNames { get; set; } = new();
        public int CashOffer { get; set; }
    }

    public class CardDto
    {
        public string CardType { get; set; } = string.Empty;
        public string CardEffect { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int? Value { get; set; }
    }

}