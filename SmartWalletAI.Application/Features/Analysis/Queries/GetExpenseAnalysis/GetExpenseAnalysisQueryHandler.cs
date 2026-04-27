using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartWalletAI.Application.Common.Interfaces;
using SmartWalletAI.Domain.Entities;
using SmartWalletAI.Domain.Exceptions;
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

        public GetExpenseAnalysisQueryHandler(
            IRepository<Transaction> transactionRepository,
            IRepository<Wallet> walletRepository)
        {
            _transactionRepository = transactionRepository;
            _walletRepository = walletRepository;
        }

        public async Task<ExpenseAnalysisDto> Handle(GetExpenseAnalysisQuery request, CancellationToken cancellationToken)
        {
            var userWallet = await _walletRepository.GetAsync(w => w.UserId == request.UserId);

            if (userWallet == null)
            {
                throw new NotFoundException("Bu kullanıcının henüz bir cüzdanı yok!");
            }

            var currentMonth = DateTime.UtcNow.Month;
            var currentYear = DateTime.UtcNow.Year;
            var currentDay = DateTime.UtcNow.Day;

            // Bu ayki harcamaları getir
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
                    AiAnalysisAdvice = "Henüz bir harcama verisi bulunmuyor."
                };
            }

            var totalExpense = monthlyExpenses.Sum(t => t.Amount);

            // Kategorilere göre grupla
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

            var topCategory = groupedCategories.First();

            return new ExpenseAnalysisDto
            {
                TotalMonthlyExpense = totalExpense,
                DailyAverageExpense = Math.Round(totalExpense / currentDay, 2),
                TopSendingCategory = topCategory.CategoryName,
                CategoryDetails = groupedCategories,
                AiAnalysisAdvice = null
            };
        }
    }
}