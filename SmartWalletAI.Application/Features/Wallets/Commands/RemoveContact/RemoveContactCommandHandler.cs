using MediatR;
using SmartWalletAI.Application.Common.Interfaces;
using SmartWalletAI.Domain.Entities;
using SmartWalletAI.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Features.Wallets.Commands.RemoveContact
{
    public class RemoveContactCommandHandler : IRequestHandler<RemoveContactCommand, bool>
    {
        private readonly IRepository<SavedContact> _savedContactRepository;
        private readonly IUnitOfWork _unitOfWork;


        public RemoveContactCommandHandler(IRepository<SavedContact> savedContactRepository , IUnitOfWork unitOfWork)
        {
            _savedContactRepository = savedContactRepository;
            _unitOfWork = unitOfWork;
        }
        public async Task<bool> Handle(RemoveContactCommand request, CancellationToken cancellationToken)
        {
            var contact = await _savedContactRepository.GetAsync(c => c.UserId == request.UserId && c.Iban == request.Iban);

            if(contact == null)
            {
                throw new NotFoundException("Silinmek istenen kişi bulunamadı");
            }

            await _savedContactRepository.DeleteAsync(contact);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}
