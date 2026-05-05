using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartWalletAI.Application.Features.FinancialGoals.Commands.AddFunds;
using SmartWalletAI.Application.Features.FinancialGoals.Commands.CloseFinancialGoal;
using SmartWalletAI.Application.Features.FinancialGoals.Commands.CreateGoal;
using SmartWalletAI.Application.Features.FinancialGoals.Commands.UpdateFinancialGoal;
using SmartWalletAI.Application.Features.FinancialGoals.Queries.GetFinancialGoalsList;
using SmartWalletAI.Application.Features.FinancialGoals.Queries.GetFinancialGoalsSummary;
using SmartWalletAI.Application.Features.FinancialGoals.Queries.GetGoalAiAdvice;
using SmartWalletAI.Application.Features.FinancialGoals.Queries.GetGoalContributions;
using System.Security.Claims;

namespace SmartWalletAI.WebAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class FinancialGoalsController : ControllerBase
    {
        private readonly IMediator _mediator;
        public FinancialGoalsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("create-target")]
        public async Task<IActionResult> Create([FromBody] CreateGoalCommand command)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized("Kullanıcı bilgisi doğrulanamadı.");

            command.UserId = Guid.Parse(userIdClaim);

            var result = await _mediator.Send(command);
            return Ok(new { Success = true, GoalId = result, Message = "Hedef başarıyla oluşturuldu." });
        }

        [HttpPost("add-funds")]
        public async Task<IActionResult> AddFunds([FromBody] AddFundsToGoalCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(new { Success = result, Message = "Para hedefe başarıyla aktarıldı." });
        }

        [HttpPost("{id}/close")]
        public async Task<IActionResult> Close(Guid id)
        {
            var result = await _mediator.Send(new CloseFinancialGoalCommand(id));
            return Ok(new { Success = result, Message = "Hedef kapatıldı ve bakiye cüzdana aktarıldı." });
        }

        [HttpGet("financial-goals")]
        public async Task<IActionResult> GetGoals()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var result = await _mediator.Send(new GetFinancialGoalsQuery { UserId = userId });

            return Ok(result);
        }

        [HttpGet("{id}/ai-advice")]
        public async Task<IActionResult> GetAIRecommendation(Guid id)
        {
            var result = await _mediator.Send(new GetGoalAiAdviceQuery { GoalId = id });
            return Ok(result);
        }

        [HttpGet("summary/{userId}")]
        public async Task<IActionResult> GetSummary(Guid userId)
        {
            var query = new GetFinancialGoalsSummaryQuery(userId);
            var result = await _mediator.Send(query);

            return Ok(result);
        }

        [HttpGet("{goalId}/contributions")]
        public async Task<IActionResult> GetGoalContributions(Guid goalId)
        {
            var query = new GetGoalContributionsQuery(goalId);
            var result = await _mediator.Send(query);

            return Ok(result);
        }

        [HttpPost("{goalId}/target-update")]
        public async Task<IActionResult> UpdateGoal(Guid goalId, [FromBody] UpdateFinancialGoalCommand command)
        {         
            command.GoalId = goalId;

            var result = await _mediator.Send(command);

            return Ok(result);
        }
    }
}
