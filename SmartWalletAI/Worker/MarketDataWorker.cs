using Microsoft.EntityFrameworkCore; 
using SmartWalletAI.Application.Common.Interfaces;
using SmartWalletAI.Infrastructure.Persistence; 
using SmartWalletAI.Domain.Entities;

namespace SmartWalletAI.WebAPI.Workers 
{
    public class MarketDataWorker : BackgroundService
    {
        private readonly ILogger<MarketDataWorker> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly TimeSpan _period = TimeSpan.FromMinutes(5);

        public MarketDataWorker(ILogger<MarketDataWorker> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("MarketDataWorker başlatıldı.");
           
            try
            {
                await FetchAndUpdateMarketDataAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Uygulama başlarken ilk market verileri çekilemedi.");
            }

            using PeriodicTimer timer = new PeriodicTimer(_period);
            
            while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    await FetchAndUpdateMarketDataAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Market verileri güncellenirken hata oluştu.");
                }
            }
        }
        private async Task FetchAndUpdateMarketDataAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Market verileri güncelleniyor...");

            using var scope = _scopeFactory.CreateScope();

            var marketDataService = scope.ServiceProvider.GetRequiredService<IMarketDataService>();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>(); 

            var currentPrices = await marketDataService.GetCurrentPricesAsync(cancellationToken);
           
            var existingPrices = await dbContext.MarketPrices.ToListAsync(cancellationToken);

            foreach (var newPrice in currentPrices)
            {
                var existingPrice = existingPrices.FirstOrDefault(x => x.Type == newPrice.AssetType);

                if (existingPrice == null)
                {
                    var marketPrice = new MarketPrice();
                    marketPrice.Type = newPrice.AssetType;
                    marketPrice.UpdatePrices(newPrice.BuyPrice, newPrice.SellPrice);

                    dbContext.MarketPrices.Add(marketPrice);
                }
                else
                {
                    existingPrice.UpdatePrices(newPrice.BuyPrice, newPrice.SellPrice);
                }
            }

            await dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Piyasa fiyatları başarıyla güncellendi ve veritabanına yazıldı.");
        }
    }
}