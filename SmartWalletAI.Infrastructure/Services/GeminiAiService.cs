using Microsoft.AspNetCore.Routing.Matching;
using Microsoft.Extensions.Options;
using SmartWalletAI.Application.Common.Interfaces;
using SmartWalletAI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SmartWalletAI.Infrastructure.Services
{
    public class GeminiAiService : IAiService
    {

        private readonly HttpClient _httpClient;
        private readonly AiSettings _settings;

        public GeminiAiService(HttpClient httpClient, IOptions<AiSettings> settings)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
        }

        public async Task<string> GetChatResponseAsync(string userPrompt, IEnumerable<ChatMessage> history)
        {
            var contents = new List<object>();

            // 1. ADIM: Sadece geçerli bir geçmiş oluştur (Sıralama hatasını kökten çözelim)
            var cleanHistory = history.TakeLast(4).ToList();
            foreach (var msg in cleanHistory)
            {
                contents.Add(new
                {
                    role = msg.Role.ToLower() == "assistant" ? "model" : "user",
                    parts = new[] { new { text = msg.Message } }
                });
            }

            // Eğer geçmişin sonu user ise, yeni mesajı ayrı bir user mesajı olarak DEĞİL, 
            // son mesajın devamı olarak ekle (Gemini kuralı).
            if (contents.Count > 0 && ((dynamic)contents.Last()).role == "user")
            {
                var lastMsg = (dynamic)contents.Last();
                contents[contents.Count - 1] = new { role = "user", parts = new[] { new { text = lastMsg.parts[0].text + "\n" + userPrompt } } };
            }
            else
            {
                contents.Add(new { role = "user", parts = new[] { new { text = userPrompt } } });
            }

            // 2. ADIM: PAYLOAD (Hem deve hörgücü hem alt çizgili versiyonu destekleyen en garanti yapı)
            var payload = new
            {
                // v1beta için en doğru isimlendirme budur:
                system_instruction = new { parts = new[] { new { text = _settings.SystemPrompt } } },
                contents = contents.ToArray(),
                generationConfig = new
                {
                    temperature = 0.5, // 0.6 yerine 0.5 daha tutarlıdır
                    maxOutputTokens = 1000,
                    topP = 0.9
                }
            };

            string modelPath = _settings.ModelName.StartsWith("models/") ? _settings.ModelName : $"models/{_settings.ModelName}";
            var url = $"v1beta/{modelPath}:generateContent?key={_settings.ApiKey}";

            var response = await _httpClient.PostAsJsonAsync(url, payload);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"--- GEMINI ERROR: {error}");
                return "Şu an bağlantıda bir sorun var, hemen geliyorum! 🚀";
            }

            var result = await response.Content.ReadFromJsonAsync<GeminiResponse>();
            return result?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text?.Trim()
                   ?? "Üzgünüm, yanıt üretemedim.";
        }

        public async Task<string> GetFinancialAdviceAsync(string contextData)
        {
            var prompt = $"Aşağıdaki finansal verilerimi analiz et ve kısa, samimi bir tavsiye ver: {contextData}";

            return await GetChatResponseAsync(prompt, Enumerable.Empty<ChatMessage>());
        }

        //gemini api için gerekli yardımcı modeller
        public class GeminiResponse
        {
            [JsonPropertyName("candidates")]
            public List<Candidate?> Candidates { get; set; }
        }
        public class Candidate
        {
            [JsonPropertyName("content")]
            public Content? Content { get; set; }
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
