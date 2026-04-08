using MediatR;
using SmartWalletAI.Application.Common.Interfaces;
using SmartWalletAI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Features.Auth.Commands.VerifyEmail
{
    public class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, bool>
    {
        private readonly IRepository<User> _userRepository;
        private readonly IUnitOfWork _unitOfWork;

        public VerifyEmailCommandHandler(IRepository<User> userRepository , IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }
        public async Task<bool> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
        {
            
            var user = await _userRepository.GetAsync(u => u.Email == request.Email);

            if (user == null || user.EmailVerificationCode != request.Code)
                throw new Exception("Geçersiz doğrulama kodu.");

            if (user.EmailVerificationCodeExpiry < DateTime.UtcNow)
                throw new Exception("Doğrulama kodunun süresi doldu . Lütfen yeni bir kod isteyin.");

            user.IsEmailVerified = true;
            user.EmailVerificationCode = null;
            user.EmailVerificationCodeExpiry = null;

            await _userRepository.UpdateAsync(user);

            
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}
