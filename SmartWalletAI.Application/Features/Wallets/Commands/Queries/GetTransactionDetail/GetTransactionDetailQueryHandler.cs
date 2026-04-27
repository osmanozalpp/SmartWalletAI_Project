using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using SmartWalletAI.Application.Common.Interfaces;
using SmartWalletAI.Application.Features.Wallets.Queries.GetTransactionDetail;
using SmartWalletAI.Domain.Entities;
using SmartWalletAI.Domain.Exceptions;


public class GetTransactionDetailQueryHandler : IRequestHandler<GetTransactionDetailQuery, TransactionReceiptDto>
{
    private readonly IRepository<Transaction> _transactionRepository;

    public GetTransactionDetailQueryHandler(IRepository<Transaction> transactionRepository)
    {
        _transactionRepository = transactionRepository;
    }

    public async Task<TransactionReceiptDto> Handle(GetTransactionDetailQuery request, CancellationToken cancellationToken)
    {       
        var transaction = await _transactionRepository.GetAllAsQueryable()
            .Include(t => t.SenderWallet).ThenInclude(w => w.User)
            .Include(t => t.ReceiverWallet).ThenInclude(w => w.User)
            .FirstOrDefaultAsync(t => t.Id == request.TransactionId, cancellationToken);

        
        if (transaction == null)
            throw new NotFoundException("İşlem kaydı bulunamadı.");

        
        bool isUserAuthorized = transaction.SenderWallet.UserId == request.UserId ||
                                transaction.ReceiverWallet.UserId == request.UserId;

        if (!isUserAuthorized)
        {
            throw new UnauthorizedException("Bu işlem detayını görüntüleme yetkiniz bulunmamaktadır.");
        }


        return new TransactionReceiptDto
        {
            ReferenceNumber = transaction.ReferenceNumber,
            Amount = transaction.Amount,
            Description = transaction.Description ?? "Para Transferi",
            TransactionDate = transaction.TransactionDate,
            Category = transaction.Category.ToString(),
            SenderName = transaction.SenderWallet.User.Name,
            SenderIban = transaction.SenderWallet.IBAN,
            ReceiverName = transaction.ReceiverWallet.User.Name,
            ReceiverIban = transaction.ReceiverWallet.IBAN,           
            IsIncoming = transaction.ReceiverWallet.UserId == request.UserId
        };
    }
}
