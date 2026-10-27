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

        public GameService(IHubContext<GameHub> hubContext)
        {
            _hubContext = hubContext;
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

            Engine.OnPlayerMoneyChanged = (player) =>
            {
                Console.WriteLine($"💵 {player.Name}'s balance updated: ${player.Money}");
                BroadcastNotification($"💵 {player.Name}'s balance updated: ${player.Money}");
            };


            Engine.OnPlayerBankrupt = (player) =>
            {
                Console.WriteLine($"[EVENT: Bankrupt] *** {player.Name} has gone bankrupt and is eliminated! ***");
                BroadcastNotification($"[EVENT: Bankrupt] *** {player.Name} has gone bankrupt and is eliminated! ***");
            };

            Engine.OnTurnChanged = (player) =>
            {
                Console.WriteLine($"[EVENT: TurnChanged] >>> It is now {player.Name}'s turn.");
                BroadcastNotification($"[EVENT: TurnChanged] >>> It is now {player.Name}'s turn.");
            };

            Engine.OnDiceRolled = (player, d1, d2) =>
            {
                string doubleText = (d1 == d2) ? " [DOUBLES!]" : "";
                Console.WriteLine($"[EVENT: DiceRolled] {player.Name} rolled {d1} + {d2} = {d1 + d2}{doubleText}");
                // BroadcastNotification($"[EVENT: DiceRolled] {player.Name} rolled {d1} + {d2} = {d1 + d2}{doubleText}");
            };

            Engine.OnPlayerMoved = (token, space) =>
            {
                Console.WriteLine($"[EVENT: PlayerMoved] Token {token.Color} landed on '{space.Name}'.");
                BroadcastNotification($"[EVENT: PlayerMoved] Token {token.Color} landed on '{space.Name}'.");
            };

            Engine.OnCardEffect = (player, card) =>
            {
                Console.WriteLine($"[EVENT: CardEffect] {player.Name} drew {card.CardType} card: \"{card.Description}\"");
                // BroadcastNotification($"[EVENT: CardEffect] {player.Name} drew {card.CardType} card: \"{card.Description}\"");
            };

            Engine.OnAuctionStarted = (property) =>
            {
                Console.WriteLine($"[EVENT: AuctionStarted] Auction started for '{property.Name}' (Base Price: ${property.Price}).");
                BroadcastNotification($"[EVENT: AuctionStarted] Auction started for '{property.Name}' (Base Price: ${property.Price}).");
            };

            Engine.OnBidPlaced = (player, amount) =>
            {
                Console.WriteLine($"[EVENT: BidPlaced] {player.Name} placed a bid of ${amount}.");
                // BroadcastNotification($"[EVENT: BidPlaced] {player.Name} placed a bid of ${amount}.");
            };

            Engine.OnAuctionEnded = (property, winner, winningBid) =>
            {
                if (winner != null)
                {
                    Console.WriteLine($"[EVENT: AuctionEnded] '{property.Name}' sold to {winner.Name} for ${winningBid}!");
                    BroadcastNotification($"[EVENT: AuctionEnded] '{property.Name}' sold to {winner.Name} for ${winningBid}!");
                }
                else
                {
                    Console.WriteLine($"[EVENT: AuctionEnded] Auction for '{property.Name}' ended with no bids.");
                    BroadcastNotification($"[EVENT: AuctionEnded] Auction for '{property.Name}' ended with no bids.");
                }
            };

            Engine.OnTradeOffered = (buyer, seller, properties, offer) =>
            {
                var propNames = (properties != null && properties.Count > 0)
                    ? string.Join(", ", properties.Select(p => p.Name))
                    : "No properties";
                Console.WriteLine($"[EVENT: TradeOffered] {buyer.Name} offered ${offer} to {seller.Name} for [{propNames}].");
                BroadcastNotification($"[EVENT: TradeOffered] {buyer.Name} offered ${offer} to {seller.Name} for [{propNames}].");
            };

            Engine.OnCardTradeOffered = (buyer, seller, offer) =>
            {
                Console.WriteLine($"[EVENT: CardTradeOffered] {buyer.Name} offered ${offer} to {seller.Name} for a 'Get Out of Jail Free' card.");
                BroadcastNotification($"[EVENT: CardTradeOffered] {buyer.Name} offered ${offer} to {seller.Name} for a 'Get Out of Jail Free' card.");
            };

            // START THE MATCH
            Engine.StartGame();


            return Engine;
        }

        private void BroadcastNotification(string message)
        {
            // Console.WriteLine(message);

            // Broadcast live to all connected React clients via SignalR
            _ = _hubContext.Clients.All.SendAsync("GameNotification", message);
        }
    }
}