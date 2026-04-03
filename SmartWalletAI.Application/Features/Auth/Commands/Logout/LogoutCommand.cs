using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Features.Auth.Commands.Logout
{
    public class LogoutCommand : IRequest<bool>
    {
        [JsonIgnore]
        public Guid UserId { get; set; }
    }
}
