using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Features.Wallets.Queries.GetTransactionDetail
{
    public class GetTransactionDetailQuery : IRequest<TransactionReceiptDto>
    {
        public Guid TransactionId { get; set; }

        [JsonIgnore]
        public Guid UserId { get; set; }
    }
}
