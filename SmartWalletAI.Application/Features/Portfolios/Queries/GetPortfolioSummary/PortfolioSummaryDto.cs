using SmartWalletAI.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Features.Portfolios.Queries.GetPortfolioSummary
{
    public class PortfolioSummaryDto
    {
        public decimal TotalPortfolioValue { get; set; }
        public decimal TotalProfitLoss { get; set; }
        public decimal ProfitLossPercentage { get; set; }
        public decimal PreciousMetalsPercentage { get; set; }
        public decimal FiatCurrencyPercentage { get; set; }

        public List<AssetDetailDto> Assets { get; set; } = new();
    }

    public class AssetDetailDto
    {
        public AssetType AssetType { get; set; }
        public decimal Amount { get; set; }
        public decimal CurrentPrice { get; set; }
        public decimal AverageCost { get; set; }
        public decimal TotalValue { get; set; }
        public decimal ProfitLoss { get; set; }
        public decimal ProfitLossPercentage { get; set; }
    }
}
