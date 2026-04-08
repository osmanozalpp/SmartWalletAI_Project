using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Features.Auth.Commands.VerifyResetCode
{
    public class VerifyResetCodeCommand :IRequest<VerifyResetCodeResponse>
    {
        public string Email { get; set; }
        public string Code { get; set; }
    }
    public class VerifyResetCodeResponse
    {
        public bool IsSucces { get; set; }
        public string Message { get; set; }
    }
}
