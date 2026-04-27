using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartWalletAI.Application.Common.Interfaces;
using SmartWalletAI.Infrastructure.Persistence;
using SmartWalletAI.Infrastructure.Persistence.Configurations;
using SmartWalletAI.Infrastructure.Services;
using Microsoft.Extensions.Http.Resilience;
using System;
using Polly;

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
            })
            .AddStandardResilienceHandler(options =>
            {
                options.Retry.MaxRetryAttempts = 3;
                options.Retry.BackoffType = DelayBackoffType.Exponential;
                options.Retry.Delay = TimeSpan.FromSeconds(2);

                options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);
                options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(10);
            });
            

            services.AddHttpClient<IMarketDataService, CollectApiMarketService>();

         
            services.AddScoped<IMarketPriceManager, MarketPriceManager>();

            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IEmailService, SmtpEmailService>();

            return services;
        }
    }
}