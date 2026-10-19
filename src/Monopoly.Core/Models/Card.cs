using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Monopoly.Core.Enums;
using Monopoly.Core.Interfaces;

namespace Monopoly.Core.Models
{
    /// <summary>
    /// A Chance or Community Chest card. This is only data.
    /// </summary>
    public class Card : ICard
    {


        public CardType CardType { get; }
        public CardEffect CardEffect { get; }
        public string Description { get; }
        public int? Value { get; }

        public Card(CardType cardType, CardEffect cardEffect, string description, int? value = null)
        {
            CardType = cardType;
            CardEffect = cardEffect;
            Description = description;
            Value = value;
        }
    }
}