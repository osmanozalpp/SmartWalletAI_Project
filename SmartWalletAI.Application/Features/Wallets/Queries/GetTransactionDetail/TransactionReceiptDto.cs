using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Features.Wallets.Queries.GetTransactionDetail
{
    public class TransactionReceiptDto
    {
        public string ReferenceNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }
        public string Category { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        public string SenderIban { get; set; } = string.Empty;
        public string ReceiverName { get; set; } = string.Empty;
        public string ReceiverIban { get; set; } = string.Empty;
        public bool IsIncoming { get; set; }
    }
}
