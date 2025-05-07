using Microsoft.EntityFrameworkCore;
using CardService.Models;

namespace CardService.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Card> Cards { get; set; }
    }
}
