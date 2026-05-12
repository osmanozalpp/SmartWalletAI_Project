using MediatR;
using SmartWalletAI.Application.Common.Helpers;
using SmartWalletAI.Application.Common.Interfaces;
using SmartWalletAI.Domain.Entities;
using SmartWalletAI.Domain.Enums;
using SmartWalletAI.Domain.Exceptions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Features.FinancialGoals.Commands.CloseFinancialGoal
{
    public class CloseFinancialGoalHandler : IRequestHandler<CloseFinancialGoalCommand, bool>
    {
        private readonly IRepository<FinancialGoal> _goalRepository;
        private readonly IRepository<Wallet> _walletRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Transaction> _transactionRepository;

        public CloseFinancialGoalHandler(
            IRepository<FinancialGoal> goalRepository,
            IRepository<Wallet> walletRepository,
            IUnitOfWork unitOfWork,
            IRepository<Transaction> transactionRepository)
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

                // --- HATAYI ÇÖZDÜĞÜMÜZ YER ---
                // Sadece iptal edilmişleri VEYA tamamlanıp parası çoktan alınmışları engelliyoruz.
                // Yani hedef "Completed" olsa bile içinde para (CurrentAmount > 0) varsa İŞLEME DEVAM EDECEK!
                if (goal.Status == GoalStatus.Cancelled || (goal.Status == GoalStatus.Completed && goal.CurrentAmount == 0))
                    throw new BusinessException("Bu hedef zaten kapatılmış ve birikim iade edilmiş.");

                var wallet = await _walletRepository.GetAsync(w => w.UserId == goal.UserId);

                if (wallet == null)
                    throw new NotFoundException("Kullanıcıya ait cüzdan bulunamadı.");

                // --- 1. SENARYOLARI VE DURUMLARI BELİRLEME ---
                GoalStatus finalStatus;
                string transactionDescription;

                // Hedef dolduysa VEYA zaten Completed statüsündeyse
                bool isTargetReached = goal.CurrentAmount >= goal.TargetAmount || goal.Status == GoalStatus.Completed;
                bool isExpired = goal.TargetDate <= DateTime.UtcNow.ToTurkeyTime();

                if (isTargetReached) // Hedef tamamlandı
                {
                    finalStatus = GoalStatus.Completed;
                    transactionDescription = $"{goal.Title} hedefi başarıyla tamamlandı, birikim ana bakiyeye aktarıldı.";
                }
                else if (isExpired) // Süre doldu
                {
                    finalStatus = GoalStatus.Cancelled;
                    transactionDescription = $"{goal.Title} hedefinin süresi doldu, birikim ana bakiyeye iade edildi.";
                }
                else // Kullanıcı erken kapattı
                {
                    finalStatus = GoalStatus.Cancelled;
                    transactionDescription = $"{goal.Title} hedefi kullanıcı tarafından iptal edildi, birikim iade edildi.";
                }

                // --- 2. PARA TRANSFERİ (İADE) İŞLEMİ ---
                var amountToReturn = goal.CurrentAmount;

                if (amountToReturn > 0)
                {
                    wallet.Deposit(amountToReturn);
                    goal.CurrentAmount = 0; 

                    string reference = $"#HT-{DateTime.UtcNow:yyyyMMddHHmmss}-{new Random().Next(100, 999)}";

                    var transactionRecord = new Transaction
                    {
                        Id = Guid.NewGuid(),
                        SenderWalletId = null,
                        ReceiverWalletId = wallet.Id,
                        Amount = amountToReturn,
                        TransactionDate = DateTime.UtcNow.AddHours(3),
                        Description = transactionDescription,
                        Category = TransactionCategory.Diğer,
                        ReferenceNumber = reference,
                        FinancialGoalId = goal.Id
                    };

                    await _transactionRepository.AddAsync(transactionRecord);
                }

                goal.Status = finalStatus;

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