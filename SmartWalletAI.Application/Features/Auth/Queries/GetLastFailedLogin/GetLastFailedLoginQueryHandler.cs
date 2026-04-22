using MediatR;
using SmartWalletAI.Application.Common.Interfaces;
using SmartWalletAI.Domain.Entities;
using SmartWalletAI.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Features.Auth.Queries.GetLastFailedLogin
{
    public class GetLastFailedLoginQueryHandler : IRequestHandler<GetLastFailedLoginQuery, LastFailedLoginDto>
    {
        private readonly IRepository<User> _userRepository;

        public GetLastFailedLoginQueryHandler(IRepository<User> userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<LastFailedLoginDto> Handle(GetLastFailedLoginQuery request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetAsync(u => u.Id == request.UserId);

            if (user == null) throw new NotFoundException("Kullanıcı bulunamadı.");

            return new LastFailedLoginDto
            {
                LastFailedLoginDate = user.LastFailedLoginDate
            };
        }
    }
}
