using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartWalletAI.Application.Common.Interfaces;
using SmartWalletAI.Domain.Entities;
using SmartWalletAI.Domain.Enums;

namespace SmartWalletAI.Application.Features.Portfolios.Queries.GetPortfolioSummary
{
    public class GetPortfolioSummaryQueryHandler : IRequestHandler<GetPortfolioSummaryQuery, PortfolioSummaryDto>
    {
        private readonly IRepository<Asset> _assetRepository;
        private readonly IRepository<MarketPrice> _marketPriceRepository;

        public GetPortfolioSummaryQueryHandler(
            IRepository<Asset> assetRepository,
            IRepository<MarketPrice> marketPriceRepository)
        {
            _assetRepository = assetRepository;
            _marketPriceRepository = marketPriceRepository;
        }

        public async Task<PortfolioSummaryDto> Handle(GetPortfolioSummaryQuery request, CancellationToken cancellationToken)
        {
            var summary = new PortfolioSummaryDto();

            // 1. Kullanıcının varlıklarını getir (Repository üzerinden doğrudan veritabanı sorgusu)
            var userAssets = await _assetRepository.GetAllAsQueryable()
                .Where(a => a.UserId == request.UserId && a.Amount > 0)
                .ToListAsync(cancellationToken);

            if (!userAssets.Any())
                return summary; 

            // 2. Worker'ın güncellediği anlık piyasa fiyatlarını getir
            var currentMarketPrices = await _marketPriceRepository.GetAllAsQueryable()
                .ToListAsync(cancellationToken);

            decimal totalInvestedAmount = 0;
            decimal preciousMetalsTotalValue = 0;
            decimal fiatCurrencyTotalValue = 0;

            // 3. Her bir varlık için hesaplamaları yap
            foreach (var asset in userAssets)
            {
                var marketPrice = currentMarketPrices.FirstOrDefault(m => m.Type == asset.Type);
                if (marketPrice == null) continue;

                var currentPrice = marketPrice.CurrentBuyPrice;

                var assetTotalValue = asset.Amount * currentPrice;
                var assetTotalCost = asset.Amount * asset.AverageCost;
                var profitLoss = (currentPrice - asset.AverageCost) * asset.Amount;
                var profitLossPercentage = asset.AverageCost > 0 ? (profitLoss / assetTotalCost) * 100 : 0;

                summary.TotalPortfolioValue += assetTotalValue;
                totalInvestedAmount += assetTotalCost;
                summary.TotalProfitLoss += profitLoss;

                if (asset.Type == AssetType.Gold || asset.Type == AssetType.Silver)
                    preciousMetalsTotalValue += assetTotalValue;
                else if (asset.Type == AssetType.USD || asset.Type == AssetType.EUR)
                    fiatCurrencyTotalValue += assetTotalValue;

                summary.Assets.Add(new AssetDetailDto
                {
                    AssetType = asset.Type,
                    Amount = asset.Amount,
                    CurrentPrice = currentPrice,
                    AverageCost = asset.AverageCost,
                    TotalValue = assetTotalValue,
                    ProfitLoss = profitLoss,
                    ProfitLossPercentage = profitLossPercentage
                });
            }

            // 4. Genel Yüzdelik Kar/Zarar ve Dağılım Hesaplamaları
            if (totalInvestedAmount > 0)
            {
                summary.ProfitLossPercentage = (summary.TotalProfitLoss / totalInvestedAmount) * 100;
            }

            if (summary.TotalPortfolioValue > 0)
            {               
                summary.PreciousMetalsPercentage = (preciousMetalsTotalValue / summary.TotalPortfolioValue) * 100;
                summary.FiatCurrencyPercentage = (fiatCurrencyTotalValue / summary.TotalPortfolioValue) * 100;
            }

            return summary;
        }
    }
}