using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Features.Auth.Commands.ResendVerificationCode
{
    public class ResendVerificationCodeCommand : IRequest<Unit>
    {
        public string Email { get; set; }
    }
}
