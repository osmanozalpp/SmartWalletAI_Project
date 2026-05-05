using MediatR;
using SmartWalletAI.Application.Features.FinancialGoals.Queries.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Features.FinancialGoals.Queries.GetFinancialGoalsList
{
    public class GetFinancialGoalsQuery : IRequest<List<FinancialGoalDto>>
    {
        [JsonIgnore]
        public Guid UserId { get; set; }
    }
}
