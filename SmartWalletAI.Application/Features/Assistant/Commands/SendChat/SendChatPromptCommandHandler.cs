using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartWalletAI.Application.Common.Interfaces;
using SmartWalletAI.Application.Features.Portfolios.Command.BuyAsset;
using SmartWalletAI.Application.Features.Portfolios.Command.SellAsset;
using SmartWalletAI.Application.Features.Wallets.Commands.TransferMoney;
using SmartWalletAI.Domain.Entities;
using SmartWalletAI.Domain.Enums;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using static SmartWalletAI.Application.Features.Assistant.Commands.SendChat.SendChatPrompt;

namespace SmartWalletAI.Application.Features.Assistant.Commands.SendChat;

public class SendChatPromptCommandHandler : IRequestHandler<SendChatPromptCommand, AssistantResponseDto>
{
    private readonly IAiService _aiService;
    private readonly IRepository<ChatMessage> _chatMessageRepository;
    private readonly IMediator _mediator;
    private readonly IRepository<SavedContact> _savedContactRepository;
    private readonly IRepository<Wallet> _walletRepository;
    private readonly IRepository<MarketPrice> _marketPriceRepository;
    private readonly IRepository<Asset> _assetRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SendChatPromptCommandHandler(
        IAiService aiService,
        IRepository<ChatMessage> chatMessageRepository,
        IMediator mediator,
        IRepository<SavedContact> savedContactRepository,
        IRepository<Wallet> walletRepository,
        IRepository<MarketPrice> marketPriceRepository,
        IRepository<Asset> assetRepository,
        IUnitOfWork unitOfWork)
    {
        _aiService = aiService;
        _chatMessageRepository = chatMessageRepository;
        _mediator = mediator;
        _savedContactRepository = savedContactRepository;
        _walletRepository = walletRepository;
        _marketPriceRepository = marketPriceRepository;
        _assetRepository = assetRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<AssistantResponseDto> Handle(SendChatPromptCommand request, CancellationToken cancellationToken)
    {
        var userId = request.UserId;

        // 1. DİNAMİK BAĞLAM (CONTEXT) OLUŞTURMA
        var contextBuilder = new StringBuilder();
        contextBuilder.AppendLine("### SYSTEM_DATA: INTERNAL USE ONLY ###");
        contextBuilder.AppendLine($"- Current_Time: {DateTime.Now:G}");

        var wallet = await _walletRepository.GetAllAsQueryable()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
        contextBuilder.AppendLine($"- User_Balance: {wallet?.Balance ?? 0:N2} TL");

        var rates = await _marketPriceRepository.GetAllAsQueryable()
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        contextBuilder.AppendLine("\n- CURRENT_MARKET_RATES:");
        foreach (var r in rates) contextBuilder.AppendLine($"  * {r.Type}: {r.CurrentBuyPrice} TL");

        var contacts = await _savedContactRepository.GetAllAsQueryable()
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .Select(x => x.ContactName)
            .ToListAsync(cancellationToken);
        contextBuilder.AppendLine("\n- SAVED_CONTACTS:");
        contextBuilder.AppendLine(contacts.Any() ? string.Join(", ", contacts) : "Kayıtlı kişi yok.");

        var userAssets = await _assetRepository.GetAllAsQueryable()
            .AsNoTracking()
            .Where(a => a.UserId == userId && a.Amount > 0)
            .ToListAsync(cancellationToken);
        contextBuilder.AppendLine("\n- USER_PORTFOLIO_SUMMARY:");
        if (userAssets.Any())
        {
            foreach (var asset in userAssets)
            {
                var marketPrice = rates.FirstOrDefault(m => m.Type == asset.Type);
                var currentVal = asset.Amount * (marketPrice?.CurrentBuyPrice ?? 0);
                contextBuilder.AppendLine($"  * {asset.Type}: {asset.Amount:N2} units (Value: {currentVal:N2} TL)");
            }
        }
        else contextBuilder.AppendLine("Henüz bir yatırım bulunmuyor.");

        var appContext = contextBuilder.ToString();

        // 2. GEÇMİŞİ ÇEK
        var history = await _chatMessageRepository.GetAllAsQueryable()
            .AsNoTracking()
            .Where(x => x.UserId == userId && !x.Message.Contains("Hata:"))
            .OrderByDescending(x => x.Timestamp)
            .Take(6)
            .OrderBy(x => x.Timestamp)
            .ToListAsync(cancellationToken);

        // 3. AI SERVİS ÇAĞRISI
        var aiReply = await _aiService.GetChatResponseAsync(request.Message, history, appContext);
        AssistantResponseDto finalResponse;

        // 4. INTENT ANALİZİ
        if (aiReply.Contains("intent:"))
        {
            finalResponse = await HandleAiIntent(aiReply, userId, cancellationToken);

            if (finalResponse.Reply.Contains("intent:"))
            {
                var cleanMessage = Regex.Replace(finalResponse.Reply, @"intent:.*", "", RegexOptions.Singleline).Trim();
                if (string.IsNullOrWhiteSpace(cleanMessage))
                {
                    cleanMessage = "İşlemi anladım ancak arka planda bir kopukluk oldu. Lütfen tekrar dener misin?";
                }
                finalResponse = new AssistantResponseDto(cleanMessage);
            }
        }
        else
        {
            finalResponse = new AssistantResponseDto(aiReply);
        }

        // 5. MESAJLARI KAYDET
        if (!aiReply.Contains("Hata:") && !aiReply.Contains("işlemini gerçekleştiremedim") && !string.IsNullOrEmpty(aiReply))
        {
            await _chatMessageRepository.AddAsync(new ChatMessage { UserId = userId, Role = "user", Message = request.Message });
            await _chatMessageRepository.AddAsync(new ChatMessage { UserId = userId, Role = "assistant", Message = finalResponse.Reply });
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return finalResponse;
    }

    private async Task<AssistantResponseDto> HandleAiIntent(string aiReply, Guid userId, CancellationToken ct)
    {
        try
        {
            var buyMatch = Regex.Match(aiReply, @"intent:\s*BUY_ASSET,\s*asset:\s*([^,]+),\s*amount:\s*(\d+(?:\.\d+)?)", RegexOptions.IgnoreCase);
            var sellMatch = Regex.Match(aiReply, @"intent:\s*SELL_ASSET,\s*asset:\s*([^,]+),\s*amount:\s*(\d+(?:\.\d+)?)", RegexOptions.IgnoreCase);

            if (buyMatch.Success || sellMatch.Success)
            {
                var match = buyMatch.Success ? buyMatch : sellMatch;
                var assetStr = match.Groups[1].Value.Trim().ToUpper();

                if (!decimal.TryParse(match.Groups[2].Value, CultureInfo.InvariantCulture, out decimal amount))
                    return new AssistantResponseDto("Miktar anlaşılamadı, lütfen tekrar dener misin?");

                var assetType = MapAssetNameToEnum(assetStr);

                if (assetType == null)
                    return new AssistantResponseDto($"'{assetStr}' sistemimizde alım-satıma açık bir varlık değil. Lütfen geçerli bir yatırım aracı seçin.");

                bool result = buyMatch.Success
                    ? await _mediator.Send(new BuyAssetCommand { UserId = userId, AssetType = assetType.Value, Amount = amount }, ct)
                    : await _mediator.Send(new SellAssetCommand { UserId = userId, AssetType = assetType.Value, Amount = amount }, ct);

                if (result)
                {
                    string action = buyMatch.Success ? "alım" : "satım";
                    return new AssistantResponseDto($"{amount.ToString("N2", CultureInfo.GetCultureInfo("tr-TR"))} birim {assetStr} {action} işlemini başarıyla tamamladım. ✅", true, $"{assetStr}_SUCCESS");
                }
            }

            var transferMatch = Regex.Match(aiReply, @"intent:\s*SEND_MONEY,\s*contact:\s*([^,]+),\s*amount:\s*(\d+(?:\.\d+)?),\s*category:\s*([^,]+),\s*description:\s*(.*)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            if (transferMatch.Success)
            {
                string contactName = transferMatch.Groups[1].Value.Trim();
                string amountStr = transferMatch.Groups[2].Value;
                string categoryStr = transferMatch.Groups[3].Value.Trim();
                string desc = transferMatch.Groups[4].Value.Trim();

                if (!decimal.TryParse(amountStr, CultureInfo.InvariantCulture, out decimal amount))
                    return new AssistantResponseDto("Gönderilecek miktar formatı hatalı.");

                var contact = await _savedContactRepository.GetAsync(c => c.UserId == userId && c.ContactName.ToLower() == contactName.ToLower());
                if (contact == null) return new AssistantResponseDto($"'{contactName}' rehberde bulunamadı.", false, "CONTACT_NOT_FOUND");

                var response = await _mediator.Send(new TransferMoneyCommand
                {
                    SenderId = userId,
                    ReceiverIban = contact.Iban,
                    Amount = amount,
                    Description = string.IsNullOrWhiteSpace(desc) || desc == "Yok" ? "-" : desc,
                    Category = MapCategoryToEnum(categoryStr)
                }, ct);

                if (response.Success)
                    return new AssistantResponseDto($"{contactName} kişisine {amount.ToString("N2", CultureInfo.GetCultureInfo("tr-TR"))} TL başarıyla gönderildi. ✅", true, "SEND_MONEY_SUCCESS");

                return new AssistantResponseDto($"Transfer başarısız: {response.Message}");
            }
        }
        catch (Exception ex)
        {
            return new AssistantResponseDto($"İşlem sırasında teknik bir hata oluştu: {ex.Message}", false, "INTENT_EXCEPTION");
        }

        return new AssistantResponseDto(aiReply);
    }

    private AssetType? MapAssetNameToEnum(string name) => name switch
    {
        "ALTIN" or "GOLD" => AssetType.Gold,
        "GÜMÜŞ" or "SILVER" => AssetType.Silver,
        "USD" or "DOLAR" => AssetType.USD,
        "EUR" or "EURO" => AssetType.EUR,
        "GBP" or "STERLİN" => AssetType.GBP,
        "CHF" or "FRANK" => AssetType.CHF,
        "SAR" or "RİYAL" => AssetType.SAR,
        "KWD" or "DİNAR" => AssetType.KWD,
        _ => null
    };

    private TransactionCategory MapCategoryToEnum(string category) => category.ToLower().Replace(" ", "") switch
    {
        "market" => TransactionCategory.Market,
        "yemek" => TransactionCategory.Yemek,
        "fatura" => TransactionCategory.Fatura,
        "eğlence" or "eglence" => TransactionCategory.Eğlence,
        "eğitim" or "egitim" => TransactionCategory.Eğitim,
        "sağlık" or "saglik" => TransactionCategory.Sağlık,
        "bireyselödeme" or "bireyselodeme" => TransactionCategory.BireyselÖdeme,
        "kiraödeme" or "kiraodeme" => TransactionCategory.KiraÖdeme,
        _ => TransactionCategory.Diğer
    };
}