using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartWalletAI.Application.Common.Interfaces;
using SmartWalletAI.Application.Features.Portfolios.Command.BuyAsset;
using SmartWalletAI.Application.Features.Portfolios.Command.SellAsset;
using SmartWalletAI.Application.Features.Wallets.Commands.TransferMoney;
using SmartWalletAI.Domain.Entities;
using SmartWalletAI.Domain.Enums;
using System.Text.RegularExpressions;
using static SmartWalletAI.Application.Features.Assistant.Commands.SendChatPrompt;

namespace SmartWalletAI.Application.Features.Assistant.Commands;

public class SendChatPromptCommandHandler : IRequestHandler<SendChatPromptCommand, AssistantResponseDto>
{
    private readonly IAiService _aiService;
    private readonly IRepository<ChatMessage> _chatMessageRepository;
    private readonly IMediator _mediator;
    private readonly IRepository<SavedContact> _savedContactRepository;

    public SendChatPromptCommandHandler(IAiService aiService, IRepository<ChatMessage> chatMessageRepository, IMediator mediator, IRepository<SavedContact> savedContactRepository)
    {
        _aiService = aiService;
        _chatMessageRepository = chatMessageRepository;
        _mediator = mediator;
        _savedContactRepository =savedContactRepository;
    }

    public async Task<AssistantResponseDto> Handle(SendChatPromptCommand request, CancellationToken cancellationToken)
    {
        var userId = request.UserId;

        await _chatMessageRepository.AddAsync(new ChatMessage
        {
            UserId = userId,
            Role = "user",
            Message = request.Message
        });
        await _chatMessageRepository.SaveChangesAsync();

        var history = await _chatMessageRepository.GetAllAsQueryable()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.Timestamp)
            .Take(10)
            .OrderBy(x => x.Timestamp)
            .ToListAsync(cancellationToken);

        var aiReply = await _aiService.GetChatResponseAsync(request.Message, history);

        AssistantResponseDto finalResponse;

        if (aiReply.Contains("intent:"))
        {
            finalResponse = await HandleAiIntent(aiReply, userId, cancellationToken);
        }
        else
        {
            finalResponse = new AssistantResponseDto(aiReply);
        }

        await _chatMessageRepository.AddAsync(new ChatMessage
        {
            UserId = userId,
            Role = "assistant",
            Message = finalResponse.Reply
        });
        await _chatMessageRepository.SaveChangesAsync();

        return finalResponse;
    }

    // KÖPRÜ METODU: AI Niyetini Gerçek Komutlara Bağlar
    private async Task<AssistantResponseDto> HandleAiIntent(string aiReply, Guid userId, CancellationToken ct)
    {
        // 1. ALTIN ALMA SENARYOSU
        var buyMatch = Regex.Match(aiReply, @"intent:\s*BUY_GOLD,\s*amount:\s*(\d+(\.\d+)?)");
        if (buyMatch.Success && decimal.TryParse(buyMatch.Groups[1].Value, out decimal amount))
        {
            try
            {
                var buyCommand = new BuyAssetCommand
                {
                    UserId = userId,
                    AssetType = AssetType.Gold,
                    Amount = amount
                };

                bool isSuccess = await _mediator.Send(buyCommand, ct);

                if (isSuccess)
                {
                    return new AssistantResponseDto(
                        $"{amount} gram altın alım işlemini başarıyla tamamladın. Cüzdanın güncellendi! 🚀",
                        true,
                        "BUY_GOLD_SUCCESS");
                }
            }
            catch (Exception ex)
            {
                return new AssistantResponseDto(
                    $"Altın alırken bir sorun oluştu: {ex.Message}",
                    false,
                    "BUY_GOLD_ERROR");
            }
        }

        // 2. ALTIN SATMA SENARYOSU
        var sellMatch = Regex.Match(aiReply, @"intent:\s*SELL_GOLD,\s*amount:\s*(\d+(\.\d+)?)");
        if (sellMatch.Success && decimal.TryParse(sellMatch.Groups[1].Value, out decimal sellAmount))
        {
            try
            {
                var sellCommand = new SellAssetCommand
                {
                    UserId = userId,
                    AssetType = AssetType.Gold,
                    Amount = sellAmount
                };

                bool isSuccess = await _mediator.Send(sellCommand, ct);

                if (isSuccess)
                {
                    return new AssistantResponseDto(
                        $"{sellAmount} gram altın satım işlemin başarıyla tamamlandı. Cüzdanın güncellendi! 💸",
                        true,
                        "SELL_GOLD_SUCCESS");
                }
            }
            catch (Exception ex)
            {
                return new AssistantResponseDto(
                    $"Altın satarken bir sorun oluştu: {ex.Message}",
                    false,
                    "SELL_GOLD_ERROR");
            }
        }

        var transferMatch = Regex.Match(aiReply, @"intent:\s*SEND_MONEY,\s*contact:\s*([a-zA-ZğüşıöçĞÜŞİÖÇ\s]+),\s*amount:\s*(\d+(?:\.\d+)?),\s*category:\s*([a-zA-ZğüşıöçĞÜŞİÖÇ\s]+),\s*description:\s*(.+)");

        if (transferMatch.Success && decimal.TryParse(transferMatch.Groups[2].Value, out decimal transferAmount))
        {
            try
            {
                string targetContactName = transferMatch.Groups[1].Value.Trim();
                string categoryText = transferMatch.Groups[3].Value.Trim(); 
                string transferDescription = transferMatch.Groups[4].Value.Trim();

               
                TransactionCategory selectedCategory = TransactionCategory.Diğer; 
               
                string cleanCategoryText = categoryText.ToLower().Replace(" ", "");

                switch (cleanCategoryText)
                {
                    case "market":
                        selectedCategory = TransactionCategory.Market;
                        break;
                    case "yemek":
                        selectedCategory = TransactionCategory.Yemek;
                        break;
                    case "fatura":
                        selectedCategory = TransactionCategory.Fatura;
                        break;
                    case "eğlence":
                    case "eglence":
                        selectedCategory = TransactionCategory.Eğlence;
                        break;
                    case "eğitim":
                    case "egitim":
                        selectedCategory = TransactionCategory.Eğitim;
                        break;
                    case "sağlık":
                    case "saglik":
                        selectedCategory = TransactionCategory.Sağlık;
                        break;
                    case "bireyselödeme":
                    case "bireyselodeme":
                        selectedCategory = TransactionCategory.BireyselÖdeme;
                        break;
                    default:
                        selectedCategory = TransactionCategory.Diğer;
                        break;
                }

                var contact = await _savedContactRepository.GetAsync(c =>
                    c.UserId == userId &&
                    c.ContactName.ToLower() == targetContactName.ToLower());

                if (contact == null)
                {
                    return new AssistantResponseDto(
                        $"Rehberinde '{targetContactName}' adında kayıtlı bir alıcı bulamadım.",
                        false,
                        "SEND_MONEY_CONTACT_NOT_FOUND");
                }

                // 3.3: Gerçek Transfer Komutunu Tetikle
                var transferCommand = new TransferMoneyCommand
                {
                    SenderId = userId,
                    ReceiverIban = contact.Iban,
                    Amount = transferAmount,
                    Description = transferDescription,
                    Category = selectedCategory 
                };

                var response = await _mediator.Send(transferCommand, ct);

                if (response.Success)
                {
                    return new AssistantResponseDto(
                        $"{targetContactName} adlı kişiye {transferAmount} TL başarıyla gönderildi.\nKategori: {categoryText}\nNot: {transferDescription} 💸",
                        true,
                        "SEND_MONEY_SUCCESS");
                }
            }
            catch (Exception ex)
            {
                return new AssistantResponseDto(
                    $"Para transferi sırasında bir sorun oluştu: {ex.Message}",
                    false,
                    "SEND_MONEY_ERROR");
            }
        }


        return new AssistantResponseDto(aiReply);
    }
}