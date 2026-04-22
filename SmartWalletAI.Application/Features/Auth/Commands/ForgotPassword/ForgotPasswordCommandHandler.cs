using MediatR;
using SmartWalletAI.Application.Common.Interfaces;
using SmartWalletAI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Features.Auth.Commands.ForgotPassword
{
    public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, bool>
    {
        private readonly IRepository<User> _userRepository;
        private readonly IEmailService _emailService;
        private readonly IUnitOfWork _unitOfWork;
        public ForgotPasswordCommandHandler(IRepository<User> userRepository, IEmailService emailService, IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _emailService = emailService;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetAsync(u => u.Email == request.Email);

            /* GÜVENLİK SIRRI
             Kullanıcı yoksa bile "hata" dönmüyoruz. İşlem başarılıymış gibi davranıyoruz. */
            if (user == null)
                return true;

            string resetCode = new Random().Next(100000, 999999).ToString();
            user.PasswordResetCode = resetCode;
            user.PasswordResetCodeExpiry = DateTime.UtcNow.AddMinutes(15);

            await _userRepository.UpdateAsync(user);

            string mailBody = $@"
            <h3>Şifre Sıfırlama Talebi</h3>
            <p>Şifrenizi yenilemek için doğrulama kodunuz:</p>
            <h2>{resetCode}</h2>
            <p>Eğer bu işlemi siz yapmadıysanız bu maili dikkate almayın.</p>";

            await _unitOfWork.SaveChangesAsync();

            await _emailService.SendEmailAsync(user.Email, "SmartWallet AI - Şifre Sıfırlama", mailBody);

            return true;

        }
    }
}
