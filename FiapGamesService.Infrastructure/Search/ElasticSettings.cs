using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiapGamesService.Infrastructure.Search
{
    public interface IElasticSettings
    {
        string ApiKey { get; set; }
        string CloudId { get; set; }
        string Index { get; set; }
    }

    public class ElasticSettings : IElasticSettings
    {
        public string ApiKey { get; set; } = default!;
        public string CloudId { get; set; } = default!;
        public string Index { get; set; } = "games";

    }
}
