using courseProject.Data;
using courseProject.Services;
using courseProject.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Read connection string (prefer appsettings, fallback to env or a local default)
string _connectionString = builder.Configuration.GetConnectionString("Default")
    ?? Environment.GetEnvironmentVariable("DEFAULT_CONN")
    ?? "server=127.0.0.1;port=3306;user=root;password=;database=courseproject";

if (builder.Configuration.GetConnectionString("Default") == null)
{
    Console.WriteLine("Warning: Connection string 'Default' not found in configuration; using fallback. " +
                      "Set ConnectionStrings:Default in appsettings.json or DEFAULT_CONN env var to use production DB.");
}

builder.Services.AddDbContextPool<AppDbContext>(options =>
{
    // only try to configure MySQL when connection string is non-empty
    if (!string.IsNullOrWhiteSpace(_connectionString))
    {
        options.UseMySQL(_connectionString);
    }
});

// Read JWT settings with safe fallbacks
var jwtSettings = builder.Configuration.GetSection("Jwt");
var jwtSecret = jwtSettings["SecretKey"] ?? builder.Configuration["Jwt:SecretKey"] ?? Environment.GetEnvironmentVariable("JWT_SECRET") ?? "dev_secret_change_in_production";
var jwtIssuer = jwtSettings["Issuer"] ?? "courseProjectIssuer";
var jwtAudience = jwtSettings["Audience"] ?? "courseProjectAudience";
var expirationMinutesStr = jwtSettings["ExpirationMinutes"] ?? "60";
int jwtExpirationMinutes = 60;
int.TryParse(expirationMinutesStr, out jwtExpirationMinutes);
var secretKey = Encoding.UTF8.GetBytes(jwtSecret);

// Configure authentication: default = Cookie (server-side sessions), also enable JWT for API usage
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
    options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest;
    options.LoginPath = "/"; // we use modal login
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(secretKey),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,
        RoleClaimType = ClaimTypes.Role
    };
});

builder.Services.AddAuthorization();

// Add Services
builder.Services.AddScoped<AuthService>();

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// --- Инициализация БД и сидирование учётных записей ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        context.Database.EnsureCreated();

        var authService = services.GetRequiredService<AuthService>();

        // demo credentials from config or defaults
        var demo = builder.Configuration.GetSection("DemoAccounts");
        var admin1Email = demo["Admin1Email"] ?? "admin1@local";
        var admin1Pass = demo["Admin1Password"] ?? "Admin123!";
        var admin2Email = demo["Admin2Email"] ?? "admin2@local";
        var admin2Pass = demo["Admin2Password"] ?? "Admin123!";
        var client1Email = demo["Client1Email"] ?? "client1@local";
        var client1Pass = demo["Client1Password"] ?? "Client123!";
        var client2Email = demo["Client2Email"] ?? "client2@local";
        var client2Pass = demo["Client2Password"] ?? "Client123!";

        // helper to create user if missing
        async Task EnsureUser(string email, string pass, string fullName, string role)
        {
            if (!context.Users.Any(u => u.Email == email))
            {
                try
                {
                    await authService.Register(new UserRegistrationRequest
                    {
                        Email = email,
                        Password = pass,
                        FullName = fullName,
                        Role = role
                    });
                }
                catch
                {
                    // ignore race conditions / concurrent creation
                }
            }
        }

        // create two admins
        EnsureUser(admin1Email, admin1Pass, "Админ 1", "admin").GetAwaiter().GetResult();
        EnsureUser(admin2Email, admin2Pass, "Админ 2", "admin").GetAwaiter().GetResult();

        // create two clients (roles: accountant/assistant)
        EnsureUser(client1Email, client1Pass, "Клиент 1", "assistant").GetAwaiter().GetResult();
        EnsureUser(client2Email, client2Pass, "Клиент 2", "assistant").GetAwaiter().GetResult();

        // optionally create simple client entries for clients (if Clients table exists)
        if (!context.Clients.Any(c => c.Email == client1Email))
        {
            context.Clients.Add(new Client { Name = "Client One", Email = client1Email, CreatedAt = DateTime.UtcNow });
        }
        if (!context.Clients.Any(c => c.Email == client2Email))
        {
            context.Clients.Add(new Client { Name = "Client Two", Email = client2Email, CreatedAt = DateTime.UtcNow });
        }
        context.SaveChanges();
    }
    catch (Exception ex)
    {
        Console.WriteLine("Warning: DB initialization error: " + ex.Message);
    }
}
// --- end initialization ---

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
