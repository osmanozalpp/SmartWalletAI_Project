using MediatR;
using SmartWalletAI.Application.Common.Interfaces;
using SmartWalletAI.Domain.Entities;
using SmartWalletAI.Domain.Enums;
using SmartWalletAI.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Features.FinancialGoals.Commands.UpdateFinancialGoal
{
    public class UpdateFinancialGoalCommandHandler : IRequestHandler<UpdateFinancialGoalCommand, bool>
    {
        private readonly IRepository<FinancialGoal> _goalRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateFinancialGoalCommandHandler(IRepository<FinancialGoal> goalRepository, IUnitOfWork unitOfWork)
        {
            _goalRepository = goalRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(UpdateFinancialGoalCommand request, CancellationToken ct)
        {
            // 1. Hedefi bul
            var goal = await _goalRepository.GetAsync(g => g.Id == request.GoalId);

            if (goal == null)
                throw new NotFoundException("Güncellenmek istenen hedef bulunamadı.");

            if (goal.Status != GoalStatus.Active)
                throw new BusinessException("Sadece aktif durumdaki hedefler güncellenebilir.");

            if (request.TargetAmount < goal.CurrentAmount)
                throw new BusinessException("Yeni hedef tutarı, şu ana kadar biriken tutardan az olamaz.");

            goal.Title = request.Title;
            goal.TargetAmount = request.TargetAmount;
            goal.TargetDate = request.TargetDate;

            await _goalRepository.UpdateAsync(goal);
            await _unitOfWork.SaveChangesAsync(ct);

            return true;
        }
    }
}
