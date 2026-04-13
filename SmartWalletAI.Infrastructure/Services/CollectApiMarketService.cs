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
                        AddPriceIfExist(prices, currencyData.Result, "USD", AssetType.USD);
                        AddPriceIfExist(prices, currencyData.Result, "EUR", AssetType.EUR);
                        AddPriceIfExist(prices, currencyData.Result, "GBP", AssetType.GBP);
                        AddPriceIfExist(prices, currencyData.Result, "CHF", AssetType.CHF);
                        AddPriceIfExist(prices, currencyData.Result, "SAR", AssetType.SAR);
                        AddPriceIfExist(prices, currencyData.Result, "KWD", AssetType.KWD);
                    }
                }

                
                var goldResponse = await _httpClient.GetAsync("economy/goldPrice", cancellationToken);
                if (goldResponse.IsSuccessStatusCode)
                {
                    var goldData = await goldResponse.Content.ReadFromJsonAsync<CollectApiResult>(cancellationToken: cancellationToken);
                    if (goldData?.Success == true)
                    {
                        AddPriceIfExist(prices, goldData.Result, "Gram Altın", AssetType.Gold);
                        AddPriceIfExist(prices, goldData.Result, "Gümüş", AssetType.Silver, "Has Gümüş");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n=== BAĞLANTI HATASI: {ex.Message} ===\n");
            }

            return prices;
        }

        private void AddPriceIfExist(List<MarketPriceDto> list, List<CollectApiCurrency> results, string key, AssetType type, string alternativeKey = null)
        {
            if (results == null) return;

           
            var item = results.FirstOrDefault(c =>
                (c.Code != null && c.Code.Equals(key, StringComparison.OrdinalIgnoreCase)) ||
                (c.Name != null && c.Name.Equals(key, StringComparison.OrdinalIgnoreCase)) ||
                (alternativeKey != null && c.Name != null && c.Name.Equals(alternativeKey, StringComparison.OrdinalIgnoreCase)));

            if (item != null)
            {
                list.Add(new MarketPriceDto
                {
                    AssetType = type,
                    BuyPrice = item.Buying,
                    SellPrice = item.Selling,
                    DailyChangePercentage = item.Rate 
                });
            }
        }
    }
}  
