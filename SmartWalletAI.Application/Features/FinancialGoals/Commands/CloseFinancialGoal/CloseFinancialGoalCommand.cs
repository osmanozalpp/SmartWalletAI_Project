using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Features.FinancialGoals.Commands.CloseFinancialGoal
{
    public record CloseFinancialGoalCommand(Guid GoalId) : IRequest<bool>;
}
