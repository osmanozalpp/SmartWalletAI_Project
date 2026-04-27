using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using SmartWalletAI.Application.Common.Interfaces;
using SmartWalletAI.Domain.Entities;
using SmartWalletAI.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Features.Auth.Queries.Profile
{
    public class GetUserProfileQueryHandler : IRequestHandler<GetUserProfileQuery, UserProfileDto>
    {
        private readonly IRepository<User> _userRepository;

        public GetUserProfileQueryHandler(IRepository<User> userRepository)
        {
            _userRepository = userRepository;
        }
        public async Task<UserProfileDto> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetAllAsQueryable()
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

            if (user == null)
            {
                throw new NotFoundException("Kullanıcı bulunamadı.");
            }

            return new UserProfileDto
            {
                Name = user.Name,
                Email = user.Email,
                // İlk harfi alıyoruz, eğer isim boşsa '?' dönüyoruz güvenlik için
                Initial = !string.IsNullOrEmpty(user.Name) ? char.ToUpper(user.Name[0]) : '?'
            };
        }
    }
}
