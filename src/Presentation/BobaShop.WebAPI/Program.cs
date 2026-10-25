using BobaShop.Persistence;

var builder = WebApplication.CreateBuilder(args);

// 1. Cấu hình Controllers & Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 2. Cấu hình Middleware Pipeline
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "BobaShop API v1");
    c.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();

// 3. Cấu hình phục vụ file tĩnh (wwwroot / index.html)
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthorization();
app.MapControllers();

app.Run();