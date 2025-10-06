using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiapGamesService.Application.DTOs
{
    public class PurchaseRequest
    {
        public int UserId { get; set; }
        public string Currency { get; set; } = "BRL";
    }
}
