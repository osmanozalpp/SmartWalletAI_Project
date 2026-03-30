using MediatR;
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
           var wallet = await _walletRepository.GetAsync(w=> w.UserId == request.UserId);

            if (wallet == null)
            {
                throw new Exception("Kullanıcıya ait cüzdan bulunamadı.");
            }

            return new WalletSummaryDto
            {
                Id = wallet.Id,
                IBAN = wallet.IBAN,
                Balance = wallet.Balance
            };
        }
    }
}
