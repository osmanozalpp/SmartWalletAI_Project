using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Features.FinancialGoals.Queries.GetGoalContributions
{
    public class GoalContributionDto
    {
        public decimal Amount { get; set; }
        public DateTime ContributionDate { get; set; }
    }
}
