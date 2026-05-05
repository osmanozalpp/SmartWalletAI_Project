using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Features.FinancialGoals.Queries.GetFinancialGoalsSummary
{
    public record GetFinancialGoalsSummaryQuery(Guid UserId) : IRequest<FinancialGoalsSummaryDto>;
}
