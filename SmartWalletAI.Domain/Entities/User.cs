using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalletAI.Domain.Entities
{
    public class User : BaseEntity
    {

        public string Name { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } =string.Empty;
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime  { get; set; }

        public bool TwoFactorEnabled { get; set; } = false;

        public bool IsEmailVerified { get; set; } = false;
        public string? EmailVerificationCode { get; set; }
        public DateTime? EmailVerificationCodeExpiry { get; set; }

        
        public string? PasswordResetCode { get; set; }
        public DateTime? PasswordResetCodeExpiry { get; set; }
        public Wallet Wallet { get; set; }=null;

        public DateTime? LastFailedLoginDate { get; set; } 


    }
}
