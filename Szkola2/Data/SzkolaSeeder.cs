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
                    SeedTexts(dbContext);
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
            if (!dbContext.Users.Any(u => u.UserName == "admin@szkola.pl"))
            {
                var user = new AppUser
                {
                    UserName = "admin@szkola.pl",
                    NormalizedUserName = "admin@szkola.pl",
                    Email = "admin@szkola.pl",
                    EmailConfirmed = true,
                    LockoutEnabled = false,
                    FirstName = "Jan",
                    LastName = "Nowak",
                    Information = "Dyrektor szkoły podstawowej W Goleszynie",
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

            if (!dbContext.Users.Any(u => u.UserName == "nauczyciel@szkola.pl"))
            {
                var user = new AppUser
                {
                    UserName = "nauczyciel@szkola.pl",
                    NormalizedUserName = "nauczyciel@szkola.pl",
                    Email = "nauczyciel@szkola.pl",
                    EmailConfirmed = true,
                    LockoutEnabled = false,
                    FirstName = "Krzysztof",
                    LastName = "Stanowski",
                    Information = "Nauczyciel w szkole podstawowej W Goleszynie",
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

            if (!dbContext.Users.Any(u => u.UserName == "dyrektor@szkola.pl"))
            {
                var user = new AppUser
                {
                    UserName = "dyrektor@szkola.pl",
                    NormalizedUserName = "dyrektor@szkola.pl",
                    Email = "dyrektor@szkola.pl",
                    EmailConfirmed = true,
                    LockoutEnabled = false,
                    FirstName = "Piotr",
                    LastName = "Kowalski",
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

            if (!dbContext.Users.Any(u => u.UserName == "rodzic@szkola.pl"))
            {
                var user = new AppUser
                {
                    UserName = "rodzic@szkola.pl",
                    NormalizedUserName = "rodzic@szkola.pl",
                    Email = "rodzic@szkola.pl",
                    EmailConfirmed = true,
                    LockoutEnabled = false,
                    FirstName = "Piotr",
                    LastName = "Żyła",
                    Information = "Rodzic ucznia szkoły podstawowej W Goleszynie",
                    IsActive = true,
                    CanAddTexts = true
                };
                var password = new PasswordHasher<AppUser>();
                var hashed = password.HashPassword(user, "Golsp4!");
                user.PasswordHash = hashed;
                var userStore = new UserStore<AppUser>(dbContext);
                userStore.CreateAsync(user).Wait();
                userStore.AddToRoleAsync(user, "rodzic").Wait();
                dbContext.SaveChanges();
            }

            if (!dbContext.Users.Any(u => u.UserName == "uczen@szkola.pl"))
            {
                var user = new AppUser
                {
                    UserName = "uczen@szkola.pl",
                    NormalizedUserName = "uczen@szkola.pl",
                    Email = "uczen@szkola.pl",
                    EmailConfirmed = true,
                    LockoutEnabled = false,
                    FirstName = "Kamil",
                    LastName = "Stoch",
                    Information = "Uczeń w szkole podstawowej W Goleszynie",
                    IsActive = true,
                    CanAddTexts = true
                };
                var password = new PasswordHasher<AppUser>();
                var hashed = password.HashPassword(user, "Golsp5!");
                user.PasswordHash = hashed;
                var userStore = new UserStore<AppUser>(dbContext);
                userStore.CreateAsync(user).Wait();
                userStore.AddToRoleAsync(user, "uczen").Wait();
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

        private static void SeedTexts(ApplicationDbContext dbContext)
        {
            if(!dbContext.Texts.Any())
            {
                for(int i = 1; i <= 3; i++)
                {
                    var idUser1 = dbContext.AppUsers
                        .Where(u => u.UserName == "admin@szkola.pl")
                        .FirstOrDefault()
                        .Id;

                    for(int j = 0; j < 4; j++)
                    {
                        var text = new Text()
                        {
                            TextName = "Tytuł" + i.ToString() + j.ToString(),
                            Content = "Lorem ipsum dolor sit amet, consectetur\r\nadipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna\r\naliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi\r\nut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in\r\nvoluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint\r\noccaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim\r\nid est laborum. Sed ut perspiciatis unde omnis iste natus error sit voluptatem\r\naccusantium doloremque laudantium, totam rem aperiam, eaque ipsa quae ab illo\r\ninventore veritatis et quasi architecto beatae vitae dicta sunt explicabo. Nemo\r\nenim ipsam voluptatem quia voluptas sit aspernatur aut odit aut fugit, sed quia\r\nconsequuntur magni dolores eos qui ratione voluptatem sequi nesciunt. Neque porro\r\nquisquam est, qui dolorem ipsum quia dolor sit amet, consectetur, adipisci velit,\r\nsed quia non numquam eius modi tempora incidunt ut labore et dolore magnam\r\naliquam quaerat voluptatem. Ut enim ad minima veniam, quis nostrum exercitationem\r\nullam corporis suscipit laboriosam, nisi ut aliquid ex ea commodi consequatur?\r\nQuis autem vel eum iure reprehenderit qui in ea voluptate velit esse quam nihil\r\nmolestiae consequatur, vel illum qui dolorem eum fugiat quo voluptas nulla\r\npariatur?",
                            DateAdded = DateTime.Now.AddDays(-i * j),
                            CategoryId = i,
                            IsActive = true,
                            Id = idUser1
                        };
                        dbContext.Set<Text>().Add(text);
                    }
                    dbContext.SaveChanges();

                    var idUser2 = dbContext.AppUsers
                        .Where(u => u.UserName == "dyrektor@szkola.pl")
                        .FirstOrDefault()
                        .Id;

                    for (int j = 5; j < 9; j++)
                    {
                        var text = new Text()
                        {
                            TextName = "Tytuł" + i.ToString() + j.ToString(),
                            Content = "Lorem ipsum dolor sit amet, consectetur\r\nadipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna\r\naliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi\r\nut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in\r\nvoluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint\r\noccaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim\r\nid est laborum. Sed ut perspiciatis unde omnis iste natus error sit voluptatem\r\naccusantium doloremque laudantium, totam rem aperiam, eaque ipsa quae ab illo\r\ninventore veritatis et quasi architecto beatae vitae dicta sunt explicabo. Nemo\r\nenim ipsam voluptatem quia voluptas sit aspernatur aut odit aut fugit, sed quia\r\nconsequuntur magni dolores eos qui ratione voluptatem sequi nesciunt. Neque porro\r\nquisquam est, qui dolorem ipsum quia dolor sit amet, consectetur, adipisci velit,\r\nsed quia non numquam eius modi tempora incidunt ut labore et dolore magnam\r\naliquam quaerat voluptatem. Ut enim ad minima veniam, quis nostrum exercitationem\r\nullam corporis suscipit laboriosam, nisi ut aliquid ex ea commodi consequatur?\r\nQuis autem vel eum iure reprehenderit qui in ea voluptate velit esse quam nihil\r\nmolestiae consequatur, vel illum qui dolorem eum fugiat quo voluptas nulla\r\npariatur?",
                            DateAdded = DateTime.Now.AddDays(-i * j),
                            CategoryId = i,
                            IsActive = true,
                            Id = idUser2
                        };
                        dbContext.Set<Text>().Add(text);
                    }
                    dbContext.SaveChanges();

                    var idUser3 = dbContext.AppUsers
                        .Where(u => u.UserName == "nauczyciel@szkola.pl")
                        .FirstOrDefault()
                        .Id;

                    for (int j = 10; j < 14; j++)
                    {
                        var text = new Text()
                        {
                            TextName = "Tytuł" + i.ToString() + j.ToString(),
                            Content = "Lorem ipsum dolor sit amet, consectetur\r\nadipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna\r\naliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi\r\nut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in\r\nvoluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint\r\noccaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim\r\nid est laborum. Sed ut perspiciatis unde omnis iste natus error sit voluptatem\r\naccusantium doloremque laudantium, totam rem aperiam, eaque ipsa quae ab illo\r\ninventore veritatis et quasi architecto beatae vitae dicta sunt explicabo. Nemo\r\nenim ipsam voluptatem quia voluptas sit aspernatur aut odit aut fugit, sed quia\r\nconsequuntur magni dolores eos qui ratione voluptatem sequi nesciunt. Neque porro\r\nquisquam est, qui dolorem ipsum quia dolor sit amet, consectetur, adipisci velit,\r\nsed quia non numquam eius modi tempora incidunt ut labore et dolore magnam\r\naliquam quaerat voluptatem. Ut enim ad minima veniam, quis nostrum exercitationem\r\nullam corporis suscipit laboriosam, nisi ut aliquid ex ea commodi consequatur?\r\nQuis autem vel eum iure reprehenderit qui in ea voluptate velit esse quam nihil\r\nmolestiae consequatur, vel illum qui dolorem eum fugiat quo voluptas nulla\r\npariatur?",
                            DateAdded = DateTime.Now.AddDays(-i * j),
                            CategoryId = i,
                            IsActive = true,
                            Id = idUser3
                        };
                        dbContext.Set<Text>().Add(text);
                    }
                    dbContext.SaveChanges();

                    var idUser4 = dbContext.AppUsers
                        .Where(u => u.UserName == "rodzic@szkola.pl")
                        .FirstOrDefault()
                        .Id;

                    for (int j = 15; j < 19; j++)
                    {
                        var text = new Text()
                        {
                            TextName = "Tytuł" + i.ToString() + j.ToString(),
                            Content = "Lorem ipsum dolor sit amet, consectetur\r\nadipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna\r\naliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi\r\nut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in\r\nvoluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint\r\noccaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim\r\nid est laborum. Sed ut perspiciatis unde omnis iste natus error sit voluptatem\r\naccusantium doloremque laudantium, totam rem aperiam, eaque ipsa quae ab illo\r\ninventore veritatis et quasi architecto beatae vitae dicta sunt explicabo. Nemo\r\nenim ipsam voluptatem quia voluptas sit aspernatur aut odit aut fugit, sed quia\r\nconsequuntur magni dolores eos qui ratione voluptatem sequi nesciunt. Neque porro\r\nquisquam est, qui dolorem ipsum quia dolor sit amet, consectetur, adipisci velit,\r\nsed quia non numquam eius modi tempora incidunt ut labore et dolore magnam\r\naliquam quaerat voluptatem. Ut enim ad minima veniam, quis nostrum exercitationem\r\nullam corporis suscipit laboriosam, nisi ut aliquid ex ea commodi consequatur?\r\nQuis autem vel eum iure reprehenderit qui in ea voluptate velit esse quam nihil\r\nmolestiae consequatur, vel illum qui dolorem eum fugiat quo voluptas nulla\r\npariatur?",
                            DateAdded = DateTime.Now.AddDays(-i * j),
                            CategoryId = i,
                            IsActive = true,
                            Id = idUser4
                        };
                        dbContext.Set<Text>().Add(text);
                    }
                    dbContext.SaveChanges();

                    var idUser5 = dbContext.AppUsers
                        .Where(u => u.UserName == "uczen@szkola.pl")
                        .FirstOrDefault()
                        .Id;

                    for (int j = 20; j < 24; j++)
                    {
                        var text = new Text()
                        {
                            TextName = "Tytuł" + i.ToString() + j.ToString(),
                            Content = "Lorem ipsum dolor sit amet, consectetur\r\nadipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna\r\naliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi\r\nut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in\r\nvoluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint\r\noccaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim\r\nid est laborum. Sed ut perspiciatis unde omnis iste natus error sit voluptatem\r\naccusantium doloremque laudantium, totam rem aperiam, eaque ipsa quae ab illo\r\ninventore veritatis et quasi architecto beatae vitae dicta sunt explicabo. Nemo\r\nenim ipsam voluptatem quia voluptas sit aspernatur aut odit aut fugit, sed quia\r\nconsequuntur magni dolores eos qui ratione voluptatem sequi nesciunt. Neque porro\r\nquisquam est, qui dolorem ipsum quia dolor sit amet, consectetur, adipisci velit,\r\nsed quia non numquam eius modi tempora incidunt ut labore et dolore magnam\r\naliquam quaerat voluptatem. Ut enim ad minima veniam, quis nostrum exercitationem\r\nullam corporis suscipit laboriosam, nisi ut aliquid ex ea commodi consequatur?\r\nQuis autem vel eum iure reprehenderit qui in ea voluptate velit esse quam nihil\r\nmolestiae consequatur, vel illum qui dolorem eum fugiat quo voluptas nulla\r\npariatur?",
                            DateAdded = DateTime.Now.AddDays(-i * j),
                            CategoryId = i,
                            IsActive = true,
                            Id = idUser5
                        };
                        dbContext.Set<Text>().Add(text);
                    }
                    dbContext.SaveChanges();
                }
            }
        }
    }
}
