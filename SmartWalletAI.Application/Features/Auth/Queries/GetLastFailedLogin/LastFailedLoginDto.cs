using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Features.Auth.Queries.GetLastFailedLogin
{
    public class LastFailedLoginDto
    {
        public DateTime? LastFailedLoginDate { get; set; }
        public string Message => "Başarısız deneme";
    }
}
