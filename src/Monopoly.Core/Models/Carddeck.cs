using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Monopoly.Core.Interfaces;

namespace Monopoly.Core.Models
{
    public class Carddeck : ICardDeck
    {
        public Queue<ICard> Cards { get; set; } = new Queue<ICard>();
    }
}