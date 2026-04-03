using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Features.Wallets.Commands.RemoveContact
{
    public class RemoveContactCommandValidator : AbstractValidator<RemoveContactCommand>
    {
        public RemoveContactCommandValidator()
        {
            RuleFor(x => x.Iban)
                .NotEmpty().WithMessage("IBAN alanı boş geçilemez.")
                .Length(26).WithMessage("Geçersiz IBAN formatı."); // Türkiye standartları için
        }
    }
}
