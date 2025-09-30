using FiapGamesService.Application.Mappings;
using FiapGamesService.Application.Services;
using FiapGamesService.Domain.Interfaces;
using FiapGamesService.Infrastructure.Repositories;

namespace FiapGamesService.API.IoC
{
    public static class DependencyInjectionConfiguration
    {
        public static IServiceCollection AddDependencyInjection(this IServiceCollection services)
        {
            services.AddRepositories();
            services.AddServices();
            services.AddMappers();
            return services;
        }

        private static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IGameCreatedEventRepository, GameCreatedEventRepository>();
            services.AddScoped<IGameChangedEventRepository, GameChangedEventRepository>();

            return services;
        }

        private static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddScoped<GameService>();
            return services;
        }

        private static IServiceCollection AddMappers(this IServiceCollection services)
        {
            services.AddAutoMapper(cfg =>
            {
                cfg.AddMaps(typeof(GameMapper).Assembly);
            });
            return services;
        }
    }
}
