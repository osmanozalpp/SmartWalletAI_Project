using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Features.Auth.Commands.Register
{
    public class RegisterUserCommand : IRequest<RegisterUserResponse>
    {
        public string Name { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterUserResponse
    {
        public Guid UserId { get; set; }
        public Guid WalletId { get; set; }
        public string IBAN { get; set; } = string.Empty;      
    }
}