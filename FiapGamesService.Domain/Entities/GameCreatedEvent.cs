namespace FiapGamesService.Domain.Entities
{
    public class GameCreatedEvent : BaseEntity
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string Genre { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        private GameCreatedEvent() { }

        private GameCreatedEvent(string name, string? description, decimal price, string genre)
        {
            Update(name, description, price, genre);
            CreatedAt = DateTime.UtcNow;
        }

        public static GameCreatedEvent Create(string name, string? description, decimal price, string genre)
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
