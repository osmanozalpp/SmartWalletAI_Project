using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartWalletAI.Application.Common.Interfaces;
using SmartWalletAI.Application.Features.FinancialGoals.Queries.Common;
using SmartWalletAI.Domain.Entities;
using SmartWalletAI.Domain.Enums; // GoalStatus'ü kullanabilmek için eklendi
using System; // DateTime.UtcNow kullanabilmek için eklendi
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Features.FinancialGoals.Queries.GetFinancialGoalsList
{
    public class GetFinancialGoalsQueryHandler : IRequestHandler<GetFinancialGoalsQuery, List<FinancialGoalDto>>
    {
        private readonly IRepository<FinancialGoal> _goalRepository;
        private readonly IMediator _mediator;

        public GetFinancialGoalsQueryHandler(IRepository<FinancialGoal> goalRepository, IMediator mediator)
        {
            _goalRepository = goalRepository;
            _mediator = mediator;
        }

        public async Task<List<FinancialGoalDto>> Handle(GetFinancialGoalsQuery request, CancellationToken cancellationToken)
        {
           
            var goals = await _goalRepository.GetAllAsQueryable()
                .Where(g => g.UserId == request.UserId
                         && g.Status == GoalStatus.Active
                         && g.TargetDate > DateTime.UtcNow)
                .OrderBy(g => g.TargetDate)
                .ToListAsync(cancellationToken);

            var goalDtos = goals.Select(goal => new FinancialGoalDto
            {
                Id = goal.Id,
                Title = goal.Title,
                TargetAmount = goal.TargetAmount,
                CurrentAmount = goal.CurrentAmount,
                TargetDate = goal.TargetDate
            }).ToList();

            return goalDtos;
        }
    }
}