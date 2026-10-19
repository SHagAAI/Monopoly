using Monopoly.Core.Interfaces;

namespace Monopoly.Core.Models
{
    public class Player : IPlayer
    {
        private const int StartingMoney = 1500;

        public string Name { get; }
        // public IToken Token { get; }
        public int Money { get; set; } = StartingMoney;
        public bool IsBankrupt { get; set; }
        public bool IsInJail { get; set; }
        public int JailFreeCard { get; set; }

        public Player(string name)
        {
            Name = name;
            // Token = token;
        }
    }
}