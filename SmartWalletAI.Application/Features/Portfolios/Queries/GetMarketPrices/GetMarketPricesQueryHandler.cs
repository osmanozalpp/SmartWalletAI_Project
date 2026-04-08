using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartWalletAI.Application.Common.DTOs;
using SmartWalletAI.Application.Common.Interfaces;
using SmartWalletAI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Features.Portfolios.Queries.GetMarketPrices
{
    public class GetMarketPricesQueryHandler : IRequestHandler<GetMarketPricesQuery, List<MarketPriceDto>>
    {
        private readonly IRepository<MarketPrice> _marketPriceRepository;

        public GetMarketPricesQueryHandler(IRepository<MarketPrice> marketPriceRepository)
        {
            _marketPriceRepository = marketPriceRepository;
        }
        public async Task<List<MarketPriceDto>> Handle(GetMarketPricesQuery request, CancellationToken cancellationToken)
        {
            var marketPrices = await _marketPriceRepository.GetAllAsQueryable().ToListAsync(cancellationToken);

            var result = marketPrices.Select(m => new MarketPriceDto
            {
                AssetType = m.Type,
                BuyPrice = m.CurrentBuyPrice,
                SellPrice = m.CurrentSellPrice
            }).ToList();
            return result;
        }
    }
}
