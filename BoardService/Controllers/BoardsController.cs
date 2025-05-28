using BoardService.Data;
using BoardService.Dtos;
using BoardService.Messaging;
using BoardService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BoardService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BoardsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BoardsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/boards
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BoardDto>>> GetBoards()
        {
            var boards = await _context.Boards
                .Select(b => new BoardDto
                {
                    Id = b.Id,
                    Name = b.Name,
                    Description = b.Description,
                    CreatedAt = b.CreatedAt
                })
                .ToListAsync();

            return Ok(boards);
        }

        // GET: api/boards/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BoardDto>> GetBoard(int id)
        {
            var board = await _context.Boards.FindAsync(id);
            if (board == null) return NotFound();

            return new BoardDto
            {
                Id = board.Id,
                Name = board.Name,
                Description = board.Description,
                CreatedAt = board.CreatedAt
            };
        }

        // POST: api/boards
        [HttpPost]
        public async Task<ActionResult<BoardDto>> CreateBoard([FromBody] BoardDto dto)
        {
            var board = new Board
            {
                Name = dto.Name,
                Description = dto.Description,
                CreatedAt = DateTime.UtcNow
            };

            _context.Boards.Add(board);
            await _context.SaveChangesAsync();

            dto.Id = board.Id;
            dto.CreatedAt = board.CreatedAt;

            return CreatedAtAction(nameof(GetBoard), new { id = dto.Id }, dto);
        }

        // PUT: api/boards/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBoard(int id, [FromBody] BoardDto dto)
        {
            if (id != dto.Id) return BadRequest();

            var board = await _context.Boards.FindAsync(id);
            if (board == null) return NotFound();

            board.Name = dto.Name;
            board.Description = dto.Description;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/boards/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBoard(int id)
        {
            var board = await _context.Boards.FindAsync(id);
            if (board == null) return NotFound();

            _context.Boards.Remove(board);
            await _context.SaveChangesAsync();

            var bus = new MessageBusClient();
            bus.PublishBoardDeleted(id);

            return NoContent();
        }
    }
}