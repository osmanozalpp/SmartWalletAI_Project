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
            contents.Add(new { role = "user", parts = new[] { new { text = _settings.SystemPrompt } } });
            contents.Add(new { role = "model", parts = new[] { new { text = "Anladım. Sadece SmartWallet AI kapsamındaki finansal konularda yardımcı olacağım." } } });

            //geçmiş mesajları ekle
            foreach (var msg in history)
            {
                contents.Add(new
                {
                    role = msg.Role == "user" ? "user" : "model",
                    parts = new[] { new { text = msg.Message } }
                });
            }

            //güncel kullanıcı mesajını ekle
            contents.Add(new { role = "user", parts = new[] { new { text = userPrompt } } });

            var payload = new
            {
                contents = contents.ToArray(),
            };

            //api isteği

            var response = await _httpClient.PostAsJsonAsync(
               $"v1beta/models/{_settings.ModelName}:generateContent?key={_settings.ApiKey}",
                payload);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                return "Şu anda finansal konularda yardımcı olamıyorum. Lütfen daha sonra tekrar deneyin. (Hata: " + errorBody + ")";
            }

            var result = await response.Content.ReadFromJsonAsync<GeminiResponse>();

            return result?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text
               ?? "Üzgünüm, bu isteği şu an işleyemiyorum.";
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
