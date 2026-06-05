using Microsoft.EntityFrameworkCore;
using X_Chang.API.Helpers;
using X_Chang.CORE.Core.Interfaces;
using X_Chang.CORE.Core.Services;
using X_Chang.CORE.Core.Settings;
using X_Chang.CORE.Infrastructure.Data;
using X_Chang.CORE.Infrastructure.Repositories;
using X_Chang.CORE.Infrastructure.Shared;
using X_Chang.CORE.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("dev", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<ExchangeDivisasDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositorios base
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<ISesionUsuarioRepository, SesionUsuarioRepository>();
builder.Services.AddScoped<IBilleteraRepository, BilleteraRepository>();
builder.Services.AddScoped<IMonedaRepository, MonedaRepository>();
builder.Services.AddScoped<IPreciosParRepository, PreciosParRepository>();
builder.Services.AddScoped<IHistorialTransaccionesRepository, HistorialTransaccionesRepository>();
builder.Services.AddScoped<INotificacionesCorreoRepository, NotificacionesCorreoRepository>();

// Servicios de dominio
builder.Services.AddScoped<IMatchingService, MatchingService>();
builder.Services.AddScoped<IBilleteraService, BilleteraService>();
builder.Services.AddScoped<IMonedaService, MonedaService>();
builder.Services.AddScoped<IPreciosParService, PreciosParService>();
builder.Services.AddScoped<IOrdenService, OrdenService>();
builder.Services.AddScoped<IOfertaService, OfertaService>();
builder.Services.AddScoped<IHistorialTransaccionesService, HistorialTransaccionesService>();
builder.Services.AddScoped<IConfiguracionUsuarioService, ConfiguracionUsuarioService>();

// US-018: notificaciones por correo electronico
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<INotificacionesCorreoService, NotificacionesCorreoService>();
builder.Services.AddHostedService<NotificacionesBackgroundService>();

// TODO: registrar cuando sus repositorios sean implementados por el equipo:
// builder.Services.AddScoped<IAuditoriaAdministrativaRepository, AuditoriaAdministrativaRepository>();
// builder.Services.AddScoped<IAuditoriaAdministrativaService, AuditoriaAdministrativaService>();
// builder.Services.AddScoped<ICancelacionRepository, CancelacionRepository>();
// builder.Services.AddScoped<ICancelacionService, CancelacionService>();
// builder.Services.AddScoped<ICompraInmediataRepository, CompraInmediataRepository>();
// builder.Services.AddScoped<ICompraInmediataService, CompraInmediataService>();
// builder.Services.AddScoped<IDepositoRepository, DepositoRepository>();
// builder.Services.AddScoped<IDepositoService, DepositoService>();
// builder.Services.AddScoped<IGestionUsuariosAdminRepository, GestionUsuariosAdminRepository>();
// builder.Services.AddScoped<IGestionUsuariosAdminService, GestionUsuariosAdminService>();
// builder.Services.AddScoped<IVentaInmediataRepository, VentaInmediataRepository>();
// builder.Services.AddScoped<IVentaInmediataService, VentaInmediataService>();

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
