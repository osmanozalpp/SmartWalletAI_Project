using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartWalletAI.Application.Common.Interfaces;
using SmartWalletAI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Features.Portfolios.Queries.GetInvestmentHistory
{
    public class GetInvestmentHistoryQueryHandler : IRequestHandler<GetInvestmentHistoryQuery, InvestmentHistoryDto>
    {
        private readonly IRepository<TransactionHistory> _transactionHistory;

        public GetInvestmentHistoryQueryHandler(IRepository<TransactionHistory> transactionHistory)
        {
            _transactionHistory = transactionHistory;
        }

        public async Task<InvestmentHistoryDto> Handle(GetInvestmentHistoryQuery request, CancellationToken cancellationToken)
        {
            var transactions = await _transactionHistory.GetAllAsQueryable()
                .AsNoTracking()
                .Where(t => t.UserId == request.UserId)
                .OrderByDescending(t => t.CreatedDate)
                .ToListAsync(cancellationToken);

            var transactionDtos = transactions.Select(t => new InvestmentTransactionItemDto
            {
                AssetName = GetAssetName((int)t.AssetType),
                TransactionType = (int)t.TransactionType == 1 ? "Alım" : "Satım",
                Amount = t.Amount,
                TotalPrice = t.TotalPrice,
                Date = t.CreatedDate
            }).ToList();

            return new InvestmentHistoryDto
            {
                Transactions = transactionDtos,
                MonthlyAiSummary = null
            };
        }

        private string GetAssetName(int assetType) => assetType switch
        {
            1 => "Altın (XAU)",
            2 => "Gümüş (XAG)",
            3 => "Dolar (USD)",
            4 => "Euro (EUR)",
            5 => "İngiliz Sterlini (GBP)",
            6 => "İsviçre Frangı (CHF)",
            7 => "Suudi Riyali (SAR)",
            8 => "Kuveyt Dinarı (KWD)",
            _ => "Bilinmeyen Varlık"
        };
    }
}