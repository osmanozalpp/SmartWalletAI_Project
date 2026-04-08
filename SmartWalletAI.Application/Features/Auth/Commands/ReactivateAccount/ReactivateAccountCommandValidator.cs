using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Features.Auth.Commands.ReactivateAccount
{
    public class ReactivateAccountCommandValidator :AbstractValidator<ReactivateAccountCommand>
    {
        public ReactivateAccountCommandValidator()
        {
            RuleFor(x => x.Email)
             .NotEmpty().WithMessage("Email alanı boş bırakılamaz.")
             .EmailAddress().WithMessage("Geçerli bir email adresi giriniz.");
        }
    }
}
