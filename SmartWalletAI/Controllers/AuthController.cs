using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SmartWalletAI.Application.Features.Auth.Commands.DeleteAccount;
using SmartWalletAI.Application.Features.Auth.Commands.ForgotPassword;
using SmartWalletAI.Application.Features.Auth.Commands.Login;
using SmartWalletAI.Application.Features.Auth.Commands.Logout;
using SmartWalletAI.Application.Features.Auth.Commands.ReactivateAccount;
using SmartWalletAI.Application.Features.Auth.Commands.Register;
using SmartWalletAI.Application.Features.Auth.Commands.ResendVerificationCode;
using SmartWalletAI.Application.Features.Auth.Commands.ResetPassword;
using SmartWalletAI.Application.Features.Auth.Commands.VerifyEmail;
using SmartWalletAI.Application.Features.Auth.Commands.VerifyResetCode;
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

        [HttpPost("forgot-password")]
        [EnableRateLimiting("AuthPolicy")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordCommand command)
        {
            await _mediator.Send(command);
            
            return Ok(new { Message = "Doğrulama kodu e-posta adresinize gönderildi.", Succes = true });
        }

        [HttpPost("verify-code")]
        [EnableRateLimiting("AuthPolicy")]
        public async Task<IActionResult> VerifyCode([FromBody] VerifyResetCodeCommand command)
        {
            var response =await _mediator.Send(command);

            if (!response.IsSucces)
                return BadRequest(new { Message = response.Message, Succes = false });

            return Ok(new { Message = response.Message, Succes = true });
        }

        [HttpPost("reset-password")]
        [EnableRateLimiting("AuthPolicy")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand command)
        {         
            var response = await _mediator.Send(command);

            if (!response.IsSuccess)
            {
                
                return BadRequest(new { Message = response.Message, Success = false });
            }            
            return Ok(new { Message = response.Message, Success = true });
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
        [Authorize]
        [HttpDelete("delete-account")]
        public async Task<IActionResult> DeleteAccount()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdString))
                return Unauthorized();

            var result = await _mediator.Send(new DeleteAccountCommand { UserId = Guid.Parse(userIdString) });

            return Ok(new
            {
                Message = "Hesabınız başarıyla silindi.",
                Success = result
            });
        }
        [HttpPost("reactivate-account")]
        public async Task<IActionResult> ReactivateAccount([FromBody] ReactivateAccountCommand command)
        {
            var result = await _mediator.Send(command);

            return Ok(new
            {
                Message = "Hesabınız başarıyla reaktive edildi. Artık giriş yapabilirsiniz.",
                Success = result
            });
        }
        }
}
