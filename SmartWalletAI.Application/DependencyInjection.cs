using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using SmartWalletAI.Application.Features.Auth.Commands.Register;
using MediatR;
using SmartWalletAI.Application.Behaviors;

namespace SmartWalletAI.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
                
                cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            });

            return services;
        }
   }
}