using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIAP.Games.Application.DTOs
{
    public class GameUpdateDto
    {
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string Genre { get; set; } = default!;

        public GameUpdateDto() { }

        public GameUpdateDto(string name, string? description, decimal price, string genre)
        {
            Name = name;
            Description = description;
            Price = price;
            Genre = genre;
        }
    }
}
