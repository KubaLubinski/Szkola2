using Hangfire;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using Szkola2.Data;
using Szkola2.Infrastructure;
using Szkola2.Models;
using static Szkola2.Controllers.JobsController;

// Logger startowy: łapie błędy z samego początku działania aplikacji, zanim wczyta się pełna konfiguracja.
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

// Serilog: konsola + plik dzienny (logs/szkola-RRRRMMDD.log, 14 dni). Poziomy można zmienić w appsettings.json
// w sekcji "Serilog:MinimumLevel". Pliki z logami nie mogą trafić do repozytorium (.gitignore: logs/).
builder.Host.UseSerilog((context, services, configuration) => configuration
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        path: "logs/szkola-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 14,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}"));

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<AppUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = true;
        // hasła kont personelu: min. 10 znaków (domyślnie ASP.NET wymaga tylko 6)
        options.Password.RequiredLength = 10;
        options.User.RequireUniqueEmail = true;
        // blokada po serii błędnych haseł (działa po włączeniu lockoutOnFailure w Login.cshtml.cs)
        options.Lockout.AllowedForNewUsers = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
// Ciasteczka niezbędne do działania (logowanie, tokeny formularzy, komunikaty) nie wymagają zgody -
// bez tego przy CheckConsentNeeded logowanie nie działało, dopóki ktoś nie kliknął "Akceptuję".
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.IsEssential = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
});
builder.Services.AddAntiforgery(options =>
{
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
});
builder.Services.Configure<CookieTempDataProviderOptions>(options =>
{
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});
// jawny limit wielkości przesyłanych plików (wcześniej tylko domyślne ~28 MB)
builder.Services.Configure<FormOptions>(options => options.MultipartBodyLengthLimit = 20 * 1024 * 1024);
// za reverse proxy (IIS/nginx) adres IP klienta pochodzi z nagłówka X-Forwarded-For;
// włącz, ustawiając w konfiguracji "ForwardedHeaders:Enabled": true
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});
// Limit prób na wrażliwych adresach (logowanie, odzyskiwanie hasła, konto testowe): 10 żądań POST / 5 minut z jednego adresu IP.
// Uzupełnia blokadę konta po 5 błędnych hasłach – chroni też przed zgadywaniem haseł wielu kont naraz.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        if (!IsRateLimitedRequest(context.Request))
        {
            return RateLimitPartition.GetNoLimiter("none");
        }

        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter("auth:" + ip, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 10,
            Window = TimeSpan.FromMinutes(5),
            QueueLimit = 0
        });
    });
    options.OnRejected = async (rejected, cancellationToken) =>
    {
        // ślad prób nadużyć: kto (adres IP) i na jaki adres trafił na limit
        rejected.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>()
            .CreateLogger("RateLimiter")
            .LogWarning("Przekroczono limit żądań: {Path} z adresu {Ip}",
                rejected.HttpContext.Request.Path, rejected.HttpContext.Connection.RemoteIpAddress);
        rejected.HttpContext.Response.ContentType = "text/plain; charset=utf-8";
        await rejected.HttpContext.Response.WriteAsync(
            "Zbyt wiele prób. Spróbuj ponownie za kilka minut.", cancellationToken);
    };
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<IDemoContext, DemoContext>();
builder.Services.AddScoped<FileCleaner>();
builder.Services.AddScoped<GalleryService>();
builder.Services.AddScoped<DemoCleanupJob>();
builder.Services.AddControllersWithViews(options =>
{
    // zamienia DemoReadOnlyException na komunikat + powrót na poprzednią stronę
    options.Filters.Add<DemoExceptionFilter>();
});
builder.Services.AddHangfire((sp, config) =>
    {
        config.UseSqlServerStorage(connectionString);
    });
builder.Services.AddHangfireServer();

builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded = context => true;
    options.MinimumSameSitePolicy = SameSiteMode.Lax;
});

var app = builder.Build();

if (app.Configuration.GetValue<bool>("ForwardedHeaders:Enabled"))
{
    app.UseForwardedHeaders();
}

// zadania cykliczne rejestrujemy na gotowej aplikacji (wcześniej: drugi BuildServiceProvider tylko po to, by zainicjować Hangfire)
var recurringJobs = app.Services.GetRequiredService<IRecurringJobManager>();
recurringJobs.AddOrUpdate<DataTransferJob>("CopyAndArchiveTexts", job => job.CopyAndArchiveTextsAsync(), "0 0 31 8 *");
recurringJobs.AddOrUpdate<DemoCleanupJob>("DemoCleanup", job => job.RunAsync(), "*/10 * * * *");

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await SzkolaSeeder.InitializeAsync(services);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseSecurityHeaders();
app.UseStaticFiles(new StaticFileOptions
{
    // nagłówki cache: mniej pobrań przy kolejnych wizytach (Lighthouse: "Use efficient cache lifetimes")
    OnPrepareResponse = ctx =>
    {
        var request = ctx.Context.Request;
        var path = request.Path;
        string? cache = null;

        if (request.Query.ContainsKey("v"))
        {
            // css/js z asp-append-version – adres zmienia się razem z zawartością pliku
            cache = "public,max-age=31536000,immutable";
        }
        else if (path.StartsWithSegments("/gallery") || path.StartsWithSegments("/miniGallery")
                 || (path.StartsWithSegments("/img") && !path.StartsWithSegments("/img/logo")))
        {
            // zdjęcia wgrywane przez formularze mają nazwy z GUID – nigdy nie są nadpisywane
            cache = "public,max-age=2592000";
        }
        else if (path.StartsWithSegments("/lib") || path.StartsWithSegments("/img/logo"))
        {
            cache = "public,max-age=604800";
        }

        if (cache != null)
            ctx.Context.Response.Headers.CacheControl = cache;
    }
});

// jedna linia na żądanie (metoda, ścieżka, status, czas); po plikach statycznych, żeby nie zaśmiecać logu
app.UseSerilogRequestLogging();

app.UseCookiePolicy();
app.UseRouting();
app.UseRateLimiter();

app.UseAuthentication();

// Wygaśnięcie konta demo: wylogowanie bez zaglądania do bazy (czas siedzi w claimie)
app.Use(async (context, next) =>
{
    var expires = DemoClaims.GetExpiresUtc(context.User);
    if (expires != null && expires <= DateTime.UtcNow)
    {
        await context.SignOutAsync(IdentityConstants.ApplicationScheme);
        context.Response.Redirect("/");
        return;
    }
    await next();
});
// Konto wyłączone przez administratora ("Czy konto jest aktywne?") traci dostęp także w już otwartej sesji.
// Wynik jest trzymany w pamięci przez 30 s, żeby nie odpytywać bazy przy każdym żądaniu. Konta demo pomijamy.
app.Use(async (context, next) =>
{
    var principal = context.User;
    if (principal.Identity?.IsAuthenticated == true && DemoClaims.GetDemoUserId(principal) == null)
    {
        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId != null)
        {
            var cache = context.RequestServices.GetRequiredService<IMemoryCache>();
            var cacheKey = "user-active:" + userId;
            if (!cache.TryGetValue(cacheKey, out bool isActive))
            {
                var db = context.RequestServices.GetRequiredService<ApplicationDbContext>();
                var flag = await db.AppUsers!.AsNoTracking()
                    .Where(u => u.Id == userId)
                    .Select(u => (bool?)u.IsActive)
                    .FirstOrDefaultAsync();
                isActive = flag == true;
                cache.Set(cacheKey, isActive, TimeSpan.FromSeconds(30));
            }

            if (!isActive)
            {
                await context.SignOutAsync(IdentityConstants.ApplicationScheme);
                context.Response.Redirect("/");
                return;
            }
        }
    }
    await next();
});
app.UseAuthorization();

// panel Hangfire po uwierzytelnieniu, żeby filtr widział zalogowanego użytkownika
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireAdminFilter() }
});
app.MapControllerRoute(
    name: "pages",
    pattern: "Texts/Strona{PageNumber}",
    defaults: new { controller = "Texts", action = "Index" });
app.MapControllerRoute(
    name: "texts",
    pattern: "Texts",
    defaults: new { controller = "Texts", action = "Index" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

try
{
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Aplikacja zatrzymana z powodu nieobsłużonego błędu");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

static bool IsRateLimitedRequest(HttpRequest request)
{
    if (!HttpMethods.IsPost(request.Method)) return false;

    string[] sensitivePaths =
    {
        "/Identity/Account/Login",
        "/Identity/Account/LoginWith2fa",
        "/Identity/Account/LoginWithRecoveryCode",
        "/Identity/Account/ForgotPassword",
        "/Identity/Account/ResetPassword",
        "/Identity/Account/ResendEmailConfirmation",
        "/Demo/Start"
    };
    return sensitivePaths.Any(path => request.Path.StartsWithSegments(path));
}
