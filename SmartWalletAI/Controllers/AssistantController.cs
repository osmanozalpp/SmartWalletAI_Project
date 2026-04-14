using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static SmartWalletAI.Application.Features.Assistant.Commands.SendChatPrompt;
using System.Security.Claims;
using MediatR;
using SmartWalletAI.Application.Features.Assistant.Queries;

namespace SmartWalletAI.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AssistantController : ControllerBase
    {
        private readonly IMediator _mediator;
        public AssistantController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetHistory()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Unauthorized();

            var userId = Guid.Parse(userIdClaim);

            // Yeni bir Query oluşturup MediatR ile geçmişi çekiyoruz
            var result = await _mediator.Send(new GetChatHistoryQuery(userId));

            return Ok(result);
        }

        [HttpPost("chat")]
        public async Task<IActionResult> Chat([FromBody] string message)
        {
            
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Unauthorized();

            var userId = Guid.Parse(userIdClaim);

            
            var result = await _mediator.Send(new SendChatPromptCommand(message, userId));

            return Ok(result);
        }
    }
}
