using MediatR;
using SmartWalletAI.Application.Common.Interfaces;
using SmartWalletAI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Features.Wallets.Commands.SaveContact
{
    public class SaveContactCommandHandler : IRequestHandler<SaveContactCommand, bool>
    {
        private readonly IRepository<SavedContact> _savedContactRepository;
        private readonly IUnitOfWork _unitOfWork;

        public SaveContactCommandHandler(IRepository<SavedContact> savedContactRepository , IUnitOfWork unitOfWork)
        {
            _savedContactRepository = savedContactRepository;
            _unitOfWork = unitOfWork;
        }
        public async Task<bool> Handle(SaveContactCommand request, CancellationToken cancellationToken)
        {
            var existingContact = await _savedContactRepository.GetAsync(c => c.UserId == request.UserId && c.Iban == request.Iban);    

            if(existingContact != null)
            {
                existingContact.ContactName = request.ContactName;
                existingContact.IsFavorite = request.IsFavorite;

                existingContact.IsDeleted = false;

                await _savedContactRepository.UpdateAsync(existingContact);
                await _unitOfWork.SaveChangesAsync();
            } 
            else
            {
                var newContact = new SavedContact
                {
                    UserId = request.UserId,
                    ContactName = request.ContactName,
                    Iban = request.Iban,
                    IsFavorite = request.IsFavorite
                };
                await _savedContactRepository.AddAsync(newContact);
                await _unitOfWork.SaveChangesAsync();
            }
            return true;
        }
    }
}
