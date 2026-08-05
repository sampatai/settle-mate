using Microsoft.EntityFrameworkCore;
using SettleMate.Features.Book;

namespace SettleMate.Database;

public class ApplicationDbContext(DbContextOptions options)
 : DbContext(options)
{
    public DbSet<Book> Books { get; set; } = null!;
}

