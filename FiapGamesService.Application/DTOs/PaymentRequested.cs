using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiapGamesService.Application.DTOs
{
    public record PaymentRequested(
        int UserId,
        string GameId,
        decimal Amount,
        string Currency
    );
}