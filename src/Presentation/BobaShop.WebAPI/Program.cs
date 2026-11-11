using BobaShop.Persistence;
using BobaShop.Persistence.Context;
using BobaShop.Persistence.Seed;
using BobaShop.Infrastructure;
using BobaShop.Application.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

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
{
    app.Logger.LogWarning("Không tìm thấy serviceAccountKey.json: bỏ qua Firebase, api/firebase/* trả 503.");
}

// 4. Development: tự Migrate + seed
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();
    await DbSeeder.SeedAsync(db, app.Configuration, scope.ServiceProvider.GetRequiredService<IPasswordHasher>());
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
