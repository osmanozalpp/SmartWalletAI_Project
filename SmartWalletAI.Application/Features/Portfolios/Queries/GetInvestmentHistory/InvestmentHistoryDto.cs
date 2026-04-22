using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Features.Portfolios.Queries.GetInvestmentHistory
{
    public class InvestmentHistoryDto
    {
        public List<InvestmentTransactionItemDto> Transactions { get; set; } = new();
        public string MonthlyAiSummary { get; set; }
    }


    public class InvestmentTransactionItemDto
        {
            public string AssetName { get; set; }     
            public string TransactionType { get; set; } 
            public decimal Amount { get; set; }       
            public decimal TotalPrice { get; set; }   
            public DateTime Date { get; set; }
        }
}
