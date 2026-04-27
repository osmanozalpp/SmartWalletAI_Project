    using SmartWalletAI.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalletAI.Domain.Entities
{
    public class MarketPrice : BaseEntity
    {
      
            public AssetType Type { get; set; }
            public decimal CurrentBuyPrice { get; set; }
            public decimal CurrentSellPrice { get; set; }
            public double DailyChangePercentage { get; set; } // API'den gelen hazır yüzde (%)
            public DateTime LastUpdated { get; set; }
        

        public void UpdatePrices(decimal buyPrice , decimal sellPrice)
        {
            CurrentBuyPrice = buyPrice;
            CurrentSellPrice = sellPrice;
            UpdatedDate = DateTime.UtcNow.AddHours(3);
        }
    }
}
