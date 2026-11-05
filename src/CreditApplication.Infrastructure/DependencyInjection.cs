using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CreditApplication.Application.Abstractions.Authentication;
using CreditApplication.Application.Abstractions.Persistence;
using CreditApplication.Infrastructure.Authentication;
using CreditApplication.Infrastructure.Persistence;
using CreditApplication.Infrastructure.Persistence.Repositories;
using CreditApplication.Infrastructure.Persistence.Seeding;
using CreditApplication.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace CreditApplication.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString,
        IConfiguration configuration)
    {
        services.AddDbContext<CreditApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.Configure<JwtOptions>(
            configuration.GetSection(JwtOptions.SectionName));
        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<DemoUserSeeder>();
        services.AddScoped<ICreditApplicationRepository, CreditApplicationRepository>();

        return services;
    }
}
