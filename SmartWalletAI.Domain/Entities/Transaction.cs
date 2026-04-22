using SmartWalletAI.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SmartWalletAI.Domain.Entities
{
    public class Transaction : BaseEntity
    {
        public Guid SenderWalletId { get; set; }
        public Guid ReceiverWalletId { get; set; }
        public Decimal Amount { get; set; }
        
        public DateTime TransactionDate { get; set; }
        public string? Description { get; set; }

        public TransactionCategory Category { get; set; }
        public Wallet SenderWallet { get; set; }
        public Wallet ReceiverWallet { get; set; }

        public string ReferenceNumber { get; set; } = string.Empty;

    }
}
