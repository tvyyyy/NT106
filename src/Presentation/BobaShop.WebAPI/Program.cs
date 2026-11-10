using BobaShop.Persistence;
using BobaShop.Persistence.Context;
using BobaShop.Persistence.Seed;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. PostgreSQL (nguồn dữ liệu chính). Mật khẩu nằm trong User Secrets
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Thiếu ConnectionStrings:DefaultConnection.");
builder.Services.AddPersistence(connectionString);

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
    await DbSeeder.SeedAsync(db);
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

app.UseAuthorization();
app.MapControllers();

app.Run();