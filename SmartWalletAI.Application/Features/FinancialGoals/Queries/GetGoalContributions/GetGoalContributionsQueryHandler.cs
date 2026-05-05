using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartWalletAI.Application.Common.Interfaces;
using SmartWalletAI.Application.Features.FinancialGoals.Queries.GetGoalContributions;
using SmartWalletAI.Domain.Entities;

namespace SmartWalletAI.Application.Features.FinancialGoals.Queries.GetGoalContributions
{
    public class GetGoalContributionsQueryHandler : IRequestHandler<GetGoalContributionsQuery, List<GoalContributionDto>>
    {
        private readonly IRepository<Transaction> _transactionRepository;

        public GetGoalContributionsQueryHandler(IRepository<Transaction> transactionRepository)
        {
            _transactionRepository = transactionRepository;
        }

        public async Task<List<GoalContributionDto>> Handle(GetGoalContributionsQuery request, CancellationToken cancellationToken)
        {
            return await _transactionRepository.GetAllAsQueryable()
                .Where(t => t.FinancialGoalId == request.GoalId) 
                .OrderByDescending(t => t.TransactionDate) 
                .Select(t => new GoalContributionDto
                {
                    Amount = t.Amount,
                    ContributionDate = t.TransactionDate
                })
                .ToListAsync(cancellationToken);
        }
    }
}