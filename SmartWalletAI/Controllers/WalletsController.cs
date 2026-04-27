using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens.Experimental;
using SmartWalletAI.Application.Features.Analysis.Queries.GetExpenseAnalysis;
using SmartWalletAI.Application.Features.Wallets.Queries.GetMyWallet;
using SmartWalletAI.Application.Features.Wallets.Queries.GetRecipients;
using SmartWalletAI.Application.Features.Wallets.Queries.GetMaskedOwnerName;
using SmartWalletAI.Application.Features.Wallets.Commands.SaveContact;
using SmartWalletAI.Application.Features.Wallets.Commands.TransferMoney;
using System.Security.Claims;
using SmartWalletAI.Application.Features.Wallets.Commands.RemoveContact;
using SmartWalletAI.Application.Features.Wallets.Queries.GetTransactionDetail;
using SmartWalletAI.Application.Features.Wallets.Queries.GetWalletTransactions;
using SmartWalletAI.Application.Features.Analysis.Queries.GetExpenseAiAdvice;

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

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdString))
                return Unauthorized();

            query.WalletId = walletId;
            query.UserId = Guid.Parse(userIdString);

            // 3. Mediator ile gönder
            var result = await _mediator.Send(query);

            return Ok(result);
        }

        [HttpGet("{id}/receipt")]
        public async Task<IActionResult> GetReceipt(Guid id)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var result = await _mediator.Send(new GetTransactionDetailQuery { TransactionId = id, UserId = userId });
            return Ok(result);
        }

        [HttpGet("analysis")]
        public async Task<IActionResult> GetExpenseAnalysis()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
            {
                return Unauthorized(new { Message = "Geçersiz veya eksik token" });
            }

            var query = new GetExpenseAnalysisQuery { UserId = userId };
            var result = await _mediator.Send(query);

            return Ok(result);
        }
        [HttpGet("ai-advice")]
        public async Task<IActionResult> GetAiAdvice()
        {
            var userId =Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (userId == Guid.Empty) return Unauthorized(new { Message = "Geçersiz veya eksik token" });

            var query = new GetExpenseAiAdviceQuery(userId);
            var advice = await _mediator.Send(query);

            return Ok(new { Advice = advice });
        }

        [HttpGet("favorite")]
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
