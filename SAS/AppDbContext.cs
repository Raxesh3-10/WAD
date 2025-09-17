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
        public DbSet<PreviousStudent> PreviousStudents { get; set; }
        public DbSet<Bill> Bills { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>().HasKey(u => u.Id);
            modelBuilder.Entity<UserDetails>().HasKey(d => d.Id);
            modelBuilder.Entity<Bill>().HasKey(b => b.Id);
            modelBuilder.Entity<Notice>().HasKey(n => n.NoticeId);
            modelBuilder.Entity<Student>().HasKey(s => s.Id);
            modelBuilder.Entity<PreviousStudent>().HasKey(p => p.Id);

            modelBuilder.Entity<User>()
                .HasOne(u => u.UserDetails)
                .WithOne(d => d.User)
                .HasForeignKey<UserDetails>(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Notice>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notices)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Student>()
                .Property(s => s.Email)
                .IsRequired();

            modelBuilder.Entity<Student>()
                .HasIndex(s => s.Email)
                .IsUnique();

            modelBuilder.Entity<PreviousStudent>()
                .Property(p => p.Email)
                .IsRequired();

            modelBuilder.Entity<Bill>()
                .Property(b => b.Amount)
                .IsRequired();

            modelBuilder.Entity<Bill>()
                .Property(b => b.BillDate)
                .IsRequired();
        }
    }
}