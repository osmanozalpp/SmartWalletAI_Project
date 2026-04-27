using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartWalletAI.Application.Common.Interfaces;
using SmartWalletAI.Domain.Entities;
using SmartWalletAI.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Features.Wallets.Queries.GetMaskedOwnerName
{
    public class GetMaskedOwnerNameQuery : IRequest<string>
    {
        public string Iban { get; set; }
    }
    public class GetMaskedOwnerNameQueryHandler : IRequestHandler<GetMaskedOwnerNameQuery, string>
    {
        private readonly IRepository<Wallet> _walletRepository;

        public GetMaskedOwnerNameQueryHandler(IRepository<Wallet> walletRepository)
        {
            _walletRepository = walletRepository;
        }
        public async Task<string> Handle(GetMaskedOwnerNameQuery request, CancellationToken cancellationToken)
        {
            var fullName = await _walletRepository.GetAllAsQueryable()
                .Where(w => w.IBAN == request.Iban)
                .Select(w => w.User.Name)
                .FirstOrDefaultAsync(cancellationToken);

            if (string.IsNullOrEmpty(fullName))
                throw new NotFoundException("Bu IBAN'a ait bir kullanıcı bulunamadı.");

            //maskeleme
            var words = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < words.Length; i++)
            {
                if (words[i].Length > 1)
                {
                    words[i] = char.ToUpper(words[i][0]) +
                               new string('*', words[i].Length - 2) +
                               char.ToUpper(words[i][words[i].Length - 1]);
                }
                else if (words[i].Length == 1)
                {
                    words[i] = words[i].ToUpper();
                }
            }

            return string.Join(" ", words);
        }
    }
}
