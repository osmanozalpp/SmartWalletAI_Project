using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Features.Auth.Commands.ResendVerificationCode
{
    public class ResendVerificationCodeCommandValidator : AbstractValidator<ResendVerificationCodeCommand>
    {
        public ResendVerificationCodeCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email kısmı boş bırakılamaz.")
                .EmailAddress().WithMessage("Geçerli bir email adresi giriniz.");
        }
    }
}
