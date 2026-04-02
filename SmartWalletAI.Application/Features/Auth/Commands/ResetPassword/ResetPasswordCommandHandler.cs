using MediatR;
using Microsoft.AspNetCore.Identity;
using SmartWalletAI.Application.Common.Interfaces;
using SmartWalletAI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Features.Auth.Commands.ResetPassword
{
    public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, bool>
    {
        private readonly IRepository<User> _userRepository;

        public ResetPasswordCommandHandler(IRepository<User> userRepository)
        {
            _userRepository = userRepository;
        }
        public async Task<bool> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetAsync(u => u.Email == request.Email);

            if (user == null || user.PasswordResetCode != request.Code)
                throw new Exception("Geçersiz doğrulama kodu.");

            if (user.PasswordResetCodeExpiry < DateTime.UtcNow)
                throw new Exception("Kodun süresi dolmuş. Lütfen tekrar şifre sıfırlama talebinde bulunun.");
           
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

            user.PasswordResetCode = null;
            user.PasswordResetCodeExpiry = null;

            
            await _userRepository.UpdateAsync(user);

            await _userRepository.SaveChangesAsync();

            return true;

        }
    }
}
