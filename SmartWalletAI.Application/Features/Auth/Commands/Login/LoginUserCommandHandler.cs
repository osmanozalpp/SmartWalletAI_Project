using FluentValidation;
using MediatR;
using Microsoft.IdentityModel.Tokens;
using SmartWalletAI.Application.Common.Interfaces;
using SmartWalletAI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ValidationException = FluentValidation.ValidationException;


namespace SmartWalletAI.Application.Features.Auth.Commands.Login
{
    public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, LoginUserResponse>
    {
        private readonly IRepository<User> _userRepository;
        private readonly ITokenService _tokenService;
        private readonly IValidator<LoginUserCommand> _validator;

        public LoginUserCommandHandler(IRepository<User> userRepository , ITokenService tokenService , IValidator<LoginUserCommand> validator)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
            _validator = validator;
        }

       
        public async Task<LoginUserResponse> Handle(LoginUserCommand request, CancellationToken cancellationToken)
        {
           var validationResult = await _validator.ValidateAsync(request, cancellationToken);

           if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            


            var user = await _userRepository.GetAsync(u=> u.Email == request.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                throw new Exception("Kullanıcı adı veya şifre hatalı.");
            }

            if (!user.IsEmailVerified)
            {
                throw new Exception("Lütfen önce emailinize gelen kod ile hesabınızı doğrulayın.");
            }

            var accesToken = _tokenService.GenerateAccesToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();
            var refreshTokenExpiration = DateTime.UtcNow.AddDays(7);

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = refreshTokenExpiration;

            _userRepository.UpdateAsync(user);
            _userRepository.SaveChangesAsync();

            return new LoginUserResponse
            {
                AccessToken = accesToken,
                RefreshToken = refreshToken,
                RefreshTokenExpiration = refreshTokenExpiration,
            };

        }
    }
}
