using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Szkola2.Models;

namespace Szkola2.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category>? Categories { get; set; }
        public DbSet<AdvertisingPhoto>? AdvertisingPhotos { get; set; }
        public DbSet<Document>? Documents { get; set; }
        public DbSet<History>? History { get; set; }
        public DbSet<Photo>? Photos { get; set; }
        public DbSet<Text>? Texts { get; set; }
        public DbSet<AppUser>? AppUsers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelbuilder)
        {
            base.OnModelCreating(modelbuilder);

            modelbuilder.Entity<Category>()
                .HasMany(c => c.Texts)
                .WithOne(t => t.Category);

            modelbuilder.Entity<Text>()
                .HasOne(t => t.Category)
                .WithMany(c => c.Texts);

            modelbuilder.Entity<Text>()
                .HasMany(t => t.Photos)
                .WithOne(p => p.Text);

            modelbuilder.Entity<Photo>()
                .HasOne(p => p.Text)
                .WithMany(t => t.Photos);

            modelbuilder.Entity<History>()
                .HasMany(h => h.Photos)
                .WithOne(p => p.History);

            modelbuilder.Entity<Photo>()
                .HasOne(p => p.History)
                .WithMany(h => h.Photos);

            modelbuilder.Entity<AppUser>()
                .HasMany(u => u.Texts)
                .WithOne(t => t.User);

            modelbuilder.Entity<Text>()
                .HasOne(u => u.User)
                .WithMany(t => t.Texts);
        }
    }
}