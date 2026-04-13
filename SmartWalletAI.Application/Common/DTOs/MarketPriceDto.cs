using SmartWalletAI.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Common.DTOs
{
    public class MarketPriceDto
    {
        public AssetType AssetType { get; set; }
        public decimal BuyPrice { get; set; }
        public decimal SellPrice { get; set; }
        public double DailyChangePercentage { get; set; }

    }
}
