using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiapGamesService.Application.DTOs
{
    public class GameDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string Genre { get; set; } = default!;
        public DateTime CreatedAt { get; set; }

        public GameDto() { }

        public GameDto(int id, string name, string? description, decimal price, string genre, DateTime createdAt)
        {
            Id = id;
            Name = name;
            Description = description;
            Price = price;
            Genre = genre;
            CreatedAt = createdAt;
        }
    }
}
