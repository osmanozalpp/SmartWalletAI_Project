using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Features.Auth.Commands.ReactivateAccount
{
    public class ReactivateAccountCommand : IRequest<bool>
    {
        public string Email { get; set; }
    }
}
