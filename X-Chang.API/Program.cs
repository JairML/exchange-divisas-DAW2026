using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using X_Chang.API.Authentication;
using X_Chang.CORE.Core.Interfaces;
using X_Chang.CORE.Core.Services;
using X_Chang.CORE.Core.Settings;
using X_Chang.CORE.Infrastructure.Data;
using X_Chang.CORE.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTransient<ICancelacionRepository, CancelacionRepository>(); // US-022
builder.Services.AddTransient<ICancelacionService, CancelacionService>();        // US-022

// US-001 / US-002: autenticación por sesión opaca
builder.Services.Configure<SessionSettings>(builder.Configuration.GetSection("SessionSettings"));
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddAuthentication("Session")
    .AddScheme<AuthenticationSchemeOptions, SessionAuthHandler>("Session", null);

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

builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<ISesionUsuarioRepository, SesionUsuarioRepository>();
builder.Services.AddScoped<IConfiguracionUsuarioService, ConfiguracionUsuarioService>();

// US-004: visualización de órdenes y ofertas activas
builder.Services.AddScoped<IOrdenRepository, OrdenRepository>();
builder.Services.AddScoped<IOfertaRepository, OfertaRepository>();
builder.Services.AddScoped<IOrdenService, OrdenService>();
builder.Services.AddScoped<IOfertaService, OfertaService>();
builder.Services.AddScoped<ICompraInmediataRepository, CompraInmediataRepository>();
builder.Services.AddScoped<ICompraInmediataService, CompraInmediataService>();
builder.Services.AddScoped<ISesionUsuarioRepository, SesionUsuarioRepository>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("dev");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();