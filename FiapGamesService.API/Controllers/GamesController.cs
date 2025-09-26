using FIAP.Games.Application.DTOs;
using FIAP.Games.Application.Services;
using FIAP.Games.Domain.Interfaces;
using FIAP.Games.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace FIAP.Games.API.Controllers
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
        public async Task<ActionResult<PaginationResult<GameDto>>> List(
            [FromQuery] string? search,
            [FromQuery] string? genre,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken ct = default)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 20;

            var result = await _service.ListAsync(search, genre, page, pageSize, ct);
            return Ok(result);
        }

        // GET /fiap/games/{id}
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<GameDto>> GetById(Guid id, CancellationToken ct = default)
        {
            var dto = await _service.GetAsync(id, ct);
            return dto is null ? NotFound() : Ok(dto);
        }

        // POST /fiap/games
        [HttpPost]
        public async Task<ActionResult> Create([FromBody] GameCreateDto dto, CancellationToken ct = default)
        {
            if (dto is null) return BadRequest();
            var id = await _service.CreateAsync(dto, ct);
            return CreatedAtAction(nameof(GetById), new { id }, new { id });
        }

        // PUT /fiap/games/{id}
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] GameUpdateDto dto, CancellationToken ct = default)
        {
            if (dto is null) return BadRequest();
            await _service.UpdateAsync(id, dto, ct);
            return NoContent();
        }

        // DELETE /fiap/games/{id}
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
        {
            await _service.DeleteAsync(id, ct);
            return NoContent();
        }
    }
}
