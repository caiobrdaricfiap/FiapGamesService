using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiapGamesService.Infrastructure.Search
{
    public class GameSearchDocument
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public string Genre { get; set; } = default!;
        public decimal Price { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
