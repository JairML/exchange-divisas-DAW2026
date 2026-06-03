using Microsoft.EntityFrameworkCore;
using X_Chang.API.Models;
using X_Chang.CORE.Core.Interfaces;
using X_Chang.CORE.Core.Services;
using X_Chang.CORE.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// Base de datos (SQL Server)
// La cadena "DevConnection" vive en appsettings.json. Cada integrante debe
// ajustar el "Server" a su propia instancia local de SQL Server.
// ---------------------------------------------------------------------------
builder.Services.AddDbContext<ExchangeDivisasDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DevConnection")));

// ---------------------------------------------------------------------------
// Inyección de dependencias (mismo patrón que el repo del profesor: AddTransient)
// Por cada historia: Repository + Service.
// ---------------------------------------------------------------------------
builder.Services.AddTransient<IMonedaRepository, MonedaRepository>();
builder.Services.AddTransient<IMonedaService, MonedaService>();

builder.Services.AddTransient<IBilleteraRepository, BilleteraRepository>();   // US-006
builder.Services.AddTransient<IBilleteraService, BilleteraService>();         // US-006

builder.Services.AddTransient<IDepositoRepository, DepositoRepository>();     // US-007
builder.Services.AddTransient<IDepositoService, DepositoService>();           // US-007

builder.Services.AddTransient<ICancelacionRepository, CancelacionRepository>(); // US-022
builder.Services.AddTransient<ICancelacionService, CancelacionService>();        // US-022

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
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("dev");

app.UseAuthorization();

app.MapControllers();

app.Run();
