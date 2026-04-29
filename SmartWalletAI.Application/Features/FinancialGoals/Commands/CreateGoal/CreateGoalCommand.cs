using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Features.FinancialGoals.Commands.CreateGoal
{
    public record CreateGoalCommand : IRequest<Guid>
    {
        public string Title { get; init; }
        public decimal TargetAmount { get; init; }
        public DateTime TargetDate { get; init; }

        [JsonIgnore] 
        public Guid UserId { get; set; }
    }
}
