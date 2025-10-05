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

        // GET /fiap/games?search=&genre=&page=1&pageSize=20
        [HttpGet]
        [ProducesResponseType(typeof(PaginationResult<GameDto>), 200)]
        public async Task<ActionResult<PaginationResult<GameDto>>> List([FromQuery] string? search, [FromQuery] string? genre, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
        {
            page = page <= 0 ? 1 : page;
            pageSize = pageSize <= 0 ? 20 : Math.Min(pageSize, 100);

            var result = await _service.ListAsync(search, genre, page, pageSize, ct);
            return Ok(result);
        }

        // GET /fiap/games/{id}
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<GameDto>> GetById(Guid id, CancellationToken ct = default)
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
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] GameUpdateDto dto, CancellationToken ct = default)
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
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
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
    }
}
