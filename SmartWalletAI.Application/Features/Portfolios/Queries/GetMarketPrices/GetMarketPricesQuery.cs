using MediatR;
using SmartWalletAI.Application.Common.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Features.Portfolios.Queries.GetMarketPrices
{
    public class GetMarketPricesQuery : IRequest<List<MarketPriceDto>>
    {
    }
}
