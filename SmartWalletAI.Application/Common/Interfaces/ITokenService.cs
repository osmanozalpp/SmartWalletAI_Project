using SmartWalletAI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Common.Interfaces
{
    public interface ITokenService
    {
        
        string GenerateAccesToken(User user);
        string GenerateRefreshToken();   
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);

    }
}
