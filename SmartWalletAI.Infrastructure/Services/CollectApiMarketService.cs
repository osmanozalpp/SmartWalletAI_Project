using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using SmartWalletAI.Application.Common.DTOs;
using SmartWalletAI.Application.Common.Interfaces;
using SmartWalletAI.Domain.Enums;
using System;
using System.Linq;

namespace SmartWalletAI.Infrastructure.Services
{
    public class CollectApiMarketService : IMarketDataService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public CollectApiMarketService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;

            _httpClient.BaseAddress = new Uri(_configuration["CollectApi:BaseUrl"]);
            _httpClient.DefaultRequestHeaders.Add("authorization", $"apikey {_configuration["CollectApi:ApiKey"]}");
        }

        public async Task<List<MarketPriceDto>> GetCurrentPricesAsync(CancellationToken cancellationToken)
        {
            var prices = new List<MarketPriceDto>();

            try
            {
                
                var currencyResponse = await _httpClient.GetAsync("economy/allCurrency", cancellationToken);

                if (currencyResponse.IsSuccessStatusCode)
                {
                    var currencyData = await currencyResponse.Content.ReadFromJsonAsync<CollectApiResult>(cancellationToken: cancellationToken);
                    if (currencyData?.Success == true)
                    {

                        var usd = currencyData.Result.FirstOrDefault(c => c.Code == "USD");
                        if (usd != null)
                            prices.Add(new MarketPriceDto { AssetType = AssetType.USD, BuyPrice = usd.Buying, SellPrice = usd.Selling });

                        var eur = currencyData.Result.FirstOrDefault(c => c.Code == "EUR");
                        if (eur != null)
                            prices.Add(new MarketPriceDto { AssetType = AssetType.EUR, BuyPrice = eur.Buying, SellPrice = eur.Selling });
                    }
                }
                else
                {
                    await LogApiError("DÖVİZ", currencyResponse, cancellationToken);
                }

                
                var goldResponse = await _httpClient.GetAsync("economy/goldPrice", cancellationToken);

                if (goldResponse.IsSuccessStatusCode)
                {
                    var goldData = await goldResponse.Content.ReadFromJsonAsync<CollectApiResult>(cancellationToken: cancellationToken);
                    if (goldData?.Success == true)
                    {
                     
                        var gramGold = goldData.Result.FirstOrDefault(c => c.Name == "Gram Altın");
                        if (gramGold != null)
                            prices.Add(new MarketPriceDto { AssetType = AssetType.Gold, BuyPrice = gramGold.Buying, SellPrice = gramGold.Selling });

                        
                        var silver = goldData.Result.FirstOrDefault(c => c.Name == "Gümüş")
                                     ?? goldData.Result.FirstOrDefault(c => c.Name == "Has Gümüş");

                        if (silver != null)
                            prices.Add(new MarketPriceDto { AssetType = AssetType.Silver, BuyPrice = silver.Buying, SellPrice = silver.Selling });
                    }
                }
                else
                {
                    await LogApiError("ALTIN/METAL", goldResponse, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n=== [KRİTİK] BAĞLANTI HATASI: {ex.Message} ===\n");
            }

            return prices;
        }

        private async Task LogApiError(string type, HttpResponseMessage response, CancellationToken ct)
        {
            var errorDetail = await response.Content.ReadAsStringAsync(ct);
            Console.WriteLine($"\n=== COLLECT API {type} HATASI (KOD: {response.StatusCode}) ===");
            Console.WriteLine($"DETAY: {errorDetail}");
            Console.WriteLine($"==================================\n");
        }
    }
}