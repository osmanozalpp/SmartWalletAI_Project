using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens.Experimental;
using SmartWalletAI.Application.Features.Analysis.Queries.GetExpenseAnalysis;
using SmartWalletAI.Application.Features.Wallets.Commands.Queries.GetMyWallet;
using SmartWalletAI.Application.Features.Wallets.Commands.Queries.GetRecipients;
using SmartWalletAI.Application.Features.Wallets.Commands.Queries.GetWalletTransactions;
using SmartWalletAI.Application.Features.Wallets.Commands.Queries.GetMaskedOwnerName;
using SmartWalletAI.Application.Features.Wallets.Commands.SaveContact;
using SmartWalletAI.Application.Features.Wallets.Commands.TransferMoney;
using System.Security.Claims;
using SmartWalletAI.Application.Features.Wallets.Commands.RemoveContact;

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
        public async Task<IActionResult> GetWalletTransactions([FromRoute] Guid walletId, [FromQuery] GetWalletTransactionsQuery query)
        {
            query.WalletId = walletId;

            var result = await _mediator.Send(query);

            return Ok(result);
        }

        [HttpGet("analysis")]
        public async Task<IActionResult> GetExpenseAnalysis()
        {
            //kullanıcının Id si url den değil tokendan alır.
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);


            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
            {
                return Unauthorized(new { Message = "Geçersiz veya eksik token" });
            }

            //query oluşturuldu ve mediatra fırlatıldı
            var query = new GetExpenseAnalysisQuery { UserId = userId };
            var result = await _mediator.Send(query);

            return Ok(result);
        }

        [HttpGet("recipients")]
        public async Task<IActionResult> GetRecipients()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
            {
                return Unauthorized(new { Message = "Geçersiz veya eksik token" });
            }
            var query = new GetRecipientQuery { UserId = userId };
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpPost("contacts")]
        public async Task<IActionResult> SaveContact([FromBody] SaveContactCommand command)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
            {
                return Unauthorized(new { Message = "Geçersiz veya eksik token" });
            }

            command.UserId = userId;

            var result = await _mediator.Send(command);
            string returnMessage = command.IsFavorite ? "Kişi favorilere başarıyla eklendi." : "Kişi başarıyla kaydedildi.";


            return Ok(new
            {
                Message = returnMessage,
                Success = result
            });

        }
        [HttpGet("iban/{iban}/owner-name")]
        public async Task<IActionResult> GetMaskedOwnerName([FromRoute] string iban)
        {
            var query = new GetMaskedOwnerNameQuery { Iban = iban };
            var result = await _mediator.Send(query);

            return Ok(new { MaskedName = result });

        }
        [HttpDelete("remove-contact/{iban}")]
        public async Task<IActionResult> RemoveContact(string iban)
        {
            
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString))
            {
                return Unauthorized("Kullanıcı kimliği doğrulanamadı.");
            }          
            var result = await _mediator.Send(new RemoveContactCommand
            {
                Iban = iban,
                UserId = Guid.Parse(userIdString) 
            });

            
            return Ok(result);
        }
    }
    }
