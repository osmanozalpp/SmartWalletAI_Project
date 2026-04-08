using MediatR;
using SmartWalletAI.Application.Common.Interfaces;
using SmartWalletAI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Features.Auth.Commands.ReactivateAccount
{
    public class ReactivateAccountCommandHandler : IRequestHandler<ReactivateAccountCommand, bool>
    {
        private readonly IRepository<User> _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;

        public ReactivateAccountCommandHandler(IRepository<User> userRepository, IUnitOfWork unitOfWork , IEmailService emailService)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _emailService = emailService;
        }

        public async Task<bool> Handle(ReactivateAccountCommand request, CancellationToken cancellationToken)
        {
            
            var user = await _userRepository.GetAsync(u => u.Email == request.Email, ignoreFilters: true);

            if (user == null) throw new Exception("Kullanıcı bulunamadı.");
            if (!user.IsDeleted) throw new Exception("Hesabınız zaten aktif durumda.");

            var verificationCode = new Random().Next(100000, 999999).ToString();

            user.IsDeleted = false; 
            user.UpdatedDate = DateTime.UtcNow;

            user.EmailVerificationCode = verificationCode;
            user.EmailVerificationCodeExpiry = DateTime.UtcNow.AddMinutes(3);

            await _userRepository.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            var emailSubject = "SmartWallet AI - Hesap Canlandırma Kodu";
            var emailBody = $@"
            <h2>Hesabınız Yeniden Aktifleştirildi!</h2>
            <p>Güvenliğiniz için hesabınıza giriş yapmadan önce doğrulama yapmanız gerekmektedir.</p>
            <p>Doğrulama Kodunuz: <strong>{verificationCode}</strong></p>";

            await _emailService.SendEmailAsync(user.Email, emailSubject, emailBody);

            return true;
        }
    }
}
