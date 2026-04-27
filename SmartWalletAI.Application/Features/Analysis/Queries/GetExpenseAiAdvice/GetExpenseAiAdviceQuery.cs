using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Features.Analysis.Queries.GetExpenseAiAdvice
{
    public record GetExpenseAiAdviceQuery(Guid UserId) : IRequest<string>;
    
}
