using MediatR;
using SmartWalletAI.Application.Common.Interfaces;
using SmartWalletAI.Domain.Entities;
using SmartWalletAI.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Features.FinancialGoals.Commands.CreateGoal
{
    public class CreateGoalHandler : IRequestHandler<CreateGoalCommand, Guid>
    {
        private readonly IRepository<FinancialGoal> _goalRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<User> _userRepository;

        public CreateGoalHandler(IRepository<FinancialGoal> goalRepository, IUnitOfWork unitOfWork, IRepository<User> userRepository)
        {
            _goalRepository = goalRepository;
            _unitOfWork = unitOfWork;
            _userRepository = userRepository;
        }

        public async Task<Guid> Handle(CreateGoalCommand request, CancellationToken cancellationToken)
        {
            var goal = new FinancialGoal
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId, 
                Title = request.Title,
                TargetAmount = request.TargetAmount,
                TargetDate = request.TargetDate,
                CurrentAmount = 0,
                Status = GoalStatus.Active
            };

            await _goalRepository.AddAsync(goal);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return goal.Id;
        }
    }
}
