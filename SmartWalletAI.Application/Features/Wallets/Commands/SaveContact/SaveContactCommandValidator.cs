using FluentValidation;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Features.Wallets.Commands.SaveContact
{
    public class SaveContactCommandValidator : AbstractValidator<SaveContactCommand>
    {
        public SaveContactCommandValidator()
        {

            RuleFor(x => x.ContactName)
                .NotEmpty().WithMessage("Kayıt ismi boş bırakılamaz.")
                .MaximumLength(50).WithMessage("Kayıt ismi en fazla 50 karakter olabilir.");


            RuleFor(x => x.Iban)
                .NotEmpty().WithMessage("IBAN boş bırakılamaz.")
                .Length(26).WithMessage("Lütfen 26 haneli geçerli bir IBAN giriniz.")
                .Must(iban => iban.StartsWith("TR")).WithMessage("IBAN 'TR' ile başlamalıdır.");

        } 
        }
    }
