using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Features.Auth.Commands.ProfileChangeEmail
{
    public class UpdateEmailCommand : IRequest
    {
        [JsonIgnore]
        public Guid UserId { get; set; }
        public string NewEmail { get; set; }
        public string ConfirmEmail { get; set; }
    }
    }

