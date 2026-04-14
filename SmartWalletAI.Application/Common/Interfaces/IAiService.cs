using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Common.Interfaces
{
    public interface IAiService
    {
        Task<string> GetChatResponseAsync(string userPrompt , IEnumerable<Domain.Entities.ChatMessage> history);

        Task<string> GetFinancialAdviceAsync(string contextData);
    }
}
