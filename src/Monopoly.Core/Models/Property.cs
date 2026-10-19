using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Monopoly.Core.Enums;
using Monopoly.Core.Interfaces;
using Monopoly.Enums;

namespace Monopoly.Core.Models
{
    public class Property : IProperty
    {
        public string Name { get; }
        public PropertyType PropertyType { get; }
        public int Price { get; }
        public int MortgageValue { get; }
        public int[] Rents { get; }
        public ColorGroup? ColorGroup { get; }
        public int? HouseCost { get; }
        public int BuildingLevel { get; set; }
        public bool IsMortgaged { get; set; }

        public Property(
            string name,
            PropertyType propertyType,
            int price,
            int mortgageValue,
            int[] rents,
            ColorGroup? colorGroup = null,
            int? houseCost = null)
        {
            Name = name;
            PropertyType = propertyType;
            Price = price;
            MortgageValue = mortgageValue;
            Rents = rents;
            ColorGroup = colorGroup;
            HouseCost = houseCost;
        }
    }
}