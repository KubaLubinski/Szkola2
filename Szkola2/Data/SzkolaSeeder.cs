using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Szkola2.Models;

namespace Szkola2.Data
{
    /// <summary>
    /// Zakłada role, kategorie i (opcjonalnie) konta początkowe. Dane kont pochodzą z konfiguracji
    /// (user-secrets / zmienne środowiskowe), a nie z kodu – repozytorium nie zawiera danych osobowych ani haseł:
    ///   dotnet user-secrets set "Seed:Admin:Email" "admin@szkola.example"
    ///   dotnet user-secrets set "Seed:Admin:Password" "..."
    ///   dotnet user-secrets set "Seed:Admin:FirstName" "Jan"
    ///   dotnet user-secrets set "Seed:Admin:LastName" "Kowalski"
    /// Analogicznie "Seed:Dyrektor:*" i "Seed:Nauczyciel:*". Istniejące konta nie są ruszane.
    /// Używa RoleManager i UserManager, więc nazwy znormalizowane (NormalizedName) są ustawiane poprawnie.
    /// </summary>
    public static class SzkolaSeeder
    {
        private static readonly string[] Roles = { "admin", "dyrektor", "nauczyciel", "uczen", "rodzic" };

        // rola, której wartość jest jednocześnie nazwą sekcji w konfiguracji ("Seed:{Role}")
        private static readonly (string Role, string Section)[] SeedAccounts =
        {
            ("admin", "Admin"),
            ("dyrektor", "Dyrektor"),
            ("nauczyciel", "Nauczyciel"),
        };

        public static async Task InitializeAsync(IServiceProvider services)
        {
            var db = services.GetRequiredService<ApplicationDbContext>();
            var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("SzkolaSeeder");

            if (!await db.Database.CanConnectAsync())
            {
                // wcześniej błąd był połykany po cichu – teraz jest widoczny w logach
                logger.LogWarning("Seeder pominięty: brak połączenia z bazą danych.");
                return;
            }

            var userManager = services.GetRequiredService<UserManager<AppUser>>();

            await SeedRolesAsync(services.GetRequiredService<RoleManager<IdentityRole>>());
            await SeedUsersAsync(userManager, services.GetRequiredService<IConfiguration>(), logger);
            await EnsureAdminExistsAsync(userManager, services.GetRequiredService<IHostEnvironment>(), logger);
            await SeedCategoriesAsync(db);
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            foreach (var role in Roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        private static async Task SeedUsersAsync(UserManager<AppUser> userManager, IConfiguration config, ILogger logger)
        {
            foreach (var (role, section) in SeedAccounts)
            {
                var email = config[$"Seed:{section}:Email"];
                var password = config[$"Seed:{section}:Password"];
                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrEmpty(password)) continue;

                if (await userManager.FindByEmailAsync(email) != null) continue;

                var user = new AppUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    LockoutEnabled = true,
                    FirstName = config[$"Seed:{section}:FirstName"],
                    LastName = config[$"Seed:{section}:LastName"],
                    Information = config[$"Seed:{section}:Information"],
                    IsActive = true,
                    CanAddTexts = true
                };

                var created = await userManager.CreateAsync(user, password);
                if (!created.Succeeded)
                {
                    logger.LogError("Nie udało się utworzyć konta {Section}: {Errors}", section,
                        string.Join("; ", created.Errors.Select(e => e.Description)));
                    continue;
                }

                await userManager.AddToRoleAsync(user, role);
            }
        }

        /// <summary>
        /// Gwarantuje, że da się wejść do panelu. Bez konta admina:
        ///  - Development: tworzy konto admin@szkola-demo.example z losowym hasłem (pokazanym raz w logu),
        ///  - Production: tylko ostrzega – dane konta trzeba podać w konfiguracji (Seed:Admin:*).
        /// Konta testowe (demo) mają rolę admin, ale są tymczasowe, więc nie liczą się.
        /// </summary>
        private static async Task EnsureAdminExistsAsync(UserManager<AppUser> userManager, IHostEnvironment env, ILogger logger)
        {
            var admins = await userManager.GetUsersInRoleAsync("admin");
            if (admins.Any(u => !u.IsDemo)) return;

            if (!env.IsDevelopment())
            {
                logger.LogWarning("Brak konta administratora. Ustaw Seed:Admin:Email i Seed:Admin:Password w konfiguracji.");
                return;
            }

            const string email = "admin@szkola-demo.example";
            var password = Convert.ToBase64String(RandomNumberGenerator.GetBytes(12)) + "aA1!";

            var user = new AppUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                LockoutEnabled = true,
                FirstName = "Administrator",
                LastName = "Testowy",
                Information = "Konto deweloperskie (portfolio)",
                IsActive = true,
                CanAddTexts = true
            };

            var created = await userManager.CreateAsync(user, password);
            if (!created.Succeeded)
            {
                logger.LogError("Nie udało się utworzyć konta deweloperskiego: {Errors}",
                    string.Join("; ", created.Errors.Select(e => e.Description)));
                return;
            }

            await userManager.AddToRoleAsync(user, "admin");
            logger.LogWarning("Utworzono konto deweloperskie {Email}. Hasło wypisano tylko w konsoli (nie w pliku logów).", email);
            Console.WriteLine($"[SEED] Hasło konta {email}: {password}");
        }

        private static async Task SeedCategoriesAsync(ApplicationDbContext db)
        {
            if (await db.Categories!.AnyAsync()) return;

            db.Categories!.AddRange(
                new Category
                {
                    CategoryName = "Uroczystości szkolne",
                    IsActive = true,
                    Description = "Relacje z aktualnych wydarzeń z życia szkoły."
                },
                new Category
                {
                    CategoryName = "Relacje z wycieczek",
                    IsActive = true,
                    Description = "Przedstawienie wrażeń, zdjęć i wartości edukacyjnych z wycieczek poza szkołę."
                },
                new Category
                {
                    CategoryName = "Ogłoszenia",
                    IsActive = true,
                    Description = "Najważniejsze informacje dotyczące wydarzeń szkolnych i innych aktualności."
                });

            await db.SaveChangesAsync();
        }
    }
}
