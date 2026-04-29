using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Features.FinancialGoals.Commands.CreateGoal
{
    public class CreateGoalCommandValidator : AbstractValidator<CreateGoalCommand>
    {
        public CreateGoalCommandValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Hedef başlığı boş olamaz kanka.")
                .MaximumLength(40).WithMessage("Başlık çok uzun, biraz kısaltalım (maks. 40 karakter).");

            RuleFor(x => x.TargetAmount)
                .GreaterThan(0).WithMessage("Hedef tutarı 0'dan büyük olmalı.");

            RuleFor(x => x.TargetDate)
                .Must(date => date > DateTime.UtcNow.AddHours(3))
                .WithMessage("Hedef tarihi bugünden ileride bir tarih olmalı.");

            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("Kullanıcı bilgisi eksik.");
        }
    }
}
