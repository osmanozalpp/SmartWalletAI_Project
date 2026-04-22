using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Features.Wallets.Commands.Queries.GetTransactionDetailQuery
{
    public class TransactionReceiptDto
    {
        public string ReferenceNumber { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public DateTime TransactionDate { get; set; }
        public string CategoryName { get; set; }
        public string SenderName { get; set; }
        public string ReceiverName { get; set; }
        public string ReceiverIban { get; set; }
        public bool IsIncoming { get; set; }
    }
}
