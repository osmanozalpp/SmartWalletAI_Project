using MediatR;
using SmartWalletAI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Features.Portfolios.Queries.GetInvestmentHistory
{
    public class GetInvestmentHistoryQuery : IRequest<InvestmentHistoryDto>
    {
        public Guid UserId { get; set; }

        public GetInvestmentHistoryQuery(Guid userId)
        {
            UserId = userId;
        }
    }
}
