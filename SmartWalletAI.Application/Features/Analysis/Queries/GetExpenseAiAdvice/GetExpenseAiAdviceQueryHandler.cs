using MediatR;
using Microsoft.EntityFrameworkCore; // Kritik: ToListAsync için şart!
using SmartWalletAI.Application.Common.Interfaces;
using SmartWalletAI.Domain.Entities;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Features.Analysis.Queries.GetExpenseAiAdvice;

public class GetExpenseAiAdviceQueryHandler : IRequestHandler<GetExpenseAiAdviceQuery, string>
{
    private readonly IRepository<Transaction> _transactionRepository;
    private readonly IRepository<Wallet> _walletRepository;
    private readonly IAiService _aiService;

    public GetExpenseAiAdviceQueryHandler(
        IRepository<Transaction> transactionRepository,
        IRepository<Wallet> walletRepository,
        IAiService aiService)
    {
        _transactionRepository = transactionRepository;
        _walletRepository = walletRepository;
        _aiService = aiService;
    }

    public async Task<string> Handle(GetExpenseAiAdviceQuery request, CancellationToken cancellationToken)
    {
        // 1. Cüzdan Kontrolü
        var userWallet = await _walletRepository.GetAsync(w => w.UserId == request.UserId);
        if (userWallet == null) return "Cüzdan bulunamadı.";

        // 2. Tarih Ayarları (Sadece ayı değil, yılı da kontrol etmeliyiz)
        var now = DateTime.UtcNow.AddHours(3);
        var currentMonth = now.Month;
        var currentYear = now.Year;

        // 3. Veriyi Çek
        var expenses = await _transactionRepository.GetAllAsQueryable()
            .AsNoTracking()
            .Where(t => t.SenderWalletId == userWallet.Id &&
                        t.TransactionDate.Month == currentMonth &&
                        t.TransactionDate.Year == currentYear)
            .ToListAsync(cancellationToken);

        if (!expenses.Any())
            return "Henüz harcama analizi yapacak kadar veri yok. Harcamaya devam! 🚀";

        // 4. Analiz İçin Veriyi Hazırla
        var total = expenses.Sum(x => x.Amount);
        var topCategoryGroup = expenses
            .GroupBy(x => x.Category)
            .OrderByDescending(g => g.Sum(s => s.Amount))
            .First();

        var topCategoryName = topCategoryGroup.Key.ToString();
        var topCategoryAmount = topCategoryGroup.Sum(x => x.Amount);

        // 5. AI Prompt Oluştur
        string contextData = $"[ANALİZ] Kullanıcı bu ay ({currentMonth}/{currentYear}) toplam {total:N2} TL harcadı. " +
                             $"En çok harcama yapılan kategori: {topCategoryName} ({topCategoryAmount:N2} TL). " +
                             "Kısa ve samimi bir tavsiye ver.";

        // 6. AI Servis Çağrısı
        return await _aiService.GetFinancialAdviceAsync(contextData);
    }
}