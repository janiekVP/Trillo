using Microsoft.EntityFrameworkCore;
using BoardService.Models;

namespace BoardService.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Board> Boards { get; set; }
    }
}
