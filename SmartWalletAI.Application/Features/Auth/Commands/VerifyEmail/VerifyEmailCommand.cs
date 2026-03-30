using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Features.Auth.Commands.VerifyEmail
{
    public class VerifyEmailCommand : IRequest<bool>
    {
        public string Email { get; set; }
        public string Code { get; set; }

    }
}
