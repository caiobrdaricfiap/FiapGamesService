using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiapGamesService.Domain.Enums
{
    public enum GameChangeType
    {
        Updated = 0,
        Deleted = 1,
        PriceChanged = 2,
        Renamed = 3,
        GenreChanged = 4,
        DescriptionChanged = 5
    }
}
