using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Features.Portfolios.Queries.GetPortfolioSummary
{
    public class GetPortfolioSummaryQuery : IRequest<PortfolioSummaryDto>
    {
        public Guid UserId { get; set; }

        public GetPortfolioSummaryQuery(Guid userId)
        {
            UserId = userId;
        }
    }
    

}
