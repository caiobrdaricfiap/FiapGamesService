using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIAP.Games.Domain.Entities
{
    public class Game : BaseEntity
    {
        public string Name { get; private set; } = default!;
        public string? Description { get; private set; }
        public decimal Price { get; private set; }
        public string Genre { get; private set; } = default!;
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

        private Game() { }

        private Game(string name, string? description, decimal price, string genre)
        {
            Update(name, description, price, genre);
            CreatedAt = DateTime.UtcNow;
        }

        public static Game Create(string name, string? description, decimal price, string genre)
            => new(name, description, price, genre);

        public void Update(string name, string? description, decimal price, string genre)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name required");
            if (string.IsNullOrWhiteSpace(genre)) throw new ArgumentException("Genre required");
            if (price < 0) throw new ArgumentOutOfRangeException(nameof(price));

            Name = name.Trim();
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
            Price = decimal.Round(price, 2);
            Genre = genre.Trim();
        }
    }
}
