using CozyCinema.Application.Auth;
using CozyCinema.Application.MovieLog;
using CozyCinema.Application.Movies;
using CozyCinema.Application.Sessions;
using CozyCinema.Application.Voting;
using CozyCinema.Infrastructure.Auth;
using CozyCinema.Infrastructure.MovieLog;
using CozyCinema.Infrastructure.Movies;
using CozyCinema.Infrastructure.Sessions;
using CozyCinema.Infrastructure.Voting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CozyCinema.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Supabase");

        services.AddDbContext<CozyCinemaDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ISessionService, SessionService>();
        services.AddScoped<IVotingService, VotingService>();
        services.AddScoped<IMovieLogService, MovieLogService>();

        services.Configure<TmdbOptions>(configuration.GetSection(TmdbOptions.SectionName));
        services.AddMemoryCache();
        services.AddHttpClient("TmdbClient", (provider, client) =>
        {
            var tmdbOptions = provider.GetRequiredService<IOptions<TmdbOptions>>().Value;
            client.BaseAddress = new Uri(tmdbOptions.BaseUrl.TrimEnd('/') + "/");
            client.Timeout = TimeSpan.FromSeconds(8);
        });
        services.AddScoped<ITmdbService, TmdbService>();

        return services;
    }
}
