using MediatR;
using SmartWalletAI.Application.Common.Interfaces;
using SmartWalletAI.Domain.Entities;
using SmartWalletAI.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Features.Auth.Commands.ResendVerificationCode
{
    public class ResendVerificationCodeCommandHandler : IRequestHandler<ResendVerificationCodeCommand, Unit>
    {
        private readonly IRepository<User> _userRepository;
        private readonly IEmailService _emailService;
        private readonly IUnitOfWork _unitOfWork;

        public ResendVerificationCodeCommandHandler(IRepository<User> userRepository , IEmailService emailService, IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _emailService = emailService;
            _unitOfWork = unitOfWork;
        }


        public async Task<Unit> Handle(ResendVerificationCodeCommand request, CancellationToken cancellationToken)
        {

            var user = await _userRepository.GetAsync(u => u.Email == request.Email);

            if (user == null)
            {               
                throw new NotFoundException("Bu e-posta adresine ait bir hesap bulunamadı.");
            }

            if (user.IsEmailVerified)
            {              
                throw new BusinessException("Bu e-posta adresi zaten doğrulanmış.");
            }

            string newVerificationCode = new Random().Next(100000, 999999).ToString();

            user.EmailVerificationCode = newVerificationCode;
            user.EmailVerificationCodeExpiry = DateTime.UtcNow.AddMinutes(15);

            await _userRepository.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            await _emailService.SendEmailAsync(user.Email, "Yeni Doğrulama Kodu", $"Yeni doğrulama kodunuz: {newVerificationCode}");

          

            return Unit.Value;

        }
    }
}
