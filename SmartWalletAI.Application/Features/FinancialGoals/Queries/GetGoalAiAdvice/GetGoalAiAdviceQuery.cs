using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Features.FinancialGoals.Queries.GetGoalAiAdvice
{
    public class GetGoalAiAdviceQuery : IRequest<GoalAiAdviceDto>
    {
        [JsonIgnore]
        public Guid GoalId { get; set; }
    }
}
