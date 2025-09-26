using FIAP.Games.Application.Services;
using FIAP.Games.Domain.Interfaces;
using FIAP.Games.Infrastructure.Repositories;

namespace FiapGamesService.API.IoC
{
    public static class DependencyInjectionConfiguration
    {
        public static IServiceCollection AddDependencyInjection(this IServiceCollection services)
        {
            services.AddRepositories();
            services.AddServices();
            return services;
        }

        private static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IGameRepository, GameRepository>();
            return services;
        }

        private static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddScoped<GameService>();
            return services;
        }
    }
}
