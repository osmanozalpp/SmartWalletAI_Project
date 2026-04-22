using MediatR;
using SmartWalletAI.Application.Common.Interfaces;
using SmartWalletAI.Domain.Entities;
using SmartWalletAI.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Features.Auth.Commands.ProfileChangeEmail
{
    public class UpdateEmailHandler : IRequestHandler<UpdateEmailCommand>
    {
        private readonly IRepository<User> _userRepository;
        private readonly IEmailService _emailService;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateEmailHandler(IRepository<User> userRepository, IEmailService emailService, IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _emailService = emailService;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(UpdateEmailCommand request, CancellationToken cancellationToken)
        {            
            var user = await _userRepository.GetByIdAsync(request.UserId);
            if (user == null) throw new NotFoundException("Kullanıcı bulunamadı.");

            var emailExists = await _userRepository.GetAsync(u => u.Email == request.NewEmail && u.Id != request.UserId);
           
            if (emailExists != null)
            {
                throw new BusinessException("Bu e-posta adresi başka bir hesap tarafından kullanılıyor.");
            }
            
            string verificationCode = new Random().Next(100000, 999999).ToString();

            user.Email = request.NewEmail;
            user.IsEmailVerified = false; 
            user.EmailVerificationCode = verificationCode;
            user.EmailVerificationCodeExpiry = DateTime.UtcNow.AddMinutes(15);

            _userRepository.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            string mailBody = $@"
            <div style='font-family: sans-serif;'>
                <h3>E-posta Değişikliği Doğrulama</h3>
                <p>SmartWallet AI hesabınızın yeni e-posta adresini doğrulamak için kodunuz:</p>
                <h2 style='color:#fbc02d'>{verificationCode}</h2>
                <p>Bu kod 15 dakika geçerlidir.</p>
            </div>";

            await _emailService.SendEmailAsync(user.Email, "E-posta Doğrulama Kodunuz", mailBody);
        }
    }
}
