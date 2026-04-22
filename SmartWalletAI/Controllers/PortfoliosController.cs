using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartWalletAI.Application.Features.Portfolios.Command.BuyAsset;
using SmartWalletAI.Application.Features.Portfolios.Command.SellAsset;
using SmartWalletAI.Application.Features.Portfolios.Queries.GetInvestmentHistory;
using SmartWalletAI.Application.Features.Portfolios.Queries.GetMarketPrices;
using SmartWalletAI.Application.Features.Portfolios.Queries.GetPortfolioSummary;
using System.Security.Claims;

namespace SmartWalletAI.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PortfoliosController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PortfoliosController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetPortfolioSummary()
        {

            var userIdFromToken = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdFromToken))
                return Unauthorized("Kullanıcı bilgisi token içerisinde bulunamadı.");

            var query = new GetPortfolioSummaryQuery(Guid.Parse(userIdFromToken));
            var result = await _mediator.Send(query);

            return Ok(result);
        }
        [HttpPost("buy")]
        public async Task<IActionResult> BuyAsset([FromBody] BuyAssetCommand command)
        {

            var userIdFromToken = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdFromToken))
                return Unauthorized("Kullanıcı bilgisi token içerisinde bulunamadı.");

            command.UserId = Guid.Parse(userIdFromToken);

            var result = await _mediator.Send(command);

            return Ok(new
            {
                Success = result,
                Message = "Alım işlemi başarıyla gerçekleşti."
            });
        }

        [HttpPost("sell")]
        public async Task<IActionResult> SellAsset([FromBody] SellAssetCommand command)
        {
            var userIdFromToken = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdFromToken))
                return Unauthorized("Kullanıcı bilgisi token içerisinde bulunamadı.");

            command.UserId = Guid.Parse(userIdFromToken);

            var result = await _mediator.Send(command);

            return Ok(new
            {
                Success = result,
                Message = "Satış işlemi başarıyla gerçekleşti."
            });
        }
        [HttpGet("prices")]
        public async Task<IActionResult> GetMarketPrices()
        {
            var query = new GetMarketPricesQuery();
            var result = await _mediator.Send(query);

            return Ok(result);
        }

        [HttpGet("investment-history")]
        public async Task<IActionResult> GetInvestmentHistory()
        {
            var userIdFromToken = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdFromToken))
                return Unauthorized("Kullanıcı bilgisi token içerisinde bulunamadı.");

            var query = new GetInvestmentHistoryQuery(Guid.Parse(userIdFromToken));
            var result = await _mediator.Send(query);

            return Ok(result);
        }
    }
}
