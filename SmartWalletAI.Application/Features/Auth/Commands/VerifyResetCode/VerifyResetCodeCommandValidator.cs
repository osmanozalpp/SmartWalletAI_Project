using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Features.Auth.Commands.VerifyResetCode
{
    public class VerifyResetCodeCommandValidator : AbstractValidator<VerifyResetCodeCommand>
    {
        public VerifyResetCodeCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email alanı boş bırakılamaz.")
                .EmailAddress().WithMessage("Geçerli bir email adresi giriniz.");

            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Kod alanı boş bırakılamaz.")
                .Length(6).WithMessage("Kod 6 karakter uzunluğunda olmalıdır.");
                
        }
    }
}
