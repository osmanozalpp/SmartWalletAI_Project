using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SmartWalletAI.Application.Features.Auth.Commands.ConfirmEmailUpdate;
using SmartWalletAI.Application.Features.Auth.Commands.DeleteAccount;
using SmartWalletAI.Application.Features.Auth.Commands.ForgotPassword;
using SmartWalletAI.Application.Features.Auth.Commands.Login;
using SmartWalletAI.Application.Features.Auth.Commands.Logout;
using SmartWalletAI.Application.Features.Auth.Commands.ProfileChangeEmail;
using SmartWalletAI.Application.Features.Auth.Commands.ProfileChangePassword;
using SmartWalletAI.Application.Features.Auth.Commands.ReactivateAccount;
using SmartWalletAI.Application.Features.Auth.Commands.Register;
using SmartWalletAI.Application.Features.Auth.Commands.ResendVerificationCode;
using SmartWalletAI.Application.Features.Auth.Commands.ResetPassword;
using SmartWalletAI.Application.Features.Auth.Commands.VerifyEmail;
using SmartWalletAI.Application.Features.Auth.Commands.VerifyResetCode;
using SmartWalletAI.Application.Features.Auth.Queries.Profile;
using SmartWalletAI.Domain.Entities;
using System.Security.Claims;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }


    [AllowAnonymous]
    [HttpPost("register")]
    [EnableRateLimiting("AuthPolicy")]
    public async Task<IActionResult> Register([FromBody] RegisterUserCommand command)
    {
        var response = await _mediator.Send(command);
        return Ok(response);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [EnableRateLimiting("AuthPolicy")]
    public async Task<IActionResult> Login([FromBody] LoginUserCommand command)
    {
        var response = await _mediator.Send(command);
        return Ok(response);
    }

    [AllowAnonymous]
    [HttpPost("verify-email")]
    [EnableRateLimiting("AuthPolicy")]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailCommand command)
    {
        await _mediator.Send(command);
        return Ok(new { Message = "Hesabınız başarıyla doğrulandı. Artık giriş yapabilirsiniz." });
    }

    [AllowAnonymous]
    [HttpPost("forgot-password")]
    [EnableRateLimiting("AuthPolicy")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordCommand command)
    {
        await _mediator.Send(command);
        return Ok(new { Message = "Doğrulama kodu e-posta adresinize gönderildi." });
    }

    [AllowAnonymous]
    [HttpPost("verify-code")]
    [EnableRateLimiting("AuthPolicy")]
    public async Task<IActionResult> VerifyCode([FromBody] VerifyResetCodeCommand command)
    {
        await _mediator.Send(command);
        return Ok(new { Message = "Kod doğrulandı, şifrenizi yenileyebilirsiniz." });
    }

    [AllowAnonymous]
    [HttpPost("reset-password")]
    [EnableRateLimiting("AuthPolicy")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand command)
    {
        await _mediator.Send(command);
        return Ok(new { Message = "Şifreniz başarıyla sıfırlandı." });
    }

    [AllowAnonymous]
    [HttpPost("resend-verification-code")]
    public async Task<IActionResult> ResendVerificationCode([FromBody] ResendVerificationCodeCommand command)
    {
        await _mediator.Send(command);
        return Ok(new { Message = "Yeni doğrulama kodunuz e-posta adresinize gönderilmiştir." });
    }

    [AllowAnonymous]
    [HttpPost("reactivate-account")]
    public async Task<IActionResult> ReactivateAccount([FromBody] ReactivateAccountCommand command)
    {
        await _mediator.Send(command);
        return Ok(new { Message = "Hesabınız başarıyla reaktive edildi. Artık giriş yapabilirsiniz." });
    }


    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        await _mediator.Send(new LogoutCommand { UserId = Guid.Parse(userId) });
        return Ok(new { Message = "Başarıyla çıkış yapıldı." });
    }

    [HttpPost("delete-account")]
    public async Task<IActionResult> DeleteAccount([FromBody] DeleteAccountCommand command)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        command.UserId = Guid.Parse(userId);
        await _mediator.Send(command);
        return Ok(new { Message = "Hesabınız başarıyla silindi." });
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var result = await _mediator.Send(new GetUserProfileQuery(Guid.Parse(userId)));
        return Ok(result);
    }

    [HttpPost("change-password-profile")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordCommand command)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        command.UserId = Guid.Parse(userId);
        await _mediator.Send(command);
        return Ok(new { message = "Şifreniz başarıyla güncellendi." });
    }

    [HttpPost("update-email")]
    public async Task<IActionResult> UpdateEmail([FromBody] UpdateEmailCommand command)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        command.UserId = Guid.Parse(userId);
        await _mediator.Send(command);
        return Ok(new { message = "Doğrulama kodu yeni e-posta adresinize gönderildi." });
    }

    [HttpPost("confirm-email-update")]
    public async Task<IActionResult> ConfirmEmailUpdate([FromBody] ConfirmEmailUpdateCommand command)
    {       
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userIdString))
            return Unauthorized(new { Message = "Oturum bilgisi bulunamadı." });

        command.UserId = Guid.Parse(userIdString);
      
        await _mediator.Send(command);

        return Ok(new
        {
            Message = "E-posta adresiniz başarıyla doğrulandı.",
            Success = true
        });
    }
}
