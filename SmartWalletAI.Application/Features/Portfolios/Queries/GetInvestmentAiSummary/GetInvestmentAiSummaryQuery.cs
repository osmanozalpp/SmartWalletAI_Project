using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Features.Portfolios.Queries.GetInvestmentAiSummary
{
    public record GetInvestmentAiSummaryQuery(Guid UserId) : IRequest<string>;
}
