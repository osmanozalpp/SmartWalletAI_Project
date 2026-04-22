using FluentValidation;
using MediatR;
using SmartWalletAI.Application.Common.Interfaces;
using SmartWalletAI.Domain.Entities;
using ValidationException = FluentValidation.ValidationException;
using SmartWalletAI.Domain.Exceptions;

namespace SmartWalletAI.Application.Features.Auth.Commands.Login
{
    public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, LoginUserResponse>
    {
        private readonly IRepository<User> _userRepository;
        private readonly ITokenService _tokenService;
        private readonly IValidator<LoginUserCommand> _validator;
        private readonly IUnitOfWork _unitOfWork;

        public LoginUserCommandHandler(IRepository<User> userRepository, ITokenService tokenService, IValidator<LoginUserCommand> validator, IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
            _validator = validator;
            _unitOfWork = unitOfWork;
        }

        public async Task<LoginUserResponse> Handle(LoginUserCommand request, CancellationToken cancellationToken)
        {
           
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

           var user = await _userRepository.GetAsync(u => u.Email == request.Email, ignoreFilters: true);


            
            if (user == null)
            {
                throw new UnauthorizedException("E-posta adresi veya şifre hatalı.");
            }

            
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                user.LastFailedLoginDate = DateTime.UtcNow.AddHours(3);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                throw new UnauthorizedException("E-posta adresi veya şifre hatalı.");
            }


            if (user.IsDeleted)
            {
                throw new BusinessException("Bu hesap daha önce silinmiş. Tekrar aktif etmek için destekle iletişime geçin.");
            }

           
            if (!user.IsEmailVerified)
            {
                throw new BusinessException("Lütfen önce emailinize gelen kod ile hesabınızı doğrulayın.");
            }
          
            var accessToken = _tokenService.GenerateAccesToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();
            var refreshTokenExpiration = DateTime.UtcNow.AddDays(7);

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = refreshTokenExpiration;

            await _userRepository.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return new LoginUserResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                RefreshTokenExpiration = refreshTokenExpiration,
            };
        }
    }
}