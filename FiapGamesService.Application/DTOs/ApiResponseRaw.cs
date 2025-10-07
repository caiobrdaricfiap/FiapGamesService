using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiapGamesService.Application.DTOs
{
    public class ApiResponseRaw
    {
        public bool Success { get; set; }
        public object? Data { get; set; }
        public string? Message { get; set; }
    }
}
