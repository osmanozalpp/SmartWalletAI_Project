using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartWalletAI.Application.Common.Interfaces;
using SmartWalletAI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Features.Portfolios.Queries.GetInvestmentHistory
{
    public class GetInvestmentHistoryQueryHandler : IRequestHandler<GetInvestmentHistoryQuery, InvestmentHistoryDto>
    {
        private readonly IRepository<TransactionHistory> _transactionHistory;
        private readonly IAiService _aiService;
      
        public GetInvestmentHistoryQueryHandler(IRepository<TransactionHistory> transactionHistory, IAiService aiService)
        {
            _transactionHistory = transactionHistory;
            _aiService = aiService;
        }

        public async Task<InvestmentHistoryDto> Handle(GetInvestmentHistoryQuery request, CancellationToken cancellationToken)
        {
            var transaction = await _transactionHistory.GetAllAsQueryable()
                .AsNoTracking()
                .Where(t => t.UserId == request.UserId)
                .OrderByDescending(t => t.CreatedDate)
                .ToListAsync(cancellationToken);

            var transactionDtos = transaction.Select(t => new InvestmentTransactionItemDto
            { 
                AssetName = GetAssetName((int)t.AssetType),
               
                TransactionType = (int)t.TransactionType == 1 ? "Alım" : "Satım",

                Amount = t.Amount,
                TotalPrice = t.TotalPrice,
                Date = t.CreatedDate
            }).ToList();

            var currentMonth = DateTime.UtcNow.Month;
            var currentYear = DateTime.UtcNow.Year;
           
            var currentMonthTotalBuyVolume = transaction
                .Where(t => (int)t.TransactionType == 1 &&
                            t.CreatedDate.Month == currentMonth &&
                            t.CreatedDate.Year == currentYear)
                .Sum(t => t.TotalPrice);

            string aiSummary;
            if (currentMonthTotalBuyVolume > 0)
            {
                
                string monthName = DateTime.UtcNow.ToString("MMMM", new System.Globalization.CultureInfo("tr-TR"));               
                string promptContext = $"Kullanıcı bu ay ({monthName}) toplam {currentMonthTotalBuyVolume} TL değerinde yatırım/varlık alımı yaptı. " +
                                       $"Kullanıcıya '{monthName} ayı içerisinde toplam {currentMonthTotalBuyVolume} TL değerinde varlık alımı gerçekleştirdiniz.' tarzında, " +
                                       $"motive edici, tek cümlelik kısa bir finansal özet metni yaz.";

                aiSummary = await _aiService.GetFinancialAdviceAsync(promptContext);
            }
            else
            {
                aiSummary = "Bu ay henüz bir yatırım işlemi gerçekleştirmediniz. Geleceğiniz için ufak birikimlerle başlamaya ne dersiniz?";
            }           
            return new InvestmentHistoryDto
            {
                Transactions = transactionDtos,
                MonthlyAiSummary = aiSummary
            };
        }

        private string GetAssetName(int assetType)
        {
            return assetType switch
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
}