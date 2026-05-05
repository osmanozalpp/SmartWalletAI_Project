using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Features.FinancialGoals.Queries.Common
{
    public class FinancialGoalDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public decimal TargetAmount { get; set; }
        public decimal CurrentAmount { get; set; }
        public DateTime TargetDate { get; set; }

        public double CompletionPercentage => TargetAmount > 0
            ? (double)Math.Round((CurrentAmount / TargetAmount) * 100, 1)
            : 0;

        public decimal RemainingAmount => TargetAmount - CurrentAmount;

        public int DaysRemaining => (TargetDate - DateTime.UtcNow).Days > 0
            ? (TargetDate - DateTime.UtcNow).Days
            : 0;
    }
}
