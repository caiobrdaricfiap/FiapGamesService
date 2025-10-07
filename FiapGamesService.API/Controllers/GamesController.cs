using FiapGamesService.Application.DTOs;
using FiapGamesService.Application.Services;
using FiapGamesService.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace FiapGamesService.Controllers
{
    [ApiController]
    [Route("fiap/[controller]")]
    public class GamesController : ControllerBase
    {
        private readonly GameService _service;

        public GamesController(GameService service)
        {
            _service = service;
        }

        // GET /fiap/games/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<GameDto>> GetById(int id, CancellationToken ct = default)
        {
            var dto = await _service.GetByIdAsync(id, ct);
            return dto is null ? NotFound() : Ok(dto);
        }

        // POST /fiap/games
        [HttpPost]
        public async Task<ActionResult> Create([FromBody] GameCreateDto dto, CancellationToken ct = default)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            var id = await _service.CreateAsync(dto, ct);
            return CreatedAtAction(nameof(GetById), new { id }, new { id });
        }

        // PUT /fiap/games/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] GameUpdateDto dto, CancellationToken ct = default)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            try
            {
                await _service.UpdateAsync(id, dto, ct);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // DELETE /fiap/games/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
        {
            await _service.DeleteAsync(id, ct);
            return NoContent();
        }

        [HttpGet("search-es")]
        public async Task<ActionResult<PaginationResult<GameDto>>> SearchEs(
            [FromQuery] string? q,
            [FromQuery] string? genre,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken ct = default)
        {
            var res = await _service.SearchEsAsync(q, genre, page, pageSize, ct);
            return Ok(res);
        }

        [HttpGet("metrics/top-genres")]
        public async Task<ActionResult<Dictionary<string, long>>> TopGenres(
            [FromQuery] int size = 10,
            CancellationToken ct = default)
        {
            var res = await _service.TopGenresAsync(size, ct);
            return Ok(res);
        }

        [HttpPost("{id:int}/purchase")]
        public async Task<IActionResult> PurchaseGame(int id, [FromBody] PurchaseRequest req, CancellationToken ct)
        {
            var (ok, body, status) = await _service.PurchaseAsync(id, req, ct);
            return StatusCode(status, body);
        }

        [HttpGet("users/{userId:int}/games")]
        public async Task<IActionResult> GetUserGames(int userId, [FromQuery] bool includePending = false, CancellationToken ct = default)
        {
            var (ok, body, status) = await _service.GetUserGamesAsync(userId, includePending, ct);
            return StatusCode(status, body);
        }

        [HttpGet("recommendations")]
        public async Task<ActionResult<List<GameDto>>> Recommend([FromQuery] int take = 10, CancellationToken ct = default)
        {
            var data = await _service.RecommendAsync(take, ct);
            return Ok(data);
        }
    }
}
