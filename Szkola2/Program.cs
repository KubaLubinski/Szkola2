using Hangfire;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Szkola2.Data;
using Szkola2.Models;
using static Szkola2.Controllers.JobsController;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<AppUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();
builder.Services.AddHangfire((sp, config) =>
    {
        config.UseSqlServerStorage(connectionString);
    });
builder.Services.AddHangfireServer();
builder.Services.BuildServiceProvider().GetRequiredService<IGlobalConfiguration>();

RecurringJob.AddOrUpdate<DataTransferJob>("CopyAndArchiveTexts", job => job.CopyAndArchiveTextsAsync(), "0 0 1 1 *");

//RecurringJob.AddOrUpdate<DataTransferJob>("CopyAndArchiveTexts",jobjob.CopyAndArchiveTextsAsync(),Cron.Yearly, TimeZoneInfo.Local);


builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded = context => true;
    options.MinimumSameSitePolicy = SameSiteMode.None;
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    SzkolaSeeder.Initialize(services);
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
app.UseStaticFiles();

app.UseCookiePolicy();
app.UseHangfireDashboard();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
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

app.Run();
