using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartWalletAI.Application.Common.Interfaces;
using SmartWalletAI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Features.Analysis.Queries.GetExpenseAnalysis
{
    public class GetExpenseAnalysisQueryHandler : IRequestHandler<GetExpenseAnalysisQuery, ExpenseAnalysisDto>
    {
        private readonly IRepository<Transaction> _transactionRepository;
        private readonly IRepository<Wallet> _walletRepository;
        private readonly IAiService _aiService; 

        public GetExpenseAnalysisQueryHandler(
            IRepository<Transaction> transactionRepository,
            IRepository<Wallet> walletRepository,
            IAiService aiService) 
        {
            _transactionRepository = transactionRepository;
            _walletRepository = walletRepository;
            _aiService = aiService;
        }

        public async Task<ExpenseAnalysisDto> Handle(GetExpenseAnalysisQuery request, CancellationToken cancellationToken)
        {
            var userWallet = await _walletRepository.GetAsync(w => w.UserId == request.UserId);

            if (userWallet == null)
                throw new Exception("Bu kullanıcının henüz bir cüzdanı yok!");

            var currentMonth = DateTime.UtcNow.Month;
            var currentYear = DateTime.UtcNow.Year;

            
            var monthlyExpenses = await _transactionRepository.GetAllAsQueryable()
                .AsNoTracking()
                .Where(t => t.SenderWalletId == userWallet.Id &&
                            t.TransactionDate.Month == currentMonth &&
                            t.TransactionDate.Year == currentYear)
                .ToListAsync(cancellationToken);

            if (!monthlyExpenses.Any())
            {
                return new ExpenseAnalysisDto
                {
                    TopSendingCategory = "Harcama Yok",
                    AiAnalysisAdvice = "Bu ay henüz hiç harcama yapmamışsın. Tasarrufa devam! 🚀"
                };
            }

            var totalExpense = monthlyExpenses.Sum(t => t.Amount);
            var currentDay = DateTime.UtcNow.Day;

            var groupedCategories = monthlyExpenses
                .GroupBy(t => t.Category)
                .Select(g => new CategoryExpenseDetail
                {
                    CategoryName = g.Key.ToString(),
                    TotalAmount = g.Sum(t => t.Amount),
                    Percentage = Math.Round((g.Sum(t => t.Amount) / totalExpense) * 100, 1)
                })
                .OrderByDescending(g => g.TotalAmount)
                .ToList();

            
            var lastMonthDate = DateTime.UtcNow.AddMonths(-1);
            var lastMonthTotal = await _transactionRepository.GetAllAsQueryable()
                .AsNoTracking()
                .Where(t => t.SenderWalletId == userWallet.Id &&
                            t.TransactionDate.Month == lastMonthDate.Month &&
                            t.TransactionDate.Year == lastMonthDate.Year)
                .SumAsync(t => t.Amount, cancellationToken);

            
            var topCategory = groupedCategories.First();
            
            string contextData = "[ANALİZ_GÖREVİ] " +
                                 $"Kullanıcı bu ay toplam {totalExpense} TL harcadı. " +
                                 $"Geçen ayki toplam harcaması ise {lastMonthTotal} TL idi. " +
                                 $"Bu ay en çok harcama yaptığı kategori: {topCategory.CategoryName} ({topCategory.Percentage}% oranında). " +
                                 $"Bu verilere dayanarak kullanıcıya kısa, samimi ve 2 cümleyi geçmeyen bir harcama analizi/tavsiyesi ver.";


            var aiAdvice = await _aiService.GetFinancialAdviceAsync(contextData);

            return new ExpenseAnalysisDto
            {
                TotalMonthlyExpense = totalExpense,
                DailyAverageExpense = Math.Round(totalExpense / currentDay, 2),
                TopSendingCategory = topCategory.CategoryName,
                CategoryDetails = groupedCategories,
                AiAnalysisAdvice = aiAdvice 
            };
        }
    }
}