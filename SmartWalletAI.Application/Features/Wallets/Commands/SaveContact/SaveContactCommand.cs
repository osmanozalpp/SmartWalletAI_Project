using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Features.Wallets.Commands.SaveContact
{
    public class SaveContactCommand : IRequest<bool>
    {
        [JsonIgnore]
        public Guid UserId { get; set; }
        public string ContactName { get; set; } = string.Empty;
        public string Iban { get; set; } = string.Empty;
        public bool IsFavorite { get; set; }
    }
}
