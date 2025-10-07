using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiapGamesService.Application.DTOs
{
    public record UserGameDto(
        int Id, 
        string Name, 
        decimal Price,
        string Status, 
        DateTime? PurchasedAtUtc
    );
}
