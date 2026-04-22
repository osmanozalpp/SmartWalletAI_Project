using MediatR;
using SmartWalletAI.Application.Common.Interfaces;
using SmartWalletAI.Domain.Entities;
using SmartWalletAI.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Features.Auth.Commands.ConfirmEmailUpdate
{
    public class ConfirmEmailUpdateHandler : IRequestHandler<ConfirmEmailUpdateCommand>
    {
        private readonly IRepository<User> _userRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ConfirmEmailUpdateHandler(IRepository<User> userRepository, IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(ConfirmEmailUpdateCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(request.UserId);

            if (user == null)
                throw new NotFoundException("Kullanıcı bulunamadı.");

            if (user.EmailVerificationCode != request.Code)
                throw new BusinessException("Girdiğiniz doğrulama kodu hatalı.");

            if (user.EmailVerificationCodeExpiry < DateTime.UtcNow)
                throw new BusinessException("Kodun süresi dolmuş. Lütfen tekrar deneyin.");
          
            user.IsEmailVerified = true;
            user.EmailVerificationCode = null; 
            user.EmailVerificationCodeExpiry = null;
           
            _userRepository.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
