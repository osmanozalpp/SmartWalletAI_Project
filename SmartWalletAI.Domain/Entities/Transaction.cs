using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SmartWalletAI.Domain.Entities
{
    public class Transaction
    {
        
        public Guid Id { get; set; }
        public Guid SenderWalletId { get; set; }
        public Guid ReceiverWalletId { get; set; }
        public Decimal Amount { get; set; }
        public DateTime TransactionTime { get; set; }
        public string Description { get; set; }

    }
}
