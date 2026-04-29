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

namespace SmartWalletAI.Application.Features.FinancialGoals.Commands.AddFunds
{
    public class AddFundsToGoalHandler : IRequestHandler<AddFundsToGoalCommand, bool>
    {
        private readonly IRepository<FinancialGoal> _goalRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Wallet> _walletRepository;
        private readonly IRepository<Transaction> _transactionRepository;

        public AddFundsToGoalHandler(IRepository<FinancialGoal> goalRepository, IUnitOfWork unitOfWork, IRepository<Wallet> walletRepository, IRepository<Transaction> transactionRepository)
        {
            _goalRepository = goalRepository;
            _walletRepository = walletRepository;
            _unitOfWork = unitOfWork;
            _transactionRepository = transactionRepository;
        }
        public async Task<bool> Handle(AddFundsToGoalCommand request, CancellationToken cancellationToken)
        {
            using var dbTransaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                var goal = await _goalRepository.GetAsync(g => g.Id == request.GoalId);
                if (goal.Status != GoalStatus.Active)
                    throw new BusinessException("Bu hedef aktif değil.");

                decimal remainingAmount = goal.TargetAmount - goal.CurrentAmount;
                if (request.Amount > remainingAmount)
                    throw new BusinessException($"Sadece {remainingAmount:N2} TL ekleyebilirsiniz.");

                var wallet = await _walletRepository.GetAsync(w => w.UserId == goal.UserId);

                if (wallet.Balance < request.Amount)
                    throw new BusinessException("Cüzdan bakiyesi yetersiz.");

                wallet.Withdraw(request.Amount);
                goal.Deposit(request.Amount);

                if (goal.CurrentAmount == goal.TargetAmount)
                    goal.Status = GoalStatus.Completed;

                string reference = "#HT-" + new Random().Next(1000000, 9999999).ToString();

                var transactionRecord = new Transaction
                {
                    Id = Guid.NewGuid(),
                    SenderWalletId = wallet.Id, 
                    ReceiverWalletId = null,
                    Amount = request.Amount,
                    TransactionDate = DateTime.UtcNow.AddHours(3),
                    Description = $"{goal.Title} hedefi için para biriktirildi.",
                    Category = TransactionCategory.Diğer,
                    ReferenceNumber = reference
                };

                await _walletRepository.UpdateAsync(wallet);
                await _goalRepository.UpdateAsync(goal);
                await _transactionRepository.AddAsync(transactionRecord);

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await dbTransaction.CommitAsync(cancellationToken);

                return true;
            }
            catch (Exception)
            {
                await dbTransaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
    }

