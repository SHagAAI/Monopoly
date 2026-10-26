using Monopoly.Core.Enums;
using Monopoly.Core.Interfaces;
using Monopoly.Enums;

namespace Monopoly.Core.Models
{
    public static class BoardData
    {
        public const int SquareCount = 40;

        // Squares that need an amount are told apart by their position on the path:
        // Income Tax is at index 4 ($200) and Luxury Tax is at index 38 ($100).
        public const int IncomeTaxIndex = 4;
        public const int LuxuryTaxIndex = 38;
        public const int IncomeTaxAmount = 200;
        public const int LuxuryTaxAmount = 100;

        /// <summary>Creates the 40 squares in track order. Index 0 is GO and index 39 is Boardwalk.</summary>
        public static List<ISpace> CreateSpaces()
        {
            return new List<ISpace>
        {
            new Space("GO", SpaceType.Go),                                                                    // 0
            Street("Mediterranean Avenue", ColorGroup.Brown, 60, 50, new[] { 2, 10, 30, 90, 160, 250 }),      // 1
            new Space("Community Chest", SpaceType.CommunityChest),                                           // 2
            Street("Baltic Avenue", ColorGroup.Brown, 60, 50, new[] { 4, 20, 60, 180, 320, 450 }),            // 3
            new Space("Income Tax", SpaceType.IncomeTax),                                                           // 4
            Railroad("Reading Railroad"),                                                                     // 5
            Street("Oriental Avenue", ColorGroup.LightBlue, 100, 50, new[] { 6, 30, 90, 270, 400, 550 }),     // 6
            new Space("Chance", SpaceType.Chance),                                                            // 7
            Street("Vermont Avenue", ColorGroup.LightBlue, 100, 50, new[] { 6, 30, 90, 270, 400, 550 }),      // 8
            Street("Connecticut Avenue", ColorGroup.LightBlue, 120, 50, new[] { 8, 40, 100, 300, 450, 600 }), // 9
            new Space("Jail / Just Visiting", SpaceType.Jail),                                                // 10
            Street("St. Charles Place", ColorGroup.Pink, 140, 100, new[] { 10, 50, 150, 450, 625, 750 }),     // 11
            Utility("Electric Company"),                                                                      // 12
            Street("States Avenue", ColorGroup.Pink, 140, 100, new[] { 10, 50, 150, 450, 625, 750 }),         // 13
            Street("Virginia Avenue", ColorGroup.Pink, 160, 100, new[] { 12, 60, 180, 500, 700, 900 }),       // 14
            Railroad("Pennsylvania Railroad"),                                                                // 15
            Street("St. James Place", ColorGroup.Orange, 180, 100, new[] { 14, 70, 200, 550, 750, 950 }),     // 16
            new Space("Community Chest", SpaceType.CommunityChest),                                           // 17
            Street("Tennessee Avenue", ColorGroup.Orange, 180, 100, new[] { 14, 70, 200, 550, 750, 950 }),    // 18
            Street("New York Avenue", ColorGroup.Orange, 200, 100, new[] { 16, 80, 220, 600, 800, 1000 }),    // 19
            new Space("Free Parking", SpaceType.FreeParking),                                                 // 20
            Street("Kentucky Avenue", ColorGroup.Red, 220, 150, new[] { 18, 90, 250, 700, 875, 1050 }),       // 21
            new Space("Chance", SpaceType.Chance),                                                            // 22
            Street("Indiana Avenue", ColorGroup.Red, 220, 150, new[] { 18, 90, 250, 700, 875, 1050 }),        // 23
            Street("Illinois Avenue", ColorGroup.Red, 240, 150, new[] { 20, 100, 300, 750, 925, 1100 }),      // 24
            Railroad("B&O Railroad"),                                                                         // 25
            Street("Atlantic Avenue", ColorGroup.Yellow, 260, 150, new[] { 22, 110, 330, 800, 975, 1150 }),   // 26
            Street("Ventnor Avenue", ColorGroup.Yellow, 260, 150, new[] { 22, 110, 330, 800, 975, 1150 }),    // 27
            Utility("Water Works"),                                                                           // 28
            Street("Marvin Gardens", ColorGroup.Yellow, 280, 150, new[] { 24, 120, 360, 850, 1025, 1200 }),   // 29
            new Space("Go To Jail", SpaceType.GoToJail),                                                      // 30
            Street("Pacific Avenue", ColorGroup.Green, 300, 200, new[] { 26, 130, 390, 900, 1100, 1275 }),    // 31
            Street("North Carolina Avenue", ColorGroup.Green, 300, 200, new[] { 26, 130, 390, 900, 1100, 1275 }), // 32
            new Space("Community Chest", SpaceType.CommunityChest),                                           // 33
            Street("Pennsylvania Avenue", ColorGroup.Green, 320, 200, new[] { 28, 150, 450, 1000, 1200, 1400 }), // 34
            Railroad("Short Line"),                                                                           // 35
            new Space("Chance", SpaceType.Chance),                                                            // 36
            Street("Park Place", ColorGroup.DarkBlue, 350, 200, new[] { 35, 175, 500, 1100, 1300, 1500 }),    // 37
            new Space("Luxury Tax", SpaceType.LuxuryTax),                                                           // 38
            Street("Boardwalk", ColorGroup.DarkBlue, 400, 200, new[] { 50, 200, 600, 1400, 1700, 2000 })      // 39
        };
        }

        // Create Chance Card
        public static List<ICard> CreateChanceCards()
        {
            return new List<ICard>
            {
                new Card(CardType.Chance, CardEffect.MoveToPosition, "Advance to Boardwalk", 39),
                new Card(CardType.Chance, CardEffect.MoveToPosition, "Advance to GO (Collect $200)", 0),
                new Card(CardType.Chance, CardEffect.MoveToPosition, "Advance to Illinois Avenue", 24),
                new Card(CardType.Chance, CardEffect.MoveToPosition, "Advance to St. Charles Place. If you pass GO, collect $200", 11),
                new Card(CardType.Chance, CardEffect.MoveToNearestRailroad, "Advance to nearest Railroad. If unowned, you may buy it. If owned, pay owner twice the rent", null),
                new Card(CardType.Chance, CardEffect.MoveToNearestRailroad, "Advance to nearest Railroad. If unowned, you may buy it. If owned, pay owner twice the rent", null),
                new Card(CardType.Chance, CardEffect.MoveToNearestUtility, "Advance to nearest Utility. If unowned, you may buy it. If owned, throw dice and pay owner 10 times the dice throw", null),
                new Card(CardType.Chance, CardEffect.ReceiveMoney, "Bank pays you dividend of $50", 50),
                new Card(CardType.Chance, CardEffect.GetOutOfJailFree, "Get Out of Jail Free. This card may be kept until needed or sold", null),
                new Card(CardType.Chance, CardEffect.MoveSteps, "Go Back 3 Spaces", -3),
                new Card(CardType.Chance, CardEffect.GoToJail, "Go to Jail. Go directly to Jail, do not pass GO, do not collect $200", null),
                new Card(CardType.Chance, CardEffect.Repairs, "Make general repairs on all your property. For each house pay $25, for each hotel pay $100", null),
                new Card(CardType.Chance, CardEffect.PayMoney, "Speeding fine $15", 15),
                new Card(CardType.Chance, CardEffect.MoveToPosition, "Take a trip to Reading Railroad. If you pass GO, collect $200", 5),
                new Card(CardType.Chance, CardEffect.PayEachPlayer, "You have been elected Chairman of the Board. Pay each player $50", 50),
                new Card(CardType.Chance, CardEffect.ReceiveMoney, "Your building loan matures. Collect $150", 150)
            };
        }

        // Create Community Chest
        public static List<ICard> CreateCommunityChestCards()
        {
            return new List<ICard>
            {
                new Card(CardType.CommunityChest, CardEffect.MoveToPosition, "Advance to GO (Collect $200)", 0),
                new Card(CardType.CommunityChest, CardEffect.ReceiveMoney, "Bank error in your favor. Collect $200", 200),
                new Card(CardType.CommunityChest, CardEffect.PayMoney, "Doctor’s fee. Pay $50", 50),
                new Card(CardType.CommunityChest, CardEffect.ReceiveMoney, "From sale of stock you get $50", 50),
                new Card(CardType.CommunityChest, CardEffect.GetOutOfJailFree, "Get Out of Jail Free. This card may be kept until needed or sold", null),
                new Card(CardType.CommunityChest, CardEffect.GoToJail, "Go to Jail. Go directly to Jail, do not pass GO, do not collect $200", null),
                new Card(CardType.CommunityChest, CardEffect.ReceiveMoney, "Holiday fund matures. Receive $100", 100),
                new Card(CardType.CommunityChest, CardEffect.ReceiveMoney, "Income tax refund. Collect $20", 20),
                new Card(CardType.CommunityChest, CardEffect.ReceiveFromEachPlayer, "It is your birthday. Collect $10 from every player", 10),
                new Card(CardType.CommunityChest, CardEffect.ReceiveMoney, "Life insurance matures. Collect $100", 100),
                new Card(CardType.CommunityChest, CardEffect.PayMoney, "Pay hospital fees of $100", 100),
                new Card(CardType.CommunityChest, CardEffect.PayMoney, "Pay school fees of $50", 50),
                new Card(CardType.CommunityChest, CardEffect.ReceiveMoney, "Receive $25 consultancy fee", 25),
                new Card(CardType.CommunityChest, CardEffect.Repairs, "You are assessed for street repairs. $40 per house, $115 per hotel", null),
                new Card(CardType.CommunityChest, CardEffect.ReceiveMoney, "You have won second prize in a beauty contest. Collect $10", 10),
                new Card(CardType.CommunityChest, CardEffect.ReceiveMoney, "You inherit $100", 100)
            };
        }

        // The mortgage value of every property is half of its price.

        private static ISpace Street(string name, ColorGroup group, int price, int houseCost, int[] rents)
        {
            var space = new Space(name, SpaceType.Street);
            space.Property = new Property(name, PropertyType.Street, price, price / 2, rents, group, houseCost);
            return space;
        }

        private static ISpace Railroad(string name)
        {
            const int price = 200;
            var space = new Space(name, SpaceType.Railroad);
            space.Property = new Property(name, PropertyType.Railroad, price, price / 2, Array.Empty<int>());
            return space;
        }

        private static ISpace Utility(string name)
        {
            const int price = 150;
            var space = new Space(name, SpaceType.Utility);
            space.Property = new Property(name, PropertyType.Utility, price, price / 2, Array.Empty<int>());
            return space;
        }
    }
}