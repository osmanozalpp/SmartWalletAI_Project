using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Features.FinancialGoals.Commands.AddFunds
{
    public record AddFundsToGoalCommand(Guid GoalId, decimal Amount) : IRequest<bool>;
}
