using FiapGamesService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiapGamesService.Domain.Interfaces
{
    public interface IGameChangedEventRepository : IBaseRepository<GameChangedEvent> { }
}
