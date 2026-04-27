using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalletAI.Domain.Entities
{
    public class ChatMessage
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public string Role { get; set; } = null!;
        public string Message { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow.AddHours(3);
    }
}
