using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiapGamesService.Application.DTOs
{
    public class PaymentProcessOutputDto
    {
        public Guid Id { get; set; }
        public int UserId { get; set; }
        public int GameId { get; set; }
        public decimal Amount { get; set; }
        public string? Status { get; set; }
        public string? Currency { get; set; }
        public string? Observation { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
