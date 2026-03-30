using FluentValidation;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Features.Wallets.Commands.TransferMoney
{
    public class TransferMoneyCommandValidator : AbstractValidator<TransferMoneyCommand>
    {
        public TransferMoneyCommandValidator()
        {
            RuleFor(p => p.Amount)
                .GreaterThan(0).WithMessage("Gönderilecek tutar 0 ' dan büyük olmalıdır.");

            RuleFor(p => p.ReceiverIban)
                .NotEmpty().WithMessage("Alıcı iban kısmı boş bırakılamaz.")
                .Length(26).WithMessage("Geçersiz iban formatı")
                .Matches("^TR[0-9]{24}$").WithMessage("IBAN 'TR' ile başlamalı ve sadece rakamlardan oluşmalıdır.");

            RuleFor(p => p.SenderId)
                .NotEmpty().WithMessage("Gönderen bilgisi eksik.");
        }
    }
}
