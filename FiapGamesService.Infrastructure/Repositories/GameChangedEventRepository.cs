using FiapGamesService.Domain.Entities;
using FiapGamesService.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiapGamesService.Infrastructure.Repositories
{
    public class GameChangedEventRepository : BaseRepository<GameChangedEvent>, IGameChangedEventRepository
    {
        public GameChangedEventRepository(AppDbContext context) : base(context) { }
    }
}
