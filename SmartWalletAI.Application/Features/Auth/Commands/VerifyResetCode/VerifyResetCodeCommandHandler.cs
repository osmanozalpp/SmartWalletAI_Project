using MediatR;
using SmartWalletAI.Application.Common.Interfaces;
using SmartWalletAI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Features.Auth.Commands.VerifyResetCode
{
    public class VerifyResetCodeCommandHandler : IRequestHandler<VerifyResetCodeCommand, VerifyResetCodeResponse>
    {
        private readonly IRepository<User> _userRepository;

        public VerifyResetCodeCommandHandler(IRepository<User> userRepository)
        {
            _userRepository = userRepository;
        }


        public async Task<VerifyResetCodeResponse> Handle(VerifyResetCodeCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetAsync(u => u.Email == request.Email);

            if (user == null)
            {
                return new VerifyResetCodeResponse
                {
                    IsSucces = false,
                    Message = "Kullanıcı bulunamadı."
                };
            }
            if (string.IsNullOrEmpty(user.PasswordResetCode) || user.PasswordResetCode != request.Code)
            {
                return new VerifyResetCodeResponse
                {
                    IsSucces = false,
                    Message = "Geçersiz doğrulama kodu."
                };
            }
            if (user.PasswordResetCodeExpiry < DateTime.UtcNow.AddHours(3))
            {
                return new VerifyResetCodeResponse
                {
                    IsSucces = false,
                    Message = "Doğrulama kodunun süresi dolmuş lütfen yeni bir kod isteyin."
                };
            }
            return new VerifyResetCodeResponse
            {
                IsSucces = true,
                Message = "Doğrulama işlemi başarılı."
            };
        }
        }
}
