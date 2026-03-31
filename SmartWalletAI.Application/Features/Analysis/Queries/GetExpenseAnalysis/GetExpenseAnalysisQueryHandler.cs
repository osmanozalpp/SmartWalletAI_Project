using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartWalletAI.Application.Common.Interfaces;
using SmartWalletAI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

  namespace SmartWalletAI.Application.Features.Analysis.Queries.GetExpenseAnalysis
{
    public class GetExpenseAnalysisQueryHandler : IRequestHandler<GetExpenseAnalysisQuery, ExpenseAnalysisDto>
    {

        private readonly IRepository<Transaction> _transactionRepository;
        private readonly IRepository<Wallet> _walletRepository;

        public GetExpenseAnalysisQueryHandler(IRepository<Transaction> transactionRepository,
            IRepository<Wallet> walletRepository)
        {
            _transactionRepository = transactionRepository;
            _walletRepository = walletRepository;
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
                            t.TransactionTime.Month == currentMonth &&
                            t.TransactionTime.Year == currentYear)
                .ToListAsync(cancellationToken);

            if (!monthlyExpenses.Any())
            {
                return new ExpenseAnalysisDto
                {
                    TopSendingCategory = "Harcama Yok"
                };
            }
            var totalExpense = monthlyExpenses.Sum(t => t.Amount);
            var currentDay = DateTime.UtcNow.Day == 1 ? 1 : DateTime.UtcNow.Day; // ayın 1inde bölme hatasını önlemek için

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

            return new ExpenseAnalysisDto
            {
                TotalMonthlyExpense = totalExpense,
                DailyAverageExpense = Math.Round(totalExpense / currentDay, 2),
                TopSendingCategory = groupedCategories.First().CategoryName,
                CategoryDetails = groupedCategories
            };
        }
    }
} 
