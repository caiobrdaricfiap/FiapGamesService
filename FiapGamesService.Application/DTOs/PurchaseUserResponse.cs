using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiapGamesService.Application.DTOs
{
    public record PurchaseUserResponse(
        string Status,
        string Message,
        string PaymentId,
        DateTime? CreatedAt,
        DateTime? UpdatedAt,
        object Game
    );
}