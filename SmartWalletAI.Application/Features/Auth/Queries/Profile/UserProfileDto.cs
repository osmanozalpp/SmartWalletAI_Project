using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Features.Auth.Queries.Profile
{
    public record UserProfileDto
    {
        public string Name { get; init; }
        public string Email { get; init; }
        public char Initial { get; init; }
    }
}
