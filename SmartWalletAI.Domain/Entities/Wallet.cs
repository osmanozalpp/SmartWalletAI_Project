using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalletAI.Domain.Entities
{
    public class Wallet :BaseEntity
    {
        public Guid UserId { get; set; }
        public string IBAN { get; set; }= string.Empty;
        public decimal Balance { get; set; } = 0;
        public User User { get; set; } = null!;

        public void Withdraw(decimal amount)
        {
            if (amount <= 0) throw new ArgumentException("Tutar 0'dan büyük olmalıdır.");
            if (Balance < amount) throw new InvalidOperationException("Yetersiz bakiye.");

            Balance -= amount;
            UpdatedDate = DateTime.UtcNow.AddHours(3);
        }

        // Para Ekleme / Alma
        public void Deposit(decimal amount)
        {
            if (amount <= 0) throw new ArgumentException("Tutar 0'dan büyük olmalıdır.");

            Balance += amount;
            UpdatedDate = DateTime.UtcNow.AddHours(3);
        }
    }
}
