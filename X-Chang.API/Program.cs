using Microsoft.EntityFrameworkCore;
using X_Chang.CORE.Core.Interfaces;
using X_Chang.CORE.Core.Services;
using X_Chang.CORE.Infrastructure.Data;
using X_Chang.CORE.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);


// ---------------------------------------------------------------------------
// CORS para el frontend (Vue). En desarrollo se permite cualquier origen;
// para producción conviene restringirlo a la URL real del front.
// ---------------------------------------------------------------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("dev", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<ExchangeDivisasDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors("dev");

app.UseAuthorization();

app.MapControllers();

app.Run();