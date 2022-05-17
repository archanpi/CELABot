using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Domain
{
    public class CelaHackContext : DbContext
    {
        public CelaHackContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Message> Messages { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseCosmos(
            "https://cela-hack-22.documents.azure.com:443/",
            "SkNGdXlHK46R5R5BkmDccLeHG9nFHHlModrJJ24ACWbA8ucOzDCBHk8WUCuSJZC3BUBjw6iC2E5RvXYlMdbUsg==",
            databaseName: "CelaHack");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .ToContainer("Users")
                .HasNoDiscriminator();

            modelBuilder.Entity<Message>()
                .ToContainer("Messages")
                .HasPartitionKey(o => o.UserId)
                .HasNoDiscriminator();

        }
    }
}