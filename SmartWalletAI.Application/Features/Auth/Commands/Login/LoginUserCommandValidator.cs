using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;

namespace SmartWalletAI.Application.Features.Auth.Commands.Login
{
    public class LoginUserCommandValidator : AbstractValidator<LoginUserCommand>
    {
        public LoginUserCommandValidator()
        {
            RuleFor(p => p.Email)
              .NotEmpty().WithMessage("Email boş bırakılamaz.")
              .EmailAddress().WithMessage("Geçerli bir email formatı giriniz.");

            RuleFor(p => p.Password)
                .NotEmpty().WithMessage("Şifre boş bırakılamaz");
        }

    }
}
