using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Features.Auth.Commands.ProfileChangeEmail
{
    public class UpdateEmailCommandValidator : AbstractValidator<UpdateEmailCommand>
    {
        public UpdateEmailCommandValidator()
        {
            RuleFor(x => x.NewEmail)
                .NotEmpty().WithMessage("Yeni e-posta adresi boş geçilemez.")
                .EmailAddress().WithMessage("Lütfen geçerli bir e-posta adresi giriniz.");

            RuleFor(x => x.ConfirmEmail)
                .NotEmpty().WithMessage("E-posta tekrar alanı boş geçilemez.")
                .Equal(x => x.NewEmail).WithMessage("Girdiğiniz e-posta adresleri birbiriyle eşleşmiyor.");

        }
    }
}
