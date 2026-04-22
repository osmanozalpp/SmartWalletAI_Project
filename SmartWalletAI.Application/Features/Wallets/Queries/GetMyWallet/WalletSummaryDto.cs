using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Features.Wallets.Queries.GetMyWallet
{
    public class WalletSummaryDto
    {
        public Guid Id { get; set; }
        public string IBAN { get; set; }
        public decimal Balance { get; set; }

        public string FullName { get; set; }

    }
}
