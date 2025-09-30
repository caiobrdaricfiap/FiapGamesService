using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiapGamesService.Domain.Models
{
    public class GameSnapshot
    {
        public Guid GameId { get; set; }
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string Genre { get; set; } = default!;
        public DateTime CreatedAt { get; set; }
    }
}
