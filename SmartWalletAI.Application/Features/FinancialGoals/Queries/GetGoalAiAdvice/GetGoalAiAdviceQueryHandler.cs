using MediatR;
using SmartWalletAI.Application.Common.Interfaces;
using SmartWalletAI.Application.Features.FinancialGoals.Queries.GetGoalAiAdvice;
using SmartWalletAI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Features.FinancialGoals.Queries.GetGoalAiAdvice
{
    public class GetGoalAiAdviceQueryHandler : IRequestHandler<GetGoalAiAdviceQuery, GoalAiAdviceDto>
    {
        private readonly IRepository<FinancialGoal> _goalRepository;
        private readonly IRepository<Wallet> _walletRepository;
        private readonly IAiService _aiService;

        public GetGoalAiAdviceQueryHandler(
            IRepository<FinancialGoal> goalRepository,
            IRepository<Wallet> walletRepository,
            IAiService aiService)
        {
            _goalRepository = goalRepository;
            _walletRepository = walletRepository;
            _aiService = aiService;
        }
            public async Task<GoalAiAdviceDto> Handle(GetGoalAiAdviceQuery request, CancellationToken ct)
            {
                var goal = await _goalRepository.GetAsync(g => g.Id == request.GoalId);
                if (goal == null) return null;

                var wallet = await _walletRepository.GetAsync(w => w.UserId == goal.UserId);
                int daysRemaining = (goal.TargetDate - DateTime.UtcNow).Days;

                if (goal.CurrentAmount <= 0)
                {
                    return new GoalAiAdviceDto
                    {
                        Title = "İlk Adımı Atalım!",
                        Description = $"{goal.Title} hedefin için henüz yolun başındasın. Küçük birikimler büyük hayalleri gerçekleştirir, bugün başlamaya ne dersin? 🚀",
                        Suggestions = new List<string> { "Hemen birikim hesabına para aktar."}
                    };
                }

                // 3. Prompt Kurgusu: AI'yı hem samimi olması hem de JSON formatına uyması için zorluyoruz
                string prompt = $@"
                [FİNANSAL_DANIŞMAN_MODU]
                Kullanıcı Bilgileri:
                - Hedef: {goal.Title} ({goal.TargetAmount:N2} TL)
                - Mevcut Durum: {goal.CurrentAmount:N2} TL birikti.
                - Kalan Süre: {daysRemaining} gün.
                - Mevcut Cüzdan Bakiyesi: {wallet.Balance:N2} TL.

                Senden beklenen:
                Kullanıcıyı motive eden, tek cümlelik, samimi ve Türk insanına hitap eden bir özet yaz.
                Ardından hedefe ulaşması içi kısa ve net aksiyon maddesi öner.

                Yanıt Formatı (Sadece JSON dön, ```json gibi ifadeler kullanma):
                {{
                  ""title"": ""AI Önerisi"",
                  ""description"": ""Motivasyon cümlesi buraya"",
                  ""suggestions"": [""Öneri 1"", ""Öneri 2"", ""Öneri 3""]
                }}";

                try
                {
                    // 4. AI Çağrısı ve Temizlik: Gelen yanıttaki çöp karakterleri süpürüyoruz
                    string jsonResponse = await _aiService.GetFinancialAdviceAsync(prompt);

                    // Markdown bloklarını (```json) ve boşlukları temizle
                    string cleanedJson = jsonResponse.Replace("```json", "").Replace("```", "").Trim();

                    // 5. Dönüştürme: JSON'ı DTO'ya çeviriyoruz
                    return System.Text.Json.JsonSerializer.Deserialize<GoalAiAdviceDto>(cleanedJson, new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }

                catch (Exception)
                {
                    // 6. Hata Yönetimi: AI patlarsa kullanıcıya boş ekran yerine 'safe' bir veri dönüyoruz
                    return new GoalAiAdviceDto
                    {
                        Title = "Küçük Bir Tavsiye",
                        Description = "Birikim yapmaya devam ederek hedefine adım adım yaklaşıyorsun. Disiplin en büyük dostundur!",
                        Suggestions = new List<string> { "Harcamalarını kontrol et.", "Düzenli birikime devam et." }
                    };
                }
            }
        }
}