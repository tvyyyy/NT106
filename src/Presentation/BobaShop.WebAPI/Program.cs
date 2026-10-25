using BobaShop.Persistence;

var builder = WebApplication.CreateBuilder(args);

// 1. Cấu hình kết nối Cơ sở dữ liệu PostgreSQL[cite: 2]
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Thiếu ConnectionStrings:DefaultConnection");
builder.Services.AddPersistence(connectionString);

// 2. Cấu hình Controllers & OpenApi chuẩn của .NET
builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// 3. Cấu hình Middleware Pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// 4. CẤU HÌNH HIỆN TRANG WEB INDEX.HTML
app.UseDefaultFiles(); // Tự động load file index.html trong wwwroot
app.UseStaticFiles();  // Cho phép load file tĩnh HTML/CSS/JS

app.UseAuthorization();
app.MapControllers();

app.Run();