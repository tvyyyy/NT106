using BobaShop.Persistence;
using BobaShop.Persistence.Context;
using BobaShop.Persistence.Seed;
using BobaShop.Persistence.Seeding;
using BobaShop.Infrastructure;
using BobaShop.Application.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Microsoft.EntityFrameworkCore;

// Tham số riêng của dữ liệu demo hệ khuyến nghị: dotnet run -- --seed hoặc --clear-seed (chỉ ở Development).
// Tách ra trước khi đưa args cho builder để cấu hình command line không hiểu nhầm.
const string SeedArgument = "--seed";
const string ClearSeedArgument = "--clear-seed";
var runSeed = args.Contains(SeedArgument);
var runClearSeed = args.Contains(ClearSeedArgument);
var builder = WebApplication.CreateBuilder(args.Where(a => a is not SeedArgument and not ClearSeedArgument).ToArray());
builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole();

// 1. PostgreSQL (nguồn dữ liệu chính). Mật khẩu nằm trong User Secrets
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Thiếu ConnectionStrings:DefaultConnection.");
builder.Services.AddPersistence(connectionString);
builder.Services.AddInfrastructure(builder.Configuration);
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtSettings = new AuthOptions
{
    Issuer = jwtSection["Issuer"] ?? "BobaShop",
    Audience = jwtSection["Audience"] ?? "BobaShop.Clients",
    Key = jwtSection["Key"] ?? string.Empty,
    ExpiryMinutes = int.TryParse(jwtSection["ExpiryMinutes"], out var expiryMinutes) ? expiryMinutes : 60
};
if (string.IsNullOrWhiteSpace(jwtSettings.Key) || Encoding.UTF8.GetByteCount(jwtSettings.Key) < 32)
    throw new InvalidOperationException("Jwt:Key cần được thiết lập qua User Secrets (ít nhất 32 byte).");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true, ValidIssuer = jwtSettings.Issuer,
        ValidateAudience = true, ValidAudience = jwtSettings.Audience,
        ValidateIssuerSigningKey = true, IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
        ValidateLifetime = true, ClockSkew = TimeSpan.FromSeconds(30), RoleClaimType = "role"
    };
    options.MapInboundClaims = false;
});
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Customer", policy => policy.RequireRole("Customer"));
    options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
});

// 2. Firebase (vai trò phụ): bỏ qua nếu thiếu serviceAccountKey.json
var firebaseKeyPath = Path.Combine(builder.Environment.ContentRootPath, "serviceAccountKey.json");
var firebaseEnabled = File.Exists(firebaseKeyPath);
if (firebaseEnabled)
{
#pragma warning disable CS0618
    var credential = GoogleCredential.FromFile(firebaseKeyPath);
#pragma warning restore CS0618

    if (FirebaseApp.DefaultInstance == null)
    {
        FirebaseApp.Create(new AppOptions { Credential = credential });
    }

    builder.Services.AddSingleton(new FirestoreDbBuilder
    {
        ProjectId = "bobashopfirebase",
        Credential = credential
    }.Build());
}

// 3. Controllers & Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (!firebaseEnabled)
    Console.WriteLine("Không tìm thấy serviceAccountKey.json: bỏ qua Firebase, api/firebase/* trả 503.");

// 4. Development: tự Migrate + seed
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();
    await DbSeeder.SeedAsync(db, app.Configuration, scope.ServiceProvider.GetRequiredService<IPasswordHasher>());

    if (runClearSeed)
        app.Logger.LogInformation("Đã xóa dữ liệu seed. {Summary}", await DataSeeder.ClearSeedAsync(db));
    if (runSeed)
    {
        // Mật khẩu chung của tài khoản seed đặt trong User Secrets, giống AdminSeed: không commit vào repo.
        var seedPassword = app.Configuration["SeedData:Password"];
        if (string.IsNullOrWhiteSpace(seedPassword))
            throw new InvalidOperationException("--seed cần SeedData:Password trong User Secrets.");
        app.Logger.LogInformation("Dữ liệu seed: {Summary}",
            await DataSeeder.SeedAsync(db, scope.ServiceProvider.GetRequiredService<IPasswordHasher>(), seedPassword));
    }
}
else if (runSeed || runClearSeed)
{
    app.Logger.LogWarning("Bỏ qua --seed/--clear-seed: chỉ chạy được ở môi trường Development.");
}

// 5. Middleware pipeline
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "BobaShop API v1");
    c.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
