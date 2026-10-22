using Monopoly.Core.Interfaces;

namespace Monopoly.Core.Models
{
    public class Player : IPlayer
    {
        public int Id { get; }
        private const int StartingMoney = 1500;
        public string Name { get; }
        // public IToken Token { get; }
        public int Money { get; set; } = StartingMoney;
        public bool IsBankrupt { get; set; }
        public bool IsInJail { get; set; }
        public int JailFreeCard { get; set; }

        public Player(int id, string name)
        {
            Id = id;
            Name = name;
            // Token = token;
        }
    }
}