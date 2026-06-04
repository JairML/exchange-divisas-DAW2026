using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using X_Chang.CORE.Core.Interfaces;
using X_Chang.CORE.Core.Services;
using X_Chang.CORE.Core.Settings;
using X_Chang.CORE.Infrastructure.Data;
using X_Chang.CORE.Interfaces;
using X_Chang.CORE.Services;

namespace X_Chang.CORE.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCoreServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ExchangeDivisasDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sql => sql.EnableRetryOnFailure(3)));

        services.Configure<SessionSettings>(
            configuration.GetSection(nameof(SessionSettings)));

        services.AddScoped<IOrdenService, OrdenService>();
        services.AddScoped<IOfertaService, OfertaService>();
        services.AddScoped<MatchingService>();

        return services;
    }
}