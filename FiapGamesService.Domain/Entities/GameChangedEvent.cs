using FiapGamesService.Domain.Enums;

namespace FiapGamesService.Domain.Entities
{
    public class GameChangedEvent : BaseEntity
    {
        public Guid GameId { get; set; }

        public GameChangeType ChangeType { get; set; }
        public string? OldName { get; set; }
        public string? NewName { get; set; }
        public string? OldDescription { get; set; }
        public string? NewDescription { get; set; }
        public decimal? OldPrice { get; set; }
        public decimal? NewPrice { get; set; }
        public string? OldGenre { get; set; }
        public string? NewGenre { get; set; }
        public DateTime ChangedAt { get; set; }
        public string? Observation { get; set; }
    }
}
