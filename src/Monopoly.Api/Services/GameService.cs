using Microsoft.AspNetCore.SignalR;
using Monopoly.Api.Hubs;
using Monopoly.Core;
using Monopoly.Core.Interfaces;
using Monopoly.Core.Models;

namespace Monopoly.Api.Services
{
    public class GameService
    {

        public GameEngine? Engine { get; private set; }

        private readonly IHubContext<GameHub> _hubContext;
        private readonly ILogger<GameService> _logger;

        public GameService(IHubContext<GameHub> hubContext, ILogger<GameService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        public GameEngine StartNewGame(List<IPlayer> players, List<IToken> tokens)
        {
            IBoard board = new Board();
            IDice dice = new Dice();

            Engine = new GameEngine(board, dice);

            // INITIALIZE COMPONENTS
            Engine.InitializeBoard();
            Engine.InitializePath();
            Engine.InitializeCardDecks();
            Engine.InitializePlayers(players, tokens);

            RegisterEngineListeners(Engine);

            // START THE MATCH
            Engine.StartGame();


            return Engine;
        }

        private void RegisterEngineListeners(GameEngine engine)
        {
            engine.OnPlayerMoneyChanged = (player) =>
            {
                // Console.WriteLine($"💵 {player.Name}'s balance updated: ${player.Money}");
                _logger.LogInformation("Financial: {PlayerName}'s balance updated to {NewBalance:C0}", player.Name, player.Money);
                BroadcastNotification($"💵 {player.Name}'s balance updated: ${player.Money}");
            };


            engine.OnPlayerBankrupt = (player) =>
            {
                // Console.WriteLine($"[EVENT: Bankrupt] *** {player.Name} has gone bankrupt and is eliminated! ***");
                _logger.LogWarning("Bankruptcy: {PlayerName} has gone bankrupt and is eliminated!", player.Name);
                BroadcastNotification($"[EVENT: Bankrupt] *** {player.Name} has gone bankrupt and is eliminated! ***");
            };

            engine.OnTurnChanged = (player) =>
            {
                // Console.WriteLine($"[EVENT: TurnChanged] >>> It is now {player.Name}'s turn.");
                _logger.LogInformation("Turn changed: It is now {PlayerName}'s turn (PlayerId: {PlayerId})", player.Name, player.Id);
                BroadcastNotification($"[EVENT: TurnChanged] >>> It is now {player.Name}'s turn.");
            };

            engine.OnDiceRolled = (player, d1, d2) =>
            {
                bool isDouble = d1 == d2;
                string doubleText = isDouble ? " [DOUBLES!]" : "";
                Console.WriteLine($"[EVENT: DiceRolled] {player.Name} rolled {d1} + {d2} = {d1 + d2}{doubleText}");
                _logger.LogInformation("Dice roll: {PlayerName} rolled {Die1} + {Die2} = {Total} (IsDouble: {IsDouble})",
                    player.Name, d1, d2, d1 + d2, isDouble);
                // BroadcastNotification($"[EVENT: DiceRolled] {player.Name} rolled {d1} + {d2} = {d1 + d2}{doubleText}");
            };

            engine.OnPlayerMoved = (token, space) =>
            {
                // Console.WriteLine($"[EVENT: PlayerMoved] Token {token.Color} landed on '{space.Name}'.");
                _logger.LogInformation("Movement: Token {TokenColor} landed on space {SpaceName}", token.Color, space.Name);


                BroadcastNotification($"[EVENT: PlayerMoved] Token {token.Color} landed on '{space.Name}'.");
            };

            engine.OnCardEffect = (player, card) =>
            {
                // Console.WriteLine($"[EVENT: CardEffect] {player.Name} drew {card.CardType} card: \"{card.Description}\"");

                _logger.LogInformation("Card drawn: {PlayerName} drew {CardType} card: \"{CardDescription}\"", player.Name, card.CardType, card.Description);


                BroadcastNotification($"[EVENT: CardEffect] {player.Name} drew {card.CardType} card: \"{card.Description}\"");
            };

            engine.OnAuctionStarted = (property) =>
            {
                // Console.WriteLine($"[EVENT: AuctionStarted] Auction started for '{property.Name}' (Base Price: ${property.Price}).");
                _logger.LogInformation("Auction started: {PropertyName} (BasePrice: {Price:C0})", property.Name, property.Price);
                BroadcastNotification($"[EVENT: AuctionStarted] Auction started for '{property.Name}' (Base Price: ${property.Price}).");
            };

            // engine.OnBidPlaced = (player, amount) =>
            // {
            //     // Console.WriteLine($"[EVENT: BidPlaced] {player.Name} placed a bid of ${amount}.");
            //     _logger.LogInformation("Auction bid: {PlayerName} placed bid of {BidAmount:C0}", player.Name, amount);
            //     BroadcastNotification($"[EVENT: BidPlaced] {player.Name} placed a bid of ${amount}.");
            // };

            // engine.OnAuctionEnded = (property, winner, winningBid) =>
            // {
            //     if (winner != null)
            //     {
            //         // Console.WriteLine($"[EVENT: AuctionEnded] '{property.Name}' sold to {winner.Name} for ${winningBid}!");
            //         _logger.LogInformation("Auction won: {PropertyName} sold to {WinnerName} for {WinningBid:C0}", property.Name, winner.Name, winningBid);
            //         BroadcastNotification($"[EVENT: AuctionEnded] '{property.Name}' sold to {winner.Name} for ${winningBid}!");
            //     }
            //     else
            //     {
            //         // Console.WriteLine($"[EVENT: AuctionEnded] Auction for '{property.Name}' ended with no bids.");
            //         _logger.LogInformation("Auction ended with no bids for {PropertyName}", property.Name);
            //         BroadcastNotification($"[EVENT: AuctionEnded] Auction for '{property.Name}' ended with no bids.");
            //     }
            // };

            engine.OnTradeOffered = (buyer, seller, properties, offer) =>
            {
                var propNames = (properties != null && properties.Count > 0)
                    ? string.Join(", ", properties.Select(p => p.Name))
                    : "No properties";
                // Console.WriteLine($"[EVENT: TradeOffered] {buyer.Name} offered ${offer} to {seller.Name} for [{propNames}].");
                _logger.LogInformation("Trade offered: {BuyerName} offered {OfferAmount:C0} to {SellerName} for [{Properties}]",
                    buyer.Name, offer, seller.Name, propNames);
                BroadcastNotification($"[EVENT: TradeOffered] {buyer.Name} offered ${offer} to {seller.Name} for [{propNames}].");
            };

            engine.OnCardTradeOffered = (buyer, seller, offer) =>
            {
                // Console.WriteLine($"[EVENT: CardTradeOffered] {buyer.Name} offered ${offer} to {seller.Name} for a 'Get Out of Jail Free' card.");
                _logger.LogInformation("Card trade offered: {BuyerName} offered {OfferAmount:C0} to {SellerName} for Jail Free Card",
                    buyer.Name, offer, seller.Name);
                BroadcastNotification($"[EVENT: CardTradeOffered] {buyer.Name} offered ${offer} to {seller.Name} for a 'Get Out of Jail Free' card.");
            };
        }
        private void BroadcastNotification(string message)
        {
            // Console.WriteLine(message);

            // Broadcast live to all connected React clients via SignalR
            _ = _hubContext.Clients.All.SendAsync("GameNotification", message);
        }
    }
}