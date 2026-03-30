using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartWalletAI.Application.Common.Interfaces;
using SmartWalletAI.Infrastructure.Persistence;
using SmartWalletAI.Infrastructure.Persistence.Configurations;
using SmartWalletAI.Infrastructure.Services;

namespace SmartWalletAI.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            services.AddScoped<ITokenService, TokenService>();
            
            services.AddScoped<IEmailService, SmtpEmailService>();

            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
}