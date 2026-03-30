using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Common.Behaviors
{
    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
         where TRequest : notnull
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if(_validators.Any())
            {
                var context = new ValidationContext<TRequest>(request);

                //tüm kuralları çalıştır
                var validationResults= await Task.WhenAll(_validators.Select(v=>v.ValidateAsync(context ,cancellationToken)));

                //Hataları topla
                var failures = validationResults
                    .SelectMany(r=>r.Errors)
                    .Where(f=> f != null)
                    .ToList();

                // Eğer hata varsa Exception fırlat (İstek Handler'a gitmez!)
                if (failures.Count() != 0)
                    throw new ValidationException(failures);
            }
            return await next();
        }
    }
}
