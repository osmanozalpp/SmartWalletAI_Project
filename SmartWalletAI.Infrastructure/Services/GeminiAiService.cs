using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartWalletAI.Application.Common.Interfaces;
using SmartWalletAI.Domain.Entities;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Polly.Timeout;

namespace SmartWalletAI.Infrastructure.Services
{
    public class GeminiAiService : IAiService
    {
        private readonly HttpClient _httpClient;
        private readonly AiSettings _settings;
        private readonly ILogger<GeminiAiService> _logger; 

        public GeminiAiService(HttpClient httpClient, IOptions<AiSettings> settings, ILogger<GeminiAiService> logger)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _logger = logger;
        }
        public async Task<string> GetChatResponseAsync(string userPrompt, IEnumerable<ChatMessage> history, string appContext)
        {
            try
            {
                var contents = history.Select(msg => new
                {
                    role = msg.Role == "user" ? "user" : "model",
                    parts = new[] { new { text = msg.Message } }
                }).ToList();

                contents.Add(new { role = "user", parts = new[] { new { text = userPrompt } } });

                var payload = new
                {
                    system_instruction = new { parts = new[] { new { text = _settings.SystemPrompt + "\n\n" + appContext } } },
                    contents = contents.ToArray(),
                    generationConfig = new { temperature = 0.7, maxOutputTokens = 800 },
                    // GÜVENLİK AYARLARI: Yanıtın boş dönmesini (Safety Block) engeller
                    safetySettings = new[]
                    {
                new { category = "HARM_CATEGORY_HARASSMENT", threshold = "BLOCK_NONE" },
                new { category = "HARM_CATEGORY_HATE_SPEECH", threshold = "BLOCK_NONE" },
                new { category = "HARM_CATEGORY_SEXUALLY_EXPLICIT", threshold = "BLOCK_NONE" },
                new { category = "HARM_CATEGORY_DANGEROUS_CONTENT", threshold = "BLOCK_NONE" }
            }
                };

                var url = $"v1beta/models/gemini-2.5-flash:generateContent?key={_settings.ApiKey}";
                var response = await _httpClient.PostAsJsonAsync(url, payload);

                if (!response.IsSuccessStatusCode)
                {
                    var err = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Sway API Hatası: {Err}", err);
                    return "Yoğunluktan dolayı işlemini gerçekleştiremedim tekrar dener misin :(";
                }

                var result = await response.Content.ReadFromJsonAsync<GeminiResponse>();

                // Yanıtın neden boş geldiğini loglayalım (Safety kısıtlaması mı?)
                var candidate = result?.Candidates?.FirstOrDefault();
                if (candidate?.FinishReason == "SAFETY")
                {
                    _logger.LogWarning("Gemini Yanıtı GÜVENLİK nedeniyle engelledi!");
                    return "Üzgünüm, bu isteği güvenlik kuralları nedeniyle cevaplayamıyorum.";
                }

                return candidate?.Content?.Parts?.FirstOrDefault()?.Text ?? "Yanıt boş.";
            }
            catch (Polly.Timeout.TimeoutRejectedException)
            {
                _logger.LogWarning("Sway: Polly üzerinden zaman aşımı (Timeout) hatası alındı.");
                return "Şu an biraz yoğunum ve düşünmem normalden uzun sürüyor 😅. Kısa bir süre sonra tekrar dener misin?";
            }        
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Sway: İstek iptal edildi veya HttpClient zaman aşımına uğradı.");
                return "İsteğini işlerken bir gecikme oldu, bağlantı koptu sanırım. Tekrar dener misin?";
            }          
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sway: Beklenmedik bir hata oluştu: {Msg}", ex.Message);
                return "Ufak bir teknik aksaklık oldu ama ben buradayım! Tekrar deneyebilir misin? 🛠️";
            }
        }
        public async Task<string> GetFinancialAdviceAsync(string contextData)
        {
            var prompt = $"Aşağıdaki finansal verileri analiz et ve kısa, samimi bir tavsiye ver: {contextData}";
            return await GetChatResponseAsync(prompt, Enumerable.Empty<ChatMessage>(), contextData);
        }

        public class GeminiResponse
        {
            [JsonPropertyName("candidates")]
            public List<Candidate?> Candidates { get; set; }
        }
        public class Candidate
        {
            [JsonPropertyName("content")]
            public Content? Content { get; set; }

            [JsonPropertyName("finishReason")]
            public string? FinishReason { get; set; }
        }
        public class Content
        {
            [JsonPropertyName("parts")]
            public List<Part?> Parts { get; set; }
        }
        public class Part
        {
            [JsonPropertyName("text")]
            public string? Text { get; set; }
        }
    }
}