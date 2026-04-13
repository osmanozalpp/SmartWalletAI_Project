using SmartWalletAI.Application.Common.DTOs;
using SmartWalletAI.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Common.Interfaces
{
    public interface IMarketDataService
    {
        Task<List<MarketPriceDto>> GetCurrentPricesAsync(CancellationToken cancellationToken);
    }
}

