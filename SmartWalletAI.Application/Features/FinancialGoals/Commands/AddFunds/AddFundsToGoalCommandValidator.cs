using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Features.FinancialGoals.Commands.AddFunds
{
    public class AddFundsToGoalCommandValidator :AbstractValidator<AddFundsToGoalCommand>
    {
        public AddFundsToGoalCommandValidator()
        {
            RuleFor(x => x.GoalId)
                .NotEmpty().WithMessage("Hangi hedefe para eklediğini belirtmelisin.");

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Eklemek istediğin tutar 0'dan büyük olmalı.");
        }
    }
}
