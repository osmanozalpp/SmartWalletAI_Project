using MediatR;
using SmartWalletAI.Application.Common.Interfaces;
using SmartWalletAI.Domain.Entities;

namespace SmartWalletAI.Application.Features.Auth.Commands.ResetPassword
{
    public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, ResetPasswordResponse>
    {
        private readonly IRepository<User> _userRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ResetPasswordCommandHandler(IRepository<User> userRepository, IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ResetPasswordResponse> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetAsync(u => u.Email == request.Email);
         
            if (user == null || user.PasswordResetCode != request.Code)
            {
                return new ResetPasswordResponse
                {
                    IsSuccess = false,
                    Message = "Geçersiz doğrulama kodu."
                };
            }

            if (user.PasswordResetCodeExpiry < DateTime.UtcNow.AddHours(3))
            {
                return new ResetPasswordResponse
                {
                    IsSuccess = false,
                    Message = "Kodun süresi dolmuş. Lütfen tekrar şifre sıfırlama talebinde bulunun."
                };
            }
           
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);         
            user.PasswordResetCode = null;
            user.PasswordResetCodeExpiry = null;

            await _userRepository.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
          
            return new ResetPasswordResponse
            {
                IsSuccess = true,
                Message = "Şifreniz başarıyla güncellendi. Yeni şifrenizle giriş yapabilirsiniz."
            };
        }
    }
}