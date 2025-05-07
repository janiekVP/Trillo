using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CardService.Data;
using CardService.Models;
using CardService.Dtos;

namespace CardService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CardsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CardsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CardDto>>> GetCards()
        {
            var cards = await _context.Cards
                .Select(c => new CardDto
                {
                    Id = c.Id,
                    BoardId = c.BoardId,
                    Name = c.Name,
                    Description = c.Description,
                    Status = c.Status,
                    Priority = c.Priority,
                    Due_date = c.Due_date,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();

            return Ok(cards);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CardDto>> GetCard(int id)
        {
            var card = await _context.Cards.FindAsync(id);
            if (card == null) return NotFound();

            return new CardDto
            {
                Id = card.Id,
                BoardId = card.BoardId,
                Name = card.Name,
                Description = card.Description,
                Status = card.Status,
                Priority = card.Priority,
                Due_date = card.Due_date,
                CreatedAt = card.CreatedAt
            };
        }

        [HttpGet("board/{boardId}")]
        public async Task<ActionResult<IEnumerable<CardDto>>> GetCardsByBoardId(int boardId)
        {
            var cards = await _context.Cards
                .Where(c => c.BoardId == boardId)
                .Select(c => new CardDto
                {
                    Id = c.Id,
                    BoardId = c.BoardId,
                    Name = c.Name,
                    Description = c.Description,
                    Status = c.Status,
                    Priority = c.Priority,
                    Due_date = c.Due_date,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();

            return Ok(cards);
        }

        [HttpPost]
        public async Task<ActionResult<CardDto>> CreateCard(CardDto dto)
        {
            var card = new Card
            {
                BoardId = dto.BoardId,
                Name = dto.Name,
                Description = dto.Description,
                Status = dto.Status,
                Priority = dto.Priority,
                Due_date = dto.Due_date,
                CreatedAt = DateTime.UtcNow
            };

            _context.Cards.Add(card);
            await _context.SaveChangesAsync();

            dto.Id = card.Id;
            dto.CreatedAt = card.CreatedAt;

            return CreatedAtAction(nameof(GetCard), new { id = card.Id }, dto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCard(int id, CardDto dto)
        {
            if (id != dto.Id) return BadRequest();

            var card = await _context.Cards.FindAsync(id);
            if (card == null) return NotFound();

            card.Name = dto.Name;
            card.Description = dto.Description;
            card.Status = dto.Status;
            card.Priority = dto.Priority;
            card.Due_date = dto.Due_date;
            card.BoardId = dto.BoardId;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCard(int id)
        {
            var card = await _context.Cards.FindAsync(id);
            if (card == null) return NotFound();

            _context.Cards.Remove(card);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
