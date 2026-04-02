using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartWalletAI.Application.Common.Interfaces;
using SmartWalletAI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Features.Wallets.Commands.Queries.GetMyWallet
{
    public class GetMyWalletQueryHandler : IRequestHandler<GetMyWalletQuery, WalletSummaryDto>
    {
        private readonly IRepository<Wallet> _walletRepository;


        public GetMyWalletQueryHandler(IRepository<Wallet> walletRepository)
        {
            _walletRepository = walletRepository;
        }

        public async Task<WalletSummaryDto> Handle(GetMyWalletQuery request, CancellationToken cancellationToken)
        {
            
            var wallet = await _walletRepository.GetAllAsQueryable()
                .Include(w => w.User)
                .FirstOrDefaultAsync(w => w.UserId == request.UserId, cancellationToken);

            if (wallet == null)
            {
                throw new Exception("Kullanıcıya ait cüzdan bulunamadı.");
            }

            return new WalletSummaryDto
            {
                Id = wallet.Id,
                IBAN = wallet.IBAN,
                Balance = wallet.Balance,
                FullName = wallet.User.Name 
            };
        }
    }
}
