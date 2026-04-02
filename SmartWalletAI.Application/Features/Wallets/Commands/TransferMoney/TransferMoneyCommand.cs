using MediatR;
using SmartWalletAI.Domain.Enums;
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

        public string? Description { get; set; }
        public TransactionCategory Category { get; set; }
    }

    public class TransferMoneyResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public decimal NewBalance { get; set; }

        public string ReferenceNumber { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }
        public string ReceiverName { get; set; } = string.Empty;
        public string ReceiverIban { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string? Description { get; set; }
    }
}
