using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Monopoly.Core.Enums;

namespace Monopoly.Api.Dtos
{
    public class StartGameRequest
    {
        public List<PlayerSetupDto> Players { get; set; } = new();
    }


    public class PlayerSetupDto
    {
        public string Name { get; set; } = string.Empty;
        public TokenColor Color { get; set; }
    }
}