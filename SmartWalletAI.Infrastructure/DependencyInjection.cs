using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartWalletAI.Application.Common.Interfaces;
using SmartWalletAI.Infrastructure.Persistence;
using SmartWalletAI.Infrastructure.Persistence.Configurations;
using SmartWalletAI.Infrastructure.Services;
using System;

namespace SmartWalletAI.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            services.Configure<AiSettings>(configuration.GetSection("AiSettings"));
            services.AddHttpClient<IAiService, GeminiAiService>(client =>
            {
                client.BaseAddress = new Uri("https://generativelanguage.googleapis.com/");
            });

            
            services.AddHttpClient<IMarketDataService, CollectApiMarketService>();

         
            services.AddScoped<IMarketPriceManager, MarketPriceManager>();

            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IEmailService, SmtpEmailService>();

            return services;
        }
    }
}