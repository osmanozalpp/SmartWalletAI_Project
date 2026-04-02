using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Features.Wallets.Commands.Queries.GetRecipients
{
    public class RecipientDto
    {
        public string FullName { get; set; }
        public string Iban { get; set; }
        public string Badge { get; set; } // Son işlem yapılan , Favori veya Kayıtlı
        public string Initials { get; set; } // İsim baş harfi
    }
}
