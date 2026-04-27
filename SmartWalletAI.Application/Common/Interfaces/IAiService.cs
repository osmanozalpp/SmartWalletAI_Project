using SmartWalletAI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Common.Interfaces
{
    public interface IAiService
    {
        Task<string> GetChatResponseAsync(string userPrompt, IEnumerable<ChatMessage> history, string appContext);

        Task<string> GetFinancialAdviceAsync(string contextData);
    }
}
