using MediatR;
using Microsoft.EntityFrameworkCore; // DbUpdateException için eklendi
using SmartWalletAI.Application.Common.Helpers;
using SmartWalletAI.Application.Common.Interfaces;
using SmartWalletAI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Features.Auth.Commands.Register
{
    public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, RegisterUserResponse>
    {
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<Wallet> _walletRepository;
        private readonly IEmailService _emailService;
        private readonly IUnitOfWork _unitOfWork;

        
        public RegisterUserCommandHandler(
            IRepository<User> userRepository,
            IRepository<Wallet> walletRepository,
            IEmailService emailService , IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _walletRepository = walletRepository;
            _emailService = emailService;
            _unitOfWork = unitOfWork;
        }

        public async Task<RegisterUserResponse> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            try
            {
                string verificationCode = new Random().Next(100000, 999999).ToString();

                var user = new User
                {
                    Name = request.Name,
                    Email = request.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),

                    
                    IsEmailVerified = false,
                    EmailVerificationCode = verificationCode,
                    EmailVerificationCodeExpiry = DateTime.UtcNow.AddMinutes(15)
                };

                await _userRepository.AddAsync(user);

                var wallet = new Wallet
                {
                    UserId = user.Id,
                    IBAN = UniqueIbanGenerator.Generate(),
                    Balance = 900000
                };

                await _walletRepository.AddAsync(wallet);

               
                await _unitOfWork.SaveChangesAsync();

                
                string mailBody = $@"
                <h3>SmartWallet AI'ye Hoş Geldiniz!</h3>
                <p>Hesabınızı aktifleştirmek için doğrulama kodunuz:</p>
                <h2>{verificationCode}</h2>
                <p>Bu kod 15 dakika boyunca geçerlidir.</p>";

                
                await _emailService.SendEmailAsync(user.Email, "Hesap Doğrulama Kodunuz", mailBody);

                return new RegisterUserResponse
                {
                    UserId = user.Id,
                    WalletId = wallet.Id,
                    IBAN = wallet.IBAN
                };
            }
            catch (DbUpdateException)
            {               
                throw new Exception("Kayıt sırasında bir çakışma oluştu. E-posta veya IBAN zaten kullanımda olabilir.");
            }
            catch (Exception ex)
            {
                // Gerçek hatayı (ex.Message) ve varsa alt detayını (InnerException) ekrana basıyoruz!
                throw new Exception($"Gerçek Hata: {ex.Message} | Detay: {ex.InnerException?.Message}");
            }
        }
    }
}