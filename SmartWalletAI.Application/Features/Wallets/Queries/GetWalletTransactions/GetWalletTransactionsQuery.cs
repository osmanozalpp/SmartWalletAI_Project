using MediatR;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Features.Wallets.Queries.GetWalletTransactions
{
    public class GetWalletTransactionsQuery : IRequest<List<TransactionDto>>
    {
        [JsonIgnore]
        public Guid WalletId { get; set; }
        public Guid UserId { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 5;

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
