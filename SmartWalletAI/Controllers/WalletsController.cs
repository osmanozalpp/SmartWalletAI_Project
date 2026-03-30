using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartWalletAI.Application.Features.Wallets.Commands.Queries.GetMyWallet;
using SmartWalletAI.Application.Features.Wallets.Commands.Queries.GetWalletTransactions;
using SmartWalletAI.Application.Features.Wallets.Commands.TransferMoney;
using System.Security.Claims;

namespace SmartWalletAI.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class WalletsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public WalletsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("mywallet")]
        public async Task<IActionResult> GetMyWallet()
        {
            var userIdFromToken = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdFromToken))
                return Unauthorized("Geçerli bir kimlik bulunamadı.");

            var query = new GetMyWalletQuery
            {
                UserId = Guid.Parse(userIdFromToken)
            };

            var result = await _mediator.Send(query);

            return Ok(result);
        }


        [HttpPost("transfer")]
        public async Task<IActionResult> TransferMoney([FromBody] TransferMoneyCommand command)
        {
           
            var userIdFromToken = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdFromToken))
                return Unauthorized("Geçerli bir kimlik bulunamadı.");

            command.SenderId = Guid.Parse(userIdFromToken);

            var response = await _mediator.Send(command);
            return Ok(response);
        }
        [HttpGet("{walletId}/transactions")]
        public async Task<IActionResult> GetWalletTransactions([FromRoute] Guid walletId , [FromQuery] GetWalletTransactionsQuery query)
        {
            query.WalletId = walletId;

            var result = await _mediator.Send(query);

            return Ok(result);
        }
    }
}
