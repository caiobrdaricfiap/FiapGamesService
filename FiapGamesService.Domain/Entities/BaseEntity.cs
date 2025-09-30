namespace FiapGamesService.Domain.Entities
{
    public abstract class BaseEntity
    {
        protected BaseEntity() { }
        public Guid Id { get; set; }
    }
}
