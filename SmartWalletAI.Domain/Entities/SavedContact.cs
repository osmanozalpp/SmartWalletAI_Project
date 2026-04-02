using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalletAI.Domain.Entities
{
    public class SavedContact : BaseEntity
    {
        public Guid UserId { get; set; }
        public string ContactName { get; set; } = string.Empty;
        public string Iban { get; set; } = string.Empty;
        public bool IsFavorite { get; set; } = false; //true ise favori false ise kayıtlı 

    }
}
