using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartWalletAI.Application.Common.Interfaces;
using SmartWalletAI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Transaction = SmartWalletAI.Domain.Entities.Transaction;

namespace SmartWalletAI.Application.Features.Wallets.Queries.GetRecipients
{
    public class GetRecipientsQueryHandler : IRequestHandler<GetRecipientQuery, List<RecipientDto>>
    {
        private readonly IRepository<Transaction> _transactionRepository;
        private readonly IRepository<SavedContact> _savedContactRepository;
        private readonly IRepository<Wallet> _walletRepository;

        public GetRecipientsQueryHandler(IRepository<Transaction> transactionRepository, IRepository<SavedContact> savedContactRepository, IRepository<Wallet> walletRepository)
        {
            _transactionRepository = transactionRepository;
            _savedContactRepository = savedContactRepository;
            _walletRepository = walletRepository;
        }

        public async Task<List<RecipientDto>> Handle(GetRecipientQuery request, CancellationToken cancellationToken)
        {
            var recipients = new List<RecipientDto>();

            var savedContacts = await _savedContactRepository.GetAllAsQueryable()
               .AsNoTracking()
               .Where(c => c.UserId == request.UserId && !c.IsDeleted)
               .OrderByDescending(c => c.IsFavorite)
               .ThenBy(c => c.ContactName)
               .ToListAsync(cancellationToken);

            var userWallet = await _walletRepository.GetAsync(w => w.UserId == request.UserId);

            if (userWallet != null)
            {
                var lastTransaction = await _transactionRepository.GetAllAsQueryable()
                    .Include(t => t.ReceiverWallet)
                    .ThenInclude(w => w.User)
                    .AsNoTracking()
                    .Where(t => t.SenderWalletId == userWallet.Id)
                    .OrderByDescending(t => t.TransactionDate)
                    .FirstOrDefaultAsync(cancellationToken);

                if (lastTransaction != null && lastTransaction.ReceiverWallet?.User != null)
                {

                    var receiverIban = lastTransaction.ReceiverWallet.IBAN;
                    var existingSavedContact = savedContacts.FirstOrDefault(c => c.Iban == receiverIban);


                    string displayName = existingSavedContact != null
                        ? existingSavedContact.ContactName
                        : lastTransaction.ReceiverWallet.User.Name;

                    recipients.Add(new RecipientDto
                    {
                        FullName = displayName,
                        Iban = receiverIban,
                        Badge = "Son İşlem Yapılan",
                        Initials = GetInitials(displayName)
                    });
                }
            }

            foreach (var contact in savedContacts)
            {
                // UI temiz dursun diye, son işlem yapılan kişi zaten kayıtlı listesindeyse onu tekrar basmıyoruz
                if (!recipients.Any(r => r.Iban == contact.Iban))
                {
                    recipients.Add(new RecipientDto
                    {
                        FullName = contact.ContactName,
                        Iban = contact.Iban,
                        Badge = contact.IsFavorite ? "Favori kişi" : "Kayıtlı Alıcı",
                        Initials = GetInitials(contact.ContactName)
                    });
                }
            }

            return recipients;
        }

        private string GetInitials(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "";

            return name.Substring(0, 1).ToUpper();
        }
    }
}