using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalletAI.Infrastructure.Services
{
    public class AiSettings
    {
        public string ApiKey { get; set; } = string.Empty;
        public string ModelName { get; set; } = string.Empty;
        public string SystemPrompt { get; set; } = string.Empty;
    }
}
