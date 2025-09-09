using Microsoft.EntityFrameworkCore;
using SAS.Models;

namespace SAS
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        public DbSet<Student> Students { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Notice> Notices { get; set; }
        public DbSet<UserDetails> UserDetails { get; set; }
    }
}
