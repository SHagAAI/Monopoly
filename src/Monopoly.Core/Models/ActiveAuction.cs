using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Monopoly.Core.Interfaces;

namespace Monopoly.Core.Models
{
    public record  ActiveAuction(IProperty Property, IPlayer? HighestBidder, int HighestBid);
}