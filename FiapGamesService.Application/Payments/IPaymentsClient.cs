using FiapGamesService.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiapGamesService.Application.Payments
{
    public interface IPaymentsClient
    {
        Task<ApiResponseRaw> ProcessAsync(PaymentProcessInputForPayments dto, CancellationToken ct = default);
        Task<(bool ok, PaymentProcessOutputDto? dto, string? message)> GetByIdAsync(string id, CancellationToken ct = default);
        Task<(bool ok, List<PaymentProcessOutputDto>? dtos, string? message)> GetAllByUserAsync(int userId, CancellationToken ct = default);
    }
}
