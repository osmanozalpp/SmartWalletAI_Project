using FluentValidation;
using MediatR;
using SmartWalletAI.Application.Common.Interfaces;
using SmartWalletAI.Domain.Entities;
using SmartWalletAI.Domain.Exceptions;
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
        private readonly IRepository<User> _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidator<TransferMoneyCommand> _validator;

        public TransferMoneyCommandHandler(IRepository<Wallet> walletRepository, IUnitOfWork unitOfWork, IValidator<TransferMoneyCommand> validator, IRepository<Transaction> transactionRepository, IRepository<User> userRepository)
        {
            _walletRepository = walletRepository;
            _unitOfWork = unitOfWork;
            _validator = validator;
            _transactionRepository = transactionRepository;
            _userRepository = userRepository;
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

            if (senderWallet == null)
                throw new NotFoundException("Gönderen cüzdanı bulunamadı.");

            if (receiverWallet == null)
                throw new BusinessException("Geçersiz veya hatalı bir IBAN girdiniz. Lütfen bilgileri kontrol edin.");

            if (senderWallet.Id == receiverWallet.Id)
                throw new BusinessException("Kendi kendinize para gönderemezsiniz.");

            if (senderWallet.Balance < request.Amount)
                throw new BusinessException("Yetersiz bakiye! İşlemi gerçekleştirmek için bakiyeniz yetersiz.");

            var senderUser = await _userRepository.GetAsync(u => u.Id == senderWallet.UserId);
            var receiverUser = await _userRepository.GetAsync(u => u.Id == receiverWallet.UserId);

            using var dbTransaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                // Gönderen cüzdandan düş
                senderWallet.Withdraw(request.Amount);
                await _walletRepository.UpdateAsync(senderWallet);

                // Alıcı cüzdana ekle
                receiverWallet.Deposit(request.Amount);
                await _walletRepository.UpdateAsync(receiverWallet);


                string generatedReference;
                bool isReferenceExists;
                var random = new Random();

                do
                {                  
                    generatedReference = "#TR-" + random.Next(1000000, 9999999).ToString();
                    
                    var existingTransaction = await _transactionRepository.GetAsync(t => t.ReferenceNumber == generatedReference);
                    isReferenceExists = existingTransaction != null;

                } while (isReferenceExists);

                var transactionRecord = new Transaction
                {
                    Id = Guid.NewGuid(),
                    SenderWalletId = senderWallet.Id,
                    ReceiverWalletId = receiverWallet.Id,
                    Amount = request.Amount,
                    TransactionDate = DateTime.UtcNow.AddHours(3),
                    Description = request.Description ?? "Para Transferi",
                    Category = request.Category,
                    ReferenceNumber = generatedReference
                };

                await _transactionRepository.AddAsync(transactionRecord);
                await _unitOfWork.SaveChangesAsync();
                await dbTransaction.CommitAsync();

                return new TransferMoneyResponse
                {
                    Success = true,
                    Message = "Transfer işlemi başarıyla gerçekleştirildi.",
                    NewBalance = senderWallet.Balance,
                    SenderName = senderUser?.Name ?? "Bilinmeyen Gönderen",
                    ReceiverName = receiverUser?.Name ?? "Bilinmeyen Alıcı",
                    ReceiverIban = request.ReceiverIban,
                    Amount = request.Amount,
                    TransactionDate = transactionRecord.TransactionDate,
                    ReferenceNumber = transactionRecord.ReferenceNumber,
                    Category = transactionRecord.Category,
                    Description = transactionRecord.Description ?? "-"
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
