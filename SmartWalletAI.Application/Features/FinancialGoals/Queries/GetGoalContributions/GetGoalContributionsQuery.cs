using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Features.FinancialGoals.Queries.GetGoalContributions
{
    public record GetGoalContributionsQuery(Guid GoalId) : IRequest<List<GoalContributionDto>>;
}
