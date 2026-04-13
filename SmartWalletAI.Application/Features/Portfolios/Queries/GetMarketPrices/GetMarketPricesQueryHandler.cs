using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartWalletAI.Application.Common.DTOs;
using SmartWalletAI.Application.Common.Interfaces;
using SmartWalletAI.Domain.Entities;

namespace SmartWalletAI.Application.Features.Portfolios.Queries.GetMarketPrices
{
    public class GetMarketPricesQueryHandler : IRequestHandler<GetMarketPricesQuery, List<MarketPriceDto>>
    {
        private readonly IRepository<MarketPrice> _marketPriceRepository;
        private readonly IMarketPriceManager _marketPriceManager; 

        public GetMarketPricesQueryHandler(
            IRepository<MarketPrice> marketPriceRepository,
            IMarketPriceManager marketPriceManager)
        {
            _marketPriceRepository = marketPriceRepository;
            _marketPriceManager = marketPriceManager;
        }

        public async Task<List<MarketPriceDto>> Handle(GetMarketPricesQuery request, CancellationToken cancellationToken)
        {
            
            await _marketPriceManager.SyncPricesAsync(cancellationToken);

           
            var marketPrices = await _marketPriceRepository.GetAllAsQueryable().ToListAsync(cancellationToken);

           
            return marketPrices.Select(m => new MarketPriceDto
            {
                AssetType = m.Type,
                BuyPrice = m.CurrentBuyPrice,
                SellPrice = m.CurrentSellPrice,
                DailyChangePercentage = m.DailyChangePercentage,
            }).ToList();
        }
    }
}