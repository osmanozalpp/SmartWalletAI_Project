using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Features.Wallets.Commands.RemoveContact
{
    public class RemoveContactCommand : IRequest<bool>
    {
        public string Iban { get; set; }

        [JsonIgnore]
        public Guid UserId { get; set; }

    }
}
