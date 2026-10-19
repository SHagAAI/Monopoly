using Monopoly.Core.Enums;
using Monopoly.Enums;

namespace Monopoly.Core.Interfaces;

/// <summary>
/// Contract representing a Monopoly property (Title Deed).
/// 
/// A street, railroad or utility that can be owned. This is only data.
/// The owner is not stored here: the game controller keeps it in dictionary.
/// </summary>
public interface IProperty
{
    string Name { get; }
 
    PropertyType PropertyType { get; }
 
    /// <summary>Printed price, paid to the Bank when buying.</summary>
    int Price { get; }
 
    /// <summary>Amount the Bank lends when the property is mortgaged (half of the price).</summary>
    int MortgageValue { get; }
 
    /// <summary>
    /// Rent by level.
    /// Railroad: 4 entries, index 0 = 1 railroad owned ... index 3 = 4 railroads owned.
    /// Utility: empty (rent is 4x or 10x the dice total).
    /// 
    /// Streets only: rent by building level, 6 entries (index 0 = no buildings, 1 to 4 = houses, 5 = hotel).
    /// Railroads and utilities: empty. Railroad rent (25 / 50 / 100 / 200 by railroads owned) and
    /// utility rent (4x or 10x the dice total) are worked out in the game controller.
    /// </summary>
    int[] Rents { get; }
 
    /// <summary>Streets only: the color group. Null for railroads and utilities.</summary>
    ColorGroup? ColorGroup { get; }
 
    /// <summary>
    /// Streets only: cost of one house. The hotel costs the same. 
    /// Null for railroads and utilities.
    /// </summary>
    int? HouseCost { get; }
 
    /// <summary>Streets only: 0 = no buildings, 1 to 4 = houses, 5 = hotel. Always 0 for railroads and utilities.</summary>
    int BuildingLevel { get; set; }
 
    bool IsMortgaged { get; set; }
}
