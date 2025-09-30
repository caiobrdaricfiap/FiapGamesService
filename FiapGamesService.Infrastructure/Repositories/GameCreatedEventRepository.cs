using FiapGamesService.Domain.Entities;
using FiapGamesService.Domain.Enums;
using FiapGamesService.Domain.Interfaces;
using FiapGamesService.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiapGamesService.Infrastructure.Repositories
{
    public class GameCreatedEventRepository : BaseRepository<GameCreatedEvent>, IGameCreatedEventRepository
    {
        public GameCreatedEventRepository(AppDbContext context) : base(context) { }
    }
}
