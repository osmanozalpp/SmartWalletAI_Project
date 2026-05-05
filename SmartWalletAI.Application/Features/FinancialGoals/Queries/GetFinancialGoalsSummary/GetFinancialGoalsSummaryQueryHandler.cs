using MediatR;
using Microsoft.EntityFrameworkCore; 
using SmartWalletAI.Application.Common.Interfaces;
using SmartWalletAI.Domain.Entities;
using SmartWalletAI.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Features.FinancialGoals.Queries.GetFinancialGoalsSummary
{
    public class GetFinancialGoalsSummaryQueryHandler : IRequestHandler<GetFinancialGoalsSummaryQuery, FinancialGoalsSummaryDto>
    {
        private readonly IRepository<FinancialGoal> _financialGoalRepository;

        public GetFinancialGoalsSummaryQueryHandler(IRepository<FinancialGoal> financialGoalRepository)
        {
            _financialGoalRepository = financialGoalRepository;
        }

        public async Task<FinancialGoalsSummaryDto> Handle(GetFinancialGoalsSummaryQuery request, CancellationToken cancellationToken)
        {
             var userGoals = await _financialGoalRepository.GetAllAsQueryable()
                .Where(g => g.UserId == request.UserId && g.Status == GoalStatus.Active)
                .ToListAsync(cancellationToken);

            var totalSaved = userGoals.Sum(g => g.CurrentAmount);
            var totalTarget = userGoals.Sum(g => g.TargetAmount);

            int percentage = totalTarget > 0
                ? (int)Math.Round((totalSaved / totalTarget) * 100)
                : 0;

            return new FinancialGoalsSummaryDto
            {
                TotalGoalCount = userGoals.Count,
                TotalSavedAmount = totalSaved,
                TotalTargetAmount = totalTarget,
                CompletionPercentage = percentage 
            };
        }
    }
}