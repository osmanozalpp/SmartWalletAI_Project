using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Features.Wallets.Commands.TransferMoney
{
    public class TransferMoneyCommand : IRequest<TransferMoneyResponse>
    {
        [JsonIgnore]
        public Guid SenderId { get; set; }

        public string ReceiverIban { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }

    public class TransferMoneyResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public decimal NewBalance { get; set; }
    }
}
