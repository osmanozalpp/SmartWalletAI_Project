using MediatR;
using Microsoft.EntityFrameworkCore;
using Polly;
using SmartWalletAI.Application.Common.Interfaces;
using SmartWalletAI.Application.Features.Portfolios.Command.BuyAsset;
using SmartWalletAI.Application.Features.Wallets.Commands.TransferMoney;
using SmartWalletAI.Domain.Entities;
using SmartWalletAI.Domain.Enums;
using System.Globalization;
using System.Text.RegularExpressions;
using static SmartWalletAI.Application.Features.Assistant.Commands.SendChatPrompt;

namespace SmartWalletAI.Application.Features.Assistant.Commands;

public class SendChatPromptCommandHandler : IRequestHandler<SendChatPromptCommand, AssistantResponseDto>
{
    private readonly IAiService _aiService;
    private readonly IRepository<ChatMessage> _chatMessageRepository;
    private readonly IMediator _mediator;
    private readonly IRepository<SavedContact> _savedContactRepository;
    private readonly IRepository<MarketPrice> _marketRepository;

    // Regex'leri Compiled yaparak performansı artırıyoruz
    private static readonly Regex TransferRegex = new(@"intent:\s*SEND_MONEY,\s*contact:\s*([^,]+),\s*amount:\s*(\d+(?:\.\d+)?),\s*category:\s*([^,]+),\s*description:\s*(.*)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex BuyRegex = new(@"intent:\s*BUY_ASSET,\s*asset:\s*([a-zA-ZçğıöşüÇĞİÖŞÜ]+),\s*amount:\s*(\d+(?:\.\d+)?),\s*unit:\s*([a-zA-Z]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex NavRegex = new(@"intent:\s*NAVIGATE,\s*target:\s*([a-zA-Z0-9_]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public SendChatPromptCommandHandler(IAiService aiService, IRepository<ChatMessage> chatMessageRepository, IMediator mediator, IRepository<SavedContact> savedContactRepository, IRepository<MarketPrice> marketRepository)
    {
        _aiService = aiService;
        _chatMessageRepository = chatMessageRepository;
        _mediator = mediator;
        _savedContactRepository = savedContactRepository;
        _marketRepository = marketRepository;
    }

    public async Task<AssistantResponseDto> Handle(SendChatPromptCommand request, CancellationToken cancellationToken)
    {
        var userId = request.UserId;
        string aiReply = string.Empty;

        try
        {
            // 1. Verileri EF Core çakışması olmadan sırayla çek (En güvenlisi budur)
            var history = await _chatMessageRepository.GetAllAsQueryable().AsNoTracking()
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.Timestamp).Take(5).OrderBy(x => x.Timestamp).ToListAsync(cancellationToken);

            var userContacts = await _savedContactRepository.GetAllAsQueryable().AsNoTracking()
                .Where(c => c.UserId == userId && !c.IsDeleted).Select(c => c.ContactName).ToListAsync(cancellationToken);

            var marketPrices = await _marketRepository.GetAllAsQueryable().AsNoTracking()
                .Select(m => $"{m.Type}:{m.CurrentSellPrice}").ToListAsync(cancellationToken);

            // 2. Promptu Oluştur
            string contactStr = userContacts.Any() ? string.Join(", ", userContacts) : "Kayıtlı kişi yok";
            string priceStr = marketPrices.Any() ? string.Join(" | ", marketPrices) : "Fiyat bilgisi yok";

            string enrichedMessage = @$"Kullanıcı Mesajı: {request.Message}
            ---
            [Sistem Bilgisi - Bu verileri kullanarak işlem yapabilirsin]:
            Rehber: {contactStr}
            Piyasa: {priceStr}
            ---";

            // 3. AI Çağrısı (Retry politikasını basitleştirdik)
            var retryPolicy = Policy
                .Handle<Exception>()
                .OrResult<string>(r => string.IsNullOrEmpty(r) || r.Contains("sorun var"))
                .WaitAndRetryAsync(1, _ => TimeSpan.FromMilliseconds(300));

            try
            {
                aiReply = await retryPolicy.ExecuteAsync(async () =>
                    await _aiService.GetChatResponseAsync(enrichedMessage, history));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--- AI HATASI: {ex.Message}");
                aiReply = "Sway şu an yoğun, lütfen tekrar dener misin? ⚡";
            }

            // 4. Yanıtı İşle ve Kaydet
            AssistantResponseDto finalResponse;
            if (aiReply.Contains("intent:"))
            {
                finalResponse = await HandleAiIntent(aiReply, userId, cancellationToken);
                string cleanReply = aiReply.Split("intent:")[0].Trim();
                finalResponse = finalResponse with { Reply = string.IsNullOrEmpty(cleanReply) ? finalResponse.Reply : cleanReply };
            }
            else
            {
                finalResponse = new AssistantResponseDto(aiReply);
            }

            // Asistan yanıtını kaydet (Kullanıcı mesajını daha önce kaydettiğini varsayıyorum)
            await _chatMessageRepository.AddAsync(new ChatMessage { UserId = userId, Role = "assistant", Message = finalResponse.Reply });
            await _chatMessageRepository.SaveChangesAsync();

            return finalResponse;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"--- GLOBAL HATA: {ex.Message}");
            return new AssistantResponseDto("Sistemde bir yoğunluk var, hemen bakıyorum. 🛠️");
        }
    }

    private async Task<AssistantResponseDto> HandleAiIntent(string aiReply, Guid userId, CancellationToken ct)
    {
        // --- PARA TRANSFERİ ---
        var transferMatch = TransferRegex.Match(aiReply);
        if (transferMatch.Success && decimal.TryParse(transferMatch.Groups[2].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal transferAmount))
        {
            var contact = await _savedContactRepository.GetAsync(c => c.UserId == userId && !c.IsDeleted && c.ContactName.ToLower() == transferMatch.Groups[1].Value.Trim().ToLower());
            if (contact != null)
            {
                var response = await _mediator.Send(new TransferMoneyCommand { SenderId = userId, ReceiverIban = contact.Iban, Amount = transferAmount, Category = MapCategory(transferMatch.Groups[3].Value), Description = transferMatch.Groups[4].Value.Trim() }, ct);
                if (response.Success) return new AssistantResponseDto($"{contact.ContactName} kişisine {transferAmount} TL gönderildi! 💸", true);
            }
        }

        // --- VARLIK ALIMI ---
        var buyMatch = BuyRegex.Match(aiReply);
        if (buyMatch.Success && decimal.TryParse(buyMatch.Groups[2].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal buyAmount))
        {
            var asset = MapAssetType(buyMatch.Groups[1].Value.Trim());
            if (asset != 0)
            {
                var success = await _mediator.Send(new BuyAssetCommand { UserId = userId, AssetType = asset, Amount = buyAmount, IsFiatAmount = buyMatch.Groups[3].Value.ToUpper() == "TL" }, ct);
                if (success) return new AssistantResponseDto($"{buyAmount} {buyMatch.Groups[3].Value} değerinde {buyMatch.Groups[1].Value} alımı tamamlandı! 🚀", true);
            }
        }

        // --- NAVİGASYON ---
        var navMatch = NavRegex.Match(aiReply);
        if (navMatch.Success)
        {
            var page = MapNavigationPage(navMatch.Groups[1].Value.ToLower());
            if (page != NavigationPage.Unknown) return new AssistantResponseDto("Yönlendiriyorum...", true, $"NAVIGATE_{page.ToString().ToUpper()}");
        }

        return new AssistantResponseDto("İşlem anlaşıldı ancak parametreler eksik.");
    }
    private AssetType MapAssetType(string asset)
    {
        return asset switch
        {
            "GOLD" or "ALTIN" => AssetType.Gold,
            "SILVER" or "GÜMÜŞ" or "GUMUS" => AssetType.Silver,
            "USD" or "DOLAR" => AssetType.USD,
            "EUR" or "EURO" => AssetType.EUR,
            "GBP" or "STERLİN" or "STERLIN" => AssetType.GBP,
            "CHF" or "FRANK" => AssetType.CHF,
            "SAR" or "RİYAL" or "RIYAL" => AssetType.SAR,
            "KWD" or "DİNAR" or "DINAR" => AssetType.KWD,
            _ => 0
        };
    }

    private TransactionCategory MapCategory(string category)
    {
        string clean = category.ToLower().Replace(" ", "");
        return clean switch
        {
            "market" => TransactionCategory.Market,
            "yemek" => TransactionCategory.Yemek,
            "fatura" => TransactionCategory.Fatura,
            "eğlence" or "eglence" => TransactionCategory.Eğlence,
            "eğitim" or "egitim" => TransactionCategory.Eğitim,
            "sağlık" or "saglik" => TransactionCategory.Sağlık,
            "bireyselödeme" or "bireyselodeme" => TransactionCategory.BireyselÖdeme,
            "kira" or "kiraödeme" or "kiraodeme" => TransactionCategory.KiraÖdeme,
            _ => TransactionCategory.Diğer
        };
    }

    private NavigationPage MapNavigationPage(string target)
    {
        return target switch
        {
            "buygold" => NavigationPage.BuyGold,
            "sellgold" => NavigationPage.SellGold,
            "transfer" => NavigationPage.TransferMoney,
            "history" => NavigationPage.InvestmentHistory,
            "analysis" => NavigationPage.ExpenseAnalysis,
            "portfolio" => NavigationPage.Portfolio,
            _ => NavigationPage.Unknown
        };
    }
}