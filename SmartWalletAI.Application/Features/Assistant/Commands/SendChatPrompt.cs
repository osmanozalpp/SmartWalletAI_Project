using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Features.Assistant.Commands
{
    public class SendChatPrompt
    {
        public record SendChatPromptCommand(string Message , Guid UserId) : IRequest<AssistantResponseDto>;

        public record AssistantResponseDto(
            string Reply,
            bool IsActionExecuted = false,
            string? ActionType = null
        );
    }
}
