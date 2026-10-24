using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Szkola2.Infrastructure;
using Szkola2.Models;
using Szkola2.Models.ViewModels;

namespace Szkola2.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        private readonly IHttpContextAccessor? _accessor;

        // accessor jest opcjonalny: SzkolaSeeder tworzy kontekst ręcznie, bez DI
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IHttpContextAccessor? accessor = null)
            : base(options)
        {
            _accessor = accessor;
        }

        /// <summary>Id zalogowanego konta demo, albo null (zwykły użytkownik, gość, Hangfire).</summary>
        public string? CurrentDemoOwnerId => DemoClaims.GetDemoUserId(_accessor?.HttpContext?.User);

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

            // --- Konta demo: widoczność treści ---
            // Zwykły użytkownik / gość / job widzi tylko DemoOwnerId == null.
            // Konto demo widzi treść szkoły + wyłącznie własne rekordy.
            // UWAGA: DbSet.Find/FindAsync NIE stosuje filtrów – w kodzie używamy FirstOrDefaultAsync.
            modelbuilder.Entity<Text>().HasQueryFilter(e => e.DemoOwnerId == null || e.DemoOwnerId == CurrentDemoOwnerId);
            modelbuilder.Entity<Document>().HasQueryFilter(e => e.DemoOwnerId == null || e.DemoOwnerId == CurrentDemoOwnerId);
            modelbuilder.Entity<PastSchoolEvent>().HasQueryFilter(e => e.DemoOwnerId == null || e.DemoOwnerId == CurrentDemoOwnerId);

            modelbuilder.Entity<Text>().HasIndex(e => e.DemoOwnerId);
            modelbuilder.Entity<Document>().HasIndex(e => e.DemoOwnerId);
            modelbuilder.Entity<PastSchoolEvent>().HasIndex(e => e.DemoOwnerId);
        }

        // --- Konta demo: siatka bezpieczeństwa przy zapisie ---
        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            EnforceDemoRules();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            EnforceDemoRules();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        /// <summary>
        /// Gdy zapisuje konto demo, każda zmieniana encja musi być jego własna.
        /// Dozwolone: własne IDemoOwned (nowe rekordy dostają znacznik automatycznie),
        /// zdjęcia własnych rekordów, własny wiersz AppUser (tylko modyfikacja).
        /// Wszystko inne (kategorie, historia, role, cudzy użytkownik...) kończy się DemoReadOnlyException.
        /// </summary>
        private void EnforceDemoRules()
        {
            var demoId = CurrentDemoOwnerId;
            if (demoId == null) return;

            var changed = ChangeTracker.Entries()
                .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
                .ToList();

            foreach (var entry in changed)
            {
                switch (entry.Entity)
                {
                    case IDemoOwned owned:
                        if (entry.State == EntityState.Added)
                        {
                            owned.DemoOwnerId = demoId;
                        }
                        else
                        {
                            var original = entry.Property(nameof(IDemoOwned.DemoOwnerId)).OriginalValue as string;
                            if (original != demoId || owned.DemoOwnerId != demoId)
                                throw new DemoReadOnlyException();
                        }
                        break;

                    case Graphic graphic:
                        if (!GraphicBelongsToDemo(entry, graphic, demoId))
                            throw new DemoReadOnlyException();
                        break;

                    case IdentityUser user:
                        if (entry.State != EntityState.Modified || user.Id != demoId)
                            throw new DemoReadOnlyException();
                        break;

                    default:
                        throw new DemoReadOnlyException();
                }
            }
        }

        private bool GraphicBelongsToDemo(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry, Graphic graphic, string demoId)
        {
            if (graphic.HistoryId != null) return false;

            object? parent = entry.Reference(nameof(Graphic.Texts)).CurrentValue
                          ?? entry.Reference(nameof(Graphic.PastEvent)).CurrentValue;

            if (parent != null)
            {
                var parentEntry = Entry(parent);
                if (parentEntry.State == EntityState.Added) return true; // nowy rekord – zostanie oznaczony tym samym kontem
                return parent is IDemoOwned
                    && parentEntry.Property(nameof(IDemoOwned.DemoOwnerId)).OriginalValue as string == demoId;
            }

            // brak załadowanego rodzica – sprawdzamy po kluczu obcym
            if (graphic.TextId != null)
                return Set<Text>().IgnoreQueryFilters().Any(t => t.TextId == graphic.TextId && t.DemoOwnerId == demoId);
            if (graphic.EventId != null)
                return Set<PastSchoolEvent>().IgnoreQueryFilters().Any(e => e.Id == graphic.EventId && e.DemoOwnerId == demoId);
            return false;
        }
    }
}
