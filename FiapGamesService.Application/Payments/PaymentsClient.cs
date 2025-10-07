using FiapGamesService.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace FiapGamesService.Application.Payments
{
    public class PaymentsClient : IPaymentsClient
    {
        private readonly HttpClient _http;
        public PaymentsClient(HttpClient http) => _http = http;

        public async Task<ApiResponseRaw> ProcessAsync(PaymentProcessInputForPayments dto, CancellationToken ct = default)
        {
            var resp = await _http.PostAsJsonAsync("Payments/process", dto, ct);
            var json = await resp.Content.ReadAsStringAsync(ct);

            var parsed = System.Text.Json.JsonSerializer.Deserialize<ApiResponseRaw>(
                json,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return parsed ?? new ApiResponseRaw { Success = resp.IsSuccessStatusCode, Message = json };
        }

        public async Task<(bool ok, PaymentProcessOutputDto? dto, string? message)> GetByIdAsync(string id, CancellationToken ct = default)
        {
            var resp = await _http.GetAsync($"Payments/{id}", ct);
            var json = await resp.Content.ReadAsStringAsync(ct);

            var envelope = System.Text.Json.JsonSerializer.Deserialize<ApiResponseRaw>(
                json,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (envelope?.Success == true && envelope.Data is not null)
            {
                var dataJson = System.Text.Json.JsonSerializer.Serialize(envelope.Data);
                var typed = System.Text.Json.JsonSerializer.Deserialize<PaymentProcessOutputDto>(
                    dataJson,
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return (true, typed, envelope.Message);
            }

            return (false, null, envelope?.Message ?? json);
        }

        public async Task<(bool ok, List<PaymentProcessOutputDto>? dtos, string? message)> GetAllByUserAsync(int userId, CancellationToken ct = default)
        {
            var resp = await _http.GetAsync($"Payments/get-by-user/{userId}", ct);
            var json = await resp.Content.ReadAsStringAsync(ct);

            var envelope = System.Text.Json.JsonSerializer.Deserialize<ApiResponseRaw>(
                json, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (envelope?.Success == true && envelope.Data is not null)
            {
                var dataJson = System.Text.Json.JsonSerializer.Serialize(envelope.Data);
                var typed = System.Text.Json.JsonSerializer.Deserialize<List<PaymentProcessOutputDto>>(
                    dataJson, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return (true, typed, envelope.Message);
            }

            return (false, null, envelope?.Message ?? json);
        }
    }
}
