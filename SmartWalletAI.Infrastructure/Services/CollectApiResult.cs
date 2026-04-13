using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalletAI.Infrastructure.Services
{
    public class CollectApiResult
    {
        public bool Success { get; set; }
        public List<CollectApiCurrency> Result { get; set; }
    }
    public class CollectApiCurrency
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public decimal Buying { get; set; }
        public decimal Selling { get; set; }
        public double Rate { get; set; } // API'den gelen % değişim oranı
    }
}
