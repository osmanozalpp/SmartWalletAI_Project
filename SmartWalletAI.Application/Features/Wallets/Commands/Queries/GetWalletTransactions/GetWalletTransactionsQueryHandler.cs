using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartWalletAI.Application.Common.Interfaces;
using SmartWalletAI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace SmartWalletAI.Application.Features.Wallets.Commands.Queries.GetWalletTransactions
{
    public class GetWalletTransactionsQueryHandler : IRequestHandler<GetWalletTransactionsQuery, List<TransactionDto>>
    {
        private readonly IRepository<Transaction> _transactionRepository;

        public GetWalletTransactionsQueryHandler(IRepository<Transaction> transactionRepository)
        {
            _transactionRepository = transactionRepository;
        }

        public async Task<List<TransactionDto>> Handle(GetWalletTransactionsQuery request, CancellationToken cancellationToken)
        {
           
            var query = _transactionRepository.GetAllAsQueryable()
                .Where(t => t.SenderWalletId == request.WalletId || t.ReceiverWalletId == request.WalletId);

          
            if (request.StartDate.HasValue)
            {
                query = query.Where(t => t.TransactionTime >= request.StartDate.Value);
            }

           
            if (request.EndDate.HasValue)
            {
                query = query.Where(t => t.TransactionTime <= request.EndDate.Value);
            }

           
            var transactions = await query
                .OrderByDescending(t => t.TransactionTime) 
                .Skip((request.PageNumber - 1) * request.PageSize) 
                .Take(request.PageSize)
                .ToListAsync(cancellationToken); 

           
            return transactions.Select(t => new TransactionDto
            {
                Id = t.Id,
                Amount = t.Amount,
                TransactionTime = t.TransactionTime,
                Description = t.Description,
                IsIncoming = t.ReceiverWalletId == request.WalletId
            }).ToList();
        }

      
    }
}
