using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Features.FinancialGoals.Queries.GetFinancialGoalsSummary
{
    public class FinancialGoalsSummaryDto
    {
        public int TotalGoalCount { get; set; }
        public decimal TotalSavedAmount { get; set; }
        public decimal TotalTargetAmount { get; set; }
        public int CompletionPercentage { get; set; }
    }
}
