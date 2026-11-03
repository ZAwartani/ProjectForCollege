using Microsoft.EntityFrameworkCore;
namespace SmartFood.Presentation.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }
    }
}
