using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiapGamesService.Application.DTOs
{
    public class GameCreateDto
    {
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string Genre { get; set; } = default!;

        public GameCreateDto() { }

        public GameCreateDto(string name, string? description, decimal price, string genre)
        {
            Name = name;
            Description = description;
            Price = price;
            Genre = genre;
        }
    }
}
