using SmartWalletAI.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalletAI.Domain.Entities
{
    public class TransactionHistory :BaseEntity 
    {
        public Guid UserId { get; set; }
        public AssetType AssetType { get; set; }
        public TransactionType TransactionType { get; set; }
        public decimal Amount { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }

        protected TransactionHistory() { }

        public TransactionHistory(Guid userId, AssetType assetType, TransactionType transactionType, decimal amount, decimal unitPrice)
        {
            UserId = userId;
            AssetType = assetType;
            TransactionType = transactionType;
            Amount = amount;
            TotalPrice = amount * unitPrice;

        }

    }
}
