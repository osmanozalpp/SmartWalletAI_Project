using MediatR;
using SmartWalletAI.Application.Common.Interfaces;
using SmartWalletAI.Domain.Entities;
using SmartWalletAI.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Features.Auth.Commands.DeleteAccount
{
    public class DeleteAccountCommandHandler : IRequestHandler<DeleteAccountCommand, bool>
    {
        private readonly IRepository<User> _userRepository;
        private readonly IUnitOfWork _unitOfWork;    
        public DeleteAccountCommandHandler(IRepository<User> userRepository, IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
        {
            
            var user = await _userRepository.GetAsync(u => u.Id == request.UserId);

            
            if (user == null)
            {
                throw new NotFoundException("Kullanıcı bulunamadı.");
            }

            
            bool isPasswordCorrect = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

            
            if (!isPasswordCorrect)
            {
                throw new BusinessException("Mevcut şifreniz hatalı. Lütfen kontrol ediniz.");
            }

            
            await _userRepository.DeleteAsync(user);           
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}
