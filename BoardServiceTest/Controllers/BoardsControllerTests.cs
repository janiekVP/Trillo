using BoardService.Controllers;
using BoardService.Data;
using BoardService.Models;
using BoardService.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Net.Http;
using Xunit;
using BoardServiceTest.Helpers;
using MongoDB.Driver;

namespace BoardServiceTest.Controllers
{
    public class BoardsControllerTests
    {
        private readonly AppDbContext _context;
        private readonly BoardsController _controller;
        private readonly Board _existingBoard;

        public BoardsControllerTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()) // Use a unique DB for each test run
                .Options;
            _context = new AppDbContext(options);

            _existingBoard = new Board
            {
                Name = "Test Board",
                Description = "Test Desc",
                CreatedAt = DateTime.UtcNow
            };

            _context.Boards.Add(_existingBoard);
            _context.SaveChanges();

            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            var client = new HttpClient(new FakeHttpMessageHandler());
            mockHttpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            var mockMongoClient = new Mock<IMongoClient>();

            _controller = new BoardsController(_context, mockMongoClient.Object, mockHttpClientFactory.Object);
        }

        [Fact]
        public async Task GetBoards_ReturnsOk_WithData()
        {
            var result = await _controller.GetBoards();
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var boards = Assert.IsAssignableFrom<IEnumerable<BoardDto>>(ok.Value);
            Assert.Single(boards);
        }

        [Fact]
        public async Task GetBoard_ReturnsNotFound_ForInvalidId()
        {
            var result = await _controller.GetBoard(999);
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task UpdateBoard_UpdatesData_WhenValid()
        {
            // Arrange
            var updatedDto = new BoardDto
            {
                Id = _existingBoard.Id,
                Name = "Updated Name",
                Description = "Updated Desc",
                CreatedAt = _existingBoard.CreatedAt
            };

            // Act
            var result = await _controller.UpdateBoard(_existingBoard.Id, updatedDto);

            // Assert
            Assert.IsType<NoContentResult>(result);

            var updatedBoard = await _context.Boards.FindAsync(_existingBoard.Id);
            Assert.Equal("Updated Name", updatedBoard.Name);
            Assert.Equal("Updated Desc", updatedBoard.Description);
        }

        [Fact]
        public async Task UpdateBoard_ReturnsBadRequest_WhenIdMismatch()
        {
            var updatedDto = new BoardDto
            {
                Id = _existingBoard.Id + 1, // ID mismatch
                Name = "Mismatch",
                Description = "Should fail",
                CreatedAt = _existingBoard.CreatedAt
            };

            var result = await _controller.UpdateBoard(_existingBoard.Id, updatedDto);
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task UpdateBoard_ReturnsNotFound_WhenBoardMissing()
        {
            var updatedDto = new BoardDto
            {
                Id = 999, // non-existent
                Name = "Ghost",
                Description = "Ghost board",
                CreatedAt = DateTime.UtcNow
            };

            var result = await _controller.UpdateBoard(999, updatedDto);
            Assert.IsType<NotFoundResult>(result);
        }
    }
}
