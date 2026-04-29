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

namespace SmartWalletAI.Application.Features.FinancialGoals.Commands.CloseFinancialGoal
{
    public class CloseFinancialGoalHandler : IRequestHandler<CloseFinancialGoalCommand, bool>
    {
        private readonly IRepository<FinancialGoal> _goalRepository;
        private readonly IRepository<Wallet> _walletRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Transaction> _transactionRepository;

        public CloseFinancialGoalHandler(IRepository<FinancialGoal> goalRepository, IRepository<Wallet> walletRepository, IUnitOfWork unitOfWork , IRepository<Transaction> transactionRepository)
        {
            _goalRepository = goalRepository;
            _walletRepository = walletRepository;
            _unitOfWork = unitOfWork;
            _transactionRepository = transactionRepository;
        }

        public async Task<bool> Handle(CloseFinancialGoalCommand request, CancellationToken ct)
        {
            using var dbTransaction = await _unitOfWork.BeginTransactionAsync(ct);
            try
            {
                var goal = await _goalRepository.GetAsync(g => g.Id == request.GoalId);

                if (goal == null) 
                    throw new NotFoundException("Hedef bulunamadı.");

                if (goal.Status == GoalStatus.Cancelled || (goal.Status == GoalStatus.Completed && goal.CurrentAmount == 0))
                    throw new BusinessException("Bu hedef zaten kapatılmış.");

                var wallet = await _walletRepository.GetAsync(w => w.UserId == goal.UserId);

                var amountToReturn = goal.CurrentAmount;

                if (amountToReturn > 0)
                {
                    wallet.Deposit(amountToReturn);
                    goal.CurrentAmount = 0;

                    string reference = "#HI-" + new Random().Next(1000000, 9999999).ToString();
                    var transactionRecord = new Transaction
                    {
                        Id = Guid.NewGuid(),
                        SenderWalletId = null,       
                        ReceiverWalletId = wallet.Id, 
                        Amount = amountToReturn,
                        TransactionDate = DateTime.UtcNow.AddHours(3),
                        Description = amountToReturn >= goal.TargetAmount
                            ? $"{goal.Title} hedefi başarıyla tamamlandı, birikim aktarıldı."
                            : $"{goal.Title} hedefi iptal edildi, birikim iade edildi.",
                        Category = TransactionCategory.Diğer,
                        ReferenceNumber = reference
                    };
                    await _transactionRepository.AddAsync(transactionRecord);
                }

                if (goal.Status == GoalStatus.Active)
                {
                    goal.Status = amountToReturn >= goal.TargetAmount ? GoalStatus.Completed : GoalStatus.Cancelled;
                }

                await _walletRepository.UpdateAsync(wallet);
                await _goalRepository.UpdateAsync(goal);
                await _unitOfWork.SaveChangesAsync(ct);
                await dbTransaction.CommitAsync(ct);

                return true;
            }
            catch (Exception)
            {
                await dbTransaction.RollbackAsync(ct);
                throw;
            }
        }
    }
}
