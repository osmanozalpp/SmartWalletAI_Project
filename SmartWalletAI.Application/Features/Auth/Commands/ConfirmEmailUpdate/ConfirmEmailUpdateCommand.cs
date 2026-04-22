using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Features.Auth.Commands.ConfirmEmailUpdate
{
    public class ConfirmEmailUpdateCommand : IRequest
    {
        [JsonIgnore]
        public Guid UserId { get; set; }
        public string Code { get; set; }
    }
}
