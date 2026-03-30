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
    }
}
