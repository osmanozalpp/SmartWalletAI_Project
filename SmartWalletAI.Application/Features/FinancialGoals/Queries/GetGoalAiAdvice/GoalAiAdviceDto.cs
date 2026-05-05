using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Features.FinancialGoals.Queries.GetGoalAiAdvice
{
    public class GoalAiAdviceDto
    {
        public string Header { get; set; }
        public string Summary { get; set; }
        public List<string> ActionItems { get; set; }
        public decimal? RecommendedMonthlySavings { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }
        public List<string> Suggestions { get; set; } = new List<string>();
    }
}
