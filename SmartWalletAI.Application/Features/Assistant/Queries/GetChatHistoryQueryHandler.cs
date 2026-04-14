using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartWalletAI.Application.Common.Interfaces;
using SmartWalletAI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Features.Assistant.Queries
{
    public class GetChatHistoryQueryHandler : IRequestHandler<GetChatHistoryQuery, List<ChatHistoryDto>>
    {
        private readonly IRepository<ChatMessage> _chatMessageRepository;

        public GetChatHistoryQueryHandler(IRepository<ChatMessage> chatMessageRepository)
        {
            _chatMessageRepository = chatMessageRepository;
        }

        public async Task<List<ChatHistoryDto>> Handle(GetChatHistoryQuery request, CancellationToken cancellationToken)
        {
            
            var messages = await _chatMessageRepository.GetAllAsQueryable()
                .Where(x => x.UserId == request.UserId)
                .OrderBy(x => x.Timestamp)
                .ToListAsync(cancellationToken);

           
            return messages.Select(m => new ChatHistoryDto(
                m.Role,
                m.Message,
                m.Timestamp
            )).ToList();
        }
        }
    }
