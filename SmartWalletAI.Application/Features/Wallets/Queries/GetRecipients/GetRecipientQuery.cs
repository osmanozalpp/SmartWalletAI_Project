using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Features.Wallets.Queries.GetRecipients
{
    public class GetRecipientQuery : IRequest<List<RecipientDto>>
    {
        public Guid UserId { get; set; }
    }
}
