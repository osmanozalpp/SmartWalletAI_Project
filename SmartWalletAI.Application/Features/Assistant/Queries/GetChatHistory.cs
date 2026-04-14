using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Features.Assistant.Queries
{
    public record GetChatHistoryQuery(Guid UserId) : IRequest<List<ChatHistoryDto>>;

    public record ChatHistoryDto(
        string Role,
        string Message,
        DateTime Timestamp
    );
}
