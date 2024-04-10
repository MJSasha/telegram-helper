using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TelegramHelper.Infrastructure.Interfaces;
using TelegramHelper.Infrastructure.Repositories;
using TelegramHelper.Infrastructure.Services;

namespace TelegramHelper.Infrastructure;

public static class ServiceProvider
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AppDbContext>(options =>
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)),
            ServiceLifetime.Transient
        );

        services
            .AddTransient<NotesRepository>()
            .AddTransient<CategoriesRepository>()
            .AddTransient<ICategoriesService, CategoriesService>()
            .AddTransient<INotesService, NotesService>()
            ;

        return services;
    }
}