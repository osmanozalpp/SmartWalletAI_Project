using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalletAI.Domain.Enums
{
    public enum AssetType
    {
        Gold = 1,
        Silver = 2,
        USD = 3,
        EUR =4,
        GBP = 5,    
        CHF = 6,
        SAR = 7, 
        KWD = 8  
    }

    public enum TransactionType
    {
        Buy = 1,
        Sell = 2,
    }
}
