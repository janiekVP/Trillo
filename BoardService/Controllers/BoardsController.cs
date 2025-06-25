using BoardService.Data;
using BoardService.Dtos;
using BoardService.Messaging;
using BoardService.Models;
using Ganss.Xss;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using Prometheus;
using System.Net.Http;

namespace BoardService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BoardsController : ControllerBase
    {
        private readonly AppDbContext _context;

        private static readonly Counter CreateCounter = Metrics.CreateCounter("boards_create_total", "Total number of board creations");
        private static readonly Counter ReadCounter = Metrics.CreateCounter("boards_read_total", "Total number of board reads");
        private static readonly Counter UpdateCounter = Metrics.CreateCounter("boards_update_total", "Total number of board updates");
        private static readonly Counter DeleteCounter = Metrics.CreateCounter("boards_delete_total", "Total number of board deletions");

        // histograms for latency
        private static readonly Histogram CreateLatency = Metrics.CreateHistogram("boards_create_duration_seconds", "Time taken to create a board");
        private static readonly Histogram LoadLatency = Metrics.CreateHistogram("boards_load_duration_seconds", "Time taken to load a single board");

        private readonly HttpClient _httpClient;

        private readonly IMongoClient _mongoClient;

        public BoardsController(AppDbContext context, IMongoClient mongoClient, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _mongoClient = mongoClient;
            _httpClient = httpClientFactory.CreateClient();
        }

        // GET: api/boards/counter
        [HttpGet("counter")]
        public async Task<IActionResult> CallCounter()
        {
            try
            {
                var functionUrl = "https://learningoutcome.azurewebsites.net/api/HttpTriggerCount?code=qcWZa27YJ4vaDBcWQXjG0f1gCCo-BGD7bgLR-If7PZ-4AzFutyvDGw==";

                var response = await _httpClient.GetAsync(functionUrl);

                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, "Failed to call Azure Function.");
                }

                var content = await response.Content.ReadAsStringAsync();
                return Ok(new { result = content });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error calling Azure Function: {ex.Message}");
            }
        }

        // GET: api/boards
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BoardDto>>> GetBoards()
        {
            ReadCounter.Inc();

            SentrySdk.CaptureMessage("Hello Sentry from Program.cs!");

            var boards = await _context.Boards
                .Select(b => new BoardDto
                {
                    Id = b.Id,
                    Name = b.Name,
                    Description = b.Description,
                    CreatedAt = b.CreatedAt
                })
                .ToListAsync();

            SentrySdk.CaptureMessage("Hello Sentry from Program.cs!");

            return Ok(boards);
        }

        // GET: api/boards/5
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<BoardDto>> GetBoard(int id)
        {
            ReadCounter.Inc();

            using (LoadLatency.NewTimer()) // measure latency
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
        }

        // POST: api/boards
        [HttpPost]
        public async Task<ActionResult<BoardDto>> CreateBoard([FromBody] BoardDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var sanitizer = new HtmlSanitizer();
            sanitizer.AllowedTags.Clear();

            CreateCounter.Inc();

            using (CreateLatency.NewTimer())
            {
                var board = new Board
                {
                    Name = sanitizer.Sanitize(dto.Name),
                    Description = sanitizer.Sanitize(dto.Description),
                    CreatedAt = DateTime.UtcNow
                };

                _context.Boards.Add(board);
                await _context.SaveChangesAsync();

                dto.Id = board.Id;
                dto.CreatedAt = board.CreatedAt;

                return CreatedAtAction(nameof(GetBoard), new { id = dto.Id }, dto);
            }
        }

        // PUT: api/boards/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBoard(int id, [FromBody] BoardDto dto)
        {
            UpdateCounter.Inc();

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
            DeleteCounter.Inc();

            var board = await _context.Boards.FindAsync(id);
            if (board == null) return NotFound();

            _context.Boards.Remove(board);
            await _context.SaveChangesAsync();

            var bus = new MessageBusClient();
            bus.PublishBoardDeleted(id);

            return NoContent();
        }
        
        // POST: api/boards/upload
        [HttpPost("upload")]
        public async Task<ActionResult<BoardDto>> UploadFile([FromForm] BoardDto dto)
        {
            CreateCounter.Inc();

            using (CreateLatency.NewTimer()) // measure latency 
            {
                var board = new Board
                {
                    Name = dto.Name,
                    Description = dto.Description,
                    CreatedAt = DateTime.UtcNow
                };

                var db = _mongoClient.GetDatabase("Files");
                var collection = db.GetCollection<Board>("FilesCollection");
                await collection.InsertOneAsync(board);

            }

                return CreatedAtAction(nameof(GetBoard), new { id = dto.Id }, dto);
        }

    }
}
