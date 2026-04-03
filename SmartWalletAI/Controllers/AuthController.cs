using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SmartWalletAI.Application.Features.Auth.Commands.ForgotPassword;
using SmartWalletAI.Application.Features.Auth.Commands.Login;
using SmartWalletAI.Application.Features.Auth.Commands.Logout;
using SmartWalletAI.Application.Features.Auth.Commands.Register;
using SmartWalletAI.Application.Features.Auth.Commands.ResendVerificationCode;
using SmartWalletAI.Application.Features.Auth.Commands.ResetPassword;
using SmartWalletAI.Application.Features.Auth.Commands.VerifyEmail;
using System.Security.Claims;

namespace SmartWalletAI.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;

        }

        [HttpPost("register")]
        [EnableRateLimiting("AuthPolicy")]
        public async Task<IActionResult> Register([FromBody] RegisterUserCommand command)
        {
            var response = await _mediator.Send(command);
            return Ok(response);
        }

        [HttpPost("login")]
        [EnableRateLimiting("AuthPolicy")]
        public async Task<IActionResult> Login([FromBody] LoginUserCommand command)
        {
            var response = await _mediator.Send(command);
            return Ok(response);
        }

        [HttpPost("verify-email")]
        [EnableRateLimiting("AuthPolicy")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailCommand command)
        {
            var result = await _mediator.Send(command);

            return Ok(new
            {
                Message = "Hesabınız başarıyla doğrulandı. Artık giriş yapabilirsiniz.",
                Succes = result
            });
        }
        [HttpPost("reset-password")]
        [EnableRateLimiting("AuthPolicy")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand command)
        {
            var result = await _mediator.Send(command);

            return Ok(new
            {
                Message = "Şifreniz başarıyla güncellendi. Yeni şifrenizle giriş yapabilirsiniz.",
                Succes = result
            });
        }
        [HttpPost("forgot-password")]
        [EnableRateLimiting("AuthPolicy")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordCommand command)
        {
            var result = await _mediator.Send(command);

            return Ok(new
            {
                Message = "Eğer bu email sistemimizde kayıtlıysa şifre sıfırlama kodunuz gönderilmiştir.",
                Success = result
            });
        }

        [HttpPost("resend-verification-code")]
        public async Task<IActionResult> ResendVerificationCode([FromBody] ResendVerificationCodeCommand command)
        {
            await _mediator.Send(command);

            return Ok(new
            {
                Message = "Yeni doğrulama kodunuz e-posta adresinize gönderilmiştir."
            });
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdString))
                return Unauthorized();

            var result = await _mediator.Send(new LogoutCommand { UserId = Guid.Parse(userIdString) });

            return Ok(new
            {
                Message = "Başarıyla çıkış yapıldı.",
                Success = result
            });
        }
    }
}
