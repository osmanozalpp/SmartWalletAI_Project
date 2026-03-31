using FluentValidation;
using MediatR;
using SmartWalletAI.Application.Common.Interfaces;
using SmartWalletAI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Features.Wallets.Commands.TransferMoney
{
    public class TransferMoneyCommandHandler : IRequestHandler<TransferMoneyCommand, TransferMoneyResponse>
    {
        private readonly IRepository<Wallet> _walletRepository;
        private readonly IRepository<Transaction> _transactionRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidator<TransferMoneyCommand> _validator;

        public TransferMoneyCommandHandler(IRepository<Wallet> walletRepository, IUnitOfWork unitOfWork, IValidator<TransferMoneyCommand> validator, IRepository<Transaction> transactionRepository)
        {
            _walletRepository = walletRepository;
            _unitOfWork = unitOfWork;
            _validator = validator;
            _transactionRepository = transactionRepository;
        }

        public async Task<TransferMoneyResponse> Handle(TransferMoneyCommand request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }
            var senderWallet = await _walletRepository.GetAsync(w => w.UserId == request.SenderId);
            var receiverWallet = await _walletRepository.GetAsync(w => w.IBAN == request.ReceiverIban);

            if (senderWallet == null) throw new Exception("Gönderen cüzdanı bulunamadı.");
            if (receiverWallet == null) throw new Exception("Geçersiz veya hatalı bir IBAN girdiniz.");
            if (senderWallet.Id == receiverWallet.Id) throw new Exception("Kendi kendinize para gönderemezsiniz.");
            if (senderWallet.Balance < request.Amount) throw new Exception("Yetersiz bakiye!");

            using var dbTransaction = await _unitOfWork.BeginTransactionAsync();


            try
            {

                senderWallet.Balance -= request.Amount;
                await _walletRepository.UpdateAsync(senderWallet);

                receiverWallet.Balance += request.Amount;
                await _walletRepository.UpdateAsync(receiverWallet);

                var transactionRecord = new Transaction
                {
                    Id = Guid.NewGuid(),
                    SenderWalletId = senderWallet.Id,
                    ReceiverWalletId = receiverWallet.Id,
                    Amount = request.Amount,
                    TransactionTime = DateTime.UtcNow,
                    Description = request.Description ?? "Para Transferi",
                    Category = request.Category
                };

                await _transactionRepository.AddAsync(transactionRecord);


                await _unitOfWork.SaveChangesAsync();
                await dbTransaction.CommitAsync();

                return new TransferMoneyResponse
                {
                    Success = true,
                    Message = "Para transferi başarıyla gerçekleşti.",
                    NewBalance = senderWallet.Balance
                };
            }
            catch (Exception ex)
            {
                try
                {
                    await dbTransaction.RollbackAsync(cancellationToken);
                }
                catch
                {
                    // Rollback sırasındaki olası bağlantı kopmalarını yutuyoruz ki asıl hatamız ezilmesin
                }

                // 2. Hatayı temizle (InnerException kontrolü ile en anlamlı mesajı seç)
                var errorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;


                throw new Exception($"Transfer işlemi sırasında sistemsel bir sorun oluştu. Detay: {errorMessage}");
            }
        }
    }
}
