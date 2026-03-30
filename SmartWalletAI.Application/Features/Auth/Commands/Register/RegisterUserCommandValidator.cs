using FluentValidation;
using SmartWalletAI.Application.Common.Interfaces;
using SmartWalletAI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Features.Auth.Commands.Register
{
    public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
    {
        private readonly IRepository<User> _userRepository;

        public RegisterUserCommandValidator(IRepository<User> userRepository)
        {
            _userRepository = userRepository;

            RuleFor(p => p.Name)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("İsim alanı boş bırakılamaz.")
                .MinimumLength(2).WithMessage("İsim en az 2 karakter olmalıdır.");

            RuleFor(p => p.Email)
                .NotEmpty().WithMessage("Email boş bırakılamaz.")
                .EmailAddress().WithMessage("Geçerli bir email formatı giriniz.")
                .MustAsync(BeUniqueEmail).WithMessage("Bu email adresi zaten kullanımda.");

            RuleFor(p => p.Password)
                .NotEmpty().WithMessage("Şifre boş bırakılamaz.")
                .MinimumLength(6).WithMessage("Şifre en az 6 haneli olmalıdır.")
                .Matches(@"[A-Z]").WithMessage("Şifre en az bir büyük harf içermelidir.")
                .Matches(@"[a-z]").WithMessage("Şifre en az bir küçük harf içermelidir.")
                .Matches(@"[0-9]").WithMessage("Şifre en az bir rakam içermelidir.");   
        }
        private async Task<bool> BeUniqueEmail(string email , CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetAsync(u=>u.Email == email);
            return user == null;
        }
    }
}
