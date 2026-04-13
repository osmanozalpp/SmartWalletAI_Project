using Microsoft.EntityFrameworkCore;
using SmartWalletAI.Application.Common.Interfaces;
using SmartWalletAI.Domain.Entities;

namespace SmartWalletAI.Infrastructure.Services
{
    public class MarketPriceManager : IMarketPriceManager
    {
        private readonly IMarketDataService _apiService;
        private readonly IRepository<MarketPrice> _marketRepository;
        private readonly IUnitOfWork _unitOfWork;

        public MarketPriceManager(IMarketDataService apiService, IRepository<MarketPrice> marketRepository, IUnitOfWork unitOfWork)
        {
            _apiService = apiService;
            _marketRepository = marketRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task SyncPricesAsync(CancellationToken ct)
        {
            var apiPrices = await _apiService.GetCurrentPricesAsync(ct);
            var existingPrices = await _marketRepository.GetAllAsQueryable().ToListAsync(ct);

            foreach (var apiPrice in apiPrices)
            {
                var dbPrice = existingPrices.FirstOrDefault(x => x.Type == apiPrice.AssetType);

                if (dbPrice == null)
                {
                    var newPrice = new MarketPrice
                    {
                        Type = apiPrice.AssetType,
                        CurrentBuyPrice = apiPrice.BuyPrice,
                        CurrentSellPrice = apiPrice.SellPrice,
                        DailyChangePercentage = apiPrice.DailyChangePercentage,
                        LastUpdated = DateTime.Now
                    };
                    await _marketRepository.AddAsync(newPrice);
                    existingPrices.Add(newPrice);
                }
                else
                {
                    dbPrice.CurrentBuyPrice = apiPrice.BuyPrice;
                    dbPrice.CurrentSellPrice = apiPrice.SellPrice;
                    dbPrice.DailyChangePercentage = apiPrice.DailyChangePercentage;
                    dbPrice.LastUpdated = DateTime.Now;
                    await _marketRepository.UpdateAsync(dbPrice);
                }
            }
            await _unitOfWork.SaveChangesAsync(ct);
        }
    }
}