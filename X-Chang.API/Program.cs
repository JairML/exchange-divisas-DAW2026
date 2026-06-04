using Microsoft.EntityFrameworkCore;
using X_Chang.CORE.Core.Interfaces;
using X_Chang.CORE.Core.Services;
using X_Chang.CORE.Infrastructure.Data;
using X_Chang.CORE.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Billetera
builder.Services.AddScoped<IBilleteraConsultaRepository, BilleteraConsultaRepository>();
builder.Services.AddScoped<IBilleteraConsultaService, BilleteraConsultaService>();
builder.Services.AddScoped<IDepositosRepository, DepositosRepository>();
builder.Services.AddScoped<IDepositosService, DepositosService>();
builder.Services.AddScoped<IRetirosRepository, RetirosRepository>();
builder.Services.AddScoped<IRetirosService, RetirosService>();

// Catálogos
builder.Services.AddScoped<IMonedasRepository, MonedasRepository>();
builder.Services.AddScoped<IMonedasService, MonedasService>();
builder.Services.AddScoped<IPaisesRepository, PaisesRepository>();
builder.Services.AddScoped<IPaisesService, PaisesService>();
builder.Services.AddScoped<IParesMonedaRepository, ParesMonedaRepository>();
builder.Services.AddScoped<IParesMonedaService, ParesMonedaService>();
builder.Services.AddScoped<IMetodosPagoRepository, MetodosPagoRepository>();
builder.Services.AddScoped<IMetodosPagoService, MetodosPagoService>();

// Auth (US-001 / US-002)
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();

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
builder.Services.AddOpenApi();

builder.Services.AddDbContext<ExchangeDivisasDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<ISesionUsuarioRepository, SesionUsuarioRepository>();
builder.Services.AddScoped<IConfiguracionUsuarioService, ConfiguracionUsuarioService>();
builder.Services.AddScoped<ICompraInmediataRepository, CompraInmediataRepository>();
builder.Services.AddScoped<ICompraInmediataService, CompraInmediataService>();
builder.Services.AddScoped<IVentaInmediataRepository, VentaInmediataRepository>();
builder.Services.AddScoped<IVentaInmediataService, VentaInmediataService>();
builder.Services.AddScoped<IGestionUsuariosAdminRepository, GestionUsuariosAdminRepository>();
builder.Services.AddScoped<IGestionUsuariosAdminService, GestionUsuariosAdminService>();
builder.Services.AddScoped<IAuditoriaAdministrativaRepository, AuditoriaAdministrativaRepository>();
builder.Services.AddScoped<IAuditoriaAdministrativaService, AuditoriaAdministrativaService>();

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