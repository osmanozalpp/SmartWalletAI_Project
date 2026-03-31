using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Features.Analysis.Queries.GetExpenseAnalysis
{
    public class GetExpenseAnalysisQuery : IRequest<ExpenseAnalysisDto>
    {
        public Guid UserId { get; set; }
    }
}
