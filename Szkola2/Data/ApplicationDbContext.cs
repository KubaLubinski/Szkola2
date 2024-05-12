using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Szkola2.Models;
using Szkola2.Models.ViewModels;

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
        public DbSet<Text>? Texts { get; set; }
        public DbSet<PastSchoolEvent>? PastSchoolEvents { get; set; }
        public DbSet<AppUser>? AppUsers { get; set; }
        public DbSet<Graphic>? GraphicGallery { get; set; }
        public DbSet<UserViewModel>? UserViewModel { get; set; }

        protected override void OnModelCreating(ModelBuilder modelbuilder)
        {
            base.OnModelCreating(modelbuilder);

            modelbuilder.Entity<Category>()
                .HasMany(c => c.Texts)
                .WithOne(t => t.Category);

            modelbuilder.Entity<Text>()
                .HasOne(t => t.Category)
                .WithMany(c => c.Texts);

            modelbuilder.Entity<AppUser>()
                .HasMany(u => u.Texts)
                .WithOne(t => t.User);

            modelbuilder.Entity<Text>()
                .HasOne(u => u.User)
                .WithMany(t => t.Texts);

            modelbuilder.Entity<History>()
                .HasMany(h => h.GraphicGallery)
                .WithOne(p => p.History);

            modelbuilder.Entity<Graphic>()
                .HasOne(p => p.History)
                .WithMany(h => h.GraphicGallery);

            modelbuilder.Entity<Text>()
                .HasMany(t => t.GraphicGallery)
                .WithOne(p => p.Texts);

            modelbuilder.Entity<Graphic>()
                .HasOne(p => p.Texts)
                .WithMany(t => t.GraphicGallery);

            modelbuilder.Entity<PastSchoolEvent>()
                .HasMany(g => g.GraphicGallery)
                .WithOne(e => e.PastEvent);

            modelbuilder.Entity<Graphic>()
                .HasOne(e => e.PastEvent)
                .WithMany(g => g.GraphicGallery);
        }


    }
}