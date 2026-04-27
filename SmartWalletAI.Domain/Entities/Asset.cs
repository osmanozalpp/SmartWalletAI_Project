using SmartWalletAI.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalletAI.Domain.Entities
{
    public class Asset : BaseEntity
    {
        public Guid UserId { get; set; }
        public AssetType Type { get; set; }
        public decimal Amount { get; set; }
        public decimal AverageCost { get; set; }

        protected Asset() { }

        public Asset(Guid userId , AssetType type)
        {
            UserId = userId;
            Type = type;
            Amount = 0;
            AverageCost = 0;
        }
        public void AddAmount(decimal additionalAmount , decimal unitPrice)
        {
            if(additionalAmount <= 0) throw new ArgumentException("Eklenen miktar sıfırdan büyük olmalıdır.");

            var totalCost = (Amount * AverageCost) + (additionalAmount * unitPrice);
            Amount += additionalAmount;
            AverageCost = totalCost / Amount;
            UpdatedDate = DateTime.UtcNow.AddHours(3);
        }
        public void RemoveAmount(decimal amountToRemove)
        {
            if (amountToRemove <= 0) throw new ArgumentException("Çıkarılan miktar sıfırdan büyük olmalıdır.");
            if (amountToRemove > Amount) throw new InvalidOperationException("Yetersiz varlık bakiyesi.");

            Amount -= amountToRemove;
            UpdatedDate = DateTime.UtcNow.AddHours(3);
        }
    }
}
