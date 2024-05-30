using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using Szkola2.Models;

namespace Szkola2.Data
{
    public class SzkolaSeeder
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var dbContext = new ApplicationDbContext
                (serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))

                if (dbContext.Database.CanConnect())
                {
                    SeedRoles(dbContext);
                    SeedUsers(dbContext);
                    SeedCategories(dbContext);
                }
        }

        private static void SeedRoles(ApplicationDbContext dbContext)
        {
            var roleStore = new RoleStore<IdentityRole>(dbContext);

            if (!dbContext.Roles.Any(r => r.Name == "admin"))
            {
                roleStore.CreateAsync(new IdentityRole
                {
                    Name = "admin",
                    NormalizedName = "admin"
                }).Wait();
            }
            if (!dbContext.Roles.Any(r => r.Name == "dyrektor"))
            {
                roleStore.CreateAsync(new IdentityRole
                {
                    Name = "dyrektor",
                    NormalizedName = "dyrektor"
                }).Wait();
            }
            if (!dbContext.Roles.Any(r => r.Name == "rodzic"))
            {
                roleStore.CreateAsync(new IdentityRole
                {
                    Name = "rodzic",
                    NormalizedName = "rodzic"
                }).Wait();
            }
            if (!dbContext.Roles.Any(r => r.Name == "uczen"))
            {
                roleStore.CreateAsync(new IdentityRole
                {
                    Name = "uczen",
                    NormalizedName = "uczen"
                }).Wait();
            }
            if (!dbContext.Roles.Any(r => r.Name == "nauczyciel"))
            {
                roleStore.CreateAsync(new IdentityRole
                {
                    Name = "nauczyciel",
                    NormalizedName = "nauczyciel"
                }).Wait();
            }
        }

        private static void SeedUsers(ApplicationDbContext dbContext)
        {
            if (!dbContext.Users.Any(u => u.UserName == "agnieszka.bajkowska@spgoleszyn.pl"))
            {
                var user = new AppUser
                {
                    UserName = "agnieszka.bajkowska@spgoleszyn.pl",
                    NormalizedUserName = "agnieszka.bajkowska@spgoleszyn.pl",
                    Email = "agnieszka.bajkowska@spgoleszyn.pl",
                    EmailConfirmed = true,
                    LockoutEnabled = false,
                    FirstName = "Agnieszka",
                    LastName = "Bajkowska",
                    Information = "Administrator strony",
                    IsActive = true,
                    CanAddTexts = true
                };
                var password = new PasswordHasher<AppUser>();
                var hashed = password.HashPassword(user, "Golsp1!");
                user.PasswordHash = hashed;
                var userStore = new UserStore<AppUser>(dbContext);
                userStore.CreateAsync(user).Wait();
                userStore.AddToRoleAsync(user, "admin").Wait();
                dbContext.SaveChanges();
            }

            if (!dbContext.Users.Any(u => u.UserName == "marek.czerminski@spgoleszyn.pl"))
            {
                var user = new AppUser
                {
                    UserName = "marek.czerminski@spgoleszyn.pl",
                    NormalizedUserName = "marek.czerminski@spgoleszyn.pl",
                    Email = "marek.czerminski@spgoleszyn.pl",
                    EmailConfirmed = true,
                    LockoutEnabled = false,
                    FirstName = "Marek",
                    LastName = "Czermiński",
                    Information = "Nauczyciel informatyki w szkole podstawowej W Goleszynie",
                    IsActive = true,
                    CanAddTexts = true
                };
                var password = new PasswordHasher<AppUser>();
                var hashed = password.HashPassword(user, "Golsp3!");
                user.PasswordHash = hashed;
                var userStore = new UserStore<AppUser>(dbContext);
                userStore.CreateAsync(user).Wait();
                userStore.AddToRoleAsync(user, "nauczyciel").Wait();
                dbContext.SaveChanges();
            }

            if (!dbContext.Users.Any(u => u.UserName == "magdalena.pesta@spgoleszyn.pl"))
            {
                var user = new AppUser
                {
                    UserName = "magdalena.pesta@spgoleszyn.pl",
                    NormalizedUserName = "magdalena.pesta@spgoleszyn.pl",
                    Email = "magdalena.pesta@spgoleszyn.pl",
                    EmailConfirmed = true,
                    LockoutEnabled = false,
                    FirstName = "Magdalena",
                    LastName = "Pesta",
                    Information = "Dyrektor szkoły podstawowej W Goleszynie",
                    IsActive = true,
                    CanAddTexts = true
                };
                var password = new PasswordHasher<AppUser>();
                var hashed = password.HashPassword(user, "Golsp2!");
                user.PasswordHash = hashed;
                var userStore = new UserStore<AppUser>(dbContext);
                userStore.CreateAsync(user).Wait();
                userStore.AddToRoleAsync(user, "dyrektor").Wait();
                dbContext.SaveChanges();
            }
        }

        private static void SeedCategories(ApplicationDbContext dbContext)
        {
            if (!dbContext.Categories.Any())
            {
                var kat = new List<Category>
                {
                    new Category
                    {
                        CategoryName = "Uroczystości szkolne",
                        IsActive = true,
                        Description = "Relacje z aktualnych wydarzeń z życia szkoły."
                    },
                    new Category
                    {
                        CategoryName = "Relacje z wycieczek",
                        IsActive= true,
                        Description = "Przedstawienie wrażeń, zdjęć i wartości edukacyjnych z wycieczek poza szkołę."
                    },
                    new Category
                    {
                        CategoryName = "Ogłoszenia",
                        IsActive = true,
                        Description = "Najważniejsze informacje dotyczące wydarzeń szkolnycj i innych aktualności."
                    }
                };
                dbContext.AddRange(kat);
                dbContext.SaveChanges();
            }
        }
    }
}
