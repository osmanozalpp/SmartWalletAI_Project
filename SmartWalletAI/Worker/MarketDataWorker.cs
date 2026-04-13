using SmartWalletAI.Application.Common.Interfaces;
using SmartWalletAI.Infrastructure.Services;

namespace SmartWalletAI.WebAPI.Workers
{
    public class MarketDataWorker : BackgroundService
    {
        private readonly ILogger<MarketDataWorker> _logger;

        //MarketDataWorker -> Singleton(uzun ömürlü) , MarketPriceManager -> Scoped(kısa ömürlü)
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
                await SyncPricesInternalAsync(stoppingToken);
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
                    await SyncPricesInternalAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Market verileri periyodik olarak güncellenirken hata oluştu.");
                }
            }
        }
        private async Task SyncPricesInternalAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Piyasa verileri için senkronizasyon işlemi başlatıldı...");

            // Scoped servisleri Singleton (Worker) içinde kullanmak için scope oluşturuyoruz
            using var scope = _scopeFactory.CreateScope();

           
            var marketPriceManager = scope.ServiceProvider.GetRequiredService<IMarketPriceManager>();

            // Tek bir satırla tüm veritabanını ve fiyatlarını güncelliyoruz
            await marketPriceManager.SyncPricesAsync(cancellationToken);

            _logger.LogInformation("Piyasa fiyatları başarıyla güncellendi (Açılış fiyatı kontrolü dahil).");
        }
    }
}