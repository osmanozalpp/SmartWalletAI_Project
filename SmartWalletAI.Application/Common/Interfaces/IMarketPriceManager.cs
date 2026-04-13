using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Common.Interfaces
{
    public interface IMarketPriceManager
    {
        Task SyncPricesAsync(CancellationToken ct);
    }
}
