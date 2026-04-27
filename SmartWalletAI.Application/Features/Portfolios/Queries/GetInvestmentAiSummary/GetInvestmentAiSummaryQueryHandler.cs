using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartWalletAI.Application.Common.Interfaces;
using SmartWalletAI.Application.Features.Wallets.Queries.GetWalletTransactions;
using SmartWalletAI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Features.Portfolios.Queries.GetInvestmentAiSummary
{
    public class GetInvestmentAiSummaryQueryHandler : IRequestHandler<GetInvestmentAiSummaryQuery, string>
    {
        private readonly IRepository<TransactionHistory> _transactionHistory;
        private readonly IAiService _aiService;

        public GetInvestmentAiSummaryQueryHandler(IRepository<TransactionHistory> transactionHistory, IAiService aiService)
        {
            _transactionHistory = transactionHistory;
            _aiService = aiService;
        }

        public async Task<string> Handle(GetInvestmentAiSummaryQuery request, CancellationToken cancellationToken)
        {
            var now =DateTime.UtcNow.AddHours(3);


            var monthlyBuyVolume = await _transactionHistory.GetAllAsQueryable()
                .AsNoTracking()
                .Where(t => t.UserId == request.UserId &&
                            (int)t.TransactionType == 1 &&
                            t.CreatedDate.Month == now.Month &&
                            t.CreatedDate.Year == now.Year)
                .SumAsync(t => t.TotalPrice, cancellationToken);

            if (monthlyBuyVolume <= 0)
            {
                return "Bu ay henüz bir yatırım işlemi gerçekleştirmediniz. Geleceğiniz için ufak birikimlerle başlamaya ne dersiniz? 🚀";
            }

            string monthName = now.ToString("MMMM", new CultureInfo("tr-TR"));
            string prompt = $"[YATIRIM_ANALİZİ] Kullanıcı {monthName} ayında toplam {monthlyBuyVolume:N2} TL'lik varlık alımı yaptı. " +
                            "Kullanıcıyı motive eden, tek cümlelik, samimi bir özet yaz.";

            return await _aiService.GetFinancialAdviceAsync(prompt);
        }
    }
}
