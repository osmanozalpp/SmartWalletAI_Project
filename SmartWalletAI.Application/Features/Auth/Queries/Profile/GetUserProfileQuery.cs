using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Features.Auth.Queries.Profile
{
    public record GetUserProfileQuery(Guid UserId) : IRequest<UserProfileDto>
    {

    }
}
