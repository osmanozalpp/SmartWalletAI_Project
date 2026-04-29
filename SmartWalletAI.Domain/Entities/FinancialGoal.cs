using SmartWalletAI.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SmartWalletAI.Domain.Entities
{
    public class FinancialGoal : BaseEntity
    {
        [JsonIgnore]
        public Guid UserId { get; set; }
        public string Title { get; set; }
        public decimal TargetAmount { get; set; }
        public decimal CurrentAmount { get; set; }
        public DateTime TargetDate { get; set; }
        public GoalStatus Status { get; set; } = GoalStatus.Active;


        public void Deposit(decimal amount)
        {
            CurrentAmount += amount;
        }

        public decimal CloseAndReturnFunds()
        {
            var fundsToReturn = CurrentAmount;
            CurrentAmount = 0;
            return fundsToReturn;
        }
    }
}
