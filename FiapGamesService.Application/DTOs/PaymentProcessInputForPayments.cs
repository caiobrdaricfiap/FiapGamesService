using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiapGamesService.Application.DTOs
{
    public class PaymentProcessInputForPayments
    {
        public int UserId { get; set; }
        public int GameId { get; set; }
        public decimal Amount { get; set; }
        public int Currency { get; set; }
    }
}