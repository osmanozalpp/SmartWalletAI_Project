using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Common.Helpers
{
    public class UniqueIbanGenerator
    {
        public static string Generate()
        {
            string countryCode = "TR";
            string bankCode = "00034"; 
            string reserveDigit = "0";

            
            string accountNo = GetSecureRandomDigits(16);

            
            string bban = $"{bankCode}{reserveDigit}{accountNo}";

            
            string checkDigit = CalculateMod97(bban);

            return $"{countryCode}{checkDigit}{bban}";
        }

        private static string GetSecureRandomDigits(int length)
        {
            var bytes = new byte[length];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            // Her byte'ı 0-9 arası bir rakama dönüştürür
            return string.Concat(bytes.Select(b => (b % 10).ToString()));
        }

        private static string CalculateMod97(string bban)
        {
            
            string combined = $"{bban}292700";
            BigInteger ibanNumber = BigInteger.Parse(combined);
            int remainder = (int)(ibanNumber % 97);
            int checkSum = 98 - remainder;
            return checkSum.ToString("D2");
        }
    }
}
